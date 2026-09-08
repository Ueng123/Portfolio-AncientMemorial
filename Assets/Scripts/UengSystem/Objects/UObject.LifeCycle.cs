using System;
using System.Collections.Generic;
using System.Linq;
using UengSystem.Objects.LifeCycle;
using UengSystem.ObjectPool;
using UengSystem.UDebug;
using UengSystem.Utility;
using UengSystem.VisualScripting.Tasks;
using UnityEngine;
using UnityEngine.Serialization;

namespace UengSystem.Objects {
	// UObject의 생명주기 관련 구현을 분리한 partial 파일임. 별도 객체가 생성되는 것은 아님.
	// 상태 전환 판단은 LifeCycleStateMachine이, 실제 컴포넌트/작업/풀 처리는 이쪽이 담당함.
	public partial class UObject {
		// 처음 접근할 때 이 객체 전용 상태 머신을 생성하고 이후에는 재사용함.
		private LifeCycleStateMachine LifeCycle;
		public LifeCycleStateMachine lifeCycle => LifeCycle ??= new LifeCycleStateMachine(this);
		// GameObject 활성화 여부와 다름. 등장/퇴장 중에는 켜져 있어도 isActive는 false임.
		public bool isActive => lifeCycle.phase == LifeCyclePhase.Active;
		// 풀에서 새 생애를 시작할 때 증가하는 번호. 이전 생애의 예약 작업/참조를 구분함.
		public long lifeNumber => lifeCycle.lifeNumber;
		// 인스펙터 기본 연출 시간. 파생 클래스는 아래 프로퍼티를 재정의하여 시간을 변경할 수 있음.
		[SerializeField, Min(0)] private float GettingDuration = 1;
		[SerializeField, Min(0)] private float ReleasingDuration = 1;
		public virtual float gettingDuration => GettingDuration;
		public virtual float releasingDuration => ReleasingDuration;
		// 생명주기 연출에 누적할 시간. UUI처럼 일시정지와 무관하게 동작하려면 재정의함.
		public virtual float lifeCycleDeltaTime => Time.deltaTime;
		// 등장 준비/퇴장 준비 시 실행할 작업 묶음. 연출 완료 훅과는 호출 시점이 다름.
		public Task GetTask;
		public Task ReleaseTask;
		// 원래 소속 풀로 반환하는 콜백. null이면 풀 소속이 아닌 씬 객체로 취급함.
		private Action<UObject> PoolReturn;
		// 최초 준비 시 자식까지 수집한 콜라이더와 각각의 원래 enabled 값.
		private Collider2D[] LifeColliders;
		private bool[] ColliderEnabled;
		// 객체 인스턴스당 한 번만 수행하는 준비가 완료되었는지 표시함.
		private bool FirstGetCompleted;
		// 취소 콜백이 정리 도중 새 작업을 등록하는 것을 막는 플래그.
		private bool StoppingRunningTask;

		// 전체 종료/자체 종료/작업 정리 중에는 소유 작업 시작 불가.
		// 준비 전에는 허용하고, 준비 후에는 Released가 아니어야 함. 퇴장용 작업도 허용하는 조건임.
		public bool canStartOwnedWork =>
			!isStoppingLifeCycles && !StoppingRunningTask && !lifeCycle.isShuttingDown && (!lifeCycle.isPrepared || !isReleased);

		// 씬 전체의 신규 획득과 업데이트를 차단하는 공통 종료 플래그.
		public static         bool                              isStoppingLifeCycles { get; private set; }
		// 객체별로 다음 Update/FixedUpdate 처리 구간에 실행할 작업을 보관함.
		private readonly        Queue<Action>                     ProcessToUpdate      = new();
		private readonly        Queue<Action>                     ProcessToFixedUpdate = new();
		// 이 객체가 소유한 Task 실행 목록. 코루틴이 다른 실행자에서 돌아가도 소유자 기준으로 취소함.
		private readonly        HashSet<TaskExecution>            Tasks                = new();
		// 활동 여부와 무관하게 최초 준비를 마친 객체들을 기억하여 씬 종료 시 모두 정리함.
		private static readonly HashSet<UObject>                  PreparedObjects      = new();
		// 활동 객체 조회용 목록과 순회용 목록을 분리함. SyncList의 변경은 Synchronize 때 순회 목록에 반영됨.
		private static readonly Dictionary<UObject, RoutineEntry> ActiveObjects        = new();
		private static readonly SyncList<RoutineEntry>            RoutineObjects       = new(100);
		// 등장/퇴장 연출은 일반 활동 루틴과 별도 목록에서 갱신함.
		private static readonly SyncList<RoutineEntry>            LifeCycleObjects     = new(100);
		// 현재 생애의 활동 등록과 현재 연출 실행의 등록. 해제 시 정확한 항목을 제거하기 위한 참조임.
		private                 RoutineEntry                      ActiveEntry;
		private                 RoutineEntry                      LifeEntry;
		// 모든 준비 객체가 아니라 활동 목록에 등록된 객체만 노출함.
		public static           IReadOnlyCollection<UObject>      instances => ActiveObjects.Keys;

