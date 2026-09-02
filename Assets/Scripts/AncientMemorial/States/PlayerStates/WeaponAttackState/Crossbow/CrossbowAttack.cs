using AncientMemorial.Cameras;
using AncientMemorial.Entities;
using AncientMemorial.Map;
using AncientMemorial.Objects;
using UengSystem.Inputs;
using UengSystem.ObjectPool;
using UengSystem.Utility;
using UnityEngine;

namespace UengSystem.States.PlayerStates.WeaponAttackState.Crossbow {
	public abstract class CrossbowAttack : PlayerWeaponAttack {
		protected static readonly int Attacking = Animator.StringToHash("attacking");

		private float oldAnimatorSpeed;
		
		protected static readonly StopWatch skillTimer = new StopWatch();
		private const             float     skillBonus = 1.5f;
		private                   float     skillDuration => 3 * (1f + Mathf.Log10(player.stat.attackSpeed));
		
		public override float attackSpeed => player.stat.attackSpeed * (skillTimer.CheckIn(skillDuration) ? skillBonus : 1f);
		
		protected GameObject GetArrowDebris(Vector2 position, float angle) {
			GameObject obj = UObjectPool.instance.Get("ArrowDebris", position);
			obj.transform.rotation = Quaternion.Euler(0, 0, angle);

			return obj;
		}

		protected void GetArrowEffect(Vector2 startPos, Vector2 endPos) {
			Vector2 pos = (endPos + startPos)/2f;
			
			Vector2 dir = (endPos - startPos);
			float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
			
			GameObject obj = UObjectPool.instance.Get("ArrowTail", pos);
			obj.transform.localScale = new Vector3(dir.magnitude, 0.05f, 1);
			
			obj.transform.rotation = Quaternion.Euler(0, 0, angle);
		}

		protected void Kick(float offset) {
			Vector2 playerPos = player.transform.position;
			Vector2 mousePos  = InputManager.mousePosition;
			
			Vector2 handDir        = (mousePos - playerPos).normalized;
			
			player.rigidbody2D.linearVelocity = handDir * -(player.stat.jumpPower * offset);
		}

		protected void HitEffect() {
			player.PlaySFX("crossbowShoot");
			CameraBrain.instance.ShakeLerp(0.5f, 7.5f);
			CameraBrain.instance.ZoomLerp(-0.2f);
		}

		protected void Shoot(float attackDamage, float angleOffset = 0f) {
			player.SetArm();
			player.handAnimator.Play(player.lookingLeft?"shootB":"shoot", 0, 0);
			
			Vector2 handPos = player.hand.transform.position;
			
			Vector2 playerPos = player.transform.position;
			Vector2 mousePos  = InputManager.mousePosition;
			
			Vector2 handDir        = (mousePos - playerPos).normalized;
			float   handAngle      = Mathf.Atan2(handDir.y, handDir.x) * Mathf.Rad2Deg;
			float   modifiedAngle  = handAngle + Random.Range(-4f, 4f) + angleOffset;
			float   modifiedRadian = modifiedAngle * Mathf.Deg2Rad;
			Vector2 modifiedDir    = new (Mathf.Cos(modifiedRadian), Mathf.Sin(modifiedRadian));
			
			float maxDistance = MapManager.instance.GetMapSize().magnitude+1;
			
			RaycastHit2D[] hits = Physics2D.RaycastAll(handPos, modifiedDir, maxDistance, LayerMask.GetMask("Map", "Entity"));
			foreach (RaycastHit2D hit in hits) {
				switch (hit.collider.gameObject.layer) {
					// MAP
					case 3: {
						HitEffect();
						
						GetArrowEffect(handPos, hit.point);
						GameObject debris = GetArrowDebris(hit.point, handAngle);
						debris.transform.SetParent(hit.transform);
					
						return;
					}
					// entity
					case 7: {
						Entity hitEntity = hit.collider.gameObject.GetComponent<Entity>();

						if (!player.isAttackTarget(hitEntity)) continue;
						HitEffect();
						
						GetArrowEffect(handPos, hit.point);
						GameObject debris = GetArrowDebris(hit.point, handAngle);
						hitEntity.Attatch(debris.GetComponent<AttatchObject>());
					
						player.SendAttackMultiplyEvent(hitEntity, attackDamage*Random.Range(0.9f, 1.2f), false);
						return;
					}
				}
			}
		}
		
		protected void ShootWithoutEffect(float attackDamage, float angleOffset = 0f) {
			Vector2 handPos = player.hand.transform.position;
			
			Vector2 playerPos = player.transform.position;
			Vector2 mousePos  = InputManager.mousePosition;
			
			Vector2 handDir        = (mousePos - playerPos).normalized;
			float   handAngle      = Mathf.Atan2(handDir.y, handDir.x) * Mathf.Rad2Deg;
			float   modifiedAngle  = handAngle + Random.Range(-4f, 4f) + angleOffset;
			float   modifiedRadian = modifiedAngle * Mathf.Deg2Rad;
			Vector2 modifiedDir    = new (Mathf.Cos(modifiedRadian), Mathf.Sin(modifiedRadian));
			
			float maxDistance = MapManager.instance.GetMapSize().magnitude+1;
			
			RaycastHit2D[] hits = Physics2D.RaycastAll(handPos, modifiedDir, maxDistance, LayerMask.GetMask("Map", "Entity"));
			foreach (RaycastHit2D hit in hits) {
				switch (hit.collider.gameObject.layer) {
					// MAP
					case 3: {
						GetArrowEffect(handPos, hit.point);
						GameObject debris = GetArrowDebris(hit.point, handAngle);
						debris.transform.SetParent(hit.transform);
					
						return;
					}
					// entity
					case 7: {
						Entity hitEntity = hit.collider.gameObject.GetComponent<Entity>();

						if (!player.isAttackTarget(hitEntity)) continue;
						GetArrowEffect(handPos, hit.point);
						GameObject debris = GetArrowDebris(hit.point, handAngle);
						hitEntity.Attatch(debris.GetComponent<AttatchObject>());
					
						player.SendAttackMultiplyEvent(hitEntity, attackDamage*Random.Range(0.9f, 1.2f), false);
						return;
					}
				}
			}
		}

		public override void OnEnter() {
			base.OnEnter();
			oldAnimatorSpeed = player.animator.speed;
			player.animator.speed = attackSpeed;
			player.animator.SetBool(Attacking, true);
		}

		public override void OnExit() {
			base.OnExit();
			player.animator.speed = oldAnimatorSpeed;
			player.animator.SetBool(Attacking, false);
			player.handAnimator.Play("handIdle");
		}
	}
}
