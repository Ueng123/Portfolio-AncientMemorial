using System.Collections;
using System.Collections.Generic;
using AncientMemorial.Objects;
using AncientMemorial.Projectiles;
using UengSystem.Events;
using UengSystem.Logic.UValues;
using UengSystem.Logic.UValues.UFloats;
using UengSystem.Logic.UValues.UObjects;
using UengSystem.Logic.UValues.UStrings;
using UengSystem.ObjectPool;
using UengSystem.Objects;
using UengSystem.States;
using UengSystem.UDebug;
using UengSystem.UI;
using UengSystem.UI.USliders;
using UengSystem.UI.UTexts;
using UengSystem.Utility;
using UnityEngine;
using Events_Event = UengSystem.Events.Event;
using EventType = UengSystem.Events.EventType;

namespace AncientMemorial.Entities {
	public abstract class Entity : UObject, IStateObject {

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
				animator.SetBool(Falling, !value); // No Overhead
				
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
			return attacker.AttackArea(damageMult, delay, hitboxPos, hitboxSize, angle, maxTargetNum, awareLerpX, awareLerpY, ignoreInvincible);
		}
		
		public static DelayedAction AttackAreaNoEffect(Entity attacker, float damageMult, float delay, Vector2 hitboxPos, Vector2 hitboxSize, float angle = 0, int maxTargetNum = -1, bool ignoreInvincible = false) {
			return attacker.AttackAreaNoEffect(damageMult, delay, hitboxPos, hitboxSize, angle, maxTargetNum, ignoreInvincible);
		}
		
		// Instance Methods //
		
		public void Attatch(AttatchObject target) {
			debrisAttached.Add(target.GetComponent<AttatchObject>());
			target.transform.SetParent(transform);
		}
		
		protected virtual void OpenEntityUI() {
			entityUI = UUIPool.instance.Open("EntityUI", entityUICanvas).GetComponent<UUI>();
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
		}

