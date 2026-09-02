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
using UengSystem.States;
using UengSystem.UAction;
using UengSystem.UDebug;
using UengSystem.Utility;
using UengSystem.VisualScripting.Tasks;
using UnityEngine;
using Event = UengSystem.Events.Event;
using EventType = UengSystem.Events.EventType;	

namespace UengSystem.Objects {
	public class UObject : MonoBehaviour, IObjectPoolable, IActionable, IEventAgent, ITaskable {

		private static readonly Dictionary<string, UObject>       IDTable       = new();
		private static readonly Dictionary<string, List<UObject>> CategoryTable = new();
		public static           BufferedList<UObject>             instances     = new();

		public WhenInitialize whenInitialize;

		// Instance Variables //
		[Header("Identify")] private string _ID;
		private                      string _Category;

		public string ID {
			get => _ID;
			set {
				if (value == null) {
					if (_ID == null) return;

					IDTable[_ID] = null;
					_ID          = null;

					return;
				}

				if (_ID != null) throw new InvalidOperationException("u just tried to set ID twice twin :(");

				IDTable[value] = this;
				_ID            = value;
				OnIdChanged(value);
			}
		}
		
		protected virtual void OnIdChanged(string id) { }

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

		protected virtual void OnCategoryChanged(string category) { }

		public Sprite whiteSpawnSprite;
		public Sprite colorSpawnSprite;
		public bool   isReleased { get; set; }

		public AudioSource PlaySFX(string clipName,    Vector2 position = default, bool  isLocalPosition = true,
								   float  volume = 1f, float   pitch    = 1f,      float pan = 0f, float spread = 0f,
								   bool   loop   = false) {
			return AudioManager.instance.PlaySFX(clipName, transform, position, isLocalPosition, volume, pitch, pan,
												 spread, loop);
		}

		public AudioSource PlaySFX(AudioClip clip,        Vector2 position = default, bool  isLocalPosition = true,
								   float     volume = 1f, float   pitch    = 1f,      float pan = 0f, float spread = 0f,
								   bool      loop   = false) {
			return AudioManager.instance.PlaySFX(clip, transform, position, isLocalPosition, volume, pitch, pan, spread,
												 loop);
		}

		// Routine System //
		protected virtual void EarlyRoutine() {
			executeRoutineMethodCache[GetType()] &= 0b_0001_1110;
			earlyRoutineObjects.Remove(this);
		}

		protected virtual void Routine() {
			executeRoutineMethodCache[GetType()] &= 0b_0001_1101;
			routineObjects.Remove(this);
		}

		protected virtual void LateRoutine() {
			executeRoutineMethodCache[GetType()] &= 0b_0001_0111;
			lateRoutineObjects.Remove(this);
		}

		protected virtual void FixedRoutine() {
			executeRoutineMethodCache[GetType()] &= 0b_0000_1111;
			fixedRoutineObjects.Remove(this);
		}

		public static BufferedList<UObject> earlyRoutineObjects = new BufferedList<UObject>(100, 10);
		public static BufferedList<UObject> routineObjects      = new BufferedList<UObject>(100, 10);
		public static BufferedList<UObject> eventRoutineObjects = new BufferedList<UObject>(100, 10);
		public static BufferedList<UObject> lateRoutineObjects  = new BufferedList<UObject>(100, 10);
		public static BufferedList<UObject> fixedRoutineObjects = new BufferedList<UObject>(100, 10);
		
		private readonly Dictionary<Type, byte> executeRoutineMethodCache = new Dictionary<Type, byte>();

		private Queue<Action> processToUpdate;
		private Queue<Action> processToFixedUpdate;

		public void RegisterRoutineList() {
			executeRoutineMethodCache.TryAdd(GetType(), 0b_0001_1111);
			byte executeRoutineMethod = executeRoutineMethodCache[GetType()];
			
			if ((executeRoutineMethod &0b_0000_0001)!=0) earlyRoutineObjects.Add(this);
			if ((executeRoutineMethod &0b_0000_0010)!=0) routineObjects     .Add(this);
			if ((executeRoutineMethod &0b_0000_0100)!=0) eventRoutineObjects.Add(this);
			if ((executeRoutineMethod &0b_0000_1000)!=0) lateRoutineObjects .Add(this);
			if ((executeRoutineMethod &0b_0001_0000)!=0) fixedRoutineObjects.Add(this);
		}
		