		// 업데이트 등록 시점의 생애/실행/프레임을 저장한 항목.
		// 목록에 오래된 항목이 남아도 풀에서 재사용된 객체를 잘못 업데이트하지 않도록 검사함.
		private sealed class RoutineEntry {
			public readonly UObject Target;
			public readonly long    Life;
			public readonly long    Execution;
			public readonly int     Frame;
			// 일반 활동 등록은 Execution 기본값 0, 연출 등록은 실제 실행 번호를 사용함.
			public RoutineEntry(UObject Target, long Execution = 0) {
				this.Target    = Target;
				Life           = Target.lifeNumber;
				this.Execution = Execution;
				Frame          = Time.frameCount;
			}
			// 같은 생애이며 활동 중이어야 함. 등록한 프레임에는 일반 루틴을 실행하지 않음.
			public bool isActive => Target && Target.isActive && Target.lifeNumber == Life && Time.frameCount > Frame;
			// 연출은 생애와 실행 번호를 모두 검사함. 일반 활동과 달리 등록 프레임 제한은 없음.
			public bool isLifeCycle => Target && Target.lifeNumber == Life
											  && Target.lifeCycle.IsCurrent(Target.lifeCycle.currentState, Execution);
		}
		
		// 연출 상태 설정을 머신에 위임함. 설정 가능 시점과 대상 객체 일치 여부는 머신이 검사함.
		public void SetGettingState(Getting State) => lifeCycle.SetGettingState(State);
		public void SetReleasingState(Releasing State) => lifeCycle.SetReleasingState(State);
		public void SetStates(Getting GettingState, Releasing ReleasingState) => lifeCycle.SetStates(GettingState, ReleasingState);
		// 파생 클래스의 기본 연출 지정용. 이미 지정된 상태를 덮어쓰지 않음.
		protected void SetDefaultStates(Getting GettingState = null, Releasing ReleasingState = null) => lifeCycle.SetDefaultStates(GettingState, ReleasingState);

		public virtual void OnFirstGet() {
			// 매 생애마다 호출하는 OnGet과 달리, 객체 인스턴스당 최초 한 번만 실행함.
			if (FirstGetCompleted) throw new InvalidOperationException("OnFirstGet must run only once per instance.");
			// 생명주기에서 반복 사용하는 컴포넌트를 캐싱함.
			rigidbody2D = GetComponent<Rigidbody2D>();
			spriteRenderer = GetComponent<SpriteRenderer>();
			animator = GetComponent<Animator>();
			// 비활성 자식의 콜라이더도 포함하고, 원래 꺼져 있던 콜라이더를 나중에 켜지 않도록 상태 저장.
			LifeColliders = GetComponentsInChildren<Collider2D>(true);
			ColliderEnabled = LifeColliders.Select(Collider => Collider.enabled).ToArray();
			// 재사용 시 외형/물리를 복원하는 설정이면 최초 값을 계층 전체에서 저장함.
			if (whenInitialize != WhenInitialize.Never) RegisterToAllChildren(transform);
			// 연출 구성을 준비한 뒤, 성공한 객체만 준비 완료 목록에 등록함.
			lifeCycle.Prepare();
			FirstGetCompleted = true;
			PreparedObjects.Add(this);
		}

		internal void BindPool(Action<UObject> Return) {
			// 반환 상태에서 한 번만 소속 풀을 연결함. 생애 도중 다른 풀로 소속을 바꾸지 못하게 함.
			if (PoolReturn != null || !isReleased) throw new InvalidOperationException("Pool ownership is immutable.");
			PoolReturn = Return ?? throw new ArgumentNullException(nameof(Return));
		}

