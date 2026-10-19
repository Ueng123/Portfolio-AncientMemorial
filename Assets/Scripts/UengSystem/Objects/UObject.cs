using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using AncientMemorial;
using AncientMemorial.Objects;
using UengSystem.Audio;
using UengSystem.Events;
using UengSystem.ObjectPool;
using UengSystem.Objects.LifeCycle;
using UengSystem.UActions;
using UengSystem.UDebug;
using UengSystem.Utility;
using UengSystem.VisualScripting.Tasks;
using UnityEngine;
using Event = UengSystem.Events.Event;
using EventType = UengSystem.Events.EventType;	

namespace UengSystem.Objects {
	public class UObject : MonoBehaviour, IActionable, IEventAgent, ITaskable {

		// 정적 프로퍼티
		private static readonly Dictionary<string, UObject>       IDTable       = new();
		private static readonly Dictionary<string, List<UObject>> CategoryTable = new();

		public static bool isStoppingLifeCycles;
		
		private static readonly HashSet<UObject> PreparedObjects = new();
		
		public static readonly SyncList<RoutineEntry> RoutineObjects   = new(100);
		public static readonly SyncList<RoutineEntry> LifeCycleObjects = new(100);
		
		// 인스턴스 프로퍼티
		public WhenInitialize whenInitialize;
		
		private string _ID;
		private string _Category;

		public string ID {
			get => _ID;
			set {
				if (value == null) {
					if (_ID == null) return;

					if (IDTable.TryGetValue(_ID, out UObject Current) && ReferenceEquals(Current, this)) IDTable.Remove(_ID);
					_ID          = null;

					return;
				}

				if (_ID != null) throw new InvalidOperationException("u just tried to set ID twice twin :(");
				if (IDTable.TryGetValue(value, out UObject Existing) && Existing && !ReferenceEquals(Existing, this))
					throw new InvalidOperationException("Duplicate runtime UObject ID: " + value);

				IDTable[value] = this;
				_ID            = value;
				OnIdChanged(value);
			}
		}
		
		public string Category {
			get => _Category;
			set {
				if (value == null) {
					if (_Category == null) return;

					CategoryTable[_Category].Remove(this);
					if (CategoryTable[_Category].Count == 0) CategoryTable.Remove(_Category);

					_Category = null;

					return;
				}

				if (_Category != null)
					throw new InvalidOperationException("u just tried to set Category twice twin :(");
				if (!CategoryTable.ContainsKey(value)) CategoryTable[value] = new List<UObject>();

				CategoryTable[value].Add(this);
				_Category = value;
				OnCategoryChanged(value);
			}
		}
		
		private readonly List<UAction> runningActions = new();
		
		[HideInInspector] public Rigidbody2D    rigidbody2D;
		[HideInInspector] public SpriteRenderer spriteRenderer;
		[HideInInspector] public Animator       animator;
		
		private Dictionary<GameObject, ObjectInitializeData> objectInitializeData = new();

		private bool                   isRigidbody2DFrozen = false;
		private Vector2                linearVelocityBeforeFreeze;
		private float                  angularVelocityBeforeFreeze;
		private RigidbodyConstraints2D constraintsBeforeFreeze;

		private   bool  isAnimatorFrozen = false;
		protected float animatorSpeedBeforeFreeze;
		private bool AnimatorEnabledBeforeFreeze;
		
		private LifeStateMachine _lifeStateMachine;
		public LifeStateMachine lifeStateMachine => _lifeStateMachine ??= new LifeStateMachine(this);
		
		public bool isActive => lifeStateMachine.phase == LifePhase.Active;
		public bool isReleased => lifeStateMachine.phase == LifePhase.Released;
		public bool OutGame => isReleased || lifeStateMachine.isShuttingDown;
		
		public int lifeNumber => lifeStateMachine.lifeNumber;
		
		public float GettingDuration = 1;
		public float ReleasingDuration = 1;
		public virtual float usingGettingDuration => GettingDuration;
		public virtual float usingReleasingDuration => ReleasingDuration;

		protected virtual float DeltaTime => Time.deltaTime;
		
		public Task GetTask;
		public Task ReleaseTask;
		
		private Action<UObject> PoolReturn;
		
		private Collider2D[] Colliders;
		
		private bool FirstGetCompleted;
		
