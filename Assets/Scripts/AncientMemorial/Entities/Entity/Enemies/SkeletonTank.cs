using System;
using AncientMemorial.Cameras;
using UengSystem;
using UengSystem.States.EnemyStates;
using UengSystem.States.EnemyStates.Skeleton;
using UengSystem.States.EnemyStates.Skeleton.Tank;
using UnityEngine;

namespace AncientMemorial.Entities.Enemies {
	public class SkeletonTank : Skeleton {

		private int        attackPhase = 0;
		private EnemyState stomp;
		private EnemyState energyBurst;
		private EnemyState skeletonSpawn;

		protected override string footstepSoundName => "tankFootStep";

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
			state = attackPhase switch {
				0 => stomp,
				1 => stomp,
				2 => energyBurst,
				3 => skeletonSpawn,
				_ => throw new NotImplementedException(),
			};
			
			attackPhase = (attackPhase+1) % 4;
		}

		protected override float GetRealDamage(float rawDamage) {
			bool groggy = (state == stateMachine.stunState);
			return rawDamage * (groggy ? 2f : 1f);
		}

		public override void OnHit(Entity attacker, float damage, Vector2? pushDir = null) {
			bool groggy = (state == stateMachine.stunState);
			
			if (groggy) ShowCriticalDamageUI(damage);
			else ShowDamageUI(damage);
			
			if (stat.HP > 0) {
				PlaySFX(groggy?"tankCritical":"tankHit");
				
				if (attacker != player) return;
				GameManager.SetTimeScale(0f, 0.05f);
				CameraBrain.instance.ShakeLerp(1f, 10f);
				CameraBrain.instance.ZoomLerp(-0.1f);
			}
		}

		protected override void Death() {
			PlaySFX("tankDeath");

			GameManager.SetTimeScale(0f, 0.05f);
			CameraBrain.instance.ShakeLerp(2f, 10);
			CameraBrain.instance.ZoomLerp(-0.2f);

			base.Death();
		}
	}
}
