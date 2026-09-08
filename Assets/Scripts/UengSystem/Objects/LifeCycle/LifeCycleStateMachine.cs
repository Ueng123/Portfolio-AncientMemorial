using System;
using UengSystem.UDebug;

namespace UengSystem.Objects.LifeCycle {
	// 한 UObject의 등장·활동·퇴장·반환 순서를 관리함.
	// 콜백 안에서 Release/Get이 다시 호출되는 상황(재진입)과 이전 실행의 지연 완료 요청을 방어함.
	public sealed class LifeCycleStateMachine {
		// 이 머신이 담당하는 객체. 상태도 반드시 같은 객체를 대상으로 해야 함.
		public UObject target { get; }
		// 최초에는 반환 상태이며, Released → Getting → Active → Releasing → Released로 순환함.
		public LifeCyclePhase phase { get; private set; } = LifeCyclePhase.Released;
		// 현재 실행 중인 등장/퇴장 연출. 활동 중이거나 정상 반환을 마치면 null임.
		public LifeCycleState currentState { get; private set; }
		// 새 생애를 시작할 때 증가. 같은 풀 객체가 재사용되어도 이전 생애와 구분함.
		public long lifeNumber { get; private set; }
		// 등장/퇴장 상태를 준비할 때마다 증가. 같은 상태 인스턴스의 재실행도 구분함.
		public long executionNumber { get; private set; }
		// 상태 구성이 확정되었는지 여부. 이후에는 상태 교체 불가.
		public bool isPrepared { get; private set; }
		// 생명주기 처리에서 오류가 발생했는지 여부. 오류 객체의 새 생애 시작을 차단함.
		public bool isFaulted { get; private set; }
		// 영구 종료 절차에 들어갔는지 여부. 일반적인 풀 반환과 달리 다시 Get할 수 없음.
		public bool isShuttingDown { get; private set; }
		// 매 생애에 재사용할 연출 상태. 실행별 값은 PrepareExecution에서 초기화함.
		private Getting GettingState;
		private Releasing ReleasingState;
		// 완료/정리 도중 다른 전환 요청이 끼어드는 것을 막는 잠금 역할.
		private bool Completing;
		// 이전 상태 정리와 퇴장 준비가 진행 중임을 표시함.
		private bool Starting;
		// 퇴장 준비 중 Release(false)가 들어오면 연출을 생략하도록 기억함.
		private bool InstantReleaseRequested;
		// OnRelease 호출 단계에 도달했음을 기록하여 Shutdown에서 중복 호출을 방지함.
		private bool ReleaseStarted;
		// State.Enter 실행 중에는 완료/퇴장을 즉시 수행하지 않고 요청만 보관함.
		private bool Entering;
		private bool PendingCompletion;
		private bool PendingRelease;
		// 보류된 퇴장 요청 중 하나라도 연출 생략을 원하면 false로 유지함.
		private bool PendingReleaseEffect;

		public LifeCycleStateMachine(UObject Target) {
			// 대상 없는 머신은 허용하지 않음.
			target = Target ?? throw new ArgumentNullException(nameof(Target));
		}

		private void CheckConfiguration(LifeCycleState State) {
			// 준비 완료 이후의 변경과 다른 UObject용 상태를 잘못 연결하는 것을 차단함.
			if (isPrepared) throw new InvalidOperationException("Life cycle states can only be configured before OnFirstGet completes.");
			if (State == null || !ReferenceEquals(State.target, target))
				throw new ArgumentException("Each life cycle state must belong to this UObject.", nameof(State));
		}

		// 준비 완료 전에 사용할 등장/퇴장 상태를 명시적으로 지정함.
		public void SetGettingState(Getting State) { CheckConfiguration(State); GettingState = State; }
		public void SetReleasingState(Releasing State) { CheckConfiguration(State); ReleasingState = State; }
		public void SetStates(Getting GettingState, Releasing ReleasingState) {
			// 둘 다 검증한 뒤 대입하므로 한쪽만 교체된 채 예외가 발생하지 않음.
			// CheckConfiguration(GettingState);
			// CheckConfiguration(ReleasingState);
			// this.GettingState = GettingState;
			// this.ReleasingState = ReleasingState;
			
			// GettingState/ReleasingState 변경 로직이 바뀌어도 기능 변경에 불편함 없음
			SetGettingState(GettingState);
			SetReleasingState(ReleasingState);
		}
		public void SetDefaultStates(Getting GettingState = null, Releasing ReleasingState = null) {
			// 이미 지정한 상태는 유지하고, 비어 있는 쪽만 기본값으로 채움(??=).
			if (isPrepared) throw new InvalidOperationException("Life cycle is already prepared.");
			if (GettingState != null) { CheckConfiguration(GettingState); this.GettingState ??= GettingState; }
			if (ReleasingState != null) { CheckConfiguration(ReleasingState); this.ReleasingState ??= ReleasingState; }
		}