		private bool StoppingRunningTask;
		
		public bool canStartOwnedWork =>
			!isStoppingLifeCycles &&
			!StoppingRunningTask &&
			!lifeStateMachine.isShuttingDown &&
			(!lifeStateMachine.isPrepared || !isReleased);

		public bool Matches(int LifeNumber) => this && lifeNumber == LifeNumber;
		
		private readonly Queue<Action> ProcessToUpdate = new();
		private readonly Queue<Action> ProcessToFixedUpdate = new();
		
		private RoutineEntry ActiveEntry;
		private RoutineEntry LifeEntry;

		// 정적 메서드
		public static bool TryGetUObject(string id, out UObject obj) {
			return IDTable.TryGetValue(id, out obj);
		}
		
		public static UObject GetUObject(string id) {
			return IDTable.GetValueOrDefault(id);
		}

		public static List<UObject> GetUObjects(string category) {
			return !CategoryTable.TryGetValue(category, out List<UObject> objects) ? new List<UObject>() : objects;
		}

		public static bool UObjectExists(string id) {
			return IDTable.ContainsKey(id) && IDTable[id];
		}

		public static bool UObjectsExist(string category) {
			return CategoryTable.ContainsKey(category) && CategoryTable[category].Count > 0;
		}
		
		public static GameObject Get(string PrefabKey, Vector2 Position, bool PlayEffect = true, Action<UObject> Configure = null) {
			UObject Target = UObjectPool.instance.Get(PrefabKey, Position);
			Target.StartGet(PlayEffect, Configure);

			return Target.gameObject;
		}
		
		public static GameObject Get(int PrefabId, Vector2 Position, bool PlayEffect = true, Action<UObject> Configure = null) {
			UObject Target = UObjectPool.instance.Get(PrefabId, Position);
			Target.StartGet(PlayEffect, Configure);

			return Target.gameObject;
		}

		public static void ShutdownLifeCycles() {
			isStoppingLifeCycles = true;

			foreach (UObject Target in PreparedObjects.ToArray()) {
				if (Target) Target.lifeStateMachine.Shutdown();
			}
		}

		public static void ResetUObjects() {
			ShutdownLifeCycles();

			PreparedObjects.Clear();
			
			RoutineObjects.Clear();
			LifeCycleObjects.Clear();

			IDTable.Clear();
			CategoryTable.Clear();
		}

		private static void RunActive(Action<UObject> Callback) {
			if (isStoppingLifeCycles) return;
			
			for (int i = 0; i < RoutineObjects.Count; i++) {
				if (isStoppingLifeCycles) break;
				RoutineEntry Entry = RoutineObjects[i];
				
				if (!Entry.isActive) continue;

				Callback.Invoke(Entry.Target);
			}
		}
		
