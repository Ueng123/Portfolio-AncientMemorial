using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AncientMemorial.Objects;
using AncientMemorial.Projectiles;
using Newtonsoft.Json;
using UengSystem.Events;
using UengSystem.Logic.UValues;
using UengSystem.Logic.UValues.UFloats;
using UengSystem.Logic.UValues.UObjects;
using UengSystem.Logic.UValues.UStrings;
using UengSystem.ObjectPool;
using UengSystem.Objects;
using UengSystem.UI;
using UengSystem.UI.USliders;
using UengSystem.UI.UTexts;
using UengSystem.Utility;
using Unity.VisualScripting;
using UnityEngine;
using Events_Event = UengSystem.Events.Event;
using EventType = UengSystem.Events.EventType;

namespace AncientMemorial.Entities {
	public abstract class Entity : UObject {

		public static           Player               player;
		public static           BufferedList<Entity> entities = new BufferedList<Entity>();
		protected static readonly int                Falling  = Animator.StringToHash("falling");

		public EntityType entityType;
		public EntityData entityData;
		public EntityStat entityStat;
		public bool       Friendly;

		public UUI     entityUI;
		public UCanvas entityUICanvas;
		public int     entityUIHeight;

		public EntityGroundChecker groundChecker;
		protected StopWatch  unGroundedTimer = new ();
		private   bool       _isGround;
		public bool isGround {
			get => _isGround;
			set {
				if (_isGround == value) return;
				
				_isGround = value;
				animator.SetBool(Falling, !value);
				
				if (value) {
					OnGrounded();
				}
				else {
					unGroundedTimer.Tick();
				}
			}
		}

		private Vector2 groundBoxOffset;
		private Vector2 groundBoxSize;
		
		public List<AttatchObject> debrisAttached = new List<AttatchObject>();
		
		// Static Methods //
		public static DelayedAction AttackArea(Entity attacker, float damageMult, float delay, Vector2 hitboxPos, Vector2 hitboxSize, float angle = 0, int maxTargetNum = -1, bool awareLerpX = true, bool awareLerpY = false, bool ignoreInvincible = false) {
			GameObject  awareObject = UObjectPool.instance.Get("AttackAware", hitboxPos);
			AttackAware attackAware = awareObject.GetComponent<AttackAware>();
			attackAware.targetSize          = new Vector2(hitboxSize.x,      hitboxSize.y);
			attackAware.transform.rotation  = Quaternion.Euler(0, 0, angle);
			attackAware.spriteRenderer.size = hitboxSize;
			attackAware.duration            = delay;
			attackAware.lerpX               = awareLerpX;
			attackAware.lerpY               = awareLerpY;
			
			if (damageMult == 0) {
				return new DelayedAction(
					delay,
					() => UObjectPool.instance.Release(awareObject, 0.1f),
					() => UObjectPool.instance.Release(awareObject, 0.1f),
					attacker).ExecuteDA();
			}
			
			return new DelayedAction(
				delay, () => {
					UObjectPool.instance.Release(awareObject, 0.1f);
					Collider2D[] hitColliders = Physics2D.OverlapBoxAll(hitboxPos, hitboxSize, angle);
					foreach (Collider2D hit in hitColliders) {
						if (!hit.CompareTag("Entity")) continue;
						Entity entity     = hit.GetComponent<Entity>();
						
						if (!attacker.isAttackTarget(entity)) continue;

						SendAttackEvent(attacker, entity, damageMult, ignoreInvincible);
						
						if (--maxTargetNum == 0) return;
					}
				}, () => UObjectPool.instance.Release(awareObject, 0.1f), attacker).ExecuteDA();
		}
		
		public static DelayedAction AttackAreaNoEffect(Entity attacker, float damageMult, float delay, Vector2 hitboxPos, Vector2 hitboxSize, float angle = 0, int maxTargetNum = -1, bool ignoreInvincible = false) {
			return new DelayedAction(delay, () => {
				Collider2D[] hitColliders = Physics2D.OverlapBoxAll(hitboxPos, hitboxSize, angle);
				foreach (Collider2D hit in hitColliders) {
					if (!hit.CompareTag("Entity")) continue;
					Entity entity = hit.GetComponent<Entity>();
					
					if (!attacker.isAttackTarget(entity)) continue;

					SendAttackEvent(attacker, entity, damageMult, ignoreInvincible);
					
					if (--maxTargetNum == 0) return;
				}
			}, () => { }, attacker).ExecuteDA();
		}
		