		internal void Prepare() {
			// 최초 준비만 수행. 사용자 지정이 없는 쪽에 기본 연출을 넣고 구성을 확정함.
			if (isPrepared) return;
			SetDefaultStates(new DefaultGetting(target), new DefaultReleasing(target));
			isPrepared = true;
		}

		internal void Get(bool PlayEffect) {
			// 준비된 정상 객체가 반환을 마친 경우에만 새 생애를 시작할 수 있음.
			if (!isPrepared || isFaulted || isShuttingDown || phase != LifeCyclePhase.Released || Completing)
				throw new InvalidOperationException("Only a prepared, released UObject can begin a new life.");
			// 생애 번호를 갱신하고 이전 생애의 퇴장 기록을 초기화함.
			lifeNumber++;
			ReleaseStarted = false;
			InstantReleaseRequested = false;
			// OnGet 등 사용자 코드가 실행되기 전에 등장 단계와 실행 번호를 확정함.
			phase = LifeCyclePhase.Getting;
			PrepareState(GettingState, PlayEffect);
			// 콜백 중 재사용되더라도 이번 실행/생애에 대한 검사와 오류 처리가 가능하도록 저장함.
			long Execution = executionNumber;
			long Life = lifeNumber;
			try {
				// 충돌/물리를 멈추고 객체 활성화, OnGet, GetTask를 처리함.
				target.PrepareGetting();
				// 준비 콜백이 퇴장/종료를 요청했다면 기존 등장 연출을 이어서 실행하지 않음.
				if (!IsCurrent(GettingState, Execution)) return;
				StartState(PlayEffect);
			}
			catch (Exception Error) { Fail(Error, Life); }
		}

		public void Release(bool PlayEffect = true) {
			// Enter 콜백이 끝나기 전에 Exit를 실행하면 시작/정리가 뒤섞이므로 요청을 보류함.
			if (Entering && !isShuttingDown && !isFaulted && !Completing) {
				// 이미 퇴장에 진입 중이라면 연출을 유지하는 중복 요청은 무시함.
				if (phase == LifeCyclePhase.Releasing && PlayEffect) return;
				PendingRelease = true;
				// false가 한 번이라도 들어오면 뒤의 true 요청으로 덮어쓰지 않음.
				PendingReleaseEffect &= PlayEffect;
				return;
			}
			// 씬 전체가 종료 중이면 일반 퇴장 대신 즉시 종료 정리로 이동함.
			if (UObject.isStoppingLifeCycles) { Shutdown(); return; }
			// 종료/오류/완료 처리 중이거나 이미 반환되었다면 중복 전환을 막음.
			if (isShuttingDown || isFaulted || Completing || phase == LifeCyclePhase.Released) return;
			// 파괴 콜백에서 이미 비활성화된 형제 객체를 반환할 수 있으므로 연출 없이 종료함.
			if (!target.gameObject.activeInHierarchy) { Shutdown(); return; }
			if (phase == LifeCyclePhase.Releasing) {
				// 퇴장 연출 중 true 요청은 무시하고, false 요청은 퇴장을 즉시 완료함.
				if (PlayEffect) return;
				// 아직 퇴장 준비 중이면 준비가 끝난 뒤 연출을 생략하도록 예약함.
				if (Starting) { InstantReleaseRequested = true; return; }
				RequestComplete(currentState, executionNumber);
				return;
			}

			long Life = lifeNumber;
			// Getting 중이라면 취소할 연출이 있고, Active 중이라면 currentState는 null임.
			LifeCycleState Previous = currentState;
			long PreviousExecution = executionNumber;
			// 정리 콜백에서도 퇴장 중임을 알 수 있도록 먼저 단계와 준비 플래그를 변경함.
			phase = LifeCyclePhase.Releasing;
			Starting = true;
			try {
				if (Previous != null) {
					// 등장 연출 취소: 연출만 정리하며 정상 등장 완료용 Initialize는 호출하지 않음.
					Previous.Exit();
					// 이전 실행의 업데이트 등록만 제거함.
					target.UnregisterLifeCycle(PreviousExecution);
				}
				// 이전 상태의 정리 콜백이 머신을 종료했을 수 있음.
				if (isShuttingDown) { Starting = false; return; }
				PrepareState(ReleasingState, PlayEffect);
				// 활동 등록 해제, 충돌/물리 정지, 기존 작업 취소, OnRelease/ReleaseTask를 처리함.
				target.PrepareReleasing();
				Starting = false;
				if (isShuttingDown || isFaulted) return;
				// 준비 중 즉시 반환 요청이 있었다면 최초 요청과 무관하게 연출을 생략함.
				StartState(PlayEffect && !InstantReleaseRequested);
			}
			catch (Exception Error) { Starting = false; Fail(Error, Life); }
		}

