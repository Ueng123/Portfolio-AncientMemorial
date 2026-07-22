using System;
using System.Collections;
using System.Linq;
using AncientMemorial.Cameras;
using AncientMemorial.Map;
using AncientMemorial.Objects;
using AncientMemorial.Projectiles;
using UengSystem.Audio;
using UengSystem.Events;
using UengSystem.Logic.UValues.UFloats;
using UengSystem.Logic.UValues.UStrings;
using UengSystem.Managers;
using UengSystem.ObjectPool;
using UengSystem.Objects;
using UengSystem.UI;
using UengSystem.UI.UTexts;
using UengSystem.Utility;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

namespace AncientMemorial.Entities {
	public class SkeletonTank : Skeleton {
		private static readonly     int attacking  = Animator.StringToHash("attacking");
		private static readonly     int attack2ing = Animator.StringToHash("attack2ing");
		private static readonly     int attack2    = Animator.StringToHash("attack2");
		private new static readonly int attack     = Animator.StringToHash("attack");
		private static readonly     int spawn      = Animator.StringToHash("spawn");
		private new static readonly int moving     = Animator.StringToHash("moving");
		private new static readonly int backward   = Animator.StringToHash("backward");
		private new static readonly int Landing    = Animator.StringToHash("landing");

		[NonSerialized] private       float oldAnimSpeed;
		[NonSerialized] private       int   attackAnimation;
		[NonSerialized] private const float attackLength = 3f;
		
		[NonSerialized] private bool unstoppable;
		[NonSerialized] private bool weakness;
		[NonSerialized] private int  groggyAttackLeft = -1;
		[NonSerialized] private bool groggy;
		[NonSerialized] private int  groggySuccess;
		
		public override void OnStunStart() { }

		public override void OnStunEnd() {
			currentExclusiveAction = FindingAggro;
		}
		
		protected override string footstepSoundName => "tankFootStep";
		protected override bool   isFootstep        => spriteRenderer.sprite.name is "SkeletonB_0" or "SkeletonB_2";

		public void GroggyEffect() {
			if (groggySuccess <= 0) return;
			
			unstoppable = false;
			weakness    = true;
			new DelayedAction(0.8F, () => {
				if (groggyAttackLeft == 0) return;
				unstoppable = true;
				weakness    = false;
			}).ExecuteDA();
			UObjectPool.instance.Get("groggyEffect", transform.position);
		}

		[NonSerialized] public int attackPhase = 1;
		public override void Attack() {
			if (!attackable) return;
			attackable = false;
			
			currentExclusiveAction = attackPhase switch {
				1 => Attacking,
				2 => Attacking,
				3 => Attack2ing,
				4 => Spawning,
				_ => throw new Exception("NO BRO THAT'S NOT WHAT I WANT :(")
			};
			
			if (++attackPhase == 5) attackPhase = 1;
		}
		
		private ExclusiveAction Attack2ing => new (Attack2Enumerator(), OnAttackCancel, OnAttackDone, 0, this);
		private ExclusiveAction Spawning  => new (SpawnSkeleton()   , OnSpawnCancel , OnSpawnDone , 0, this);
		private DelayedAction attack12DelayedAction;
		
