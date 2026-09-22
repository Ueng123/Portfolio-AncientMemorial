using System;
using System.Collections;
using System.Collections.Generic;
using AncientMemorial.Objects;
using AncientMemorial.Projectiles;
using AncientMemorial.States;
using UengSystem;
using UengSystem.Events;
using UengSystem.ObjectPool;
using UengSystem.Objects;
using UengSystem.Objects.LifeCycle;
using System.Linq;
using UengSystem.Managers;
using UengSystem.States;
using UengSystem.UDebug;
using UengSystem.UI;
using UengSystem.UI.USliders;
using UengSystem.UI.UTexts;
using UengSystem.Utility;
using UengSystem.VisualScripting.UValues;
using UengSystem.VisualScripting.UValues.UFloats;
using UengSystem.VisualScripting.UValues.UObjects;
using UengSystem.VisualScripting.UValues.UStrings;
// using UengSystem.VisualScripting.UValues.UFloats;
// using UengSystem.VisualScripting.UValues.UObjects;
// using UengSystem.VisualScripting.UValues.UStrings;
using UnityEngine;
using Event = UengSystem.Events.Event;
using EventType = UengSystem.Events.EventType;
using Random = UnityEngine.Random;

namespace AncientMemorial.Entities {
	public abstract class Entity : UObject {

		// 정적 프로퍼티
		private static readonly int ENTITY_UI = "EntityUI".GetHash();
		private static readonly int ATTACK_AWARE = "AttackAware".GetHash();
		private static readonly int HEAL_UI = "HealUI".GetHash();
		private static readonly int DAMAGE_UI = "DamageUI".GetHash();
		public static           Player               player;
		public static           SyncList<Entity> entities = new SyncList<Entity>();
		protected static readonly int                Falling  = Animator.StringToHash("falling");

		// 인스턴스 프로퍼티
		public EntityType entityType;
		public EntityData data;
		public EntityStat stat;
		public bool       Friendly;

		public UUI     entityUI;
		private long EntityUiLife;
		private readonly Dictionary<AttackArea, long> AttackAreas = new();
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
		
		

		private bool HitSubscribed;
		private bool EntityRegistered;
		public override float usingReleasingDuration => 2;
		
		private readonly StopWatch debrisCheckTimer = new StopWatch();

		private   bool forceInvincible;
		protected bool invincible => invincibleTimer.CheckIn(invincibleTime) || forceInvincible;

		private readonly StopWatch invincibleTimer = new StopWatch();
		private          float     invincibleTime;

		private Action<Event> hitEvent;
		
		private bool hasDied;

		private UState _entityState;
		public UState entityState {
			get => _entityState;
			set {
				if (_entityState == value) return;
				
				DebugManager.Log($"[State Changed] {name} : {_entityState?.GetType()} -> {value?.GetType()}");
				
				_entityState?.Exit();
				_entityState = value;
				value?.Enter();
			}
		}

		// 정적 메서드
		public static AttackArea AttackArea(Entity attacker, float damageMult, float delay, Vector2 hitboxPos, Vector2 hitboxSize, float angle = 0, int maxTargetNum = -1, bool awareLerpX = true, bool awareLerpY = false, bool ignoreInvincible = false) {
			return attacker.AttackArea(damageMult, delay, hitboxPos, hitboxSize, angle, maxTargetNum, awareLerpX, awareLerpY, ignoreInvincible);
		}
		
		public static AttackArea AttackAreaNoEffect(Entity attacker, float damageMult, float delay, Vector2 hitboxPos, Vector2 hitboxSize, float angle = 0, int maxTargetNum = -1, bool ignoreInvincible = false) {
			return attacker.AttackAreaNoEffect(damageMult, delay, hitboxPos, hitboxSize, angle, maxTargetNum, ignoreInvincible);
		}

		public static void SendAttackMultiplyEvent(Entity attacker, Entity target, float damageMult, bool ignoreInvincible, bool useProcess = true) {
			HitData hitData = new (
				attacker,
				null,
				target,
				attacker.stat.attackDamage * damageMult,
				new Vector2(target.transform.position.x - attacker.transform.position.x, 0).normalized,
				ignoreInvincible
			);
					
			DebugManager.Log($"[HIT EVENT] SendingEvent : {target}");
			
			if (useProcess) {
				GlobalObject.instance.AddProcessToUpdate(()=> { attacker.SendEvent(EventType.Entity_Hit, EventPriority.Hit, hitData); });
			}
			else {
				attacker.SendEvent(EventType.Entity_Hit, EventPriority.Hit, hitData);
			}
		}
		
