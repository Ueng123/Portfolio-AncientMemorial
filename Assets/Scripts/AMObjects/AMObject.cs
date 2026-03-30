
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Collections;
using System.Linq;
using AncientMemorial.Events;
using AncientMemorial.Events.EventDatas;
using AncientMemorial.ObjectPool;
using AncientMemorial.Tasks;
using UnityEditor.Searcher;
using UnityEngine.Serialization;
using Event = AncientMemorial.Events.Event;
using EventType = AncientMemorial.Events.EventType;

namespace AncientMemorial.AMObjects {
	// ㅅ사실상 MonoBehaviour 쓸꺼면 이거 쓰는게 3대 대대로 좋음
	public abstract class AMObject : MonoBehaviour, IObjectPoolable, IEventAgent, IStoppable, IInitializable, ITaskable {

		private static Dictionary<string,      AMObject > IDTable       = new ();
		private static Dictionary<string, List<AMObject>> CategoryTable = new ();
		
		// Instance Variables //
		[Header("Identify")] 
		private string _ID;
		private string _Category;

		public string ID {
			get => _ID;
			set {
				if (value == null) {
					if (_ID != null) {
						IDTable[_ID] = null;
                        _ID = null;
					}
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
					if (_Category != null) {
						CategoryTable[_Category].Remove(this);
	                    if (CategoryTable[_Category].Count == 0) CategoryTable.Remove(_Category);
	                    
	                    _Category = null;
					}
					
					return;
				}
				
				if (_Category != null) throw new InvalidOperationException("u just tried to set Category twice twin :(");
				if (!CategoryTable.ContainsKey(value)) CategoryTable[value] = new List<AMObject>();
				
				CategoryTable[value].Add(this);
				_Category = value;
			}
		}

		[Header("Get/Release FX")]
		public Sprite       whiteSpawnSprite;
		public Sprite       colorSpawnSprite;
		
		// IStoppable Variables //
		public  bool            stopped { get; set; }
		private Vector2         oldLinearVelocity;
		private RigidbodyType2D oldBodyType;
		private float           oldAnimatorSpeed;
		
		// Components //
		[HideInInspector] public new Rigidbody2D    rigidbody2D;
		[HideInInspector] public     SpriteRenderer spriteRenderer;
		[HideInInspector] public     Animator       animator;

		public static AMObject GetAMObject(string id) {
			return IDTable[id];
		}
		
		public static List<AMObject> GetAMObjects(string category) {
			return CategoryTable[category];
		}
		
		// IEventAgent Method //
		public void SendEvent(EventType type, EventData data = default) {
			EventManager.instance.AddEvent(new Event(type, this, data));
		}

		public virtual void OnEvent(Event e) {}

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
		public virtual void Get(float time) {
			Stop();
			OnGet();
			
			if (time == 0) {
				Initialize();
			}
			else {
				gameObject.SetActive(true);
				StartCoroutine(SpawnFX(time));
			}
		}

		public virtual void Release(float time) {
			Stop();

			switch (time) {
				case < 0:
					Uninitialize();
					break;
				case 0:
					OnRelease();
					Uninitialize();
					ID       = null;
					Category = null;
					break;
				default:
					OnRelease();
					ID       = null;
					Category = null;
					StartCoroutine(ReleaseFX(time));
					break;
			}
		}
		
		public virtual void OnGet() {}
		public virtual void OnRelease() {}

		protected virtual IEnumerator SpawnFX(float duration) {
			foreach (Collider2D c in GetComponents<Collider2D>()) {
				c.enabled = false;
			}
			
			Color spawnColor = GameManager.instance.spawnColor;
			Color oldColor   = spriteRenderer.color;
			float elapsed    = 0f;

			spriteRenderer.sprite = whiteSpawnSprite??spriteRenderer.sprite;
			
			while (elapsed < duration) {
				elapsed += Time.deltaTime;
				float t = elapsed / duration;

				Color color = new (Mathf.Lerp(spawnColor.r, 1, t),
								   Mathf.Lerp(spawnColor.g, 1, t),
								   Mathf.Lerp(spawnColor.b, 1, t),
								   Mathf.Lerp(0, 1, t));

				spriteRenderer.color = color;

				yield return null;
			}
			
			spriteRenderer.sprite = colorSpawnSprite??spriteRenderer.sprite;
			spriteRenderer.color  = oldColor;

			foreach (Collider2D c in GetComponents<Collider2D>()) {
				c.enabled = true;
			}
			
			Initialize();
		}

		public virtual IEnumerator ReleaseFX(float duration) {
			foreach (Collider2D c in GetComponents<Collider2D>()) {
				c.enabled = false;
			}
			
			Color spawnColor = GameManager.instance.spawnColor;
			Color oldColor   = spriteRenderer.color;
			
			float elapsed    = 0f;

			spriteRenderer.sprite = whiteSpawnSprite??spriteRenderer.sprite;
			
			while (elapsed < duration) {
				elapsed += Time.deltaTime;
				float t = elapsed / duration;

				Color color = new (spawnColor.r, spawnColor.g, spawnColor.b, Mathf.Lerp(1, 0, t));

				spriteRenderer.color = color;

				yield return null;
			}

			spriteRenderer.color = oldColor;
			AMObjectPool.instance.Release(gameObject, -1);
		}
		
		public abstract void Initialize();
		public abstract void Uninitialize();
	}
}