		public static GameObject Get(string PrefabKey, Vector2 Position, bool PlayEffect = true, Action<UObject> Configure = null) {
			// 종료 중 획득을 차단하고, 풀에서 배치된 객체를 받은 뒤 새 생애를 시작함.
			// Configure는 OnGet/등장 연출보다 먼저 실행되어 필요한 데이터를 설정할 수 있음.
			CheckAcquisitionAllowed();
			UObject Target = UObjectPool.instance.Acquire(PrefabKey, Position);
			Target.BeginLife(PlayEffect, Configure);
			return Target.gameObject;
		}

		// 문자열 대신 프리팹 이름의 GetHash()로 획득하는 오버로드. 런타임 UObject.ID와는 별개임.
		public static GameObject Get(int PrefabId, Vector2 Position, bool PlayEffect = true, Action<UObject> Configure = null) {
			CheckAcquisitionAllowed();
			UObject Target = UObjectPool.instance.Acquire(PrefabId, Position);
			Target.BeginLife(PlayEffect, Configure);
			return Target.gameObject;
		}

		internal void BeginLife(bool PlayEffect, Action<UObject> Configure = null) {
			// 최초 준비를 마쳤고 반환 상태이며 오류/종료 상태가 아닌 객체만 획득 가능함.
			CheckAcquisitionAllowed();
			if (!FirstGetCompleted || !isReleased || lifeCycle.isFaulted || lifeCycle.isShuttingDown)
				throw new InvalidOperationException("Object is not available for acquisition.");
			try {
				// 기본값 복원 → 파생 클래스의 문맥 준비 → 호출자의 설정 순서로 적용함.
				// 복원 작업이 Configure에서 설정한 값을 덮어쓰지 않도록 이 순서를 유지함.
				if (whenInitialize is WhenInitialize.OnGet or WhenInitialize.Both) ApplyToAllChildren();
				PrepareContext();
				Configure?.Invoke(this);
				// 여기서 생애 번호가 증가하고 Getting 단계에 진입함. Configure 시점은 아직 진입 전임.
				lifeCycle.Get(PlayEffect);
			}
			catch {
				// 준비/설정 중 예외가 밖으로 전달되면 객체를 종료 격리한 뒤 원래 예외를 다시 던짐.
				lifeCycle.Shutdown();
				throw;
			}
		}

		// 파생 클래스가 매 획득마다 필요한 기본 문맥을 준비하는 훅. 예: UI 위치/트리거 초기화.
		protected virtual void PrepareContext() { }

		internal static void CheckAcquisitionAllowed() {
			// 씬 정리 콜백에서 새 객체가 생성되어 정리 대상이 늘어나는 것을 차단함.
			if (isStoppingLifeCycles) throw new InvalidOperationException("Cannot acquire objects during scene shutdown.");
		}

		public void Get(bool PlayEffect = true) {
			// 씬 객체용 진입점. 풀 객체는 반드시 static Get으로 실제 풀 획득 절차를 거쳐야 함.
			if (PoolReturn != null) throw new InvalidOperationException("Acquire pooled objects through static Get.");
			if (!FirstGetCompleted) OnFirstGet();
			BeginLife(PlayEffect);
		}

		// 퇴장 요청만 전달함. 중복 요청, 연출 생략, 등장 중 취소는 상태 머신에서 결정함.
		public void Release(bool PlayEffect = true) => lifeCycle.Release(PlayEffect);

		internal void PrepareGetting() {
			// 활성화/OnGet 콜백이 객체를 반환하거나 재획득할 수 있으므로 현재 생애 번호를 저장함.
			long Life = lifeNumber;
			// 등장 중에는 충돌과 물리를 멈춤. GameObject 활성화는 일반 활동 시작과 별개임.
			SetLifeColliders(false);
			FreezeRigidbody2D();
			gameObject.SetActive(true);
			// OnEnable 등 활성화 콜백에서 생애/단계가 바뀌었다면 기존 준비를 중단함.
			if (lifeNumber != Life || lifeCycle.phase != LifeCyclePhase.Getting) return;
			OnGet();
			// OnGet도 사용자 코드이므로 같은 검사를 다시 수행함.
			if (lifeNumber != Life || lifeCycle.phase != LifeCyclePhase.Getting) return;
			// 소유자와 코루틴 실행자를 모두 자신으로 지정하여 등장 준비 작업을 시작함.
			GetTask?.Execute(this, this);
		}