		protected override IEnumerator AttackEnumerator() {
			rigidbody2D.linearVelocity = new Vector2(0, rigidbody2D.linearVelocity.y);
			
			yield return new WaitForSeconds(attackLength/(entityStat.attackSpeed * 9f));
			
			unstoppable      = true;
			weakness         = false;
			groggy           = false;
			groggyAttackLeft = 1;
			
			oldAnimSpeed = animator.speed;
			animator.speed  = entityStat.attackSpeed;
			
			attackAnimation = attacking;
			animator.SetBool(attackAnimation, true );
			animator.SetBool(moving,          false);
			animator.SetTrigger(attack);

			Vector2 hitboxPos  = (Vector2)transform.position + new Vector2(0, -0.985f);
			Vector2 hitboxSize = new(20f, 0.7f);
			attack12DelayedAction = AttackArea(1, (attackLength * 5f) / (entityStat.attackSpeed * 9f), hitboxPos, hitboxSize);
			
			yield return new WaitForSeconds((attackLength)/(entityStat.attackSpeed * 9f));
			groggySuccess    = 1;
			
			float weakTiming = Random.Range(1f, 3f); 
			
			yield return new WaitForSeconds((attackLength * weakTiming)/(entityStat.attackSpeed * 9f));

			GroggyEffect();
			
			yield return new WaitForSeconds((attackLength * (4f-weakTiming))/(entityStat.attackSpeed * 9f));

			PlaySFX("tankAttack1");
			CameraBrain.instance.ShakeLerp(5f, 10);
			CameraBrain.instance.ZoomLerp(-0.25f);
			
			yield return new WaitForSeconds((attackLength * 4)/(entityStat.attackSpeed * 9f));
			
			animator.speed = oldAnimSpeed;
			animator.SetBool(attackAnimation, false);
			animator.SetTrigger(attack);
			
			yield return new WaitForSeconds((attackLength * 2)/(entityStat.attackSpeed * 9f));
			
		}
		
		[NonSerialized] protected const float attack2Length       = 12f;
		[NonSerialized] protected const float projectileSpawnTime = 1;
		protected IEnumerator Attack2Enumerator() {
			AudioSource attackAudio = AudioManager.instance.PlaySFX("tankAttack2", volume: 0.6f, pitch: 1f);
			
			InfoUUI.instance.AddInfoMessage("강화된 스켈레톤이 <color=#ff5a5a>강력한 공격</color>을 사용하려 합니다! <color=#ff5a5a>치명적인 피해</color>를 <color=#ffea5a>3</color>회 적중시켜 저지하세요.");
			
			rigidbody2D.linearVelocity = new Vector2(0, rigidbody2D.linearVelocity.y);
			
			oldAnimSpeed = animator.speed;
			
			animator.speed  = entityStat.attackSpeed;
			attackAnimation = attack2ing;
			
			animator.SetBool(attackAnimation, true);
			animator.SetBool(moving,          false);
			animator.SetTrigger(attack2);
			
			float w1 = Random.Range(0f, 3f);
			float w2 = Random.Range(6f, 8.5f);
			float w3 = Random.Range(11f, 12f);
			
			yield return new WaitForSeconds((attack2Length * 4f)/(entityStat.attackSpeed * 43f));
			
			unstoppable      = true;
			groggySuccess    = 2;
			groggyAttackLeft = 3;
			
			// 개쩌는슈퍼짱짱투사채생성기만드는코드
			float   projectileSpawnTime2 = (attack2Length * 15f) / (entityStat.attackSpeed * 43f);
			Vector2 attackPos1           = (Vector2)transform.position + new Vector2(-0.5f, -0.5f);
			Vector2 attackPos2           = (Vector2)transform.position + new Vector2( 0.5f, -0.5f);
			
			GameObject projectileSpawnObject1 = UObjectPool.instance.Get(
				"BossProjectileSpawn",
				attackPos1
			);
			
			GameObject projectileSpawnObject2 = UObjectPool.instance.Get(
				"BossProjectileSpawn",
				attackPos2
			);
			
			projectileSpawnObject1.transform.rotation             = Quaternion.Euler(0f, 0f, 90*Random.Range(0, 5));
			projectileSpawnObject2.transform.rotation             = Quaternion.Euler(0f, 0f, 90*Random.Range(0, 5));
			projectileSpawnObject1.GetComponent<Animator>().speed = projectileSpawnTime / projectileSpawnTime2;
			projectileSpawnObject2.GetComponent<Animator>().speed = projectileSpawnTime / projectileSpawnTime2;
			
			attack12DelayedAction = new DelayedAction(
				projectileSpawnTime2,
				() => {
					GameObject     projectile1     = UObjectPool.instance.Get("BossProjectile", attackPos1);
					GameObject     projectile2     = UObjectPool.instance.Get("BossProjectile", attackPos2);
					BossProjectile bossProjectile1 = projectile1.GetComponent<BossProjectile>();
					BossProjectile bossProjectile2 = projectile2.GetComponent<BossProjectile>();
					bossProjectile1.damage               = entityStat.attackDamage * 5/2;
					bossProjectile2.damage               = entityStat.attackDamage * 5/2;
					bossProjectile1.spriteRenderer.flipX = false;
					bossProjectile2.spriteRenderer.flipX = true;
					bossProjectile1.owner                = this;
					bossProjectile2.owner                = this;
					
					UObjectPool.instance.Release(projectileSpawnObject1);
					UObjectPool.instance.Release(projectileSpawnObject2);
				},
				() => {
					AudioManager.instance.StopSFX(attackAudio);
					
					UObjectPool.instance.Release(projectileSpawnObject1);
					UObjectPool.instance.Release(projectileSpawnObject2);
				}
			);
			attack12DelayedAction.ExecuteDA();
			
			// 이동안 세번 빤짝할때 때리기 성공하면 공격 취소됨
			yield return new WaitForSeconds((attack2Length * (w1)) /(entityStat.attackSpeed * 43f));
			GroggyEffect();
			yield return new WaitForSeconds((attack2Length * (w2-w1)) /(entityStat.attackSpeed * 43f));
			GroggyEffect();
			yield return new WaitForSeconds((attack2Length * (w3-w2)) /(entityStat.attackSpeed * 43f));
			GroggyEffect();
			yield return new WaitForSeconds((attack2Length * (39-w3)) /(entityStat.attackSpeed * 43f));
			
			animator.speed = oldAnimSpeed;
			animator.SetBool(attackAnimation, false);
			animator.SetTrigger(attack);
			
			yield return new WaitForSeconds((attack2Length * 2)/(entityStat.attackSpeed * 43f));
		}