		public static void SendAttackEvent(Entity attacker, Entity target, float damage, bool ignoreInvincible, bool useProcess = true) {
			HitData hitData = new (
				attacker,
				null,
				target,
				damage,
				new Vector2(target.transform.position.x - attacker.transform.position.x, 0).normalized,
				ignoreInvincible
			);
					
			DebugManager.Log($"[HIT EVENT] SendingEvent : {target}");
			
			if (useProcess) {
				attacker.AddProcessToUpdate(()=> { attacker.SendEvent(EventType.Entity_Hit, EventPriority.Hit, hitData); });
			}
			else {
				attacker.SendEvent(EventType.Entity_Hit, EventPriority.Hit, hitData);
			}
		}

		// 인스턴스 메서드
		public void Attatch(AttatchObject target) {
			debrisAttached.Add(target.GetComponent<AttatchObject>());
			target.transform.SetParent(transform);
		}
		
		protected virtual void OpenEntityUI() {
			entityUI = UUI.Get(ENTITY_UI, entityUICanvas, Configure: Ui => {
				
				Ui.rectTransform.anchoredPosition = new Vector3(0, entityUIHeight, 0);
				
				UTextAction   entityTextAction = Ui.GetAction<UTextAction>("EntityName");
				USliderAction entityHPAction   = Ui.GetAction<USliderAction>("EntityHP");
				
				entityTextAction.text = new UPureString {pureValue = data.name};
				
				entityHPAction.component.GetComponent<RectTransform>().sizeDelta = new Vector2(100*Mathf.Log(data.HP, 2) , 30);
				entityHPAction.value = new UDiv {
					dynamicType = DynamicType.Dynamic,
					A = new UEntityStat {
						dynamicType  = DynamicType.Dynamic,
						targetEntity = new UEntityByUObject { obj = new UPureObject { pureValue = this } },
						type         = EntityDataType.HP
					},
					B = new UEntityStat {
						dynamicType  = DynamicType.Dynamic,
						targetEntity = new UEntityByUObject { obj = new UPureObject { pureValue = this } },
						type         = EntityDataType.MAXHP
					},
				};
				
			}).GetComponent<UUI>();
			
			EntityUiLife = entityUI.lifeNumber;
		}

