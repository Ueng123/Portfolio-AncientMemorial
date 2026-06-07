using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
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

		public ExclusiveAction _currentExclusiveAction;
		public ExclusiveAction currentExclusiveAction {
			get {
				if (_currentExclusiveAction is { executing: false }) _currentExclusiveAction = null;
				return _currentExclusiveAction;
			}
			
			set {
				// 수박맛있다
				if (_currentExclusiveAction == null) {
					_currentExclusiveAction = value;
					Debug.Log($"{name}'s ExclusiveAction is set to {value}");
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
		public  bool            stopped { get; set; }
		private Vector2         oldLinearVelocity;
		private RigidbodyType2D oldBodyType;
		private float           oldAnimatorSpeed;
		
		// Components //
		[HideInInspector] public new Rigidbody2D    rigidbody2D;
		[HideInInspector] public     SpriteRenderer spriteRenderer;
		[HideInInspector] public     Animator       animator;

		public static UObject GetUObject(string id) {
			return IDTable[id];
		}

		public static List<UObject> GetUObjects(string category) {
			return CategoryTable[category];
		}

		public static bool UObjectExists(string id) {
			return IDTable.ContainsKey(id) && IDTable[id];
		}

		public static bool UObjectsExist(string category) {
			return CategoryTable.ContainsKey(category) && IDTable[category];
		}

		// IEventAgent Method //
		public void SendEvent(Events_EventType type, int layer, EventData data = default) {
			EventManager.instance.AddEvent(new Events_Event(type, this, data), layer);
		}

		public virtual void OnEvent(Events_Event e) { }

		// IStoppable Method //
		public void Stop() {
			if (stopped) return;
			stopped = true;
			
			if (rigidbody2D) {
				oldLinearVelocity = rigidbody2D.linearVelocity;
				oldBodyType       = rigidbody2D.bodyType;
				
				rigidbody2D.linearVelocity = Vector2.zero;
				rigidbody2D.bodyType       = RigidbodyType2D.Kinematic;
			}
			
			if (animator) {
				oldAnimatorSpeed = animator.speed;
				animator.speed   = 0;
				animator.enabled = false;
			}
		}

		public void Resume() {
			if (!stopped) return;
			stopped = false;
			
			if (rigidbody2D) {
				rigidbody2D.linearVelocity = oldLinearVelocity;
				rigidbody2D.bodyType       = oldBodyType;
			}

			if (animator) {
				animator.speed   = oldAnimatorSpeed;
				animator.enabled = true;
			}
		}

		public virtual void OnFirstGet() {}

		public DelayedAction spawnFXCache = null;
		public Task          GetTask;
		
		public virtual void Get(float time) {
			if (time == 0) {
				OnGet();
				Initialize();
				return;
			}
			
			GameObject spawnFX = UObjectPool.instance.Get("SpawnEffectHelper", transform.position);
			spawnFX.GetComponent<SpawnEffectHelper>().t_s = time;
			
			PrepareSpawnFX();
			Coroutine spawnCoroutine = StartCoroutine(SpawnFX(time));
			
			spawnFXCache = new DelayedAction(time,
											 () => {
												 FinishSpawnFX();
												 spawnFXCache = null;
											 },
											 () => {
												 StopCoroutine(spawnCoroutine);
												 FinishSpawnFX();
												 spawnFXCache = null;
											 });

			spawnFXCache.Execute();
			OnGet();
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

		protected Color colorBeforeSpawnFX;
		protected virtual void PrepareSpawnFX() {
			Stop();
			
			foreach (Collider2D c in GetComponents<Collider2D>()) {
				c.enabled = false;
			}
			
			spriteRenderer.sprite = whiteSpawnSprite??spriteRenderer.sprite;
			colorBeforeSpawnFX    = spriteRenderer.color;
		}
		
		protected virtual IEnumerator SpawnFX(float duration) {
			
			float elapsed    = 0f;
			
			while (elapsed < duration) {
				elapsed += Time.deltaTime;
				float t = elapsed / duration;

				Color color = new (Mathf.Lerp(GameManager.instance.spawnColor.r, 1, t),
								   Mathf.Lerp(GameManager.instance.spawnColor.g, 1, t),
								   Mathf.Lerp(GameManager.instance.spawnColor.b, 1, t),
								   Mathf.Lerp(0,                                 1, t));

				spriteRenderer.color = color;

				yield return null;
			}
		}

		protected virtual void FinishSpawnFX() {
			Debug.Log("Finish spawning FX");
			
			spriteRenderer.sprite = colorSpawnSprite??spriteRenderer.sprite;
			spriteRenderer.color  = colorBeforeSpawnFX;

			foreach (Collider2D c in GetComponents<Collider2D>()) {
				c.enabled = true;
			}

			Resume();
			Initialize();
		}

		protected Color colorBeforeDespawnFX;
		protected virtual void PrepareDespawnFX() {
			Stop();
			
			foreach (Collider2D c in GetComponents<Collider2D>()) {
				c.enabled = false;
			}
			
			colorBeforeDespawnFX   = spriteRenderer.color;
		}
		
		protected virtual IEnumerator DespawnFX(float duration) {
			float elapsed    = 0f;

			spriteRenderer.sprite = whiteSpawnSprite??spriteRenderer.sprite;
			
			while (elapsed < duration) {
				elapsed += Time.deltaTime;
				float t = elapsed / duration;

				Color color = new (GameManager.instance.spawnColor.r,
								   GameManager.instance.spawnColor.g,
								   GameManager.instance.spawnColor.b,
								   Mathf.Lerp(1, 0, t));

				spriteRenderer.color = color;

				yield return null;
			}

			Resume();
			spriteRenderer.color = colorBeforeDespawnFX;
			UObjectPool.instance.Release(gameObject, -1);
		}

		public virtual void OnGet() {
			GetTask.Execute(this, this);
		}

		public virtual void OnRelease() {
			ReleaseTask.Execute(this, this);
			currentExclusiveAction?.Cancel();
		}
		
		public abstract void Initialize();
		public abstract void Uninitialize();
	}
}