		public void UnregisterRoutineList() {
			byte executeRoutineMethod = executeRoutineMethodCache.GetValueOrDefault(GetType(), (byte)0b_0001_1111);
			
			if ((executeRoutineMethod &0b_0000_0001)!=0) earlyRoutineObjects.Remove(this);
			if ((executeRoutineMethod &0b_0000_0010)!=0) routineObjects     .Remove(this);
			if ((executeRoutineMethod &0b_0000_0100)!=0) eventRoutineObjects.Remove(this);
			if ((executeRoutineMethod &0b_0000_1000)!=0) lateRoutineObjects .Remove(this);
			if ((executeRoutineMethod &0b_0001_0000)!=0) fixedRoutineObjects.Remove(this);
		}
		
		public static void UpdateRoutine() {
			GameManager.instance.currentUpdatePhase = UpdateRoutineType.Processing;
			for (int i = 0; i < instances.Count; i++) {
				UObject obj = instances[i];
				try {
					obj.ExecuteUpdateProcess();
				}
				catch (Exception e) {
					DebugManager.LogError(
						$"Update > UObject.UpdateRoutine() > {obj.gameObject.name}({obj.GetType().Name}).ExecuteUpdateProcess()",
						e, obj.gameObject);
				}
			}

			GameManager.instance.currentUpdatePhase = UpdateRoutineType.EarlyRoutine;
			earlyRoutineObjects.Apply();
			for (int i = 0; i < earlyRoutineObjects.Count; i++) {
				UObject obj = earlyRoutineObjects[i];
				try {
					obj.EarlyRoutine();
				}
				catch (Exception e) {
					DebugManager.LogError(
						$"Update > UObject.UpdateRoutine() > {obj.gameObject.name}({obj.GetType().Name}).EarlyRoutine()",
						e, obj.gameObject);
				}
			}

			GameManager.instance.currentUpdatePhase = UpdateRoutineType.Routine;
			routineObjects.Apply();
			for (int i = 0; i < routineObjects.Count; i++) {
				UObject obj = routineObjects[i];
				try {
					obj.Routine();
				}
				catch (Exception e) {
					DebugManager.LogError(
						$"Update > UObject.UpdateRoutine() > {obj.gameObject.name}({obj.GetType().Name}).Routine()", e,
						obj.gameObject);
				}
			}
		}

		public static void LateUpdateRoutine() {
			GameManager.instance.currentUpdatePhase = UpdateRoutineType.LateRoutine;
			lateRoutineObjects.Apply();
			for (int i = 0; i < lateRoutineObjects.Count; i++) {
				UObject obj = lateRoutineObjects[i];
				try {
					obj.LateRoutine();
				}
				catch (Exception e) {
					DebugManager.LogError(
						$"LateUpdate > UObject.UpdateRoutine() > {obj.gameObject.name}({obj.GetType().Name}).LateRoutine()",
						e, obj.gameObject);
				}
			}

			GameManager.instance.currentUpdatePhase = UpdateRoutineType.RoutineEnd;
		}
		
		public static void FixedUpdateRoutine() {
			GameManager.instance.currentFixedUpdatePhase = FixedUpdateRoutineType.Processing;
			for (int i = 0; i < instances.Count; i++) {
				UObject obj = instances[i];
				try {
					obj.ExecuteFixedUpdateProcess();
				}
				catch (Exception e) {
					DebugManager.LogError(
						$"FixedUpdate > UObject.FixedUpdateRoutine() > {obj.gameObject.name}({obj.GetType().Name}).ExecuteFixedUpdateProcess()",
						e, obj.gameObject);
				}
			}

			GameManager.instance.currentFixedUpdatePhase = FixedUpdateRoutineType.FixedRoutine;
			fixedRoutineObjects.Apply();
			for (int i = 0; i < fixedRoutineObjects.Count; i++) {
				UObject obj = fixedRoutineObjects[i];
				try {
					obj.FixedRoutine();
				}
				catch (Exception e) {
					DebugManager.LogError(
						$"FixedUpdate > UObject.FixedUpdateRoutine() > {obj.gameObject.name}({obj.GetType().Name}).FixedRoutine()",
						e, obj.gameObject);
				}
			}

			GameManager.instance.currentFixedUpdatePhase = FixedUpdateRoutineType.RoutineEnd;
		}
		