		protected void ShowDamageUI(float damage) {
			if (damage == 0) return;
			
			bool isHeal = damage < 0;
			damage = (isHeal ? -damage : damage);

			float UIScale = 1 + Mathf.Log(damage, 100);
			
			UUI.Get(isHeal ? HEAL_UI : DAMAGE_UI, GameManager.instance.mainWorldCanvas, Configure: DamageUi => {
			DamageUi.rectTransform.anchoredPosition = (transform.position + new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f), 0))*80;
			DamageUi.rectTransform.localScale = Vector3.one*UIScale;
			UTextAction textAction  = DamageUi.GetAction<UTextAction>("DamageDisplay");
			UTextAction textSAction = DamageUi.GetAction<UTextAction>("DamageDisplayShadow");
			
			string healSign = isHeal ? "+" : "";
			
			textAction.text = textSAction.text = new UPureString {pureValue = $"{Mathf.Floor(damage*100)/100f}<size=10><i> {healSign}</i></size>"};
			});
		}
		
		protected void ShowCriticalDamageUI(float damage) {
			if (damage == 0) return;
			
			bool isHeal = damage < 0;
			damage = (isHeal ? -damage : damage);
			
			float UIScale = (1 + Mathf.Log(damage, 100))*1.2f;
			
			UUI.Get(isHeal ? HEAL_UI : DAMAGE_UI, GameManager.instance.mainWorldCanvas, Configure: DamageUi => {
			DamageUi.rectTransform.anchoredPosition = (transform.position + new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f), 0))*80;
			DamageUi.rectTransform.localScale = Vector3.one*UIScale;
			UTextAction textAction  = DamageUi.GetAction<UTextAction>("DamageDisplay");
			UTextAction textSAction = DamageUi.GetAction<UTextAction>("DamageDisplayShadow");
			
			string healSign = isHeal ? "+" : "";
			
			textAction.text = textSAction.text = new UPureString {pureValue = $"{Mathf.Floor(damage*100)/100f}<size=10><i> {healSign}!!</i></size>"};
			});
		}
		
		protected virtual AttackArea AttackArea(float damageMult, float delay, Vector2 hitboxPos, Vector2 hitboxSize, float angle = 0, int maxTargetNum = -1, bool awareLerpX = true, bool awareLerpY = false, bool ignoreInvincible = false) {
			GameObject awareObject = Get(ATTACK_AWARE, hitboxPos, PlayEffect: false, Configure: Obj => {
			AttackArea attackArea = (AttackArea)Obj;
			attackArea.InitializeColor(new Color(0.509434f, 0.1850303f, 0.1850303f, 0));
			
			attackArea.targetColor         = new Color(0.9433962f, 0.244749f,  0.244749f,  0.6156863f);
			attackArea.targetSize          = new Vector2(hitboxSize.x,      hitboxSize.y);
			attackArea.transform.rotation  = Quaternion.Euler(0, 0, angle);
			attackArea.spriteRenderer.size = hitboxSize;
			attackArea.damageMult          = damageMult;
			attackArea.attacker            = this;
			attackArea.maxTargetNum        = maxTargetNum;
			attackArea.ignoreInvincible    = ignoreInvincible;
			attackArea.duration            = delay;
			attackArea.lerpX               = awareLerpX;
			attackArea.lerpY               = awareLerpY;
			attackArea.invisible           = false;
			});

			return awareObject.GetComponent<AttackArea>();
		}
		
		protected AttackArea AttackAreaNoEffect(float damageMult, float delay, Vector2 hitboxPos, Vector2 hitboxSize, float angle = 0, int maxTargetNum = -1, bool ignoreInvincible = false) {
			GameObject awareObject = UObject.Get(ATTACK_AWARE, hitboxPos, PlayEffect: false, Configure: Obj => {
			AttackArea attackArea = (AttackArea)Obj;
			attackArea.targetSize          = new Vector2(hitboxSize.x, hitboxSize.y);
			attackArea.transform.rotation  = Quaternion.Euler(0, 0, angle);
			attackArea.spriteRenderer.size = hitboxSize;
			attackArea.damageMult          = damageMult;
			attackArea.attacker            = this;
			attackArea.maxTargetNum        = maxTargetNum;
			attackArea.ignoreInvincible    = ignoreInvincible;
			attackArea.duration            = delay;
			attackArea.invisible           = true;
			});
			
			return awareObject.GetComponent<AttackArea>();
		}

		public void RegisterAttackArea(AttackArea Area) => AttackAreas[Area] = Area.lifeNumber;
		public void UnregisterAttackArea(AttackArea Area) => AttackAreas.Remove(Area);
		
		public void SendAttackMultiplyEvent(Entity target, float damageMult, bool ignoreInvincible) {
			SendAttackMultiplyEvent(this, target, damageMult, ignoreInvincible);
		}
		
		public void SendAttackEvent(Entity target, float damage, bool ignoreInvincible, bool useProcess = true) {
			SendAttackEvent(this, target, damage, ignoreInvincible, useProcess);
		}
		
		public abstract bool isAttackTarget(Entity entity);
		
		protected virtual void Death() {
			if (entityUI && entityUI.Matches(EntityUiLife) && entityUI.isActive)
				entityUI.GetAction<USliderAction>("EntityHP").GetComponent<USlider>().SetValue(0);
			Release(PlayEffect: true);
		}

		protected abstract void OnGrounded();

		protected virtual EntityData GetData() {
			return new EntityData(entityType);
		}

		protected void CheckDebris() {
			for (int i = debrisAttached.Count - 1; i >= 0; i--) {
				AttatchObject debris = debrisAttached[i];
				if (!debris.isReleased) continue;
				debrisAttached.RemoveAt(i);
			}
		}
		public void Invincible(float time) {
			float timeLeft = invincible ? invincibleTime - invincibleTimer.TryTock(0) : 0;
			if (timeLeft > time) return;
			
			invincibleTimer.Tick();
			invincibleTime = time;
		}
		public void Invincible(bool target) {
			forceInvincible = target;
		}

		public virtual void ChangeHP(float amount) {
			stat.HP = Mathf.Clamp(stat.HP + amount, 0, data.HP);
		}
		public abstract void OnHit(Entity attacker, float damage, Vector2? pushDir = null);
		protected virtual void OnHitFromEntity(Entity     attacker,   float    damage, Vector2? pushDir) { }
		protected virtual void OnHitFromProjectile(Projectile projectile, float    damage, Vector2? pushDir) { }
		protected abstract void OnHeal(float amount);
		protected abstract float GetRealDamage(float rawDamage);
		private void HitEvent(Event e) {
			if (!isActive || !e.isValid || e.data is not HitData { isValid: true } hitData) return;
			if (hitData.reciever != this) return;
			if (invincible && !hitData.ignoreInvincible) return;
					
			if (this == player) DebugManager.Log("[HIT EVENT] Player Hit!");
			
			float realDamage = GetRealDamage(hitData.damage);
			ChangeHP(-realDamage);
					
			if (realDamage >= 0) {
				if (hitData.attackedEntity) { OnHitFromEntity(hitData.attackedEntity,         realDamage, hitData.pushDir); }
				if (hitData.attackedProjectile) { OnHitFromProjectile(hitData.attackedProjectile, realDamage, hitData.pushDir); }
                        					
				OnHit(hitData.attacker, realDamage, hitData.pushDir);
				hitData.onHit?.Invoke();
			}
			else OnHeal(-realDamage);
		}

		// 오버라이드 메서드
		public override void OnFirstGet() {
			SetDefaultStates(ReleasingState: new EntityReleasing(this));
			base.OnFirstGet();
		}

		public override void Initialize() {
			hasDied = false;
			base.Initialize();
			entities.Add(this);
			EntityRegistered = true;
			
			Invincible(data.invincibleTime);

			hitEvent ??= HitEvent;
			EventType.Entity_Hit.AddListener(hitEvent);
			HitSubscribed = true;
			
			if (groundBoxSize == Vector2.zero) return;
			Collider2D groundCheckTrigger = groundChecker.GetComponent<Collider2D>();
			groundCheckTrigger.transform.localPosition = groundBoxOffset;
			groundCheckTrigger.transform.localScale    = groundBoxSize;
			
			isGround = groundChecker.isThereStandable;
		}

		public override void Uninitialize() {
			stat = default;
			data = default;
			
			base.Uninitialize();
		}
		
		public override void OnGet() {
			data = GetData();

			stat = new EntityStat(
				data.HP,
				data.moveSpeed,
				data.jumpPower,
				data.attackSpeed,
				data.attackDamage
			);
			
			groundBoxOffset = new Vector2(
				data.groundBoxOffsetX,
				data.groundBoxOffsetY
			);

			groundBoxSize = new Vector2(
				data.groundBoxSizeX,
				data.groundBoxSizeY
			);
			
			OpenEntityUI();
		}
		
		protected override void OnRelease() {
			entityState = null;
			foreach (var Entry in AttackAreas.ToArray()) {
				if (Entry.Key && Entry.Key.Matches(Entry.Value)) Entry.Key.Release(false);
			}
			AttackAreas.Clear();
			
			if (HitSubscribed) { EventType.Entity_Hit.RemoveListener(hitEvent); HitSubscribed = false; }
			
			if (entityUI && entityUI.Matches(EntityUiLife)) entityUI.Release(PlayEffect: true);
			entityUI = null;
			
			foreach (AttatchObject debris in debrisAttached) {
				debris.transform.SetParent(null);
				if (debris.isReleased) continue;
				debris.Release(PlayEffect: false);
			}

			debrisAttached.Clear();
			if (EntityRegistered) { entities.Remove(this); EntityRegistered = false; }
			
			Invincible(false);
			isGround = false;
			base.OnRelease();
		}
		protected override void EarlyRoutine() {
			entityState?.OnEarlyRoutine();
			
			if (debrisCheckTimer.CheckIn(5f)) return;
			debrisCheckTimer.Tick();
			CheckDebris();
			// Debug.Log($"[Entity Velocity] {curridnum} : {rigidbody2D.linearVelocity}");
		}

		protected override void Routine() {
			entityState?.OnRoutine();
		}
		
		protected override void LateRoutine() {
			spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, (invincible)?0.5f:1f);
			gameObject.layer = invincible?8:7;
			
			if (!hasDied && stat.HP <= 0) {
				hasDied = true;
				entityState = null;
				Death();
			}
			
			entityState?.OnLateRoutine();
		}
		
		
		
		protected override void FixedRoutine() {
			if (groundBoxSize != Vector2.zero) isGround = groundChecker.isThereStandable;
			
			entityState?.OnFixedRoutine();
		}
	}
}