		internal void ActivateLife() {
			// 머신이 Active로 전환한 뒤 호출함. 물리/콜라이더 복구 후 일반 활동 목록에 등록함.
			UnfreezeRigidbody2D();
			SetLifeColliders(true);
			ActiveEntry = new RoutineEntry(this);
			ActiveObjects.Add(this, ActiveEntry);
			RoutineObjects.Add(ActiveEntry);
		}

		internal void DeactivateLife() {
			// 활동 등록만 해제함. 퇴장 연출을 위해 GameObject 자체는 켜진 채 남을 수 있음.
			if (ActiveEntry == null) return;
			RoutineObjects.Remove(ActiveEntry);
			ActiveObjects.Remove(this);
			ActiveEntry = null;
		}

		internal void RegisterLifeCycle(long Execution) {
			// 등장/퇴장 실행 하나당 등록 하나만 허용함. 일반 활동 등록과 별도로 관리함.
			if (LifeEntry != null) throw new InvalidOperationException("Duplicate life cycle registration.");
			LifeEntry = new RoutineEntry(this, Execution);
			LifeCycleObjects.Add(LifeEntry);
		}

		internal void UnregisterLifeCycle(long Execution) {
			// 예전 실행의 정리 요청이 새 실행의 등록을 지우지 않도록 실행 번호까지 비교함.
			if (LifeEntry == null || LifeEntry.Execution != Execution) return;
			LifeCycleObjects.Remove(LifeEntry);
			LifeEntry = null;
		}

		internal void PrepareReleasing() {
			// 일반 활동을 중단하고, 연출이 진행되는 동안 충돌/물리가 개입하지 않게 함.
			DeactivateLife();
			SetLifeColliders(false);
			FreezeRigidbody2D();
			// 활동 중 시작한 작업을 먼저 취소함. 취소 콜백이 Shutdown을 요청했다면 아래 훅은 생략함.
			StopRunningTask();
			if (lifeCycle.isShuttingDown) return;
			// OnRelease 내부에서 Shutdown이 재진입해도 OnRelease를 중복 호출하지 않도록 먼저 표시함.
			lifeCycle.MarkReleaseStarted();
			OnRelease();
			if (lifeCycle.isShuttingDown) return;
			// 기존 작업 취소가 끝난 뒤 퇴장용 작업을 시작함. 퇴장 완료 시 남은 실행을 다시 취소함.
			ReleaseTask?.Execute(this, this);
			// 퇴장에 들어간 객체를 런타임 ID/Category 조회에서 제거함.
			ID = null;
			Category = null;
		}

		internal void FinishReleasing() {
			// 정상 퇴장 완료 단계. ReleaseTask 등 남은 작업을 취소한 뒤 최종 데이터 정리 훅 호출.
			StopRunningTask();
			Uninitialize();
			if (lifeCycle.isShuttingDown) return;
			// 종료로 전환되지 않았다면 정지를 해제하고 설정에 따라 최초 외형/물리 값을 복원함.
			Unfreeze();
			if (whenInitialize is WhenInitialize.OnRelease or WhenInitialize.Both) ApplyToAllChildren();
		}

		internal void ReturnToPool() {
			// 씬/객체가 종료 중이면 풀에 재삽입하지 않고 비활성화하여 격리함.
			if (isStoppingLifeCycles || lifeCycle.isShuttingDown) { QuarantineLife(); return; }
			// 풀 객체는 최초 연결한 반환 콜백을 사용함.
			if (PoolReturn != null) { PoolReturn.Invoke(this); return; }
			// 씬 객체는 비활성화만 함. 프리팹 풀에 넣거나 Destroy하지 않음.
			gameObject.SetActive(false);
		}

		internal void StopLife(bool ReleaseStarted) {
			// Shutdown 전용 정리. 일반 퇴장 완료의 Uninitialize/복원/풀 반환은 수행하지 않음.
			try { StopRunningTask(); }
			finally {
				// 작업 취소가 예외를 던져도 필요한 OnRelease는 시도하고, 식별 정보는 마지막에 해제함.
				// 이미 OnRelease 호출 단계에 도달했거나 반환 상태였으면 OnRelease는 생략함.
				try { if (!ReleaseStarted && lifeCycle.phase != LifeCyclePhase.Released) OnRelease(); }
				finally { ID = null; Category = null; }
			}
		}