		[NonSerialized] protected const float spawnLength = 1.617f;
		protected IEnumerator SpawnSkeleton() {
			unstoppable                = true;
			
			rigidbody2D.linearVelocity = new Vector2(0, rigidbody2D.linearVelocity.y);
			oldAnimSpeed               = animator.speed;
			animator.speed             = entityStat.attackSpeed;
			
			animator.SetTrigger(spawn);
			
			GameObject spawnObj = UObjectPool.instance.Get(
				Random.Range(0, 2)==0?"SkeletonWarriorSpawn":"SkeletonArcherSpawn",
				new Vector2(transform.position.x + Random.Range(-2f, 2f), 0.5f));
			spawnObj.GetComponent<UObject>().Category = "skeletonSpawnObject";
			
			yield return new WaitForSeconds(spawnLength/3f);
			
			if (Random.Range(0, 2)==0) {
				spawnObj = UObjectPool.instance.Get(
					Random.Range(0, 2)==0?"SkeletonWarriorSpawn":"SkeletonArcherSpawn",
					new Vector2(transform.position.x + Random.Range(-2f, 2f), 0.5f));
				spawnObj.GetComponent<UObject>().Category = "skeletonSpawnObject";
			}
			yield return new WaitForSeconds(spawnLength/3f);
			
			if (Random.Range(0, 2)==0) {
				spawnObj = UObjectPool.instance.Get(
					Random.Range(0, 2)==0?"SkeletonWarriorSpawn":"SkeletonArcherSpawn",
					new Vector2(transform.position.x + Random.Range(-2f, 2f), 0.5f));
				spawnObj.GetComponent<UObject>().Category = "skeletonSpawnObject";
			}
			yield return new WaitForSeconds(spawnLength/3f);
		}
		
		protected override void OnAttackDone() {
			animator.speed = oldAnimSpeed;
			animator.SetBool(attackAnimation, false);
			
			state                  = EnemyState.Alert;
			currentExclusiveAction = FindingAggro;
			
			unstoppable      = false;
			weakness         = false;
			groggySuccess    = 2;
			groggyAttackLeft = -1;
		}