		// Instance Methods //
		protected DelayedAction AttackArea(float damageMult, float delay, Vector2 hitboxPos, Vector2 hitboxSize, float angle = 0, int maxTargetNum = -1, bool awareLerpX = true, bool awareLerpY = false, bool ignoreInvincible = false) {
			return AttackArea(this, damageMult, delay, hitboxPos, hitboxSize, angle, maxTargetNum, awareLerpX, awareLerpY, ignoreInvincible);
		}
		
		protected DelayedAction AttackAreaNoEffect(float damageMult, float delay, Vector2 hitboxPos, Vector2 hitboxSize, float angle = 0, int maxTargetNum = -1, bool ignoreInvincible = false) {
			return AttackAreaNoEffect(this, damageMult, delay, hitboxPos, hitboxSize, angle, maxTargetNum, ignoreInvincible);
		}

		public static void SendAttackEvent(Entity attacker, Entity target, float damageMult, bool ignoreInvincible, bool useProcess = true) {
			EntityHitData hitData = new (
				attacker,
				null,
				target,
				attacker.entityStat.attackDamage * damageMult,
				new Vector2(target.transform.position.x - attacker.transform.position.x, 0).normalized,
				ignoreInvincible
			);
					
			Debug.Log($"[HIT EVENT] SendingEvent : {target}");
			
			if (useProcess) {
				attacker.AddProcessToUpdate(()=> { attacker.SendEvent(EventType.Entity_Behaviour_Hit, 10, hitData); });
			}
			else {
				attacker.SendEvent(EventType.Entity_Behaviour_Hit, 10, hitData);
			}
		}
		
		public void SendAttackEvent(Entity target, float damageMult, bool ignoreInvincible) {
			SendAttackEvent(this, target, damageMult, ignoreInvincible);
		}
		
		public abstract bool isAttackTarget(Entity entity);
		
		protected virtual void Death() {
			float closeTime = UUIObjectPool.instance.prefabData.FirstOrDefault((data) => data.prefab.name == entityUI.name)!.closeTime;
			UObjectPool.instance.Release(gameObject, closeTime+0.1f);
			if (instances.GetList().Contains(entityUI)) ((USlider)entityUI.GetAction<USliderAction>("EntityHP").component).SetValue(0);
			else Destroy(entityUI.gameObject);
		}

		protected abstract void OnGrounded();
		
		// ETC. Override //

		public override void Initialize() {
			base.Initialize();
			
			Invincible(entityData.invincibleTime);
			
			if (groundBoxSize == Vector2.zero) return;
			Collider2D groundCheckTrigger = groundChecker.GetComponent<Collider2D>();
			groundCheckTrigger.transform.localPosition = groundBoxOffset;
			groundCheckTrigger.transform.localScale    = groundBoxSize;
		}

		public override void Uninitialize() {
			entityStat = null;
			entityData = default;
			
			base.Uninitialize();
		}

		protected virtual EntityData GetData() {
			return new EntityData(entityType);
		}
		
		public override void OnGet() {
			entityData = GetData();
			
			entityStat      = new EntityStat(
				entityData.hp,
				entityData.moveSpeed,
				entityData.jumpPower,
				entityData.attackSpeed,
				entityData.attackDamage
			);
			
			groundBoxOffset = new Vector2(
				entityData.groundBoxOffsetX,
				entityData.groundBoxOffsetY
			);
			
			groundBoxSize   = new Vector2(
				entityData.groundBoxSizeX,
				entityData.groundBoxSizeY
			);
			
			entityUI = UUIObjectPool.instance.Open("EntityUI", entityUICanvas).GetComponent<UUI>();
			entityUI.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, entityUIHeight, 0);
			
			UTextAction   entityTextAction = entityUI.GetAction<UTextAction>("EntityName");
			USliderAction entityHPAction   = entityUI.GetAction<USliderAction>("EntityHP");
			
			entityTextAction.text = new UPureString {Text = entityData.name};
			entityTextAction.Initialize(entityUI);
			
