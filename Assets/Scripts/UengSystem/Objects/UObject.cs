using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using AncientMemorial;
using AncientMemorial.Objects;
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
	public abstract class UObject : MonoBehaviour, IObjectPoolable, IEventAgent, IStoppable, IInitializable, ITaskable {

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
					}).Execute();
				}
			}
		}

		public Sprite whiteSpawnSprite;
		public Sprite colorSpawnSprite;
		public bool   _gettable;
		public bool   gettable   { get => _gettable; set => _gettable = value; }
		public bool   isReleased { get;              set; }

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

		// IStoppable Method //
		
		
		public virtual void Stop() {
			if (stopped) { stopped = true; return; } // 현재 stop -> 중첩쌓고 ㅃㅃ
			stopped = true; // 중첩 쌓기
			
			if (rigidbody2D) {
				linearVelocityBeforeStop  = rigidbody2D.linearVelocity;
				angularVelocityBeforeStop = rigidbody2D.angularVelocity;
				bodyTypeBeforeStop        = rigidbody2D.bodyType;
				
				rigidbody2D.bodyType       = RigidbodyType2D.Kinematic;
				rigidbody2D.linearVelocity = Vector2.zero;
				rigidbody2D.angularVelocity = 0;
			}
			
			if (animator) {
				animatorSpeedBeforeStop = animator.speed;
				animator.speed   = 0;
				animator.enabled = false;
			}
		}

		public virtual void Resume() {
			stopped = false; // stop 중첩 -1
			if (stopped) return; // stop 중첨 완전 해제 -> Resume
			
			if (rigidbody2D) {
				rigidbody2D.linearVelocity  = linearVelocityBeforeStop;
				rigidbody2D.angularVelocity = angularVelocityBeforeStop;
				rigidbody2D.bodyType        = bodyTypeBeforeStop;
			}

			if (animator) {
				animator.speed   = animatorSpeedBeforeStop;
				animator.enabled = true;
			}
		}

		public virtual void OnFirstGet() {
			rigidbody2D    = GetComponent<Rigidbody2D>();
			spriteRenderer = GetComponent<SpriteRenderer>();
			animator       = GetComponent<Animator>();
		}

		public DelayedAction spawnFXCache = null;
		public Task          GetTask;
		
		public virtual void Get(float time) {
			if (time == 0) {
				// DONT NEED ANY FREEZE / UNFREEZE 그리고 어짜피 UNFREEZE 하면
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
			
			spawnFXCache.Execute();
		}
		
		public DelayedAction despawnFXCache = null;
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
			if (rigidbody2D && rigidbody2D.bodyType == RigidbodyType2D.Dynamic) rigidbody2D.linearVelocity = Vector2.zero;
		}

		public virtual void Initialize() {
			
		}

		public virtual void Uninitialize() {
			Instances.Remove(this);
		}
	}
}