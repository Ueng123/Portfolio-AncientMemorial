using AncientMemorial.Cameras;
using AncientMemorial.Entities;
using AncientMemorial.Entities.Enemies;
using AncientMemorial.Map;
using UengSystem.ObjectPool;
using UengSystem.UI;
using UengSystem.Utility;
using UnityEngine;

namespace UengSystem.States.EnemyStates.Crystal.Phase3 {
	public class CrystalDeathAttack : CrystalPhase3Attack {
		public override float attackTime => 10;

		private GameObject attackEffect;
		
		public override void OnEnter() {
			base.OnEnter();
			
			SetMoveMode(CrystalMoveMode.CircleOnCenter);
			
			crystal.PlaySFX("crystalDeathAttack");
			crystal.To<CrystalPhase3>().StopGimmick();
			
			attackEffect = UUIPool.instance.Open("CrystalUltEffect", GameManager.instance.mainScreenCanvas);
		}
		
		public override void OnRoutine() {
			if (step == 0 && isProgress(0.95f)) {
				Vector2 mapSize = MapManager.instance.GetTargetMapSize();
				Entity  target  = stateMachine.getTarget.Invoke();
				
				Entity.SendAttackEvent(crystal, target, target.stat.HP*5f, true);
				
				GameObject eff1 = UObjectPool.instance.Get("SlashEffect", crystal.transform.position);
				eff1.transform.rotation   = Quaternion.Euler(0, 0, Random.Range(40, 50));
				eff1.transform.localScale = new Vector3(mapSize.x*1.1f, mapSize.y*1.2f, 1f);
				GameObject eff2 = UObjectPool.instance.Get("SlashEffect", crystal.transform.position);
				eff2.transform.rotation   = Quaternion.Euler(0, 0, -Random.Range(40, 50));
				eff2.transform.localScale = new Vector3(mapSize.x*1.1f, mapSize.y*1.2f, 1f);
			
				// 본래라면 여기서 사망해서 정지상태지만
				// 정말 만일의 경우로 죽지 않을 경우에 대비해 연출을 준비해둠.
				GameManager.SetTimeScale(0f, 1f);
				CameraBrain.instance.ShakeLerp(75, 10f);
				CameraBrain.instance.ZoomLerp(-2f);
				step = 1;
			}

			if (step == 1 && isProgress(1)) {
				ClearMissiles();
			
				UUIPool.instance.Close(attackEffect, true);

				crystal.state = GetState();
			}
		}
	}
}
