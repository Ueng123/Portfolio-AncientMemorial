using System;
using System.Collections.Generic;
using System.Linq;
using UengSystem.Objects.LifeCycle;
using UengSystem.ObjectPool;
using UengSystem.Utility;
using UengSystem.VisualScripting.Tasks;
using UnityEngine;
using UnityEngine.Serialization;

namespace UengSystem.Objects {
	public partial class UObject {
		private LifeCycleStateMachine LifeCycle;
		public LifeCycleStateMachine lifeCycle => LifeCycle ??= new LifeCycleStateMachine(this);
		public bool isActive => lifeCycle.phase == LifeCyclePhase.Active;
		public long lifeNumber => lifeCycle.lifeNumber;
		[SerializeField, Min(0)] private float GettingDuration = 1;
		[SerializeField, Min(0)] private float ReleasingDuration = 1;
		public virtual float gettingDuration => GettingDuration;
		public virtual float releasingDuration => ReleasingDuration;
		public virtual float lifeCycleDeltaTime => Time.deltaTime;
		public Task GetTask;
		public Task ReleaseTask;
		private Action<UObject> PoolReturn;
		private Collider2D[] LifeColliders;
		private bool[] ColliderEnabled;
		private bool FirstGetCompleted;
		private bool StoppingWork;

		public bool canStartOwnedWork =>
			!StoppingWork && !lifeCycle.isShuttingDown && (!lifeCycle.isPrepared || !isReleased);

		public static         bool                              isStoppingLifeCycles { get; private set; }
		private readonly        Queue<Action>                     ProcessToUpdate      = new();
		private readonly        Queue<Action>                     ProcessToFixedUpdate = new();
		private readonly        HashSet<TaskExecution>            Tasks                = new();
		private static readonly HashSet<UObject>                  PreparedObjects      = new();
		private static readonly Dictionary<UObject, RoutineEntry> ActiveObjects        = new();
		private static readonly SyncList<RoutineEntry>            RoutineObjects       = new(100);
		private static readonly SyncList<RoutineEntry>            LifeCycleObjects     = new(100);
		private                 RoutineEntry                      ActiveEntry;
		private                 RoutineEntry                      LifeEntry;
		public static           IReadOnlyCollection<UObject>      instances => ActiveObjects.Keys;

		private sealed class RoutineEntry {
			public readonly UObject Target;
			public readonly long    Life;
			public readonly long    Execution;
			public readonly int     Frame;
			public RoutineEntry(UObject Target, long Execution = 0) {
				this.Target    = Target;
				Life           = Target.lifeNumber;
				this.Execution = Execution;
				Frame          = Time.frameCount;
			}
			public bool isActive => Target && Target.isActive && Target.lifeNumber == Life && Time.frameCount > Frame;
			public bool isLifeCycle => Target && Target.lifeNumber == Life
											  && Target.lifeCycle.IsCurrent(Target.lifeCycle.currentState, Execution);
		}
		
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

		internal void BindPool(Action<UObject> Return) {
			if (PoolReturn != null || !isReleased) throw new InvalidOperationException("Pool ownership is immutable.");
			PoolReturn = Return ?? throw new ArgumentNullException(nameof(Return));
		}

		public static GameObject Get(string PrefabKey, Vector2 Position, bool PlayEffect = true, Action<UObject> Configure = null) {
			CheckAcquisitionAllowed();
			UObject Target = UObjectPool.instance.Acquire(PrefabKey, Position);
			Target.BeginLife(PlayEffect, Configure);
			return Target.gameObject;
		}

		// PrefabId is the prefab name's GetHash(), independent of the runtime UObject.ID.
		public static GameObject Get(int PrefabId, Vector2 Position, bool PlayEffect = true, Action<UObject> Configure = null) {
			CheckAcquisitionAllowed();
			UObject Target = UObjectPool.instance.Acquire(PrefabId, Position);
			Target.BeginLife(PlayEffect, Configure);
			return Target.gameObject;
		}

