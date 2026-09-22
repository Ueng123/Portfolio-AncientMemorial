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
using UengSystem.UActions;
using UengSystem.UDebug;
using UengSystem.Utility;
using UengSystem.VisualScripting.Tasks;
using UnityEngine;
using Event = UengSystem.Events.Event;
using EventType = UengSystem.Events.EventType;	

namespace UengSystem.Objects {
	public partial class UObject : MonoBehaviour, IObjectPoolable, IActionable, IEventAgent, ITaskable {

		// 정적 프로퍼티
		private static readonly Dictionary<string, UObject>       IDTable       = new();
		private static readonly Dictionary<string, List<UObject>> CategoryTable = new();

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

		// 인스턴스 메서드
		protected virtual void OnIdChanged(string id) { }

		protected virtual void OnCategoryChanged(string category) { }

		public AudioSource PlaySFX(int   ClipId,    Vector2 position = default, bool  isLocalPosition = true,
								   float volume = 1f, float   pitch    = 1f,      float pan = 0f, float spread = 0f,
								   bool  loop   = false) {
			return AudioManager.instance.PlaySFX(ClipId, transform, position, isLocalPosition, volume, pitch, pan,
												 spread, loop);
		}

		public AudioSource PlaySFX(AudioClip clip,        Vector2 position = default, bool  isLocalPosition = true,
								   float     volume = 1f, float   pitch    = 1f,      float pan = 0f, float spread = 0f,
								   bool      loop   = false) {
			return AudioManager.instance.PlaySFX(clip, transform, position, isLocalPosition, volume, pitch, pan, spread,
												 loop);
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

		public virtual void OnGet() { }
		
		public virtual void Initialize() { }
		
		protected virtual void OnRelease() { }
		
		public virtual void Uninitialize() { }
	}
}
