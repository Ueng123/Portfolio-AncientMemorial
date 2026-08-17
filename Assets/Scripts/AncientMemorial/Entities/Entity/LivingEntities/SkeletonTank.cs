using System;
using System.Collections;
using AncientMemorial.Cameras;
using AncientMemorial.Projectiles;
using UengSystem.Audio;
using UengSystem.Logic.UValues.UFloats;
using UengSystem.Logic.UValues.UStrings;
using UengSystem.ObjectPool;
using UengSystem.Objects;
using UengSystem.States;
using UengSystem.UDebug;
using UengSystem.UI;
using UengSystem.UI.UTexts;
using UengSystem.Utility;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

namespace AncientMemorial.Entities {
	public class SkeletonTank : Skeleton {
		// private static readonly     int attack2ing = Animator.StringToHash("attack2ing");
		// private static readonly     int attack2    = Animator.StringToHash("attack2");
		// private static readonly     int spawn      = Animator.StringToHash("spawn");
		//
		// private const float attackLength = 3f;
		// protected override float marginX            => 2;
		//
		// protected override float moveTargetDistance   => 2;
		// protected override float moveAllowMargin      => 4;
		// protected override float moveTargetDistanceRM => 1;
		//
		// public override void OnStunStart() { }
		//
		// public override void OnStunEnd() { }
		//
		// protected override string footstepSoundName => "tankFootStep";
		// protected override bool   isFootstep        => spriteRenderer.sprite.name is "SkeletonB_0" or "SkeletonB_2";
		//
		// public int attackPhase = 1;
		// public override void Attack() {
		// 	if (!attackable) return;
		// 	attackable = false;
		// 	
		// 	((UObject)this).state = attackPhase switch {
		// 		1 => Attacking,
		// 		2 => Attacking,
		// 		3 => Attack2ing,
		// 		4 => Spawning,
		// 		_ => throw new Exception("NO BRO THAT'S NOT WHAT I WANT :(")
		// 	};
		// 	
		// 	if (++attackPhase == 5) attackPhase = 1;
		// }
		//
		// private UState Attack2ing => new (Attack2Enumerator(), OnAttackCancel, OnAttackDone, 0, this);
		// private UState Spawning  => new (SpawnSkeleton()   , OnSpawnCancel , OnSpawnDone , 0, this);
		// private DelayedAction attack12DelayedAction;
		//
		// protected override IEnumerator AttackEnumerator() {
		// 	rigidbody2D.linearVelocity = new Vector2(0, rigidbody2D.linearVelocity.y);
		// 	
		// 	yield return CacheManager.WaitForSeconds(attackLength/(entityStat.attackSpeed * 9f));
		// 	
		// 	oldAnimSpeed = animator.speed;
		// 	animator.speed  = entityStat.attackSpeed;
		// 	
		// 	attackAnimation = attacking;
		// 	animator.SetBool(attackAnimation, true );
		// 	animator.SetBool(moving,          false);
		// 	animator.SetTrigger(attack);
		//
		// 	Vector2 hitboxPos  = (Vector2)transform.position + new Vector2(0, -0.985f);
		// 	Vector2 hitboxSize = new(20f, 0.7f);
		// 	attack12DelayedAction = AttackArea(1, (attackLength * 5f) / (entityStat.attackSpeed * 9f), hitboxPos, hitboxSize);
		// 	
		// 	yield return CacheManager.WaitForSeconds((attackLength * 5f)/(entityStat.attackSpeed * 9f));
		//
		// 	PlaySFX("tankAttack1");
		// 	CameraBrain.instance.ShakeLerp(5f, 10);
		// 	CameraBrain.instance.ZoomLerp(-0.25f);
		// 	
		// 	yield return CacheManager.WaitForSeconds((attackLength * 4)/(entityStat.attackSpeed * 9f));
		// 	
		// 	animator.speed = oldAnimSpeed;
		// 	animator.SetBool(attackAnimation, false);
		// 	animator.SetTrigger(attack);
		// 	
		// 	yield return CacheManager.WaitForSeconds((attackLength * 2)/(entityStat.attackSpeed * 9f));
		// 	
		// }
		//
		// protected const float attack2Length       = 12f;
		// protected IEnumerator Attack2Enumerator() {
		// 	AudioSource attackAudio = AudioManager.instance.PlaySFX("tankAttack2", volume: 0.6f, pitch: 1f);
		//
		// 	rigidbody2D.linearVelocity = new Vector2(0, rigidbody2D.linearVelocity.y);
		// 	
		// 	oldAnimSpeed = animator.speed;
		// 	
		// 	animator.speed  = entityStat.attackSpeed;
		// 	attackAnimation = attack2ing;
		// 	
		// 	animator.SetBool(attackAnimation, true);
		// 	animator.SetBool(moving,          false);
		// 	animator.SetTrigger(attack2);
		// 	
		// 	
		// 	yield return CacheManager.WaitForSeconds((attack2Length * 4f)/(entityStat.attackSpeed * 43f));
		// 	
		// 	float   projectileSpawnTime = (attack2Length * 15f) / (entityStat.attackSpeed * 43f);
		// 	Vector2 attackPos1           = (Vector2)transform.position + new Vector2(-0.5f, -0.5f);
		// 	Vector2 attackPos2           = (Vector2)transform.position + new Vector2( 0.5f, -0.5f);
		// 	
		// 	GameObject projectileSpawnObject1 = UObjectPool.instance.Get(
		// 		"BossProjectileSpawn",
		// 		attackPos1
		// 	);
		// 	
		// 	GameObject projectileSpawnObject2 = UObjectPool.instance.Get(
		// 		"BossProjectileSpawn",
		// 		attackPos2
		// 	);
		// 	
		// 	projectileSpawnObject1.transform.rotation             = Quaternion.Euler(0f, 0f, 90*Random.Range(0, 5));
		// 	projectileSpawnObject2.transform.rotation             = Quaternion.Euler(0f, 0f, 90*Random.Range(0, 5));
		// 	projectileSpawnObject1.GetComponent<Animator>().speed = 1f/projectileSpawnTime;
		// 	projectileSpawnObject2.GetComponent<Animator>().speed = 1f/projectileSpawnTime;
		// 	
		// 	attack12DelayedAction = new DelayedAction(
		// 		projectileSpawnTime,
		// 		() => {
		// 			GameObject     projectile1     = UObjectPool.instance.Get("BossProjectile", attackPos1);
		// 			GameObject     projectile2     = UObjectPool.instance.Get("BossProjectile", attackPos2);
		// 			
		// 			EnergyBall bossProjectile1 = projectile1.GetComponent<EnergyBall>();
		// 			EnergyBall bossProjectile2 = projectile2.GetComponent<EnergyBall>();
		// 			
		// 			bossProjectile1.damage               = entityStat.attackDamage * 3f;
		// 			bossProjectile2.damage               = entityStat.attackDamage * 3f;
		// 			
		// 			bossProjectile1.spriteRenderer.flipX = false;
		// 			bossProjectile2.spriteRenderer.flipX = true;
		// 			
		// 			bossProjectile1.owner                = this;
		// 			bossProjectile2.owner                = this;
		// 			
		// 			UObjectPool.instance.Release(projectileSpawnObject1);
		// 			UObjectPool.instance.Release(projectileSpawnObject2);
		// 		},
		// 		() => {
		// 			AudioManager.instance.StopSFX(attackAudio);
		// 			
		// 			UObjectPool.instance.Release(projectileSpawnObject1);
		// 			UObjectPool.instance.Release(projectileSpawnObject2);
		// 		}
		// 	);
		// 	attack12DelayedAction.ExecuteDA();
		// 	
		// 	yield return CacheManager.WaitForSeconds((attack2Length * 24) /(entityStat.attackSpeed * 43f));
		// 	
		// 	animator.speed = oldAnimSpeed;
		// 	animator.SetBool(attackAnimation, false);
		// 	animator.SetTrigger(attack);
		// 	
		// 	yield return CacheManager.WaitForSeconds((attack2Length * 2)/(entityStat.attackSpeed * 43f));
		// }
		//
		// protected const float spawnLength = 1.617f;
		// protected IEnumerator SpawnSkeleton() {
		// 	
		// 	rigidbody2D.linearVelocity = new Vector2(0, rigidbody2D.linearVelocity.y);
		// 	oldAnimSpeed               = animator.speed;
		// 	animator.speed             = entityStat.attackSpeed;
		// 	
		// 	animator.SetTrigger(spawn);
		// 	
		// 	GameObject spawnObj = UObjectPool.instance.Get(
		// 		Random.Range(0, 2)==0?"SkeletonWarriorSpawn":"SkeletonArcherSpawn",
		// 		new Vector2(transform.position.x + Random.Range(-2f, 2f), 0.5f));
		// 	spawnObj.GetComponent<UObject>().Category = "skeletonSpawnObject";
		// 	
		// 	yield return CacheManager.WaitForSeconds(spawnLength/3f);
		// 	
		// 	if (Random.Range(0, 2)==0) {
		// 		spawnObj = UObjectPool.instance.Get(
		// 			Random.Range(0, 2)==0?"SkeletonWarriorSpawn":"SkeletonArcherSpawn",
		// 			new Vector2(transform.position.x + Random.Range(-2f, 2f), 0.5f));
		// 		spawnObj.GetComponent<UObject>().Category = "skeletonSpawnObject";
		// 	}
		// 	yield return CacheManager.WaitForSeconds(spawnLength/3f);
		// 	
		// 	if (Random.Range(0, 2)==0) {
		// 		spawnObj = UObjectPool.instance.Get(
		// 			Random.Range(0, 2)==0?"SkeletonWarriorSpawn":"SkeletonArcherSpawn",
		// 			new Vector2(transform.position.x + Random.Range(-2f, 2f), 0.5f));
		// 		spawnObj.GetComponent<UObject>().Category = "skeletonSpawnObject";
		// 	}
		// 	yield return CacheManager.WaitForSeconds(spawnLength/3f);
		// }
		//
		// protected override void OnAttackDone() {
		// 	animator.speed = oldAnimSpeed;
		// 	animator.SetBool(attackAnimation, false);
		// 	
		// 	state                  = EnemyState.Alert;
		// }
		//
		// protected override void OnAttackCancel() {
		// 	animator.speed = oldAnimSpeed;
		// 	animator.SetBool(attackAnimation, false);
		//
		// 	attack12DelayedAction?.Cancel();
		// 	
		// 	state                  = EnemyState.Alert;
		// }
		//
		// protected void OnSpawnDone() {
		// 	animator.speed = oldAnimSpeed;
		// 	animator.SetBool(spawn, false);
		// 	
		// 	state                  = EnemyState.Alert;
		// }
		//
		// protected void OnSpawnCancel() {
		// 	animator.speed = oldAnimSpeed;
		// 	animator.SetBool(spawn, false);
		// 	
		// 	state                  = EnemyState.Alert;
		// }
		//
		// public override void WanderRoutine() {
		// 	if (!player) return;
		// 	state = EnemyState.Alert;
		// }
		//
		// private const float attackableDist   = 30f;
		// private       float attackableDistRV;
		// public override void AttackReadyRoutine() {
		// 	float targetPositionX = player.transform.position.x;
		// 	if (Mathf.Abs(targetPositionX - transform.position.x) <= attackableDist + attackableDistRV) {
		// 		state = EnemyState.Attack;
		// 		return;
		// 	}
		// 	
		// 	Move(targetPositionX);
		// }
		//
		// public override void OnHit(Entity attacker, float damage, Vector2? pushDir = null) {
		// 	
		// 	ShowDamageUI(damage);
		// 	
		// 	if (entityStat.hp <= 0) {
		// 		PlaySFX("tankDeath");
		// 		
		// 		GameManager.SetTimeScale(0.05f, 0.25f);
		// 		CameraBrain.instance.ShakeLerp(5, 5);
		// 		CameraBrain.instance.ZoomLerp(-1.5f);
		// 		return;
		// 	}
		// 	
		// 	DebugManager.Log("[SkeletonTank] hit");
		// 	PlaySFX("tankHit");
		// 	
		// 	if (attacker != player) return;
		// 	GameManager.SetTimeScale(0f, 0.05f);
		// 	CameraBrain.instance.ShakeLerp(1, 10);
		// 	CameraBrain.instance.ZoomLerp(-0.1f);
		// }
		//
		// protected override float GetRealDamage(float rawDamage) {
		// 	return rawDamage;
		// }
		//
		// protected override void Death() {
		// 	GameObject doogaegol = UObjectPool.instance.Get("doogaegol", (Vector2)transform.position +  new Vector2(0.04166667f, 0.5416667f));
		// 	Rigidbody2D doogaegolRB = doogaegol.GetComponent<Rigidbody2D>();
		// 	
		// 	doogaegolRB.AddForce(new Vector2(Random.Range(-2f, 2f), Random.Range(4f, 6f)), ForceMode2D.Impulse);
		// 	doogaegolRB.AddTorque(Random.Range(-0.2f, 0.2f), ForceMode2D.Impulse);
		//
		// 	for (int i = 0; i < 5; i++) {
		// 		UObjectPool.instance.Get("boneDebris", transform.position);
		// 	}
		// 	
		// 	GameManager.UValueFloatVariables["skeletonTankDead"]   = new UPureFloat {number = GameManager.UValueFloatVariables["skeletonTankDead"].value + 1};
		// 	GameManager.UValueFloatVariables["EnemyDead"]   = new UPureFloat {number = GameManager.UValueFloatVariables["EnemyDead"].value + 1};
		// 	
		// 	base.Death();
		// }
		//
		// protected override void OnGrounded() {
		// 	PlaySFX("tankFootStep", pitch:0.5f, spread:270f);
		// 	
		// 	((UObject)this).state = Stun(0.5f);
		// 	state                  = EnemyState.Stun;
		// 	animator.SetBool(Landing, true);
		// 	new DelayedAction(0.5f, () => {
		// 		state = EnemyState.Alert;
		// 		animator.SetBool(Landing, false);
		// 	}).ExecuteDA();
		// }
		protected override IEnumerator AttackEnumerator() {
			throw new NotImplementedException();
		}

		protected override void   OnAttackDone() {
			throw new NotImplementedException();
		}

		protected override void   OnAttackCancel() {
			throw new NotImplementedException();
		}

		protected override string footstepSoundName    { get; }
		protected override bool   isFootstep           { get; }
		protected override float  marginX              { get; }
		protected override float  moveTargetDistance   { get; }
		protected override float  moveAllowMargin      { get; }
		protected override float  moveTargetDistanceRM { get; }
	}
}