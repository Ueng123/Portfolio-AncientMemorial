using AncientMemorial.Cameras;
using AncientMemorial.Entities;
using AncientMemorial.Map;
using AncientMemorial.Objects;
using UengSystem.ObjectPool;
using UengSystem.Utility;
using UnityEngine;

namespace UengSystem.States.PlayerStates.WeaponAttackState.Shield {
	public class FallStrike : PlayerWeaponAttack {
		private AttackAware attackAction;
		
		private Vector2 downDir = Vector2.down;

		private Vector2 targetPos;
		private float   moveDistance;
		
		private bool isCancelled;

		public override float attackTime       => 0.75f;
		public override float attackAfterTime  => 0;
		public override float attackSpeed => 1;

		public override    void OnEnter() {
			base.OnEnter();

			isCancelled = true;
			
			player.rigidbody2D.linearVelocity = Vector2.zero;
			
			player.FreezeRigidbody2D();
			player.PlaySFX("shieldFallAttack");
			player.animator.Play((player.lookingLeft?"SFallAttackB":"SFallAttack"), 0);
			
			RaycastHit2D hit       = Physics2D.Raycast(player.transform.position, downDir, MapManager.instance.GetMapSize().y, LayerMask.GetMask("Map"));
			
			targetPos    = hit.point + Vector2.up*0.5f;
			moveDistance = Vector2.Distance(targetPos, player.transform.position);
		
			Vector2 hitboxSize = new (3, 0.5f);
			Vector2 hitboxPos  = hit.point + Vector2.up*0.25f;
			float   damage     = moveDistance * Mathf.Pow(player.entityData.hp / 10f, 1.7f) / (player.entityStat.attackDamage * 2);
			
			const int maxTargetEntity = 3;
			attackAction = Entity.AttackArea(player, damage, 0.5f, hitboxPos, hitboxSize, 0, maxTargetEntity);
		}

		public override void OnRoutine() {
			if (step == 0 && isProgress(0.666f)) {
				int afterImageNumber = Mathf.CeilToInt(moveDistance) + 2;
				for (int i = 0; i < afterImageNumber; i++) {
					Vector2 pos = (Vector2)player.transform.position
								  + downDir * (moveDistance * i / afterImageNumber);

					player.transform.position = pos;
					player.SpawnAfterImage();
				}

				player.transform.position = targetPos;
				player.PlaySFX("shieldSweep");
				UObjectPool.instance.Get("Explode3", player.groundChecker.transform.position + Vector3.up * 0.25f);

				CameraBrain.instance.ShakeLerp(2f, 10f);
				CameraBrain.instance.ZoomLerp(-1f);

				step = 1;
			}

			if (step == 1 && isProgress(1f)) {
				isCancelled  = false;
				player.state = GetDefaultState();
			}
		}

		public override    void OnExit() {
			if (isCancelled) {
				attackAction?.Cancel();
			}
			
			player.UnfreezeRigidbody2D();
			player.animator.Play("idle");
		}
	}
}