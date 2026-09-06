using UengSystem.Objects;
using AncientMemorial.Projectiles;
using UengSystem.Audio;
using UengSystem.ObjectPool;
using UnityEngine;

namespace AncientMemorial.States.EnemyStates.Skeleton.Tank {
	public class TankEnergyBurst : SkeletonTankAttack {
		private static readonly int attack2ing = Animator.StringToHash("attack2ing");
		private static readonly int attack2    = Animator.StringToHash("attack2");
		
		public override float attackTime => 12f;

		public readonly float preparePercent = 4f / 43f;
		public          float weakTime;
		public readonly float shootPercent   = 19f /43f;
		public          float oldAnimSpeed;

		private Vector2 attackPos1; 
		private Vector2 attackPos2; 
		
		private AudioSource attackAudio;
		private GameObject  projectileSpawnObject1;
		private GameObject  projectileSpawnObject2;
		
		public override void OnEnter() {
			base.OnEnter();
			
			weakTime = Random.Range(6f, 17f)/43f;
			
			oldAnimSpeed         = enemy.animator.speed;
			enemy.animator.speed = enemy.stat.attackSpeed;
			
			enemy.animator.SetBool(attack2ing, true );
			enemy.animator.SetTrigger(attack2);
			
			attackPos1 = (Vector2)enemy.transform.position + new Vector2(-0.5f, -0.5f);
			attackPos2 = (Vector2)enemy.transform.position + new Vector2( 0.5f, -0.5f);
			
			attackAudio = AudioManager.instance.PlaySFX("tankAttack2", volume: 0.6f, pitch: 1f);
		}

		public override void OnRoutine() {

			if (step == 0 && isProgress(preparePercent)) {
				projectileSpawnObject1 = UObject.Get("EnergyBallSpawn", attackPos1, PlayEffect: false);
				projectileSpawnObject1.transform.rotation = Quaternion.Euler(0f, 0f, 90 * Random.Range(0, 5));
				projectileSpawnObject1.GetComponent<Animator>().speed = 1 / GetDelay(shootPercent - preparePercent);

				projectileSpawnObject2 = UObject.Get("EnergyBallSpawn", attackPos2, PlayEffect: false);
				projectileSpawnObject2.transform.rotation = Quaternion.Euler(0f, 0f, 90 * Random.Range(0, 5));
				projectileSpawnObject2.GetComponent<Animator>().speed = 1 / GetDelay(shootPercent - preparePercent);
				
				step = 1;
			}
			
			if (step == 1 && isProgress(weakTime)) {
				Weak();
				step = 2;
			}

			if (step == 2 && isProgress(shootPercent)) {
				
				projectileSpawnObject1.GetComponent<UObject>().Release(PlayEffect: false);
				projectileSpawnObject2.GetComponent<UObject>().Release(PlayEffect: false);
				projectileSpawnObject1 = null;
				projectileSpawnObject2 = null;
				
				GameObject projectile1     = UObject.Get("EnergyBall", attackPos1, PlayEffect: false);
				EnergyBall bossProjectile1 = projectile1.GetComponent<EnergyBall>();
				bossProjectile1.damage               = enemy.stat.attackDamage * 5 / 2;
				bossProjectile1.spriteRenderer.flipX = false;
				bossProjectile1.owner                = enemy;
				
				GameObject projectile2     = UObject.Get("EnergyBall", attackPos2, PlayEffect: false);
				EnergyBall bossProjectile2 = projectile2.GetComponent<EnergyBall>();
				bossProjectile2.damage               = enemy.stat.attackDamage * 5 / 2;
				bossProjectile2.spriteRenderer.flipX = true;
				bossProjectile2.owner                = enemy;

				step = 3;
			}

			if (step == 3 && isProgress(1)) {
				enemy.entityState = GetState();
			}
		}

		public override void OnExit() {
			base.OnExit();
			
			attackAudio = null;
			
			if (attackAudio) AudioManager.instance.StopSFX(attackAudio);
			if (projectileSpawnObject1) projectileSpawnObject1.GetComponent<UObject>().Release(PlayEffect: false);
			if (projectileSpawnObject2) projectileSpawnObject2.GetComponent<UObject>().Release(PlayEffect: false);

			enemy.animator.speed = oldAnimSpeed;
			enemy.animator.SetBool(attack2ing, false);
		}
	}
}