		private static void RunLife(Action<RoutineEntry> Callback) {
			if (isStoppingLifeCycles) return;
			
			for (int i = 0; i < LifeCycleObjects.Count; i++) {
				if (isStoppingLifeCycles) break;
				RoutineEntry Entry = LifeCycleObjects[i];
				
				if (!Entry.isLifeCycle) continue;

				Callback.Invoke(Entry);
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

			if (GameManager.instance) GameManager.instance.currentUpdatePhase = UpdateRoutineType.LifeCycleRoutine;
			RunLife(Entry => Entry.Target.lifeStateMachine.EffectRoutine(Entry.Execution, Entry.Target.DeltaTime));
			
			GameManager.instance.currentUpdatePhase = UpdateRoutineType.RoutineEnd;
		}

		public static void FixedUpdateRoutine() {
			GameManager.instance.currentFixedUpdatePhase = FixedUpdateRoutineType.Processing;
			RunActive(Target => Target.ExecuteProcesses(Target.ProcessToFixedUpdate));

			GameManager.instance.currentFixedUpdatePhase = FixedUpdateRoutineType.FixedRoutine;
			RunActive(Target => Target.FixedRoutine());

			GameManager.instance.currentFixedUpdatePhase = FixedUpdateRoutineType.RoutineEnd;
		}

		// 인스턴스 메서드
		protected virtual void OnIdChanged(string id) { }

		protected virtual void OnCategoryChanged(string category) { }

		public AudioSource PlaySFX(int ClipId, Vector2 position = default, bool isLocalPosition = true, float volume = 1f, float pitch = 1f, float pan = 0f, float spread = 0f, bool loop = false) {
			return AudioManager.instance.PlaySFX(ClipId, transform, position, isLocalPosition, volume, pitch, pan, spread, loop);
		}

		public AudioSource PlaySFX(AudioClip clip, Vector2 position = default, bool isLocalPosition = true, float volume = 1f, float pitch = 1f, float pan = 0f, float spread = 0f, bool loop = false) {
			return AudioManager.instance.PlaySFX(clip, transform, position, isLocalPosition, volume, pitch, pan, spread, loop);
		}

		public void RegisterAction(UAction action) {
			if (!canStartOwnedWork) { action.Cancel(); return; }
			runningActions.Add(action);
		}
		
		public void UnregisterAction(UAction action) {
			runningActions.Remove(action);
		}

		public void StopAllUActions() {
			UAction[] Actions = runningActions.ToArray();
			runningActions.Clear();
			foreach (UAction Action in Actions) Action.Cancel();
		}
		
		public void SendEvent(EventType type, EventPriority layer, IEventData data = null) {
			EventManager.instance.AddEvent(new Event(type, this, data), layer);
		}

		public bool isRegistered(GameObject obj) {
			return objectInitializeData.ContainsKey(obj);
		}
		
		public void RegisterObjectInitializeData(GameObject obj) {
			ObjectInitializeData data = new();
			
			data.initialPosition = obj.transform.localPosition;
			data.initialRotation = obj.transform.rotation;
			data.initialScale    = obj.transform.localScale;
			
			data.rigidbody2D = obj.GetComponent<Rigidbody2D>();
			if (data.rigidbody2D) {
				data.initialRigidBodyType  = data.rigidbody2D.bodyType;
				data.initialLinearDamping  = data.rigidbody2D.linearDamping;
				data.initialAngularDamping = data.rigidbody2D.angularDamping;
				data.initialGravityScale   = data.rigidbody2D.gravityScale;
			}
			
			data.spriteRenderer = obj.GetComponent<SpriteRenderer>();
			if (data.spriteRenderer) { 
				data.initialColor  = data.spriteRenderer.color;
				data.initialSprite = data.spriteRenderer.sprite;
			}
			
			objectInitializeData.Add(obj, data);
		}

		public void ApplyObjectInitializeData(GameObject obj) {
			ApplyObjectInitializeData(obj, objectInitializeData[obj]);
		}
		
		public void ApplyObjectInitializeData(GameObject obj, ObjectInitializeData data) {
			if (obj != gameObject) obj.transform.localPosition = data.initialPosition;
			obj.transform.rotation  = data.initialRotation;
			obj.transform.localScale = data.initialScale;
			
			if (data.rigidbody2D) {
				data.rigidbody2D.bodyType        = data.initialRigidBodyType;
				data.rigidbody2D.linearVelocity  = Vector2.zero;
				data.rigidbody2D.angularVelocity = 0f;
				if (data.initialRigidBodyType == RigidbodyType2D.Dynamic) {
					data.rigidbody2D.linearDamping  = data.initialLinearDamping;
					data.rigidbody2D.angularDamping = data.initialAngularDamping;
					data.rigidbody2D.gravityScale   = data.initialGravityScale;
				}
			}
			
			if (data.spriteRenderer) {
				data.spriteRenderer.sprite = data.initialSprite;
				data.spriteRenderer.color  = data.initialColor;
				data.spriteRenderer.flipX = data.initialFlipX;
				data.spriteRenderer.flipY = data.initialFlipY;
			}
		}
		
		public void RegisterToAllChildren(Transform obj) {
			if (!isRegistered(obj.gameObject)) RegisterObjectInitializeData(obj.gameObject); // 부모에서 사용
			foreach (Transform child in obj.transform) {
				RegisterToAllChildren(child);
			}
		}
		
		public void ApplyToAllChildren() {
			foreach (GameObject obj in objectInitializeData.Keys) {
				ApplyObjectInitializeData(obj);
			}
		}
		
		public void Freeze() {
			FreezeRigidbody2D();
			FreezeAnimator();
		}

		public void FreezeRigidbody2D() {
			if (isRigidbody2DFrozen) return;
			isRigidbody2DFrozen = true;
			
			if (rigidbody2D) {
				linearVelocityBeforeFreeze  = rigidbody2D.linearVelocity;
				angularVelocityBeforeFreeze = rigidbody2D.angularVelocity;
				constraintsBeforeFreeze = rigidbody2D.constraints;
				
				rigidbody2D.constraints     = RigidbodyConstraints2D.FreezeAll;
				rigidbody2D.linearVelocity  = Vector2.zero;
				rigidbody2D.angularVelocity = 0;
			}
		}

		public void FreezeAnimator() {
			if (isAnimatorFrozen) return;
			isAnimatorFrozen = true;
			
			if (animator) {
				animatorSpeedBeforeFreeze = animator.speed;
				AnimatorEnabledBeforeFreeze = animator.enabled;
				animator.speed            = 0;
				animator.enabled          = false;
			}
		}
		
		public void Unfreeze() {
			UnfreezeRigidbody2D();
			UnfreezeAnimator();
		}
		
		public void UnfreezeRigidbody2D() {
			if (!isRigidbody2DFrozen) return; 
			isRigidbody2DFrozen = false;
			
			if (rigidbody2D) {
				rigidbody2D.constraints     = constraintsBeforeFreeze;
				rigidbody2D.linearVelocity  = linearVelocityBeforeFreeze;
				rigidbody2D.angularVelocity = angularVelocityBeforeFreeze;
			}
		}

		public void UnfreezeAnimator() {
			if (!isAnimatorFrozen) return;
			isAnimatorFrozen = false;
			
			if (animator) {
				animator.speed   = animatorSpeedBeforeFreeze;
				animator.enabled = AnimatorEnabledBeforeFreeze;
			}
		}

		protected void ToggleColliders(bool targetState) {
			foreach (Collider2D c in GetComponents<Collider2D>()) {
				if (c.isTrigger) continue;
				c.enabled = targetState;
			}
		}

		protected virtual void OnGet() { }
		
		public virtual void Initialize() { }
		
		protected virtual void OnRelease() { }
		
		public virtual void Uninitialize() { }

		public void SetGettingState(Getting State) {
			lifeStateMachine.SetGettingState(State);
		}

		public void SetReleasingState(Releasing State) {
			lifeStateMachine.SetReleasingState(State);
		}

		public void SetStates(Getting   GettingState, Releasing ReleasingState) {
			lifeStateMachine.SetStates(GettingState, ReleasingState);
		}

		protected void SetDefaultStates(Getting GettingState = null, Releasing ReleasingState = null) {
			lifeStateMachine.SetDefaultStates(GettingState, ReleasingState);
		}

		public virtual void OnFirstGet() {
			if (FirstGetCompleted) throw new InvalidOperationException("Object is already initialized.");
			
			rigidbody2D = GetComponent<Rigidbody2D>();
			spriteRenderer = GetComponent<SpriteRenderer>();
			animator = GetComponent<Animator>();
			
			Colliders = GetComponentsInChildren<Collider2D>(true);
			
			if (whenInitialize != WhenInitialize.Never) RegisterToAllChildren(transform);
			
			lifeStateMachine.Prepare();

			FirstGetCompleted = true;
			PreparedObjects.Add(this);
		}

		public void SetPoolReturnMethod(Action<UObject> Return) {
			if (PoolReturn != null) throw new InvalidOperationException("Object is already pool-owned.");
			if (!isReleased) throw new InvalidOperationException("Object is not released.");

			PoolReturn = Return ?? throw new ArgumentNullException(nameof(Return));
		}

		protected void StartGet(bool PlayEffect, Action<UObject> Configure = null) {
			if (!FirstGetCompleted) throw new InvalidOperationException("Object is not initialized.");
			if (!isReleased) throw new InvalidOperationException("Object is not released.");
			if (lifeStateMachine.isShuttingDown) throw new InvalidOperationException("Object is shutting down.");

			if (whenInitialize is WhenInitialize.OnGet or WhenInitialize.Both) ApplyToAllChildren();

			ObjectConfigure();
			Configure?.Invoke(this);
			
			lifeStateMachine.Get(PlayEffect);
		}
		
		protected virtual void ObjectConfigure() { }

		public void HelperInitialize() {
			if (PoolReturn != null) throw new InvalidOperationException("Object is in pool.");
			if (!FirstGetCompleted) OnFirstGet();

			StartGet(false);
		}
		
		public void Release(bool PlayEffect = true) =>
			lifeStateMachine.Release(PlayEffect);

		public void PrepareGetting() {
			int Life = lifeNumber;
			
			SetLifeColliders(false);
			FreezeRigidbody2D();
			gameObject.SetActive(true);

			if (lifeNumber != Life) return;
			if (lifeStateMachine.phase != LifePhase.Getting) return;

			OnGet();
			
			if (lifeNumber             != Life) return;
			if (lifeStateMachine.phase != LifePhase.Getting) return;
			
			GetTask?.Execute(this, this);
		}

		public void ActivateLife() {
			if (lifeStateMachine.phase != LifePhase.Active) throw new InvalidOperationException("Object is not active.");
			if (ActiveEntry != null) throw new InvalidOperationException("Object is already active.");

			UnfreezeRigidbody2D();
			SetLifeColliders(true);

			ActiveEntry = new RoutineEntry(this);
			RoutineObjects.Add(ActiveEntry);
		}

		public void DeactivateLife() {
			if (ActiveEntry == null) return;

			RoutineObjects.Remove(ActiveEntry);
			ActiveEntry = null;
		}

		public void StartLifeRoutine(int Execution) {
			if (LifeEntry != null) throw new InvalidOperationException("Object already has an active life cycle.");
			if (!lifeStateMachine.IsCurrent(lifeStateMachine.currentState, Execution)) throw new InvalidOperationException("Object has no matching life cycle execution.");

			LifeEntry = new RoutineEntry(this, Execution);

			LifeCycleObjects.Add(LifeEntry);
		}

		public void EndLifeRoutine(int Execution) {
			if (LifeEntry == null) return;
			if (LifeEntry.Execution != Execution) return;

			LifeCycleObjects.Remove(LifeEntry);
			LifeEntry = null;
		}

		public void PrepareReleasing() {
			DeactivateLife();

			SetLifeColliders(false);
			FreezeRigidbody2D();
			
			StopRunningTask();

			if (lifeStateMachine.isShuttingDown) return;

			lifeStateMachine.ReleaseStarted = true;
			OnRelease();

			if (lifeStateMachine.isShuttingDown) return;
			
			ReleaseTask?.Execute(this, this);
			
			ID = null;
			Category = null;
		}

		public void FinishReleasing() {
			StopRunningTask();
			Uninitialize();

			if (lifeStateMachine.isShuttingDown) return;
			
			Unfreeze();

			if (whenInitialize is WhenInitialize.OnRelease or WhenInitialize.Both) ApplyToAllChildren();
		}

		public void ReturnToPool() {
			if (isStoppingLifeCycles || lifeStateMachine.isShuttingDown) {
				gameObject.SetActive(false);
				return;
			}
			
			if (PoolReturn != null) {
				PoolReturn.Invoke(this);
				return;
			}
			
			gameObject.SetActive(false);
		}

		public void StopLife(bool ReleaseStarted) {
			StopRunningTask();

			if (!ReleaseStarted && lifeStateMachine.phase != LifePhase.Released) OnRelease();

			ID = null;
			Category = null;
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
			foreach (Collider2D collider in Colliders) {
				collider.enabled = Enabled;
			}
		}

		protected virtual void OnDestroy() {
			_lifeStateMachine?.Shutdown();
			PreparedObjects.Remove(this);
		}
		
		protected virtual void EarlyRoutine() { }
		protected virtual void Routine() { }
		protected virtual void LateRoutine() { }
		protected virtual void FixedRoutine() { }
		
		public void AddProcessToUpdate(Action Process) {
			if (Process == null) throw new ArgumentNullException(nameof(Process));
			if (!isActive) return;
			
			ProcessToUpdate.Enqueue(Process);
		}

		protected void AddProcessToFixedUpdate(Action Process) {
			if (Process == null) throw new ArgumentNullException(nameof(Process));
			if (!isActive) return;
			
			ProcessToFixedUpdate.Enqueue(Process);
		}

		private void ExecuteProcesses(Queue<Action> Processes) {
			int Life  = lifeNumber;

			while (Processes.Count > 0) {
				if (!isActive) return;
				if (!Matches(Life)) return;
				Processes.Dequeue().Invoke();
			}
		}
	}
}
