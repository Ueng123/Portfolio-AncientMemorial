using UengSystem.Objects;
using AncientMemorial.Cameras;
using AncientMemorial.Entities;
using AncientMemorial.Objects;
using UengSystem.ObjectPool;
using UnityEngine;

namespace AncientMemorial.States.EnemyStates.Skeleton.Tank {
	public class TankStomp : SkeletonTankAttack {
		public override float attackTime => 3f;

		public float weakTime;
		public float attackPercent = 5f/9f;

		public float oldAnimSpeed;
		
		public AttackArea AttackArea;

		public override void OnEnter() {
			base.OnEnter();
			
			weakTime = Random.Range(0.25f, 0.35f);
			
			oldAnimSpeed         = enemy.animator.speed;
			enemy.animator.speed = enemy.stat.attackSpeed;
			
			enemy.animator.SetBool(attacking, true );
			enemy.animator.SetTrigger(attack);
			
			Vector2 hitboxPos  = (Vector2)enemy.transform.position + new Vector2(0, -0.985f);
			Vector2 hitboxSize = new(15f, 0.7f);
			AttackArea = Entity.AttackArea(enemy, 1, GetDelay(attackPercent), hitboxPos, hitboxSize);
		}

		public override void OnRoutine() {
			if (step == 0 && isProgress(weakTime)) {
				Weak();
				step = 1;
			}

			if (step == 1 && isProgress(attackPercent)) {
				Vector2 effectPos = (Vector2)enemy.transform.position + new Vector2(0, -0.985f);
				UObject.Get("Explode3", effectPos, PlayEffect: false);
				
				enemy.PlaySFX("tankAttack1");
				
				CameraBrain.instance.ShakeLerp(5f, 10);
				CameraBrain.instance.ZoomLerp(-0.25f);
				
				step = 2;
			}

			if (step == 2 && isProgress(1)) {
				AttackArea = null;
				enemy.entityState = GetState();
			}
		}

		public override void OnExit() {
			base.OnExit();
			
			AttackArea?.Cancel();

			enemy.animator.speed = oldAnimSpeed;
			enemy.animator.SetBool(attacking, false);
		}
	}
}