		protected override void OnAttackCancel() {
			animator.speed = oldAnimSpeed;
			animator.SetBool(attackAnimation, false);

			attack12DelayedAction?.Cancel();
			
			state                  = EnemyState.Alert;
			currentExclusiveAction = FindingAggro;
			
			unstoppable      = false;
			weakness         = false;
			groggySuccess    = 2;
			groggyAttackLeft = -1;
		}
		
		protected void OnSpawnDone() {
			animator.speed = oldAnimSpeed;
			animator.SetBool(spawn, false);
			
			state                  = EnemyState.Alert;
			currentExclusiveAction = FindingAggro;
			
			unstoppable            = false;
			weakness               = false;
			groggySuccess          = 1;
			groggyAttackLeft = -1;
		}

		protected void OnSpawnCancel() {
			animator.speed = oldAnimSpeed;
			animator.SetBool(spawn, false);
			
			state                  = EnemyState.Alert;
			currentExclusiveAction = FindingAggro;
			
			unstoppable      = false;
			weakness         = false;
			groggySuccess    = 2;
			groggyAttackLeft = -1;
		}

		[NonSerialized] private float currWanderTargetPosX;
		[NonSerialized] private float wanderTime;
		public override void WanderRoutine() {
			if (!aggroEntity) return;
			state = EnemyState.Alert;
		}

		[NonSerialized] private const float attackableDist   = 30f;
		[NonSerialized] private       float attackableDistRV;
		public override void AttackReadyRoutine() {
			float targetPositionX = aggroEntity.transform.position.x;
			if (Mathf.Abs(targetPositionX - transform.position.x) <= attackableDist + attackableDistRV) {
				state = EnemyState.Attack;
				return;
			}
			
			Move(targetPositionX);
		}
		
		public override void HitEffect(Entity attacker, float damage, Vector2? pushDir = null) {
			
			UUI damageUI = UUIObjectPool.instance.Open("DamageUI", GameManager.instance.mainWorldCanvas).GetComponent<UUI>();
			damageUI.GetComponent<RectTransform>().anchoredPosition =
				(transform.position + new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f), 0))*80;
			UTextAction   textAction = damageUI.GetAction<UTextAction>("DamageDisplay");
			UTextAction   textSAction = damageUI.GetAction<UTextAction>("DamageDisplayShadow");
			new DelayedAction(0.5f, () => UUIObjectPool.instance.Close(damageUI.gameObject)).ExecuteDA();
			
			textAction.text = new UPureString {Text = (weakness||groggy)?$"{Mathf.Floor(damage*100)/100f}<size=20><i> !!</i></size>":$"{Mathf.Floor(damage*100)/100f}"};
			textSAction.text = textAction.text;
			textAction.Initialize(damageUI);
			textSAction.Initialize(damageUI);
			
			if (entityStat.hp <= 0) {
				PlaySFX("tankDeath");
				
				GameManager.SetTimeScale(0.05f, 1f);
				CameraBrain.instance.ShakeLerp(3f*Mathf.Max(Mathf.Log(damage+3),0.5f), 3);
				CameraBrain.instance.ZoomLerp(-5f);
				return;
			}
			
			if (attacker != player) return;
			
			if (groggyAttackLeft == 0) {
				Debug.Log("[SkeletonTank] critical groggy hit");
				PlaySFX("tankGroggy");
				
				if (!groggyInfo) InfoUUI.instance.AddInfoMessage("강력한 스켈레톤을 공격할 절호의 기회입니다!");
				groggyInfo = true;
				
				groggy           = true;
				groggyAttackLeft = -1;
				new DelayedAction(5f, ()=>groggy=false).ExecuteDA();
				
				currentExclusiveAction = Stun(5);
				state                  = EnemyState.Stun;
				new DelayedAction(5, () => state = EnemyState.Alert).ExecuteDA();
				
				GameManager.SetTimeScale(0.05f, 0.75f);
				CameraBrain.instance.ShakeLerp(3f*Mathf.Max(Mathf.Log(damage+3),0.5f), 3);
				CameraBrain.instance.ZoomLerp(-1.25f);
				return;
			}
			
