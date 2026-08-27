using AncientMemorial.Cameras;
using AncientMemorial.Projectiles;
using UengSystem;
using UengSystem.ObjectPool;
using UengSystem.UAction;
using UnityEngine;

namespace AncientMemorial.Entities.Enemies {
	public abstract class CrystalBoss : Enemy {
		protected override void        OnGrounded() { }

		protected bool groggy;
		public void SetGroggy(bool value) {
			groggy = value;
		}
		
		public override void OnHit(Entity        attacker, float damage, Vector2? pushDir = null) {
			if (groggy) { ShowCriticalDamageUI(damage); }
			else { ShowDamageUI(damage); }
			
			if (entityStat.HP <= 0) {
				GameManager.SetTimeScale(0.5f, 0.25f);
				CameraBrain.instance.ShakeLerp(5, 1);
				CameraBrain.instance.ZoomLerp(-1f, 7.5f);
				return;
			}

			PlaySFX("crystalHit1");
			
			if (groggy) {
				if (attacker != player) return;
				GameManager.SetTimeScale(0, 0.05f);
				CameraBrain.instance.ShakeLerp(1, 10);
				CameraBrain.instance.ZoomLerp(-0.1f, 20f);
				return;
			}
			
			if (attacker != player) return;
			GameManager.SetTimeScale(0, 0.05f);
			CameraBrain.instance.ShakeLerp(1, 10);
			CameraBrain.instance.ZoomLerp(-0.1f, 20f);
		}
		
		protected override float GetRealDamage(float rawDamage) {
			return rawDamage * (groggy ? 2 : 1);
		}
	}
}