			entityHPAction.component.GetComponent<RectTransform>().sizeDelta = new Vector2(100*Mathf.Log(entityData.hp, 2) , 30);
			entityHPAction.value = new UDiv {
				dynamicType = DynamicType.Dynamic,
				A = new UEntityStat {
					dynamicType = DynamicType.Dynamic,
					targetEntity = new UEntityByUObject {
						obj = new UObjectSerialized {
							obj = this
						}
					},
					type = EntityDataType.HP
				},
				B = new UEntityStat {
					dynamicType = DynamicType.Dynamic,
					targetEntity = new UEntityByUObject {
						obj = new UObjectSerialized {
							obj = this
						}
					},
					type = EntityDataType.MAXHP
				},
			};
			
			base.OnGet();
			
			entities.Add(this);
			
			Debug.Log($"[OnGet] velocity = {rigidbody2D.linearVelocity}");
			Debug.Log($"[OnGet] isGround = {isGround}");
		}

		protected override void OnRelease() {
			UUIObjectPool.instance.Close(entityUI.gameObject);
			
			foreach (AttatchObject debris in debrisAttached) {
				Debug.Log("deleting debris!!");
				debris.transform.SetParent(null);
				if (debris.isReleased) continue;
				UObjectPool.instance.Release(debris.gameObject);
			}

			debrisAttached.Clear();
			
			entities.Remove(this);
			isGround = false;
			base.OnRelease();
		}
		
		protected override IEnumerator DespawnFX(float duration) {
			spriteRenderer.color = new Color(
				spriteRenderer.color.r,
				spriteRenderer.color.g,
				spriteRenderer.color.b,
				0);
			
			yield return new WaitForSeconds(duration);
			
			UObjectPool.instance.Release(gameObject, -1);
			spriteRenderer.color = new Color(
				spriteRenderer.color.r,
				spriteRenderer.color.g,
				spriteRenderer.color.b,
				1);
		}

		private static int  idnum = 0;
		private        int  curridnum;
		private        bool id = false;
		protected override void EarlyRoutine() {
			if (!id) {
				curridnum = idnum++;
				id = true;
			}
			
			// Debug.Log($"[Entity Velocity] {curridnum} : {rigidbody2D.linearVelocity}");
		}

		private   bool forceInvincible;
		protected bool invincible => invincibleTimer.Check(invincibleTime) || forceInvincible;

		private StopWatch invincibleTimer = new StopWatch();
		private float     invincibleTime;
		protected void Invincible(float time) {
			invincibleTimer.Tick();
			invincibleTime = time;
		}
		
		protected void ForceInvincibleTrue(bool target) {
			forceInvincible = target;
		}
		
		public abstract void HitEffect(Entity attacker, float damage, Vector2? pushDir = null);

		protected abstract void OnHit(Entity     attacker,   float    damage, Vector2? pushDir);
		protected abstract void OnHit(Projectile projectile, float    damage, Vector2? pushDir);
		protected abstract void OnHit(float      damage,     Vector2? pushDir);

		protected abstract float GetRealDamage(float rawDamage);
		
		public override void EventRoutine(Events_Event e) {
			base.EventRoutine(e);
			switch (e.type) {
				case EventType.Entity_Behaviour_Hit:
					EntityHitData hitData = (EntityHitData)e.data;
					if (invincible && !hitData.ignoreInvincible) return;
					
					if (hitData.reciever != this) return;
					
					float realDamage = GetRealDamage(hitData.damage);
					
					if (hitData.attackedEntity) { OnHit(hitData.attackedEntity,         realDamage, hitData.pushDir); }
					if (hitData.attackedProjectile) { OnHit(hitData.attackedProjectile, realDamage, hitData.pushDir); }
					if (!hitData.attackedEntity && !hitData.attackedProjectile) { OnHit(realDamage, hitData.pushDir); }
					
					HitEffect(hitData.attackedEntity??hitData.attackedProjectile?.owner,
							  realDamage,
							  hitData.pushDir);

					if (!invincible && entityStat.hp > 0) Invincible(0.05f);
					break;
			}
		}
		
		protected override void LateRoutine() {
			spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, (invincible)?0.5f:1f);
			gameObject.layer = invincible?8:7;
			
			if (entityStat.hp <= 0) {
				Death();
			}
		}
		
		// FIXED UPDATE ROUTINE //
		
		protected override void FixedRoutine() {
			if (groundBoxSize == Vector2.zero) return;
			isGround = groundChecker.isThereStandable;
		}
	}
}