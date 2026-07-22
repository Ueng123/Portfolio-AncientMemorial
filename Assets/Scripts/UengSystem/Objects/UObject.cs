using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
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
using Events_Event = UengSystem.Events.Event;
using Events_EventType = UengSystem.Events.EventType;

namespace UengSystem.Objects {
	public abstract class UObject : MonoBehaviour, IObjectPoolable, IActionable, IEventAgent, IInitializable, ITaskable {

		private static readonly Dictionary<string,      UObject > IDTable       = new ();
		private static readonly Dictionary<string, List<UObject>> CategoryTable = new ();
		public static           BufferedList<UObject>             Instances     = new();
		
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
				// 수박맛있다
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
		public bool   _gettable;
		public bool   gettable   { get => _gettable; set => _gettable = value; }
		public bool   isReleased { get;              set; }

		public AudioSource PlaySFX(string clipName, Vector2 position = default, bool isLocalPosition = true, float volume = 1f, float pitch = 1f, float pan = 0f, float spread = 0f, bool loop = false) {
			return AudioManager.instance.PlaySFX(clipName, transform, position, isLocalPosition, volume, pitch, pan, spread, loop);
		}
		
		public AudioSource PlaySFX(AudioClip clip, Vector2 position = default, bool isLocalPosition = true, float volume = 1f, float pitch = 1f, float pan = 0f, float spread = 0f, bool loop = false) {
			return AudioManager.instance.PlaySFX(clip, transform, position, isLocalPosition, volume, pitch, pan, spread, loop);
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

		// IStoppable Variables //
		private int stoppedTime;

		public bool stopped {
			get => stoppedTime != 0;
			set {
				stoppedTime += value ? 1 : -1;
				Debug.Log($"STOP({stoppedTime}) OBJECT : {name}");
				
				if (stoppedTime >= 0) return;
				Debug.LogWarning("u just tried to Resume no stopped obj twin WHY???????? :(");
				stoppedTime = 0;
			}
		}
		private   Vector2         linearVelocityBeforeStop;
		private   float           angularVelocityBeforeStop;
		private   RigidbodyType2D bodyTypeBeforeStop;
		protected float           animatorSpeedBeforeStop;
		
		public    float           DeltaTime => Time.deltaTime * (stopped ? 0 : 1);
		
		// Components //
		[HideInInspector] public new Rigidbody2D    rigidbody2D;
		[HideInInspector] public     SpriteRenderer spriteRenderer;
		[HideInInspector] public     Animator       animator;

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
			IDTable.Clear();
			CategoryTable.Clear();
		}

		// IEventAgent Method //
		public void SendEvent(Events_EventType type, int layer, EventData data = default) {
			EventManager.instance.AddEvent(new Events_Event(type, this, data), layer);
		}

		public virtual void OnEvent(Events_Event e) { }

		private Quaternion      _initialRotation;
		private Vector3         _initialScale;
		private Color           _initialColor;
		private Sprite          _initialSprite;
		private RigidbodyType2D _initialRigidBodyType;
		private float           _initialLinearDamping;
		private float           _initialAngularDamping;
		private float           _initialGravityScale;
		public virtual void OnFirstGet() {
			_initialRotation = transform.rotation;
			_initialScale    = transform.localScale;
			
			rigidbody2D    = GetComponent<Rigidbody2D>();
			if (rigidbody2D) {
				_initialRigidBodyType  = rigidbody2D.bodyType;
				_initialLinearDamping  = rigidbody2D.linearDamping;
				_initialAngularDamping = rigidbody2D.angularDamping;
				_initialGravityScale   = rigidbody2D.gravityScale;
			}
			
			spriteRenderer = GetComponent<SpriteRenderer>();
			if (spriteRenderer) {
				_initialColor  = spriteRenderer.color;
				_initialSprite = spriteRenderer.sprite;
			}
			
			animator       = GetComponent<Animator>();
		}

		private DelayedAction spawnFXCache = null;
		public Task          GetTask;
		
		public virtual void Get(float time) {
			transform.rotation   = _initialRotation;
			transform.localScale = _initialScale;
			
			if (rigidbody2D) {
				rigidbody2D.linearVelocity  = Vector2.zero;
				rigidbody2D.angularVelocity = 0f;
				rigidbody2D.bodyType        = _initialRigidBodyType;
				rigidbody2D.linearDamping   = _initialLinearDamping;
				rigidbody2D.angularDamping  = _initialAngularDamping;
				rigidbody2D.gravityScale    = _initialGravityScale;
			}
			
			if (spriteRenderer) {
				spriteRenderer.color  = _initialColor;
				spriteRenderer.sprite = _initialSprite;
			}
			
			if (time == 0) {
				OnGet();
				Initialize();
				ToggleColliders(true);
				return;
			}
			
			StopAllCoroutines();
			
			GameObject spawnFX = UObjectPool.instance.Get("SpawnEffect", transform.position);
			spawnFX.GetComponent<SpawnEffectHelper>().t_s = time;
			
			OnGet();
			
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
		
		public Task          ReleaseTask;
		
		public virtual void Release(float time) {
			if (isReleased) return;
			spawnFXCache?.Cancel();
			
			gettable = false;
			ID        = null;
			Category  = null;
			
			switch (time) {
				case < 0:
					Uninitialize();
					gettable = true;
					break;
				case 0:
					OnRelease();
					Uninitialize();
					gettable = true;
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

		public virtual void OnGet() {
			Instances.Add(this);

			if (rigidbody2D && rigidbody2D.bodyType != RigidbodyType2D.Static)
				rigidbody2D.linearVelocity = Vector2.zero;
			
			GetTask.Execute(this, this);
		}

		protected virtual void OnRelease() {
			ReleaseTask.Execute(this, this);

			currentExclusiveAction = null;
			StopAllCoroutines();
			StopAllUActions();
			
			if (rigidbody2D && rigidbody2D.bodyType == RigidbodyType2D.Dynamic) rigidbody2D.linearVelocity = Vector2.zero;
		}

		public virtual void Initialize() {
			
		}

		public virtual void Uninitialize() {
			Instances.Remove(this);
		}
	}
}