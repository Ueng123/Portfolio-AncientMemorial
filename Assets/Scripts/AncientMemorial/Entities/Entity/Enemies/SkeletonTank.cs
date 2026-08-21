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
using UengSystem.States.EnemyStates;
using UengSystem.States.EnemyStates.Skeleton;
using UengSystem.States.EnemyStates.Skeleton.Archer;
using UengSystem.States.EnemyStates.Skeleton.Tank;
using UengSystem.States.EnemyStates.Skeleton.Warrior;
using UengSystem.UDebug;
using UengSystem.UI;
using UengSystem.UI.UTexts;
using UengSystem.Utility;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

namespace AncientMemorial.Entities {
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

			stomp       = new TankStomp().Init(stateMachine);
			energyBurst = new TankEnergyBurst().Init(stateMachine);
		}

		public override void Initialize() {
			base.Initialize(); 
			attackPhase = 0;
		}

		public override Entity GetTargetEntity() => player;
		
		public override void Attack() {
			state = attackPhase switch {
				0 => energyBurst,
				1 => energyBurst,
				2 => stomp, // e..
				3 => stomp, // s..
				_ => throw new NotImplementedException(),
			};
			
			attackPhase = (attackPhase+1) % 4;
		}

		protected override float GetRealDamage(float rawDamage) {
			return rawDamage * (state == stateMachine.stunState ? 2f : 1f);
		}

		public override void OnHit(Entity attacker, float damage, Vector2? pushDir = null) {
			bool groggy = (state == stateMachine.stunState);
			
			if (groggy) ShowCriticalDamageUI(damage);
			else ShowDamageUI(damage);
			
			if (entityStat.hp <= 0) {
				PlaySFX("tankDeath");
				
				if (attacker != player) return;
				GameManager.SetTimeScale(0f, 0.05f);
				CameraBrain.instance.ShakeLerp(2f, 10);
				CameraBrain.instance.ZoomLerp(-0.2f);
			}
			else {
				PlaySFX(groggy?"tankCritical":"tankHit");
				
				if (attacker != player) return;
				GameManager.SetTimeScale(0f, 0.05f);
				CameraBrain.instance.ShakeLerp(1f, 10f);
				CameraBrain.instance.ZoomLerp(-0.1f);
			}
		}
	}
}