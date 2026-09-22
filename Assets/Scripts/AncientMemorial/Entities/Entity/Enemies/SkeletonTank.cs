using UengSystem.Utility;
using System;
using AncientMemorial.Cameras;
using AncientMemorial.States.EnemyStates;
using AncientMemorial.States.EnemyStates.Skeleton;
using AncientMemorial.States.EnemyStates.Skeleton.Tank;
using UengSystem;
using UnityEngine;

namespace AncientMemorial.Entities.Enemies {
	public class SkeletonTank : Skeleton {

		// 정적 프로퍼티
		private static readonly int TANK_DEATH = "tankDeath".GetHash();
		private static readonly int TANK_CRITICAL = "tankCritical".GetHash();
		private static readonly int TANK_HIT = "tankHit".GetHash();
		private static readonly int TANK_FOOT_STEP = "tankFootStep".GetHash();

		// 인스턴스 프로퍼티
		private int        attackPhase = 0;
		private EnemyState stomp;
		private EnemyState energyBurst;
		private EnemyState skeletonSpawn;

		protected override int footstepClipId => TANK_FOOT_STEP;

		// 오버라이드 메서드
		public override void OnFirstGet() {
			base.OnFirstGet();
			
			EnemyState wanderState      = new SkeletonWander(1, 10);
			EnemyState awareState       = new SkeletonAware(0, 8, 0f, 0f, 3);
			EnemyState attackReadyState = new EmptyAttackReady();

			InitializeStateMachine(wanderState, awareState, attackReadyState);

			stomp         = new TankStomp().Init(stateMachine);
			energyBurst   = new TankEnergyBurst().Init(stateMachine);
			skeletonSpawn = new TankSkeletonSpawn().Init(stateMachine);
		}

		public override void Initialize() {
			base.Initialize();
			attackPhase = 0;
		}

		protected override Entity GetTargetEntity() => player;
		
		public override void Attack() {
			entityState = attackPhase switch {
				0 => stomp,
				1 => stomp,
				2 => energyBurst,
				3 => skeletonSpawn,
				_ => throw new NotImplementedException(),
			};
			
			attackPhase = (attackPhase+1) % 4;
		}

		protected override float GetRealDamage(float rawDamage) {
			bool groggy = (entityState == stateMachine.stunState);
			return rawDamage * (groggy ? 2f : 1f);
		}

		public override void OnHit(Entity attacker, float damage, Vector2? pushDir = null) {
			bool groggy = (entityState == stateMachine.stunState);
			
			if (groggy) ShowCriticalDamageUI(damage);
			else ShowDamageUI(damage);
			
			if (stat.HP > 0) {
				PlaySFX(groggy ? TANK_CRITICAL : TANK_HIT);
				
				if (attacker != player) return;
				GameManager.SetTimeScale(0f, 0.05f);
				CameraManager.instance.ShakeLerp(1f, 10f);
				CameraManager.instance.ZoomLerp(-0.1f);
			}
		}

		protected override void Death() {
			PlaySFX(TANK_DEATH);

			GameManager.SetTimeScale(0.25f, 1f);
			CameraManager.instance.ShakeLerp(5f, 10);
			CameraManager.instance.ZoomLerp(-1f);

			base.Death();
		}
	}
}
