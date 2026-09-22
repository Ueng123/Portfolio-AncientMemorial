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
	public partial class UObject {

		// 정적 프로퍼티
		public static         bool                              isStoppingLifeCycles { get; private set; }
		
		private static readonly HashSet<UObject>                  PreparedObjects      = new();
		
		private static readonly Dictionary<UObject, RoutineEntry> ActiveObjects        = new();
		
		private static readonly SyncList<RoutineEntry>            RoutineObjects       = new(100);
		private static readonly SyncList<RoutineEntry>            LifeCycleObjects     = new(100);
		
		public static           IReadOnlyCollection<UObject>      instances => ActiveObjects.Keys;

		// 인스턴스 프로퍼티
		private LifeCycleStateMachine LifeCycle;
		public LifeCycleStateMachine lifeCycle => LifeCycle ??= new LifeCycleStateMachine(this);
		
		public bool isActive => lifeCycle.phase == LifeCyclePhase.Active;
		public bool isReleased => lifeCycle.phase == LifeCyclePhase.Released;
		public bool isNotWorking => isReleased || lifeCycle.isShuttingDown;
		
		public long lifeNumber => lifeCycle.lifeNumber;
		
		public float GettingDuration = 1;
		public float ReleasingDuration = 1;
		public virtual float usingGettingDuration => GettingDuration;
		public virtual float usingReleasingDuration => ReleasingDuration;
		
		public virtual float lifeCycleDeltaTime => Time.deltaTime;
		
		public Task GetTask;
		public Task ReleaseTask;
		
		private Action<UObject> PoolReturn;
		
		private Collider2D[] LifeColliders;
		private bool[] ColliderEnabled;
		
		private bool FirstGetCompleted;
		
		private bool StoppingRunningTask;
		
		public bool canStartOwnedWork =>
			!isStoppingLifeCycles && !StoppingRunningTask && !lifeCycle.isShuttingDown && (!lifeCycle.isPrepared || !isReleased);

		public bool Matches(long LifeNumber) => this && lifeNumber == LifeNumber;
		
		private readonly Queue<Action> ProcessToUpdate      = new();
		private readonly Queue<Action> ProcessToFixedUpdate = new();
		
		private RoutineEntry ActiveEntry;
		private RoutineEntry LifeEntry;

		// 정적 메서드
		public static GameObject Get(string PrefabKey, Vector2 Position, bool PlayEffect = true, Action<UObject> Configure = null) {
			CheckAcquireAllowed();
			UObject Target = UObjectPool.instance.Acquire(PrefabKey, Position);
			Target.BeginLife(PlayEffect, Configure);
			return Target.gameObject;
		}
		
		public static GameObject Get(int PrefabId, Vector2 Position, bool PlayEffect = true, Action<UObject> Configure = null) {
			CheckAcquireAllowed();
			UObject Target = UObjectPool.instance.Acquire(PrefabId, Position);
			Target.BeginLife(PlayEffect, Configure);
			return Target.gameObject;
		}

		internal static void CheckAcquireAllowed() {
			if (isStoppingLifeCycles) throw new InvalidOperationException("Cannot acquire objects during scene shutdown.");
		}

		public static void ShutdownLifeCycles() {
			
			isStoppingLifeCycles = true;
			foreach (UObject Target in PreparedObjects.ToArray()) if (Target) Target.lifeCycle.Shutdown();
		}

		public static void ResetUObjects() {
			ShutdownLifeCycles();
			PreparedObjects.Clear();
			ActiveObjects.Clear();
			
			RoutineObjects.ClearImmediately();
			LifeCycleObjects.ClearImmediately();
			IDTable.Clear();
			CategoryTable.Clear();
		}
		
		public static void BeginSceneLifeCycles() => isStoppingLifeCycles = false;

		private static void RunActive(Action<UObject> Callback) {
			if (isStoppingLifeCycles) return;
			RoutineObjects.Synchronize();
			int InitialCount = RoutineObjects.Count;
			
			for (int Index = 0; Index < InitialCount; Index++) {
				if (isStoppingLifeCycles) break;
				if (RoutineObjects.Count == 0) break; // 리스트 강제 초기화 방어

				RoutineEntry Entry = RoutineObjects[Index];
				
				if (!Entry.isActive) continue;

				try { Callback.Invoke(Entry.Target); }
				catch (Exception Error) {
					DebugManager.LogException(Error, Entry.Target);
				}
			}
		}

		public static void UpdateRoutine() {
			GameManager.instance.currentUpdatePhase = UpdateRoutineType.Processing;
			RunActive(Target => Target.ExecuteProcesses(Target.ProcessToUpdate));
			GameManager.instance.currentUpdatePhase = UpdateRoutineType.EarlyRoutine;
			RunActive(Target => Target.EarlyRoutine());
			GameManager.instance.currentUpdatePhase = UpdateRoutineType.Routine;
			RunActive(Target => Target.Routine());
		}

		public static void LateUpdateRoutine() {
			GameManager.instance.currentUpdatePhase = UpdateRoutineType.LateRoutine;
			RunActive(Target => Target.LateRoutine());
		}

		public static void LifeCycleRoutine() {
			if (isStoppingLifeCycles) return;
			if (GameManager.instance) GameManager.instance.currentUpdatePhase = UpdateRoutineType.LifeCycleRoutine;
			
			LifeCycleObjects.Synchronize();
			int InitialCount = LifeCycleObjects.Count;
			for (int Index = 0; Index < InitialCount; Index++) {
				if (isStoppingLifeCycles) break;
				if (LifeCycleObjects.Count == 0) break; // 리스트 강제 초기화 방어

				RoutineEntry Entry = LifeCycleObjects[Index];
				
				if (!Entry.isLifeCycle) continue; 
				Entry.Target.lifeCycle.Tick(Entry.Execution, Entry.Target.lifeCycleDeltaTime);
			}
			
			if (GameManager.instance) GameManager.instance.currentUpdatePhase = UpdateRoutineType.RoutineEnd;
		}

		public static void FixedUpdateRoutine() {
			GameManager.instance.currentFixedUpdatePhase = FixedUpdateRoutineType.Processing;
			RunActive(Target => Target.ExecuteProcesses(Target.ProcessToFixedUpdate));
			GameManager.instance.currentFixedUpdatePhase = FixedUpdateRoutineType.FixedRoutine;
			RunActive(Target => Target.FixedRoutine());
			GameManager.instance.currentFixedUpdatePhase = FixedUpdateRoutineType.RoutineEnd;
		}

		// 인스턴스 메서드
		public void SetGettingState(Getting State) => lifeCycle.SetGettingState(State);
		public void SetReleasingState(Releasing State) => lifeCycle.SetReleasingState(State);
		public void SetStates(Getting GettingState, Releasing ReleasingState) => lifeCycle.SetStates(GettingState, ReleasingState);
		
		protected void SetDefaultStates(Getting GettingState = null, Releasing ReleasingState = null) => lifeCycle.SetDefaultStates(GettingState, ReleasingState);

		public virtual void OnFirstGet() {
			if (FirstGetCompleted) throw new InvalidOperationException("OnFirstGet must run only once per instance.");
			
			rigidbody2D = GetComponent<Rigidbody2D>();
			spriteRenderer = GetComponent<SpriteRenderer>();
			animator = GetComponent<Animator>();
			
			LifeColliders = GetComponentsInChildren<Collider2D>(true);
			ColliderEnabled = LifeColliders.Select(Collider => Collider.enabled).ToArray();
			
			if (whenInitialize != WhenInitialize.Never) RegisterToAllChildren(transform);
			
			lifeCycle.Prepare();
			FirstGetCompleted = true;
			PreparedObjects.Add(this);
		}

		public void SetPoolReturnMethod(Action<UObject> Return) {
			if (PoolReturn != null || !isReleased) throw new InvalidOperationException("Pool ownership is immutable.");
			PoolReturn = Return;
		}

		protected void BeginLife(bool PlayEffect, Action<UObject> Configure = null) {
			CheckAcquireAllowed();
			if (!FirstGetCompleted || !isReleased || lifeCycle.isFaulted || lifeCycle.isShuttingDown)
				throw new InvalidOperationException("Object is not available.");
			
			try {
				if (whenInitialize is WhenInitialize.OnGet or WhenInitialize.Both) ApplyToAllChildren();
				PrepareContext();
				Configure?.Invoke(this);
				
				lifeCycle.Get(PlayEffect);
			}
			catch {
				lifeCycle.Shutdown();
				throw;
			}
		}

		
		protected virtual void PrepareContext() { }

		public void Get(bool PlayEffect = true) {
			if (PoolReturn != null) throw new InvalidOperationException("Acquire pooled objects through static Get.");
			if (!FirstGetCompleted) OnFirstGet();
			BeginLife(PlayEffect);
		}
		
		public void Release(bool PlayEffect = true) => lifeCycle.Release(PlayEffect);

		public void PrepareGetting() {
			long Life = lifeNumber;
			
			SetLifeColliders(false);
			FreezeRigidbody2D();
			gameObject.SetActive(true);
			
			if (lifeNumber != Life || lifeCycle.phase != LifeCyclePhase.Getting) return;
			OnGet();
			
			if (lifeNumber != Life || lifeCycle.phase != LifeCyclePhase.Getting) return;
			
			GetTask?.Execute(this, this);
		}

		public void ActivateLife() {
			UnfreezeRigidbody2D();
			SetLifeColliders(true);
			ActiveEntry = new RoutineEntry(this);
			ActiveObjects.Add(this, ActiveEntry);
			RoutineObjects.Add(ActiveEntry);
		}

		public void DeactivateLife() {
			if (ActiveEntry == null) return;
			RoutineObjects.Remove(ActiveEntry);
			ActiveObjects.Remove(this);
			ActiveEntry = null;
		}

		public void RegisterLifeCycle(long Execution) {
			
			if (LifeEntry != null) throw new InvalidOperationException("Duplicate life cycle registration.");
			LifeEntry = new RoutineEntry(this, Execution);
			LifeCycleObjects.Add(LifeEntry);
		}

		public void UnregisterLifeCycle(long Execution) {
			
			if (LifeEntry == null || LifeEntry.Execution != Execution) return;
			LifeCycleObjects.Remove(LifeEntry);
			LifeEntry = null;
		}

		public void PrepareReleasing() {
			DeactivateLife();
			SetLifeColliders(false);
			FreezeRigidbody2D();
			
			StopRunningTask();
			if (lifeCycle.isShuttingDown) return;
			
			lifeCycle.MarkReleaseStarted();
			OnRelease();
			if (lifeCycle.isShuttingDown) return;
			
			ReleaseTask?.Execute(this, this);
			
			ID = null;
			Category = null;
		}

		public void FinishReleasing() {
			StopRunningTask();
			Uninitialize();
			if (lifeCycle.isShuttingDown) return;
			
			Unfreeze();
			if (whenInitialize is WhenInitialize.OnRelease or WhenInitialize.Both) ApplyToAllChildren();
		}

		public void ReturnToPool() {
			if (isStoppingLifeCycles || lifeCycle.isShuttingDown) { QuarantineLife(); return; }
			
			if (PoolReturn != null) { PoolReturn.Invoke(this); return; }
			
			gameObject.SetActive(false);
		}

		public void StopLife(bool ReleaseStarted) {
			
			try { StopRunningTask(); }
			finally {
				
				
				try { if (!ReleaseStarted && lifeCycle.phase != LifeCyclePhase.Released) OnRelease(); }
				finally { ID = null; Category = null; }
			}
		}

		public void QuarantineLife() {
			if (this && gameObject) gameObject.SetActive(false);
		}

		private void StopRunningTask() {
			StoppingRunningTask = true;
			
			StopAllCoroutines();
			StopAllUActions();
			
			ProcessToUpdate.Clear();
			ProcessToFixedUpdate.Clear();
			StoppingRunningTask = false;
		}

		private void SetLifeColliders(bool Enabled) {
			for (int Index = 0; Index < LifeColliders.Length; Index++) {
				if (LifeColliders[Index]) LifeColliders[Index].enabled = Enabled && ColliderEnabled[Index];
			}
		}

		
		protected virtual void OnApplicationQuit() => isStoppingLifeCycles = true;

		protected virtual void OnDestroy() {
			
			LifeCycle?.Shutdown();
			PreparedObjects.Remove(this);
		}

		
		protected virtual void EarlyRoutine()                          { }
		protected virtual void Routine()                               { }
		protected virtual void LateRoutine()                           { }
		protected virtual void FixedRoutine()                          { }
		
		public            void AddProcessToUpdate(Action      Process) { if (isActive) ProcessToUpdate.Enqueue(Process); }
		protected         void AddProcessToFixedUpdate(Action Process) { if (isActive) ProcessToFixedUpdate.Enqueue(Process); }

		private void ExecuteProcesses(Queue<Action> Processes) {
			
			long Life = lifeNumber;
			int Count = Processes.Count;
			
			while (Count-- > 0 && isActive && Matches(Life) && Processes.Count > 0) Processes.Dequeue().Invoke();
		}
	}
}