		// Process System //
		protected void AddProcessToUpdate(Action process) {
			processToUpdate.Enqueue(process);
		}

		protected void AddProcessToFixedUpdate(Action process) {
			processToFixedUpdate.Enqueue(process);
		}
		
		private void ExecuteUpdateProcess() {
			if (processToUpdate.Count == 0) return;
			
			foreach (Action process in processToUpdate) {
				process.Invoke();
			}
			
			processToUpdate.Clear();
		}
		
		private void ExecuteFixedUpdateProcess() {
			if (processToFixedUpdate.Count == 0) return;
			
			foreach (Action process in processToFixedUpdate) {
				process.Invoke();
			}
			
			processToFixedUpdate.Clear();
		}
		
		// IActionable //
		private readonly List<UAction.UAction> runningActions = new();

		public void RegisterAction(UAction.UAction action) {
			runningActions.Add(action);
		}
		
		public void UnregisterAction(UAction.UAction action) {
			runningActions.Remove(action);
		}

		public void StopAllUActions() {
			foreach (UAction.UAction action in runningActions) { action.Cancel(); }
			runningActions.Clear();
		}
		
		// Components //
		[HideInInspector] public Rigidbody2D    rigidbody2D;
		[HideInInspector] public SpriteRenderer spriteRenderer;
		[HideInInspector] public Animator       animator;

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

		public static void ResetUObjects() {
			instances.ClearImmediately();
			earlyRoutineObjects?.ClearImmediately(); 
			routineObjects?.ClearImmediately(); 
			lateRoutineObjects?.ClearImmediately(); 
			eventRoutineObjects?.ClearImmediately();
			fixedRoutineObjects?.ClearImmediately();
			
			IDTable.Clear();
			CategoryTable.Clear();
		}

		// IEventAgent Method //
		public void SendEvent(EventType type, EventPriority layer, IEventData data = null) {
			EventManager.instance.AddEvent(new Event(type, this, data), layer);
		}
		
		private Dictionary<GameObject, ObjectInitializeData> objectInitializeData = new();

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
		
		public const int updatePhaseCount      = 6;
		public const int fixedUpdatePhaseCount = 3;

		// 게임 오브젝트가 오브젝트 풀을 통해 Instantiate 되었을때 실행 //
		public virtual void OnFirstGet() {
			// GET COMPONENTS //
			rigidbody2D    = GetComponent<Rigidbody2D>();
			spriteRenderer = GetComponent<SpriteRenderer>();
			animator       = GetComponent<Animator>();
			
			// INITIALIZE PROCESS //
			processToUpdate      = new Queue<Action>(); // sent from FixedUpdate
			processToFixedUpdate = new Queue<Action>(); // sent from Update
			
			// INITIALIZE OBJECT //
			if (whenInitialize == WhenInitialize.Never) return;
			RegisterToAllChildren(transform);
		}

		private DelayedAction spawnFXCache = null;
		public Task          GetTask;
		
		// 게임 오브젝트가 Pool.Get()되었을때 실행 //
		public virtual void Get(float time) {
			if (whenInitialize is WhenInitialize.OnGet or WhenInitialize.Both) ApplyToAllChildren();
			
			StopAllCoroutines();
			
			GetTask.Execute(this, this);
			OnGet();
			
			if (time == 0) {
				Initialize();
				ToggleColliders(true);
				return;
			}
			
			GameObject spawnFX = UObjectPool.instance.Get("SpawnEffect", transform.position);
			spawnFX.GetComponent<SpawnEffectHelper>().t_s = time;
			
			PrepareSpawnFX();
			Coroutine spawnCoroutine = StartCoroutine(SpawnFX(time));
			
			spawnFXCache = new DelayedAction(
				time,
				() => {
					FinishSpawnFX();
					spawnFXCache = null;
				},
				() => {
					StopCoroutine(spawnCoroutine);
					FinishSpawnFX();
					spawnFXCache = null;
				}, this);
			
			spawnFXCache.ExecuteDA();
		}
		
		public Task ReleaseTask;
		
