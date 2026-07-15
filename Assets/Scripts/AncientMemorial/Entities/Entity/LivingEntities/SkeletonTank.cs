using System;
using System.Collections;
using System.Linq;
using AncientMemorial.Cameras;
using AncientMemorial.Map;
using AncientMemorial.Objects;
using AncientMemorial.Projectiles;
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
	public class SkeletonTank : Enemy {
		private static readonly int attacking   = Animator.StringToHash("attacking");
		private static readonly int attack2ing  = Animator.StringToHash("attack2ing");
		private static readonly int attack2   = Animator.StringToHash("attack2");
		private static readonly int attack   = Animator.StringToHash("attack");
		private static readonly int spawn    = Animator.StringToHash("spawn");
		private static readonly int moving   = Animator.StringToHash("moving");
		private static readonly int backward = Animator.StringToHash("backward");
		private static readonly int Landing  = Animator.StringToHash("landing");

		private       float oldAnimSpeed;
		private       int   attackAnimation;
		private const float attackLength = 3f;
		
		private bool unstoppable;
		private bool weakness;
		private int  groggyAttackLeft;
		private bool groggy;
		private int groggySuccess;
		
		public override void OnStunStart() { }

		public override void OnStunEnd() {
			currentExclusiveAction = FindingAggro;
		}

		public void GroggyEffect() {
			if (groggySuccess <= 0) return;
			
			unstoppable = false;
			weakness    = true;
			new DelayedAction(0.8F, () => {
				if (groggyAttackLeft == 0) return;
				unstoppable = true;
				weakness    = false;
			}).Execute();
			UObjectPool.instance.Get("groggyEffect", transform.position);
		}

		public int attackPhase = 1;
		public override void Attack() {
			if (!attackable) return;
			attackable = false;

			bool isCrystalized = entityType == EntityType.SkeletonTankC;
			
			currentExclusiveAction = attackPhase switch {
				1 => Random.Range(0, 16) == 0?Attack2ing:Attacking,
				2 => Random.Range(0, 6)  == 0? (!isCrystalized ? Spawning : Attack2ing):Attacking,
				3 => (!isCrystalized ? Spawning : Attack2ing),
				4 => Random.Range(0, 16) == 0?Attack2ing:Attacking,
				5 => Random.Range(0, 6)  == 0? (!isCrystalized ? Spawning : Attack2ing):Attacking,
				6 => Attack2ing,
				_ => throw new Exception("NO BRO THAT'S NOT WHAT I WANT :(")
			};
			
			if (++attackPhase == 7) attackPhase = 1;
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
			
			CameraBrain.instance.ShakeLerp(5f, 10);
			CameraBrain.instance.ZoomLerp(-0.25f);
			
			yield return new WaitForSeconds((attackLength * 4)/(entityStat.attackSpeed * 9f));
			
			animator.speed = oldAnimSpeed;
			animator.SetBool(attackAnimation, false);
			animator.SetTrigger(attack);
			
			yield return new WaitForSeconds((attackLength * 2)/(entityStat.attackSpeed * 9f));
			
		}
		
		protected const float attack2Length = 12f;
		protected const float projectileSpawnTime = 1;
		protected IEnumerator Attack2Enumerator() {
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
					UObjectPool.instance.Release(projectileSpawnObject1);
					UObjectPool.instance.Release(projectileSpawnObject2);
				}
			);
			attack12DelayedAction.Execute();
			
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

		protected const float spawnLength = 1.617f;
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

		private int mi = 0;
		private void Move(float targetPositionX) {
			animator.SetBool(Falling, !isGround);
			
			if (!isGround) return;
			
			if (aggroEntity) {
				// (+) : 이 엔티티가 타겟엔티티보다 <-에 있음
				int signE = (int)Mathf.Sign(aggroEntity.transform.position.x - transform.position.x);
				// (+) : 이 엔티티가 타켓위치보다 <-에 있음
				int signT = (int)Mathf.Sign(targetPositionX - transform.position.x);
			
				bool movingB   = Mathf.Abs(targetPositionX - transform.position.x) > moveAllowMargin;
				bool backwardB = signT * signE                                     == -1;
			
				// 애니메이션
				spriteRenderer.flipX = signE == 1;
			
				animator.SetBool(moving,   movingB);
				animator.SetBool(backward, backwardB);
			
				if (movingB) animator.speed = entityStat.moveSpeed * (backwardB ? 1.5f : 1);
				else animator.speed         = 1;
			
				// 안움직일 경우 거르기
				if (!movingB) return;
				
				rigidbody2D.linearVelocityX = entityStat.moveSpeed * signT;
				Debug.Log($"HE IS MOVING {mi++}");
			}
			else {
				// (+) : 이 엔티티가 타켓위치보다 <-에 있음
				int signT = (int)Mathf.Sign(targetPositionX - transform.position.x);
			
				bool movingB   = Mathf.Abs(targetPositionX - transform.position.x) > moveAllowMargin;
			
				// 애니메이션
				spriteRenderer.flipX = signT == 1;
			
				animator.SetBool(moving,   movingB);
				animator.SetBool(backward, false);
			
				if (movingB) animator.speed = entityStat.moveSpeed;
				else animator.speed         = 1;
			
				// 안움직일 경우 거르기
				if (!movingB) return;
			
				Vector2 velocity = new (entityStat.moveSpeed * signT, rigidbody2D.linearVelocity.y);
				rigidbody2D.linearVelocity = velocity;
				Debug.Log($"HE IS MOVING {mi++}");
			}
		}

		private float     currWanderTargetPosX;
		private float     wanderTime;
		public override void WanderRoutine() {
			if (!aggroEntity) return;
			state = EnemyState.Alert;
		}

		private const float         moveTargetDistance   = 2f;
		private const float         moveAllowMargin      = 1f;
		private const float         moveTargetDistanceRM = 0.75f;
		private       float         moveTargetDistanceRV;
		private       DelayedAction currAlertDA;
		public override void AlertRoutine() {
			if (!aggroEntity) {
				state = EnemyState.Wander;
				return;
			}
			
			if (attackable) {
				state = EnemyState.AttackReady;
				attackableDistRV = Random.Range(-attackableDistRM, attackableDistRM);
				return;
			}

			if (currAlertDA is not { Executing: true }) {
				currAlertDA = new DelayedAction(Random.Range(1f, 3f), () => {
					moveTargetDistanceRV = Random.Range(-moveTargetDistanceRM, moveTargetDistanceRM);
				});
				currAlertDA.Execute();
			}
			
			int signE = (int)Mathf.Sign(aggroEntity.transform.position.x - transform.position.x);
			float targetPositionX = aggroEntity.transform.position.x + moveTargetDistanceRV - moveTargetDistance*signE;
			
			Move(targetPositionX);
		}
		
		private readonly float attackableDist   = 30f;
		private          float attackableDistRM = 0.2f;
		private          float attackableDistRV;
		public override void       AttackReadyRoutine() {
			float targetPositionX = aggroEntity.transform.position.x;
			if (Mathf.Abs(targetPositionX - transform.position.x) <= attackableDist + attackableDistRV) {
				state = EnemyState.Attack;
				return;
			}
			
			Move(targetPositionX);
		}

		public override void AttackRoutine() { }

		public override void StunRoutine() { }

		private bool groggyedEffect = false;
		public override void HitEffect(Entity attacker, float damage, Vector2? pushDir = null) {
			
			UUI damageUI = UUIObjectPool.instance.Open("DamageUI", GameManager.instance.mainWorldCanvas).GetComponent<UUI>();
			damageUI.GetComponent<RectTransform>().anchoredPosition =
				(transform.position + new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f), 0))*80;
			UTextAction   textAction = damageUI.GetAction<UTextAction>("DamageDisplay");
			UTextAction   textSAction = damageUI.GetAction<UTextAction>("DamageDisplayShadow");
			new DelayedAction(0.5f, () => UUIObjectPool.instance.Close(damageUI.gameObject)).Execute();
			
			textAction.text = new UPureString {Text = (weakness||groggy)?$"{Mathf.Floor(damage*100)/100f}<size=20><i> !!</i></size>":$"{Mathf.Floor(damage*100)/100f}"};
			textSAction.text = textAction.text;
			textAction.Initialize(damageUI);
			textSAction.Initialize(damageUI);
			
			if (entityStat.hp <= 0) {
				Time.timeScale = 0.05f;
				new DelayedAction(1f, ()=>Time.timeScale = 1f, () => { }).Execute(true);
				CameraBrain.instance.ShakeLerp(3f*Mathf.Max(Mathf.Log(damage+3),0.5f), 3);
				CameraBrain.instance.ZoomLerp(-5f);
				return;
			}
			
			if (attacker != player) return;
			
			if (unstoppable) {
				Time.timeScale = 0.05f;
				new DelayedAction(0.05f, ()=>Time.timeScale = 1f, () => { }).Execute(true);
				return;
			}
			
			if (groggyedEffect) {
				groggyedEffect = false;
				Time.timeScale = 0.05f;
				new DelayedAction(0.75f, ()=>Time.timeScale = 1f, () => { }).Execute(true);
				CameraBrain.instance.ShakeLerp(3f*Mathf.Max(Mathf.Log(damage+3),0.5f), 3);
				CameraBrain.instance.ZoomLerp(-1.25f);
				return;
			}
			
			if (weakness) {
				Time.timeScale = 0.05f;
				new DelayedAction(0.5f, ()=>Time.timeScale = 1f, () => { }).Execute(true);
				CameraBrain.instance.ShakeLerp(1f*Mathf.Max(Mathf.Log(damage+3),0.5f), 5);
				CameraBrain.instance.ZoomLerp(-0.5f);
				return;
			}
			
			if (groggy) {
				Time.timeScale = 0.05f;
				new DelayedAction(0.1f, ()=>Time.timeScale = 1f, () => { }).Execute(true);
				CameraBrain.instance.ShakeLerp(2f*Mathf.Max(Mathf.Log(damage+3),0.5f), 5);
				CameraBrain.instance.ZoomLerp(-0.5f);
				return;
			}
			
			Time.timeScale = 0.05f;
			new DelayedAction(0.05f, ()=>Time.timeScale = 1f, () => { }).Execute(true);
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

		private bool groggyInfo = false;
		protected override float GetRealDamage(float rawDamage) {
			if (weakness && --groggyAttackLeft == 0) {
				if (!groggyInfo) InfoUUI.instance.AddInfoMessage("강력한 스켈레톤을 공격할 절호의 기회입니다!");
				groggyInfo = true;
				
				groggy         = true;
				groggyedEffect = true;
				new DelayedAction(5f, ()=>groggy=false).Execute();
				
				currentExclusiveAction = Stun(5);
				state                  = EnemyState.Stun;
				new DelayedAction(5, () => state = EnemyState.Alert).Execute();
				
				return entityData.hp / 10f;
			}
			if (weakness) { return rawDamage*5f; }
			if (groggy) { return rawDamage*3f; }
			if (unstoppable) { 
				groggySuccess -= 1;
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
			currentExclusiveAction = Stun(0.5f);
			state                  = EnemyState.Stun;
			animator.SetBool(Landing, true);
			new DelayedAction(0.5f, () => {
				state = EnemyState.Alert;
				animator.SetBool(Landing, false);
			}).Execute();
		}
	}
}