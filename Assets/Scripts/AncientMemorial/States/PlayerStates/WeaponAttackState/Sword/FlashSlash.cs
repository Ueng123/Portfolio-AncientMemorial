using AncientMemorial.Cameras;
using AncientMemorial.Entities;
using AncientMemorial.Map;
using AncientMemorial.Objects;
using UengSystem.Inputs;
using UengSystem.ObjectPool;
using UengSystem.Utility;
using UnityEngine;

namespace UengSystem.States.PlayerStates.WeaponAttackState.Sword {
	public class FlashSlash : PlayerWeaponAttack {
		private AttackAware skillAttack;

		public override float attackTime       => 0f;
		public override float attackAfterTime  => 0f;
		public override float attackSpeed => 0f;

		private Vector2 hitboxPos;
		private Vector2 hitboxSize;
		private float   hitboxAngle;

		private float sweepDuration;
		private float flashDistance;

		private bool isCancelled;
		
		public override    void OnEnter() {
			base.OnEnter();

			isCancelled = true;
			
			player.rigidbody2D.linearVelocity = Vector2.zero;
			
			player.Freeze();
			player.PlaySFX("swordSkill");
			
			Vector2 playerPos     = player.transform.position;
			Vector2 mousePos      = InputManager.mousePosition;
			Vector2 mouseDir      = (mousePos - playerPos).normalized;
			flashDistance = 3f + player.entityStat.moveSpeed*0.2f;
			
			RaycastHit2D hit = Physics2D.Raycast(playerPos, mouseDir, flashDistance, LayerMask.GetMask("Map"));
			
			Vector2 mapSize = MapManager.instance.GetMapSize();
			
			Vector2 targetPos = hit.point==Vector2.zero?playerPos+mouseDir*flashDistance:hit.point;
			targetPos = new Vector2(
				Mathf.Clamp(targetPos.x, 0.3f -mapSize.x/2, -0.3f +mapSize.x/2), 
				Mathf.Clamp(targetPos.y, 1,                 mapSize.y)
			);
			Vector2 moveDir = (targetPos - playerPos);
			flashDistance = moveDir.magnitude;

			const int afterImageNumber = 6;
			for (int i = 0; i < afterImageNumber; i++) {
				Vector2 pos = playerPos + mouseDir*(flashDistance*i/afterImageNumber);
				player.transform.position = pos;
				player.SpawnAfterImage();
			}
			
			player.transform.position = targetPos;
			player.SpawnAfterImage();

			hitboxPos   = (playerPos + targetPos) / 2f;
			hitboxSize  = new Vector2(flashDistance, 0.4f);
			hitboxAngle = Mathf.Atan2(moveDir.y, moveDir.x) * Mathf.Rad2Deg;

			sweepDuration = Mathf.Max(0.8f / player.entityStat.attackSpeed, 0.05f);
			
			skillAttack = Entity.AttackArea(player, player.entityStat.attackSpeed, sweepDuration, hitboxPos, hitboxSize, hitboxAngle);
		}

		public override void OnRoutine() {
			if (step == 0 && !stateTimer.Check(sweepDuration)) {
				CameraBrain.instance.ShakeLerp(1.5f, 10);
                CameraBrain.instance.ZoomLerp(-1f, 15);
                
                Entity.AttackAreaNoEffect(player, 3f, 0, hitboxPos, hitboxSize, hitboxAngle);
                
                player.PlaySFX("swordSlash3");
                GameObject obj = UObjectPool.instance.Get("SlashEffect", hitboxPos);
                obj.transform.rotation   = Quaternion.Euler(0, 0, hitboxAngle+90f);
                obj.transform.localScale = new Vector3(2f, 4.5f * flashDistance/2.7f, 1f);

				isCancelled = false;
				player.state = GetDefaultState();
			}
		}
		
		public override void OnExit() {
			if (isCancelled) skillAttack?.Cancel();
			player.Unfreeze();
		}
	}
}