		protected void ShowDamageUI(float damage) {
			if (damage == 0) return;
			
			bool isHeal = damage < 0;
			damage = (isHeal ? -damage : damage);

			float UIScale = 1 + Mathf.Log(damage, 100);
			
			UUI damageUI = UUIPool.instance.Open(isHeal?"HealUI":"DamageUI", GameManager.instance.mainWorldCanvas).GetComponent<UUI>();
			damageUI.rectTransform.anchoredPosition = (transform.position + new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f), 0))*80;
			damageUI.rectTransform.localScale = Vector3.one*UIScale;
			UTextAction textAction  = damageUI.GetAction<UTextAction>("DamageDisplay");
			UTextAction textSAction = damageUI.GetAction<UTextAction>("DamageDisplayShadow");
			new DelayedAction(0.5f, () => UUIPool.instance.Close(damageUI.gameObject)).ExecuteDA();
			
			string healSign = isHeal ? "+" : "";
			
			textAction.text = textSAction.text = new UPureString {Text = $"{Mathf.Floor(damage*100)/100f}<size=10><i> {healSign}</i></size>"};
			textAction.Initialize(damageUI);
			textSAction.Initialize(damageUI);
		}
		
		protected void ShowCriticalDamageUI(float damage) {
			if (damage == 0) return;
			
			bool isHeal = damage < 0;
			damage = (isHeal ? -damage : damage);
			
			float UIScale = (1 + Mathf.Log(damage, 100))*1.2f;
			
			UUI damageUI = UUIPool.instance.Open(isHeal?"HealUI":"DamageUI", GameManager.instance.mainWorldCanvas).GetComponent<UUI>();
			damageUI.GetComponent<RectTransform>().anchoredPosition = (transform.position + new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f), 0))*80;
			damageUI.rectTransform.localScale = Vector3.one*UIScale;
			UTextAction textAction  = damageUI.GetAction<UTextAction>("DamageDisplay");
			UTextAction textSAction = damageUI.GetAction<UTextAction>("DamageDisplayShadow");
			new DelayedAction(0.5f, () => UUIPool.instance.Close(damageUI.gameObject)).ExecuteDA();
			
			string healSign = isHeal ? "+" : "";
			
			textAction.text = textSAction.text = new UPureString {Text = $"{Mathf.Floor(damage*100)/100f}<size=10><i> {healSign}!!</i></size>"};
			textAction.Initialize(damageUI);
			textSAction.Initialize(damageUI);
		}
		
		protected virtual DelayedAction AttackArea(float damageMult, float delay, Vector2 hitboxPos, Vector2 hitboxSize, float angle = 0, int maxTargetNum = -1, bool awareLerpX = true, bool awareLerpY = false, bool ignoreInvincible = false) {
			
			GameObject  awareObject = UObjectPool.instance.Get("AttackAware", hitboxPos);
			AttackAware attackAware = awareObject.GetComponent<AttackAware>();
			attackAware.initialColor        = new Color(0.509434f, 0.1850303f, 0.1850303f, 0);
			attackAware.targetColor         = new Color(0.9433962f, 0.244749f,  0.244749f,  0.6156863f);
			attackAware.targetSize          = new Vector2(hitboxSize.x,      hitboxSize.y);
			attackAware.transform.rotation  = Quaternion.Euler(0, 0, angle);
			attackAware.spriteRenderer.size = hitboxSize;
			attackAware.duration            = delay;
			attackAware.lerpX               = awareLerpX;
			attackAware.lerpY               = awareLerpY;
			attackAware.Initialize();
			
			if (damageMult == 0) {
				return new DelayedAction(
					delay,
					() => UObjectPool.instance.Release(awareObject, 0.1f),
					() => UObjectPool.instance.Release(awareObject, 0.1f),
					this).ExecuteDA();
			}
			
			return new DelayedAction(
				delay, () => {
					UObjectPool.instance.Release(awareObject, 0.1f);
					Collider2D[] hitColliders = Physics2D.OverlapBoxAll(hitboxPos, hitboxSize, angle);
					foreach (Collider2D hit in hitColliders) {
						if (!hit.CompareTag("Entity")) continue;
						Entity entity     = hit.GetComponent<Entity>();
						
						if (!isAttackTarget(entity)) continue;

						SendAttackMultiplyEvent(this, entity, damageMult, ignoreInvincible);
						
						if (--maxTargetNum == 0) return;
					}
				}, () => UObjectPool.instance.Release(awareObject, 0.1f), this).ExecuteDA();
		}
		
		protected DelayedAction AttackAreaNoEffect(float damageMult, float delay, Vector2 hitboxPos, Vector2 hitboxSize, float angle = 0, int maxTargetNum = -1, bool ignoreInvincible = false) {
			return new DelayedAction(delay, () => {
				Collider2D[] hitColliders = Physics2D.OverlapBoxAll(hitboxPos, hitboxSize, angle);
				foreach (Collider2D hit in hitColliders) {
					if (!hit.CompareTag("Entity")) continue;
					Entity entity = hit.GetComponent<Entity>();
					
					if (!isAttackTarget(entity)) continue;

					SendAttackMultiplyEvent(this, entity, damageMult, ignoreInvincible);
					
					if (--maxTargetNum == 0) return;
				}
			}, () => { }, this).ExecuteDA();
		}

		public static void SendAttackMultiplyEvent(Entity attacker, Entity target, float damageMult, bool ignoreInvincible, bool useProcess = true) {
			EntityHitData hitData = new (
				attacker,
				null,
				target,
				attacker.entityStat.attackDamage * damageMult,
				new Vector2(target.transform.position.x - attacker.transform.position.x, 0).normalized,
				ignoreInvincible
			);
					
			DebugManager.Log($"[HIT EVENT] SendingEvent : {target}");
			
			if (useProcess) {
				attacker.AddProcessToUpdate(()=> { attacker.SendEvent(EventType.Entity_Behaviour_Hit, (int)EventPriority.Hit, hitData); });
			}
			else {
				attacker.SendEvent(EventType.Entity_Behaviour_Hit, (int)EventPriority.Hit, hitData);
			}
		}
		
		public void SendAttackMultiplyEvent(Entity target, float damageMult, bool ignoreInvincible) {
			SendAttackMultiplyEvent(this, target, damageMult, ignoreInvincible);
		}
		
		public static void SendAttackEvent(Entity attacker, Entity target, float damage, bool ignoreInvincible, bool useProcess = true) {
			EntityHitData hitData = new (
				attacker,
				null,
				target,
				damage,
				new Vector2(target.transform.position.x - attacker.transform.position.x, 0).normalized,
				ignoreInvincible
			);
					
			DebugManager.Log($"[HIT EVENT] SendingEvent : {target}");
			
			if (useProcess) {
				attacker.AddProcessToUpdate(()=> { attacker.SendEvent(EventType.Entity_Behaviour_Hit, (int)EventPriority.Hit, hitData); });
			}
			else {
				attacker.SendEvent(EventType.Entity_Behaviour_Hit, (int)EventPriority.Hit, hitData);
			}
		}
		
		public void SendAttackEvent(Entity target, float damage, bool ignoreInvincible) {
			SendAttackEvent(this, target, damage, ignoreInvincible);
		}
		
		public abstract bool isAttackTarget(Entity entity);
		
		protected virtual void Death() {
			UObjectPool.instance.Release(gameObject, 2f);

			if (!entityUI) return;
			if (instances.GetList().Contains(entityUI)) entityUI.GetAction<USliderAction>("EntityHP").GetComponent<USlider>().SetValue(0);
			else Destroy(entityUI.gameObject);
		}

		protected abstract void OnGrounded();
		
		// ETC. Override //

		public override void Initialize() {
			base.Initialize();
			
			Invincible(entityData.invincibleTime);
			
			EventManager.instance.RegisterEvent(EventType.Entity_Behaviour_Hit, HitEvent);
			
			if (groundBoxSize == Vector2.zero) return;
			Collider2D groundCheckTrigger = groundChecker.GetComponent<Collider2D>();
			groundCheckTrigger.transform.localPosition = groundBoxOffset;
			groundCheckTrigger.transform.localScale    = groundBoxSize;
			
			isGround = groundChecker.isThereStandable;
		}

		public override void Uninitialize() {
			entityStat = default;
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
			
			OpenEntityUI();
			
			entities.Add(this);
			
			DebugManager.Log($"[OnGet] velocity = {rigidbody2D.linearVelocity}");
			DebugManager.Log($"[OnGet] isGround = {isGround}");
		}

		protected void CheckDebris() {
			for (int i = debrisAttached.Count - 1; i >= 0; i--) {
				AttatchObject debris = debrisAttached[i];
				if (!debris.isReleased) continue;
				debrisAttached.RemoveAt(i);
			}
		}
		
		protected override void OnRelease() {
			EventManager.instance.UnregisterEvent(EventType.Entity_Behaviour_Hit, HitEvent);
			
			if (entityUI) UUIPool.instance.Close(entityUI.gameObject);
			
			for (int i = 0; i < debrisAttached.Count; i++) {
				AttatchObject debris = debrisAttached[i];
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
		private        int  currIDNum;
		private        bool id = false;
		private readonly StopWatch debrisCheckTimer = new StopWatch();
		protected override void EarlyRoutine() {
			if (!id) {
				currIDNum = idnum++;
				id = true;
			}

			state?.OnEarlyRoutine();
			
			if (debrisCheckTimer.Check(5f)) return;
			debrisCheckTimer.Tick();
			CheckDebris();
			// Debug.Log($"[Entity Velocity] {curridnum} : {rigidbody2D.linearVelocity}");
		}

		protected override void Routine() {
			state?.OnRoutine();
		}

		private   bool forceInvincible;
		protected bool invincible => invincibleTimer.Check(invincibleTime) || forceInvincible;

		private readonly StopWatch invincibleTimer = new StopWatch();
		private          float     invincibleTime;
		public void Invincible(float time) {
			
			float timeLeft = invincible ? invincibleTime - invincibleTimer.Tock() : 0;
			if (timeLeft > time) return;
			
			invincibleTimer.Tick();
			invincibleTime = time;
		}
		public void ForceInvincibleTrue(bool target) {
			forceInvincible = target;
		}

		public virtual void ChangeHP(float amount) {
			entityStat.hp = Mathf.Clamp(entityStat.hp + amount, 0, entityData.hp);
		}
		public abstract void OnHit(Entity attacker, float damage, Vector2? pushDir = null);
		protected virtual void OnHitFromEntity(Entity     attacker,   float    damage, Vector2? pushDir) { }
		protected virtual void OnHitFromProjectile(Projectile projectile, float    damage, Vector2? pushDir) { }
		protected abstract void OnHeal(float amount);
		protected abstract float GetRealDamage(float rawDamage);
		public void HitEvent(Events_Event e) {
			EntityHitData hitData = (EntityHitData)e.data;
			if (hitData.reciever != this) return;
			if (invincible && !hitData.ignoreInvincible) return;
					
			float realDamage = GetRealDamage(hitData.damage);
			ChangeHP(-realDamage);
					
			if (realDamage >= 0) {
				if (hitData.attackedEntity) { OnHitFromEntity(hitData.attackedEntity,         realDamage, hitData.pushDir); }
				if (hitData.attackedProjectile) { OnHitFromProjectile(hitData.attackedProjectile, realDamage, hitData.pushDir); }
                        					
				OnHit(hitData.attackedEntity??hitData.attackedProjectile?.owner, realDamage, hitData.pushDir);
			}
			else OnHeal(-realDamage);
		}
		
		protected override void LateRoutine() {
			spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, (invincible)?0.5f:1f);
			gameObject.layer = invincible?8:7;
			
			if (entityStat.hp <= 0) {
				state = null;
				Death();
			}
			
			state?.OnLateRoutine();
		}
		
		// FIXED UPDATE ROUTINE //
		
		protected override void FixedRoutine() {
			if (groundBoxSize != Vector2.zero) isGround = groundChecker.isThereStandable;
			
			state?.OnFixedRoutine();
		}

		private UState _state;
		public UState state {
			get => _state;
			set {
				if (_state == value) return;
				
				DebugManager.Log($"[State Changed] {name} : {_state?.GetType()} -> {value?.GetType()}");
				
				_state?.Exit();
				_state = value;
				value?.Enter();
			}
		}
	}
}