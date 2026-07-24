using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using AncientMemorial;
using AncientMemorial.Objects;
using UengSystem.Audio;
using UengSystem.Events;
using UengSystem.Logic.Tasks;
using UengSystem.Managers;
using UengSystem.ObjectPool;
using UengSystem.Utility;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Event = UengSystem.Events.Event;
using EventType = UengSystem.Events.EventType;	

namespace UengSystem.Objects {
	public class UObject : MonoBehaviour, IObjectPoolable, IActionable, IEventAgent, IInitializable, ITaskable {

		private static readonly Dictionary<string,      UObject > IDTable       = new ();
		private static readonly Dictionary<string, List<UObject>> CategoryTable = new ();
		public static           BufferedList<UObject>             instances     = new();

		public WhenInitialize whenInitialize;
		
		// Instance Variables //
		[Header("Identify")] 
		private string _ID;
		private string _Category;

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
				
				if (_Category != null) throw new InvalidOperationException("u just tried to set Category twice twin :(");
				if (!CategoryTable.ContainsKey(value)) CategoryTable[value] = new List<UObject>();
				
				CategoryTable[value].Add(this);
				_Category = value;
			}
		}
		
		private ExclusiveAction _currentExclusiveAction;
		public ExclusiveAction currentExclusiveAction {
			get {
				if (_currentExclusiveAction is { executing: false }) _currentExclusiveAction = null;
				return _currentExclusiveAction;
			}
			
			set {
				if (_currentExclusiveAction == null) {
					_currentExclusiveAction = value;
					value?.Execute();
				}
				else {
					_currentExclusiveAction.Cancel();

					float delay = _currentExclusiveAction.changeDelay;
					new DelayedAction(delay, () => {
						_currentExclusiveAction = value;
						value?.Execute();
					}).ExecuteDA();
				}
			}
		}

		public Sprite whiteSpawnSprite;
		public Sprite colorSpawnSprite;
		public bool   isReleased { get;              set; }

		public AudioSource PlaySFX(string clipName, Vector2 position = default, bool isLocalPosition = true, float volume = 1f, float pitch = 1f, float pan = 0f, float spread = 0f, bool loop = false) {
			return AudioManager.instance.PlaySFX(clipName, transform, position, isLocalPosition, volume, pitch, pan, spread, loop);
		}
		
		public AudioSource PlaySFX(AudioClip clip, Vector2 position = default, bool isLocalPosition = true, float volume = 1f, float pitch = 1f, float pan = 0f, float spread = 0f, bool loop = false) {
			return AudioManager.instance.PlaySFX(clip, transform, position, isLocalPosition, volume, pitch, pan, spread, loop);
		}
		
		// Routine System //
		protected virtual void EarlyRoutine()        { }
		protected virtual void Routine()             { }
		protected virtual void LateRoutine()         { }
		protected virtual void FixedRoutine()        { }
		public virtual    void EventRoutine(Event e) { }

		// === NEW! === //
		public static BufferedList<UObject> earlyRoutineObjects = new BufferedList<UObject>();
		public static BufferedList<UObject> routineObjects      = new BufferedList<UObject>();
		public static BufferedList<UObject> eventRoutineObjects = new BufferedList<UObject>();
		public static BufferedList<UObject> lateRoutineObjects  = new BufferedList<UObject>();
		public static BufferedList<UObject> fixedRoutineObjects = new BufferedList<UObject>();
		
		private          byte                   executeRoutineMethod      = 0b_0000_0000;
		private readonly Dictionary<Type, byte> executeRoutineMethodCache = new Dictionary<Type, byte>();
		// ============ //
		
		private List<Action>[] processToUpdate;
		private List<Action>[] processToFixedUpdate;
		
		// === NEW! === //
		public void CheckRoutine() {
			Type type = GetType();
			if (executeRoutineMethodCache.TryGetValue(type, out byte value)) {
				executeRoutineMethod = value;
				return;
			}
			
			MethodInfo[] routineMethodInfos =  {
				type.GetMethod("EarlyRoutine", BindingFlags.Instance | BindingFlags.NonPublic),
				type.GetMethod("Routine",      BindingFlags.Instance | BindingFlags.NonPublic),
				type.GetMethod("EventRoutine", BindingFlags.Instance | BindingFlags.NonPublic),
				type.GetMethod("LateRoutine",  BindingFlags.Instance | BindingFlags.NonPublic),
				type.GetMethod("FixedRoutine", BindingFlags.Instance | BindingFlags.NonPublic),
			};
			
			for (int i = 0; i < routineMethodInfos.Length; i++) {
				MethodInfo methodInfo = routineMethodInfos[i];
				if (methodInfo == null) {
					executeRoutineMethod |= (byte)(1 << i);
					continue;
				} 
       
				byte[] ilBytes = methodInfo.GetMethodBody()?.GetILAsByteArray();
				if (ilBytes == null || ilBytes.Length == 0) {
					executeRoutineMethod |= (byte)(1 << i);
					continue;
				}
				
				bool isEmpty = ilBytes.All(b => b == OpCodes.Nop.Value || b == OpCodes.Ret.Value);
				if (isEmpty) {
					executeRoutineMethod |= (byte)(1 << i);
				}
			}
			
			executeRoutineMethodCache[type] = executeRoutineMethod;
		}

		public void RegisterRoutineList() {
			if ((executeRoutineMethod &0b_0000_0001) ==0) earlyRoutineObjects.Add(this);
			if ((executeRoutineMethod &0b_0000_0010) ==0) routineObjects     .Add(this);
			if ((executeRoutineMethod &0b_0000_0100) ==0) eventRoutineObjects.Add(this);
			if ((executeRoutineMethod &0b_0000_1000) ==0) lateRoutineObjects .Add(this);
			if ((executeRoutineMethod &0b_0001_0000) ==0) fixedRoutineObjects.Add(this);
		}
		
		public void UnregisterRoutineList() {
			if ((executeRoutineMethod &0b_0000_0001) ==0) earlyRoutineObjects.Remove(this);
			if ((executeRoutineMethod &0b_0000_0010) ==0) routineObjects     .Remove(this);
			if ((executeRoutineMethod &0b_0000_0100) ==0) eventRoutineObjects.Remove(this);
			if ((executeRoutineMethod &0b_0000_1000) ==0) lateRoutineObjects .Remove(this);
			if ((executeRoutineMethod &0b_0001_0000) ==0) fixedRoutineObjects.Remove(this);
		}
		// ============ //
		
		public static void UpdateRoutine() {
			GameManager.instance.currentUpdatePhase = UpdateRoutineType.Processing;
			foreach (UObject obj in instances) {
				obj.ExecuteUpdateProcess();
			}
			
			GameManager.instance.currentUpdatePhase = UpdateRoutineType.EarlyRoutine;
			earlyRoutineObjects.Apply();
			foreach (UObject obj in earlyRoutineObjects) {
				obj.EarlyRoutine();
			}
			
			GameManager.instance.currentUpdatePhase = UpdateRoutineType.Routine;
			routineObjects.Apply();
			foreach (UObject obj in routineObjects) {
				obj.Routine();
			}
			
			GameManager.instance.currentUpdatePhase = UpdateRoutineType.EventRoutine;
			EventRoutine();
			
			GameManager.instance.currentUpdatePhase = UpdateRoutineType.LateRoutine;
			lateRoutineObjects.Apply();
			foreach (UObject obj in lateRoutineObjects) {
				obj.LateRoutine();
			}
			
			GameManager.instance.currentUpdatePhase = UpdateRoutineType.RoutineEnd;
		}
		
		public static void EventRoutine() {
			for (int i = 0; i < EventManager.instance.events.Count; i++) {
				List<Event> events = EventManager.instance.GetEvents(i);
				if (events.Count == 0) continue;
				
				foreach (Event e in events) {
					eventRoutineObjects.Apply();
					foreach (UObject obj in eventRoutineObjects) {
						obj.EventRoutine(e);
					}
				}
			}
		}
		
		public static void FixedUpdateRoutine() {
			GameManager.instance.currentFixedUpdatePhase = FixedUpdateRoutineType.Processing;
			foreach (UObject instance in instances) {
				instance.ExecuteFixedUpdateProcess();
			}
			
			GameManager.instance.currentFixedUpdatePhase = FixedUpdateRoutineType.FixedRoutine;
			foreach (UObject instance in fixedRoutineObjects) {
				instance.FixedRoutine();
			}
			
			GameManager.instance.currentFixedUpdatePhase = FixedUpdateRoutineType.RoutineEnd;
		}
		
		// Process System //
		protected void AddProcessToUpdate(Action process, bool notAllowedToOverlapped = false) {
			if (notAllowedToOverlapped) {
				if (processToUpdate[(int)GameManager.instance.currentFixedUpdatePhase].Contains(process)) return;
			}
			processToUpdate[(int)GameManager.instance.currentFixedUpdatePhase].Add(process);
		}

		protected void AddProcessToFixedUpdate(Action process, bool notAllowedToOverlapped = false) {
			if (notAllowedToOverlapped) {
				if (processToFixedUpdate[(int)GameManager.instance.currentUpdatePhase].Contains(process)) return;
			}
			processToFixedUpdate[(int)GameManager.instance.currentFixedUpdatePhase].Add(process);
		}
		
		private void ExecuteUpdateProcess() {
			foreach (List<Action> processes in processToUpdate) {
				foreach (Action process in processes) process.Invoke();
				processes.Clear();
			}
		}
		
		private void ExecuteFixedUpdateProcess() {
			foreach (List<Action> processes in processToFixedUpdate) {
				foreach (Action process in processes) process.Invoke();
				processes.Clear();
			}
		}
		
		// IActionable //
		private readonly List<UAction> runningActions = new();

		public void RegisterAction(UAction action) {
			runningActions.Add(action);
		}
		
		public void UnregisterAction(UAction action) {
			runningActions.Remove(action);
		}

		public void StopAllUActions() {
			foreach (UAction action in runningActions) { action.Cancel(); }
			runningActions.Clear();
		}
		
		// Components //
		[HideInInspector] public Rigidbody2D    rigidbody2D;
		[HideInInspector] public SpriteRenderer spriteRenderer;
		[HideInInspector] public Animator       animator;

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
			instances.Clear();
			IDTable.Clear();
			CategoryTable.Clear();
		}

		// IEventAgent Method //
		public void SendEvent(EventType type, int layer, EventData data = default) {
			EventManager.instance.AddEvent(new Event(type, this, data), layer);
		}
		
		private Dictionary<GameObject, ObjectInitializeData> objectInitializeData = new();

		public bool isRegistered(GameObject obj) {
			return objectInitializeData.ContainsKey(obj);
		}
		
		public void RegisterObjectInitializeData(GameObject obj) {
			ObjectInitializeData data = new();
			
			data.initialPosition = obj.transform.position;
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
			if (obj != gameObject) obj.transform.position = data.initialPosition;
			obj.transform.rotation  = data.initialRotation;
			obj.transform.localScale = data.initialScale;
			
			if (data.rigidbody2D) {
				data.rigidbody2D.bodyType       = data.initialRigidBodyType;
				switch (data.initialRigidBodyType) {
					case RigidbodyType2D.Static:
						break;
					
					case RigidbodyType2D.Kinematic:
						data.rigidbody2D.linearVelocity  = Vector2.zero; 
						data.rigidbody2D.angularVelocity = 0f;
						break;
						
					case RigidbodyType2D.Dynamic:
						data.rigidbody2D.linearVelocity  = Vector2.zero; 
						data.rigidbody2D.angularVelocity = 0f;
						data.rigidbody2D.linearDamping   = data.initialLinearDamping;
                        data.rigidbody2D.angularDamping  = data.initialAngularDamping;
                        data.rigidbody2D.gravityScale    = data.initialGravityScale;
						break;
					
					default:
						throw new ArgumentOutOfRangeException();
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
			RegisterObjectInitializeData(obj.gameObject); // 부모에서 사용
			
			if (obj.childCount == 0) return;                // 자식 X - 이 재귀 끝
			for (int i = obj.childCount - 1; i >= 0; i--) { // 자식 O
				Transform child = obj.GetChild(i);
				RegisterToAllChildren(child);
			}
		}
		
		public void ApplyToAllChildren(Transform obj) {

			if (isRegistered(obj.gameObject)) {
				// 기존에 있던 children임.
				ApplyObjectInitializeData(obj.gameObject); // 부모에서 사용
			}
			// 없던 child인 경우 무시
			
			if (obj.childCount == 0) return;                // 자식 X - 이 재귀 끝
			for (int i = obj.childCount - 1; i >= 0; i--) { // 자식 O
				Transform child = obj.GetChild(i);
				ApplyToAllChildren(child);
			}
		}

		public const int updatePhaseCount      = 5;
		public const int fixedUpdatePhaseCount = 2;
		
		// 게임 오브젝트가 오브젝트 풀을 통해 Instantiate 되었을때 실행 //
		public virtual void OnFirstGet() {
			CheckRoutine();
			
			// GET COMPONENTS //
			rigidbody2D    = GetComponent<Rigidbody2D>();
			spriteRenderer = GetComponent<SpriteRenderer>();
			animator       = GetComponent<Animator>();
			
			// INITIALIZE PROCESS //
			processToUpdate      = new List<Action>[fixedUpdatePhaseCount]; // sent from FixedUpdate
			processToFixedUpdate = new List<Action>[updatePhaseCount];      // sent from Update
			
			for (int i = 0; i < fixedUpdatePhaseCount; i++) processToUpdate[i]      = new List<Action>();
			for (int i = 0; i < updatePhaseCount     ; i++) processToFixedUpdate[i] = new List<Action>();
			
			// INITIALIZE OBJECT //
			if (whenInitialize == WhenInitialize.Never) return;
			RegisterToAllChildren(transform);
		}

		private DelayedAction spawnFXCache = null;
		public Task          GetTask;
		
		// 게임 오브젝트가 Pool.Get()되었을때 실행 //
		public virtual void Get(float time) {

			if (whenInitialize is WhenInitialize.OnGet or WhenInitialize.Both) ApplyToAllChildren(transform);
			
			StopAllCoroutines();
			
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
			
			foreach (List<Action> processes in processToUpdate) {
				processes.Clear();
			}
			
			foreach (List<Action> processes in processToFixedUpdate) {
				processes.Clear();
			}
			
			switch (time) {
				case < 0:
					Uninitialize();
					if (whenInitialize is WhenInitialize.OnRelease or WhenInitialize.Both) ApplyToAllChildren(transform);
					break;
				case 0:
					OnRelease();
					Uninitialize();
					if (whenInitialize is WhenInitialize.OnRelease or WhenInitialize.Both) ApplyToAllChildren(transform);
					break;
				default:
					OnRelease();
					
					PrepareDespawnFX();
					StartCoroutine(DespawnFX(time));
					
					break;
			}
		}

		private   bool            frozen = false;
		private   Vector2         linearVelocityBeforeFreeze;
		private   float           angularVelocityBeforeFreeze;
		private   RigidbodyType2D bodyTypeBeforeFreeze;
		protected float           animatorSpeedBeforeFreeze;
		
		public void Freeze() {
			Debug.Log($"[F{name}] FREEZE TRY TO {name}");
			if (frozen) return;

			frozen = true;

			Debug.Log($"[F{name}] FREEZE TO {name}");

			if (rigidbody2D) {
				linearVelocityBeforeFreeze = rigidbody2D.linearVelocity;
				angularVelocityBeforeFreeze = rigidbody2D.angularVelocity;
				bodyTypeBeforeFreeze = rigidbody2D.bodyType;

				rigidbody2D.bodyType = RigidbodyType2D.Kinematic;
				rigidbody2D.linearVelocity = Vector2.zero;
				rigidbody2D.angularVelocity = 0;
			}
			
			if (animator) {
				animatorSpeedBeforeFreeze = animator.speed;
				animator.speed = 0;
				animator.enabled = false;
			}
		}

		public void Unfreeze() {

			Debug.Log($"[F{name}] UNFREEZE TRY TO {name}");
			
			if (!frozen) return;
			frozen = false;

			Debug.Log($"[F{name}] UNFREEZE TRY TO {name}");

			if (rigidbody2D) {
				rigidbody2D.linearVelocity  = linearVelocityBeforeFreeze;
				rigidbody2D.angularVelocity = angularVelocityBeforeFreeze;
				rigidbody2D.bodyType        = bodyTypeBeforeFreeze;
			}
			
			if (animator) {
				animator.speed   = animatorSpeedBeforeFreeze;
				animator.enabled = true;
			}
		}

		protected void ToggleColliders(bool state) {
			foreach (Collider2D c in GetComponents<Collider2D>()) {
				if (c.isTrigger) return;
				c.enabled = state;
			}
		}

		protected Color colorBeforeSpawnFX;

		protected virtual void PrepareSpawnFX() {
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
			Debug.Log("Finish spawning FX");

			if (spriteRenderer) spriteRenderer.sprite = colorSpawnSprite ?? spriteRenderer.sprite;

			if (spriteRenderer) spriteRenderer.color = colorBeforeSpawnFX;


			ToggleColliders(true);
			
			Unfreeze();
			
			Initialize();
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

		// 게임 오브젝트가 Pool.Get()되었을때 실행 (인스턴스 커스텀용) //
		// 등록 -> 행동 //
		public virtual void OnGet() {
			instances.Add(this);
			
			GetTask.Execute(this, this);
		}

		// 게임 오브젝트가 Pool.Release()되었을때 실행 (인스턴스 커스텀용) //
		// 행동 -> 등록해제 //
		protected virtual void OnRelease() {
			ReleaseTask.Execute(this, this);
			
			UnregisterRoutineList();
			
			currentExclusiveAction = null;
			StopAllCoroutines();
			StopAllUActions();
		}

		public virtual void Initialize() {
			RegisterRoutineList();
		}

		public virtual void Uninitialize() {
			instances.Remove(this);
		}
	}
}