		public virtual void Release(float time) {
			if (isReleased) return;
			spawnFXCache?.Cancel();
			
			ID        = null;
			Category  = null;
			
			switch (time) {
				case < 0:
					OnFinalRelease();
					break;
				case 0:
					OnReleaseCall();
					OnFinalRelease();
					break;
				default:
					OnReleaseCall();
					PrepareDespawnFX();
					StartCoroutine(DespawnFX(time));
					break;
			}
		}

		private bool                   isRigidbody2DFrozen = false;
		private Vector2                linearVelocityBeforeFreeze;
		private float                  angularVelocityBeforeFreeze;
		private RigidbodyConstraints2D constraintsBeforeFreeze;

		private   bool  isAnimatorFrozen = false;
		protected float animatorSpeedBeforeFreeze;
		
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
				animator.enabled = true;
			}
		}

		protected void ToggleColliders(bool targetState) {
			foreach (Collider2D c in GetComponents<Collider2D>()) {
				if (c.isTrigger) return;
				c.enabled = targetState;
			}
		}

		protected Color colorBeforeSpawnFX;

		protected virtual void PrepareSpawnFX() {
			if (rigidbody2D && rigidbody2D.bodyType!=RigidbodyType2D.Static) rigidbody2D.linearVelocity = Vector2.zero;
			Freeze();
			ToggleColliders(false);

			if (spriteRenderer) spriteRenderer.sprite = whiteSpawnSprite ?? spriteRenderer.sprite;
			if (spriteRenderer) colorBeforeSpawnFX = spriteRenderer.color;
		}


		protected virtual IEnumerator SpawnFX(float duration) {
			float elapsed = 0f;


			while (elapsed < duration) {
				elapsed += Time.deltaTime;

				float t = elapsed / duration;

				Color color = new(Mathf.Lerp(GameManager.instance.spawnColor.r, 1, t),
								  Mathf.Lerp(GameManager.instance.spawnColor.g, 1, t),
								  Mathf.Lerp(GameManager.instance.spawnColor.b, 1, t),
								  Mathf.Lerp(0,                                 1, t));


				if (spriteRenderer) spriteRenderer.color = color;

				yield return null;
			}
		}

		protected virtual void FinishSpawnFX() {
			DebugManager.Log("Finish spawning FX");
			
			if (spriteRenderer) spriteRenderer.sprite = colorSpawnSprite ?? spriteRenderer.sprite;
			if (spriteRenderer) spriteRenderer.color = colorBeforeSpawnFX;

			ToggleColliders(true);
			Unfreeze();
			
			Initialize();
		}
		
		public virtual void OnGet() { }
		
		public virtual void Initialize() {
			instances.Add(this);
			RegisterRoutineList();
		} 

		protected Color colorBeforeDespawnFX;

		protected virtual void PrepareDespawnFX() {
			Freeze();
			
			ToggleColliders(false);
			
			if (spriteRenderer) colorBeforeDespawnFX = spriteRenderer.color;
		}

		protected virtual IEnumerator DespawnFX(float duration) {
			float elapsed = 0f;
			
			if (spriteRenderer) spriteRenderer.sprite = whiteSpawnSprite ?? spriteRenderer.sprite;
			
			while (elapsed < duration) {
				elapsed += Time.deltaTime;

				float t = elapsed / duration;


				Color color = new(GameManager.instance.spawnColor.r,
								  GameManager.instance.spawnColor.g,
								  GameManager.instance.spawnColor.b,
								  Mathf.Lerp(1, 0, t));


				if (spriteRenderer) spriteRenderer.color = color;


				yield return null;
			}
			
			Unfreeze();
			if (spriteRenderer) spriteRenderer.color = colorBeforeDespawnFX;
			
			UObjectPool.instance.Release(gameObject, -1);
		}

		protected void OnReleaseCall() {
			ReleaseTask.Execute(this, this);
			
			instances.Remove(this);
			UnregisterRoutineList();
			
			StopAllCoroutines();
			StopAllUActions();
			
			processToUpdate.Clear();
			processToFixedUpdate.Clear();
			
			OnRelease();
		}

		protected void OnFinalRelease() {
			Uninitialize();
			if (whenInitialize is WhenInitialize.OnRelease or WhenInitialize.Both) ApplyToAllChildren();
		}
		
		protected virtual void OnRelease() { }
		
		public virtual void Uninitialize() { }
	}
}