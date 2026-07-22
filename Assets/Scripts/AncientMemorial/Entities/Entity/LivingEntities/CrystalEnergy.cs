using System.Collections;
using AncientMemorial.Cameras;
using AncientMemorial.Map;
using AncientMemorial.Projectiles;
using UengSystem.Logic.UValues.UFloats;
using UengSystem.Logic.UValues.UStrings;
using UengSystem.Managers;
using UengSystem.ObjectPool;
using UengSystem.Objects;
using UengSystem.UI;
using UengSystem.UI.UTexts;
using UengSystem.Utility;
using UnityEngine;

namespace AncientMemorial.Entities {
	public class CrystalEnergy : Enemy {
		private static readonly int  Dead = Animator.StringToHash("dead");
		protected override      void OnGrounded() { }

		public Animator rotatingAnimator;
		
		public override    void        HitEffect(Entity    attacker,   float    damage, Vector2? pushDir = null) {
			rotatingAnimator.speed = 15;
			
			UUI damageUI = UUIObjectPool.instance.Open("DamageUI", GameManager.instance.mainWorldCanvas).GetComponent<UUI>();
			damageUI.GetComponent<RectTransform>().anchoredPosition =
				(transform.position + new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f), 0))*80;
			UTextAction textAction  = damageUI.GetAction<UTextAction>("DamageDisplay");
			UTextAction textSAction = damageUI.GetAction<UTextAction>("DamageDisplayShadow");
			new DelayedAction(0.5f, () => UUIObjectPool.instance.Close(damageUI.gameObject)).ExecuteDA();
			
			textAction.text  = new UPureString {Text = "<size=20><i>!!!</i></size>"};
			textSAction.text = textAction.text;
			textAction.Initialize(damageUI);
			textSAction.Initialize(damageUI);
			
			if (entityStat.hp <= 0) {
				GameManager.SetTimeScale(0, 0.15f);
				CameraBrain.instance.ShakeLerp(3f*Mathf.Max(Mathf.Log(damage+3),0.5f), 3);
				CameraBrain.instance.ZoomLerp(-2f);
				return;
			}
			
			GameManager.SetTimeScale(0, 0.05f);
			CameraBrain.instance.ShakeLerp(2f*Mathf.Max(Mathf.Log(damage+3),0.5f), 5);
			CameraBrain.instance.ZoomLerp(-0.2f);
		}

		protected override void OnHit(Entity attacker, float damage, Vector2? pushDir) { entityStat.hp -= damage; }

		protected override void OnHit(Projectile projectile, float damage, Vector2? pushDir) { entityStat.hp -= damage; }

		protected override void OnHit(float damage, Vector2? pushDir) { entityStat.hp -= damage; }

		protected override float GetRealDamage(float rawDamage) { return 1; }

		public override void OnStunStart() { }

		public override void OnStunEnd() { }

		protected override IEnumerator AttackEnumerator() {
			yield return new WaitForEndOfFrame();
		}

		protected override void OnAttackDone() { }

		protected override void OnAttackCancel() { }

		public override void WanderRoutine() {
			rotatingAnimator.speed = Mathf.Lerp(rotatingAnimator.speed, 1, DeltaTime);
		}

		public override void AlertRoutine() { }

		public override void AttackReadyRoutine() { }

		public override void AttackRoutine() { }

		public override void StunRoutine() { }

		public override void Initialize() {
			base.Initialize();
			animator.Play("spawn");
		}

		protected override void PrepareDespawnFX() {
			ToggleColliders(false);
		}
		
		protected override IEnumerator DespawnFX(float duration) {
			animator.Play("dead");
			yield return new WaitForSeconds(duration);
			UObjectPool.instance.Release(gameObject, -1);
		}

		protected override void Death() {
			GameManager.UValueFloatVariables["crystalEnergyDead"] = new UPureNumber {number = GameManager.UValueFloatVariables["crystalEnergyDead"].value + 1};
			GameManager.UValueFloatVariables["crystalEnergyGimmick"] = new UPureNumber {number = GameManager.UValueFloatVariables["crystalEnergyGimmick"].value - 1};
			GameManager.UValueFloatVariables["EnemyDead"]   = new UPureNumber {number = GameManager.UValueFloatVariables["EnemyDead"].value + 1};
			base.Death();
		}
	}
}