		internal void QuarantineLife() {
			// 아직 Unity 객체가 존재하면 비활성화함. 재획득 차단 여부는 머신의 종료/오류 플래그가 담당함.
			if (this && gameObject) gameObject.SetActive(false);
		}

		internal void RegisterTask(TaskExecution Execution) {
			// 소유 작업 시작이 금지된 상태이면 등록 대신 바로 취소함.
			if (!canStartOwnedWork) { Execution.Cancel(); return; }
			Tasks.Add(Execution);
		}
		
		// 실행 완료/취소 시 소유 목록에서 제거함. 이미 제거된 실행이면 변화 없음.
		internal void UnregisterTask(TaskExecution Execution) => Tasks.Remove(Execution);

		private void StopRunningTask() {
			// 이름의 Task뿐 아니라 코루틴, UAction, 다음 업데이트 예약까지 함께 정리하는 구간임.
			// 취소 콜백에서 새 소유 작업을 시작하지 못하게 먼저 잠금.
			StoppingRunningTask = true;
			try {
				// Cancel이 UnregisterTask로 원본 목록을 변경할 수 있으므로 복사본을 순회함.
				foreach (TaskExecution Execution in Tasks.ToArray()) Execution.Cancel();
				Tasks.Clear();
				// 이 MonoBehaviour에서 실행 중인 코루틴과 등록된 UAction을 중단함.
				StopAllCoroutines();
				StopAllUActions();
			}
			finally {
				// 취소 중 예외가 나더라도 예약 큐와 정리 중 플래그는 정리함.
				ProcessToUpdate.Clear();
				ProcessToFixedUpdate.Clear();
				StoppingRunningTask = false;
			}
		}

		private void SetLifeColliders(bool Enabled) {
			// 켤 때도 최초에 enabled였던 콜라이더만 복구함. 이미 파괴된 콜라이더는 건너뜀.
			for (int Index = 0; Index < LifeColliders.Length; Index++) {
				if (LifeColliders[Index]) LifeColliders[Index].enabled = Enabled && ColliderEnabled[Index];
			}
		}

		// Play Mode 종료 시 파괴보다 먼저 전체 중단을 표시하여 다른 객체의 정리 콜백도 신규 작업을 막음.
		protected virtual void OnApplicationQuit() => isStoppingLifeCycles = true;

		protected virtual void OnDestroy() {
			// 이미 만들어진 머신만 종료함. 파괴하면서 불필요하게 새 머신을 만들지 않음.
			LifeCycle?.Shutdown();
			PreparedObjects.Remove(this);
		}

		public static void ShutdownLifeCycles() {
			// 활동 중인 객체뿐 아니라 준비된 모든 객체를 종료함. 정리 중 목록 변경에 대비해 복사본 순회.
			isStoppingLifeCycles = true;
			foreach (UObject Target in PreparedObjects.ToArray()) if (Target) Target.lifeCycle.Shutdown();
		}

		public static void ResetUObjects() {
			// 객체별 종료 훅을 먼저 실행한 뒤 전역 등록/조회 목록을 비움.
			ShutdownLifeCycles();
			PreparedObjects.Clear();
			ActiveObjects.Clear();
			// 다음 Synchronize를 기다리지 않고 순회 목록과 변경 보관 목록을 모두 비움.
			RoutineObjects.ClearImmediately();
			LifeCycleObjects.ClearImmediately();
			IDTable.Clear();
			CategoryTable.Clear();
		}

		// 새 씬에서 전역 획득/업데이트를 다시 허용함. 이미 Shutdown된 개별 머신을 되살리는 것은 아님.
		internal static void BeginSceneLifeCycles() => isStoppingLifeCycles = false;

		// 파생 클래스의 활동 훅. 아래 전역 루틴들이 유효한 Active 객체에 대해서만 호출함.
		protected virtual void EarlyRoutine()                          { }
		protected virtual void Routine()                               { }
		protected virtual void LateRoutine()                           { }
		protected virtual void FixedRoutine()                          { }
		// 활동 상태에서만 예약을 받음. 등장/퇴장 중의 요청은 큐에 넣지 않음.
		public            void AddProcessToUpdate(Action      Process) { if (isActive) ProcessToUpdate.Enqueue(Process); }
		protected         void AddProcessToFixedUpdate(Action Process) { if (isActive) ProcessToFixedUpdate.Enqueue(Process); }