			if (weakness) {
				Debug.Log("[SkeletonTank] weakness hit");
				PlaySFX("tankCritical");
				
				GameManager.SetTimeScale(0.05f, 0.5f);
				CameraBrain.instance.ShakeLerp(1f*Mathf.Max(Mathf.Log(damage+3),0.5f), 5);
				CameraBrain.instance.ZoomLerp(-0.5f);
				return;
			}
			
			if (groggy) {
				Debug.Log("[SkeletonTank] groggy hit");
				PlaySFX("tankHit");
				
				GameManager.SetTimeScale(0.05f, 0.1f);
				CameraBrain.instance.ShakeLerp(2f*Mathf.Max(Mathf.Log(damage+3),0.5f), 5);
				CameraBrain.instance.ZoomLerp(-0.5f);
				return;
			}
			
			if (unstoppable) {
				Debug.Log("[SkeletonTank] unstoppable hit");
				PlaySFX("tankWeakHit");
				groggySuccess -= 1;
				
				GameManager.SetTimeScale(0.05f, 0.05f);
				return;
			}
			
			Debug.Log("[SkeletonTank] hit");
			PlaySFX("tankHit");
			
			GameManager.SetTimeScale(0.05f, 0.05f);
			CameraBrain.instance.ShakeLerp(0.8F*Mathf.Max(Mathf.Log(damage+3),0.5f), 5);
			CameraBrain.instance.ZoomLerp(-0.5f);
		}

		protected override void OnHit(Entity attacker, float damage, Vector2? pushDir) {
			entityStat.hp -= damage;
		}

		protected override void OnHit(Projectile attacker, float damage, Vector2? pushDir) {
			entityStat.hp -= damage;
		}

		protected override void OnHit(float damage, Vector2? pushDir) {
			entityStat.hp -= damage;
		}

		[NonSerialized] private bool groggyInfo = false;
		protected override float GetRealDamage(float rawDamage) {
			if (weakness) {
				groggyAttackLeft -= 1;
				return groggyAttackLeft == 0 ? entityData.hp / 10f : rawDamage*5f;
			}

			if (groggy) {
				return rawDamage*3f;
			}
			if (unstoppable) {
				return rawDamage*0.5f;
			}
			
			return rawDamage;
		}

		public override void Initialize() {
			base.Initialize();
			
			InfoUUI.instance.AddInfoMessage("강화된 스켈레톤이 공격을 준비할 때 붉은색 이펙트와 함께 <color=#ff5a5a>빈틈 타이밍</color>이 생깁니다. 타이밍을 잘 맞춰 <color=#ff5a5a>치명적인 피해</color>를 입히세요.");
		}

		protected override void Death() {
			GameObject doogaegol = UObjectPool.instance.Get("doogaegol", (Vector2)transform.position +  new Vector2(0.04166667f, 0.5416667f));
			Rigidbody2D doogaegolRB = doogaegol.GetComponent<Rigidbody2D>();
			
			doogaegolRB.AddForce(new Vector2(Random.Range(-2f, 2f), Random.Range(4f, 6f)), ForceMode2D.Impulse);
			doogaegolRB.AddTorque(Random.Range(-0.2f, 0.2f), ForceMode2D.Impulse);
			
			GameManager.UValueFloatVariables["skeletonTankDead"]   = new UPureNumber {number = GameManager.UValueFloatVariables["skeletonTankDead"].value + 1};
			GameManager.UValueFloatVariables["EnemyDead"]   = new UPureNumber {number = GameManager.UValueFloatVariables["EnemyDead"].value + 1};
			
			base.Death();
		}

		protected override void OnGrounded() {
			PlaySFX("tankFootStep", pitch:0.5f, spread:270f);
			
			currentExclusiveAction = Stun(0.5f);
			state                  = EnemyState.Stun;
			animator.SetBool(Landing, true);
			new DelayedAction(0.5f, () => {
				state = EnemyState.Alert;
				animator.SetBool(Landing, false);
			}).ExecuteDA();
		}
	}
}