		private void PrepareState(LifeCycleState State, bool PlayEffect) {
			// 재사용할 상태에 새 실행 번호를 부여하고 시간/완료/정리 플래그를 초기화함.
			currentState = State;
			executionNumber++;
			State.PrepareExecution(this, executionNumber, PlayEffect);
		}

		// UObject가 OnRelease를 호출하기 직전에 표시함. 콜백에서 종료되어도 중복 정리하지 않음.
		internal void MarkReleaseStarted() => ReleaseStarted = true;

		private void StartState(bool PlayEffect) {
			// Enter 콜백 후에도 같은 실행인지 확인하기 위해 현재 값을 보관함.
			LifeCycleState State = currentState;
			long Execution = executionNumber;
			if (!PlayEffect) {
				// 연출 생략 시 Enter/OnStartEffect 없이 바로 완료 절차를 수행함.
				RequestComplete(State, Execution);
				return;
			}
			// 등장/퇴장 전용 업데이트에 이번 실행을 등록함.
			target.RegisterLifeCycle(Execution);
			// Enter에서 발생한 요청을 잠금 해제 후 처리할 수 있도록 옮겨 담을 지역변수.
			bool CompleteRequested = false;
			bool ReleaseRequested = false;
			bool ReleaseEffect = true;
			// 이번 Enter 구간에서만 사용할 요청 보관함을 초기화함.
			Entering = true;
			PendingCompletion = false;
			PendingRelease = false;
			PendingReleaseEffect = true;
			try {
				// OnStartEffect 실행. 그 안의 Complete/Release 요청은 위 보관함에 쌓임.
				State.Enter();
				// finally에서 보관함을 비우기 전에 요청을 복사함.
				CompleteRequested = PendingCompletion;
				ReleaseRequested = PendingRelease;
				ReleaseEffect = PendingReleaseEffect;
			}
			finally {
				// Enter가 예외로 끝나더라도 진입 중 표시와 보류 요청이 남지 않게 함.
				Entering = false;
				PendingCompletion = false;
				PendingRelease = false;
				PendingReleaseEffect = false;
			}

			// Enter에서 종료되거나 실행이 바뀌었다면 보관한 요청을 새 실행에 적용하지 않음.
			if (!IsCurrent(State, Execution)) return;
			// 완료와 퇴장이 모두 요청되면 퇴장 우선. 취소된 등장으로 객체가 활성 상태가 되는 것을 막음.
			if (ReleaseRequested) { Release(ReleaseEffect); return; }
			if (CompleteRequested) RequestComplete(State, Execution);
		}

		internal bool IsCurrent(LifeCycleState State, long Execution) {
			// 객체의 상태 참조뿐 아니라 실행 번호까지 같아야 함. 같은 상태 인스턴스도 재사용되기 때문임.
			// Active/Released에는 실행 중인 연출이 없으므로 완료/틱 요청을 허용하지 않음.
			return !isFaulted && !isShuttingDown && ReferenceEquals(currentState, State)
			       && Execution == executionNumber && (phase == LifeCyclePhase.Getting || phase == LifeCyclePhase.Releasing);
		}

		internal void Tick(long Execution, float DeltaTime) {
			// 전환 처리 도중이거나 이전 실행의 업데이트 등록이라면 연출을 진행하지 않음.
			if (Completing || Starting || Entering || !IsCurrent(currentState, Execution)) return;
			long Life = lifeNumber;
			// 상태가 경과 시간을 누적하고, 연출 갱신 또는 시간 만료에 따른 완료를 요청함.
			try { currentState.Tick(DeltaTime); }
			catch (Exception Error) { Fail(Error, Life); }
		}

