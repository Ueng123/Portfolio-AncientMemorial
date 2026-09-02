using AncientMemorial.Cameras;
using UengSystem.ObjectPool;
using UnityEngine;

namespace AncientMemorial.States.EnemyStates.Crystal.Phase2 {
	public class CrystalPhase2Enter : CrystalPhase2State {
		private float duration = 10f;
		
		public override void OnEarlyRoutine() { }
		
		public override void OnRoutine() {
			base.OnRoutine();
			crystal.transform.Translate(Vector3.up*(Time.deltaTime/5f));

			if (stateTimer.CheckOut(duration)) crystal.state = GetState();
		}
		
		public override void OnExit() {
			base.OnExit();
			
			GameObject sEff      = UObjectPool.instance.Get("SlashEffect", crystal.transform.position);
			sEff.transform.rotation   = Quaternion.Euler(0, 0, Random.Range(-5f, 5f));
			sEff.transform.localScale = new Vector3(2f, 10f);
			
			GameObject sEff2      = UObjectPool.instance.Get("SlashEffect", crystal.transform.position);
			sEff2.transform.rotation   = Quaternion.Euler(0, 0, 90+Random.Range(-5f, 5f));
			sEff2.transform.localScale = new Vector3(2f, 10f);
			
			CameraBrain.instance.ShakeLerp(5, 10);
			CameraBrain.instance.ZoomLerp(-2f, 3);
		}
	}
}