		private void ExecuteProcesses(Queue<Action> Processes) {
			// 이번 처리 시작 시의 생애와 개수를 고정함. 실행 중 추가된 작업은 이번 호출에서 계속 소비하지 않음.
			long Life = lifeNumber;
			int Count = Processes.Count;
			// 작업이 Release/재획득/큐 비우기를 수행하면 다음 반복에서 멈추도록 매번 재검사함.
			while (Count-- > 0 && isActive && lifeNumber == Life && Processes.Count > 0) Processes.Dequeue().Invoke();
		}

		private static void RunActive(Action<UObject> Callback) {
			// 각 활동 구간 시작 시 보류된 등록/해제를 순회 목록에 반영함.
			if (isStoppingLifeCycles) return;
			RoutineObjects.Synchronize();
			int Count = RoutineObjects.Count;
			// 콜백이 전역 목록을 초기화하거나 씬을 종료해도 범위를 벗어나거나 순회를 계속하지 않도록 검사함.
			for (int Index = 0; Index < Count && Index < RoutineObjects.Count && !isStoppingLifeCycles; Index++) {
				RoutineEntry Entry = RoutineObjects[Index];
				// 같은 객체 참조라도 생애가 바뀌었거나 등록 프레임이라면 이번 활동 호출을 생략함.
				if (!Entry.isActive) continue;
				// 한 객체의 예외가 나머지 객체의 활동 순회를 중단하지 않도록 로그만 남기고 계속함.
				try { Callback(Entry.Target); }
				catch (Exception Error) { DebugManager.LogException(Error, Entry.Target); }
			}
		}

		public static void UpdateRoutine() {
			// 예약 작업 → EarlyRoutine → Routine 순서. 각 구간마다 활동 대상의 유효성을 새로 검사함.
			GameManager.instance.currentUpdatePhase = UpdateRoutineType.Processing;
			RunActive(Target => Target.ExecuteProcesses(Target.ProcessToUpdate));
			GameManager.instance.currentUpdatePhase = UpdateRoutineType.EarlyRoutine;
			RunActive(Target => Target.EarlyRoutine());
			GameManager.instance.currentUpdatePhase = UpdateRoutineType.Routine;
			RunActive(Target => Target.Routine());
		}

		public static void LateUpdateRoutine() {
			// 일반 Update 뒤 수행할 파생 클래스의 활동 훅을 호출함.
			GameManager.instance.currentUpdatePhase = UpdateRoutineType.LateRoutine;
			RunActive(Target => Target.LateRoutine());
		}

		public static void LifeCycleRoutine() {
			// 일반 활동과 별도로 Getting/Releasing 연출만 진행함. 씬 종료 중에는 갱신하지 않음.
			if (isStoppingLifeCycles) return;
			if (GameManager.instance) GameManager.instance.currentUpdatePhase = UpdateRoutineType.LifeCycleRoutine;
			// 이번 연출 순회 전에 등록/해제 요청을 반영함.
			LifeCycleObjects.Synchronize();
			int Count = LifeCycleObjects.Count;
			for (int Index = 0; Index < Count && Index < LifeCycleObjects.Count && !isStoppingLifeCycles; Index++) {
				RoutineEntry Entry = LifeCycleObjects[Index];
				// 등록 당시 생애/실행이 여전히 유효한 경우만 객체별 시간 기준으로 연출을 갱신함.
				// 연출 예외의 처리와 종료 격리는 상태 머신의 Tick이 담당함.
				if (Entry.isLifeCycle) Entry.Target.lifeCycle.Tick(Entry.Execution, Entry.Target.lifeCycleDeltaTime);
			}
			if (GameManager.instance) GameManager.instance.currentUpdatePhase = UpdateRoutineType.RoutineEnd;
		}

		public static void FixedUpdateRoutine() {
			// 물리 업데이트 구간에서도 예약 작업을 먼저 실행하고 FixedRoutine 훅을 호출함.
			GameManager.instance.currentFixedUpdatePhase = FixedUpdateRoutineType.Processing;
			RunActive(Target => Target.ExecuteProcesses(Target.ProcessToFixedUpdate));
			GameManager.instance.currentFixedUpdatePhase = FixedUpdateRoutineType.FixedRoutine;
			RunActive(Target => Target.FixedRoutine());
			GameManager.instance.currentFixedUpdatePhase = FixedUpdateRoutineType.RoutineEnd;
		}
	}
}