		internal void RequestComplete(LifeCycleState State, long Execution) {
			// 중복 완료와 이전 실행에서 늦게 도착한 완료 콜백을 무시함.
			if (State == null || Completing || !IsCurrent(State, Execution)) return;
			// Enter 내부 요청은 콜백 종료 후 처리하고, 퇴장 준비 중의 완료는 허용하지 않음.
			if (Entering) { PendingCompletion = true; return; }
			if (Starting) return;
			long Life = lifeNumber;
			// 정상 등장 완료인지 퇴장 완료인지에 따라 후속 절차가 달라짐.
			bool WasGetting = phase == LifeCyclePhase.Getting;
			Completing = true;
			try {
				// 먼저 정상 완료 표시를 해 두어 ClearEffect가 취소와 완료를 구분할 수 있게 함.
				State.MarkComplete();
				// 연출 복구는 한 번만 수행. 이어지는 Exit에서 다시 요청해도 중복 실행되지 않음.
				State.ClearEffectOnce();
				if (isShuttingDown) return;
				// 정리 훅마다 Shutdown이 발생했는지 확인하여 종료 이후의 전환을 막음.
				State.Exit();
				if (isShuttingDown) return;
				// 완료한 연출을 현재 상태와 생명주기 업데이트 목록에서 제거함.
				currentState = null;
				target.UnregisterLifeCycle(Execution);
				if (WasGetting) {
					// 등장 완료: 충돌/물리를 복구하고 일반 활동 루틴에 등록한 뒤 Initialize를 호출함.
					phase = LifeCyclePhase.Active;
					target.ActivateLife();
					Completing = false;
					// Initialize에서는 Release/재획득이 가능하도록 완료 잠금을 먼저 해제함.
					// 이 콜백 이후에는 새 생애가 시작되었을 수 있으므로 머신 값을 덮어쓰지 않음.
					target.Initialize();
					return;
				}
				// 퇴장 완료: 남은 작업 취소, Uninitialize, 물리/외형 복구를 수행함.
				// 이 정리가 끝날 때까지 Completing을 유지하여 재획득/중복 반환을 막음.
				target.FinishReleasing();
				if (isShuttingDown) return;
				phase = LifeCyclePhase.Released;
				Completing = false;
				// 반환 가능 상태를 확정한 뒤 실제 풀로 반환함. 이 생애의 마지막 작업임.
				target.ReturnToPool();
			}
			catch (Exception Error) { Fail(Error, Life); }
		}

		private void Fail(Exception Error, long Life) {
			// 오류를 일으킨 생애가 여전히 현재 생애일 때만 객체를 종료함.
			// 콜백에서 재획득한 뒤 이전 호출이 예외를 던져도 새 생애를 종료하지 않음.
			if (lifeNumber == Life) {
				isFaulted = true;
				Shutdown();
			}
			// 현재 생애 여부와 무관하게 오류 자체는 기록함.
			DebugManager.LogException(Error, target);
		}

		public void Shutdown() {
			// 일반 Release와 달리 연출 완료/풀 재사용을 기다리지 않는 영구 종료 경로임.
			// 플래그를 먼저 세워 정리 콜백의 재귀 Shutdown/Release/Get을 차단함.
			if (isShuttingDown) return;
			isShuttingDown = true;
			
			Completing = true;
			
			// 살아 있던 객체만 퇴장 단계로 표시. 이미 반환된 객체에는 불필요한 OnRelease를 호출하지 않음.
			if (phase != LifeCyclePhase.Released) phase = LifeCyclePhase.Releasing;
			
			// 업데이트에서 먼저 분리한 뒤 저장해 둔 연출의 정리 훅을 호출함.
			LifeCycleState State = currentState;
			currentState = null;
			target.UnregisterLifeCycle(executionNumber);
			target.DeactivateLife();
			// 연출 정리가 실패하더라도 아래의 소유 작업/식별 정보 정리는 계속 시도함.
			try { State?.Exit(); }
			catch (Exception Error) { DebugManager.LogException(Error, target); }
			// 작업을 취소하고 필요한 경우에만 OnRelease 호출. ID/Category도 해제함.
			// 일반 반환 경로의 FinishReleasing/Uninitialize는 이 경로에서 호출하지 않음.
			try { target.StopLife(ReleaseStarted); }
			catch (Exception Error) { DebugManager.LogException(Error, target); }
			phase = LifeCyclePhase.Released;
			// 종료 객체를 비활성화하여 격리함. 파괴 중인 풀에 반환하지 않으며 재획득도 차단된 상태임.
			target.QuarantineLife();
		}
	}
}