		internal void BeginLife(bool PlayEffect, Action<UObject> Configure = null) {
			CheckAcquisitionAllowed();
			if (!FirstGetCompleted || !isReleased || lifeCycle.isFaulted || lifeCycle.isShuttingDown)
				throw new InvalidOperationException("Object is not available for acquisition.");
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

		internal static void CheckAcquisitionAllowed() {
			if (isStoppingLifeCycles) throw new InvalidOperationException("Cannot acquire objects during scene shutdown.");
		}

		public void Get(bool PlayEffect = true) {
			if (PoolReturn != null) throw new InvalidOperationException("Acquire pooled objects through static Get.");
			if (!FirstGetCompleted) OnFirstGet();
			BeginLife(PlayEffect);
		}

		public void Release(bool PlayEffect = true) => lifeCycle.Release(PlayEffect);

		internal void PrepareGetting() {
			long Life = lifeNumber;
			SetLifeColliders(false);
			FreezeRigidbody2D();
			gameObject.SetActive(true);
			if (lifeNumber != Life || lifeCycle.phase != LifeCyclePhase.Getting) return;
			OnGet();
			if (lifeNumber != Life || lifeCycle.phase != LifeCyclePhase.Getting) return;
			GetTask?.Execute(this, this);
		}

		internal void ActivateLife() {
			UnfreezeRigidbody2D();
			SetLifeColliders(true);
			ActiveEntry = new RoutineEntry(this);
			ActiveObjects.Add(this, ActiveEntry);
			RoutineObjects.Add(ActiveEntry);
		}

		internal void DeactivateLife() {
			if (ActiveEntry == null) return;
			RoutineObjects.Remove(ActiveEntry);
			ActiveObjects.Remove(this);
			ActiveEntry = null;
		}

		internal void RegisterLifeCycle(long Execution) {
			if (LifeEntry != null) throw new InvalidOperationException("Duplicate life cycle registration.");
			LifeEntry = new RoutineEntry(this, Execution);
			LifeCycleObjects.Add(LifeEntry);
		}

		internal void UnregisterLifeCycle(long Execution) {
			if (LifeEntry == null || LifeEntry.Execution != Execution) return;
			LifeCycleObjects.Remove(LifeEntry);
			LifeEntry = null;
		}

		internal void PrepareReleasing() {
			DeactivateLife();
			SetLifeColliders(false);
			FreezeRigidbody2D();
			StopOwnedWork();
			if (lifeCycle.isShuttingDown) return;
			lifeCycle.MarkReleaseStarted();
			OnRelease();
			if (lifeCycle.isShuttingDown) return;
			// Start release tasks after cancelling active work. They live until final cleanup.
			ReleaseTask?.Execute(this, this);
			ID = null;
			Category = null;
		}

		internal void FinishReleasing() {
			StopOwnedWork();
			Uninitialize();
			if (lifeCycle.isShuttingDown) return;
			Unfreeze();
			if (whenInitialize is WhenInitialize.OnRelease or WhenInitialize.Both) ApplyToAllChildren();
		}

		internal void ReturnToPool() {
			if (isStoppingLifeCycles || lifeCycle.isShuttingDown) { QuarantineLife(); return; }
			if (PoolReturn != null) { PoolReturn(this); return; }
			gameObject.SetActive(false); // Scene objects are retained, never inserted into a prefab pool.
		}

		internal void StopLife(bool ReleaseStarted) {
			try { StopOwnedWork(); }
			finally {
				try { if (!ReleaseStarted && lifeCycle.phase != LifeCyclePhase.Released) OnRelease(); }
				finally { ID = null; Category = null; }
			}
		}

		internal void QuarantineLife() {
			if (this && gameObject) gameObject.SetActive(false);
		}

		internal void ReportLifeCycleError(Exception Error) => Debug.LogException(Error, this);
		internal void RegisterTask(TaskExecution Execution) {
			if (!canStartOwnedWork) { Execution.Cancel(); return; }
			Tasks.Add(Execution);
		}
		internal void UnregisterTask(TaskExecution Execution) => Tasks.Remove(Execution);

		private void StopOwnedWork() {
			StoppingWork = true;
			try {
				foreach (TaskExecution Execution in Tasks.ToArray()) Execution.Cancel();
				Tasks.Clear();
				StopAllCoroutines();
				StopAllUActions();
			}
			finally {
				ProcessToUpdate.Clear();
				ProcessToFixedUpdate.Clear();
				StoppingWork = false;
			}
		}

		private void SetLifeColliders(bool Enabled) {
			for (int Index = 0; Index < LifeColliders.Length; Index++) {
				if (LifeColliders[Index]) LifeColliders[Index].enabled = Enabled && ColliderEnabled[Index];
			}
		}

		protected virtual void OnDestroy() {
			LifeCycle?.Shutdown();
			PreparedObjects.Remove(this);
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

		internal static void BeginSceneLifeCycles() => isStoppingLifeCycles = false;

		protected virtual void EarlyRoutine() { }
		protected virtual void Routine() { }
		protected virtual void LateRoutine() { }
		protected virtual void FixedRoutine() { }
		protected void AddProcessToUpdate(Action Process) { if (isActive) ProcessToUpdate.Enqueue(Process); }
		protected void AddProcessToFixedUpdate(Action Process) { if (isActive) ProcessToFixedUpdate.Enqueue(Process); }

		private void ExecuteProcesses(Queue<Action> Processes) {
			long Life = lifeNumber;
			int Count = Processes.Count;
			while (Count-- > 0 && isActive && lifeNumber == Life && Processes.Count > 0) Processes.Dequeue().Invoke();
		}

		private static void RunActive(Action<UObject> Callback) {
			if (isStoppingLifeCycles) return;
			RoutineObjects.Synchronize();
			int Count = RoutineObjects.Count;
			for (int Index = 0; Index < Count && Index < RoutineObjects.Count && !isStoppingLifeCycles; Index++) {
				RoutineEntry Entry = RoutineObjects[Index];
				if (!Entry.isActive) continue;
				try { Callback(Entry.Target); }
				catch (Exception Error) { Entry.Target.ReportLifeCycleError(Error); }
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
			int Count = LifeCycleObjects.Count;
			for (int Index = 0; Index < Count && Index < LifeCycleObjects.Count && !isStoppingLifeCycles; Index++) {
				RoutineEntry Entry = LifeCycleObjects[Index];
				if (Entry.isLifeCycle) Entry.Target.lifeCycle.Tick(Entry.Execution, Entry.Target.lifeCycleDeltaTime);
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
	}
}
