using UengSystem.Utility;
using UengSystem.Objects;
using AncientMemorial.Cameras;
using AncientMemorial.Entities;
using AncientMemorial.Map;
using AncientMemorial.Objects;
using UengSystem.ObjectPool;
using UnityEngine;

namespace AncientMemorial.States.PlayerStates.WeaponAttackState.Shield {
	public class FallStrike : PlayerWeaponAttack {

		// 정적 프로퍼티
		private static readonly int EXPLODE_3 = "Explode3".GetHash();
		private static readonly int SHIELD_FALL_ATTACK = "shieldFallAttack".GetHash();
		private static readonly int SHIELD_SWEEP = "shieldSweep".GetHash();

		// 인스턴스 프로퍼티
		private AttackArea attackAction;
		
		private Vector2 downDir = Vector2.down;

		private Vector2 targetPos;
		private float   moveDistance;
		
		private bool isCancelled;

		public override float attackTime       => 0.75f;
		public override float attackAfterTime  => 0;
		public override float attackSpeed => 1;

		// 오버라이드 메서드
		public override    void OnEnter() {
			base.OnEnter();

			isCancelled = true;
			
			player.rigidbody2D.linearVelocity = Vector2.zero;
			
			player.FreezeRigidbody2D();
			player.PlaySFX(SHIELD_FALL_ATTACK);
			player.animator.Play((player.lookingLeft?"SFallAttackB":"SFallAttack"), 0);
			
			RaycastHit2D hit       = Physics2D.Raycast(player.transform.position, downDir, MapManager.instance.GetMapSize().y, LayerMask.GetMask("Map"));
			
			targetPos    = hit.point + Vector2.up*0.5f;
			moveDistance = Vector2.Distance(targetPos, player.transform.position);
		
			Vector2 hitboxSize = new (3, 0.5f);
			Vector2 hitboxPos  = hit.point + Vector2.up*0.25f;
			float   damage     = moveDistance * Mathf.Pow(player.data.HP / 10f, 1.7f) / (player.stat.attackDamage * 2);
			
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
				player.PlaySFX(SHIELD_SWEEP);
				UObject.Get(EXPLODE_3, player.groundChecker.transform.position + Vector3.up * 0.25f, PlayEffect: false);

				CameraManager.instance.ShakeLerp(2f, 10f);
				CameraManager.instance.ZoomLerp(-1f);

				step = 1;
			}

			if (step == 1 && isProgress(1f)) {
				isCancelled  = false;
				player.entityState = GetDefaultState();
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
