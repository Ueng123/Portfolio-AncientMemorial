using System;
using AncientMemorial.AMObjects;
using AncientMemorial.ObjectPool;
using UnityEngine;
using Event = AncientMemorial.Events.Event;

namespace AncientMemorial {
	public class SpawnEffectHelper : AdvancedAMObject {
		[SerializeField] private LineRenderer SubLineA;
		[SerializeField] private LineRenderer SubLineB;
		[SerializeField] private Transform    SpawnEffect;
		
		private float t;
		public  float t_s;
		public  float t_n;
		
		protected override void EarlyRoutine() { }

		private (float f, float b) GetValue(float x) {
			float x_o = transform.position.x;
			float y_o = transform.position.y;
			float x_c = GameManager.instance.Crystal.crystalModel.transform.position.x;
			float y_c = GameManager.instance.Crystal.crystalModel.transform.position.y;

			float dx = x_c - x_o;
			float dy = y_c - y_o;
			
			float A = -t_n * (t_n - 1f);
			float T = 3f   / (t_n + 3f);
			
			float slope = (dx != 0) ? (dy / dx) : 0;
			float valF  = slope * (x - x_c) + y_c; 
			
			float x_g   = x   * Mathf.Sign(x_o           - x_c);
			float phase = 5f * Mathf.PI * Mathf.Pow(t_n + 1f, 2f);
			float valB  = A   * Mathf.Sin(T * x_g        - phase);

			return (valF, valB);
		}
		
		protected override void Routine() {
			
			t   += Time.deltaTime;
			t_n =  Mathf.Clamp01(t / t_s);

			if (t > t_s) {
				GameObject spawnEffect = AMObjectPool.instance.Get("SpawnParticle", transform.position);
				
				DelayedAction act = new (5, () => {
					AMObjectPool.instance.Release(spawnEffect);
				});

				act.Execute();
				AMObjectPool.instance.Release(gameObject);
				return;
			}
			
			float startX    = transform.position.x;
			float endX      = GameManager.instance.Crystal.transform.position.x;
			float totalDist = endX - startX;
			int   density   = 50;
			
			Vector3[] pointA = new Vector3[density];
			Vector3[] pointB = new Vector3[density];
    
			for (int n = 0; n < density; n++) {
				float progress = (float)n / (density - 1); 
				float x        = startX + (totalDist * progress);
       
				(float f, float b) val = GetValue(x);
				
				pointA[n] = new Vector3(x, val.f + val.b, 0);
				pointB[n] = new Vector3(x, val.f - val.b, 0);
			}
			
			SubLineA.positionCount = density;
			SubLineB.positionCount = density;
			
			SubLineA.SetPositions(pointA);
			SubLineB.SetPositions(pointB);
			
			float angle = (float)(180f * Math.Pow(t_n * 5f, 1.2));
			SpawnEffect.rotation = Quaternion.Euler(0, 0, angle);
		}

		protected override void LateRoutine() { }

		protected override void FixedRoutine() { }

		public override    void OnEvent(Event e) { }

		public override void Initialize() {

			t   = 0;
			t_n = 0;
			
			base.Initialize();
		}
	}
}