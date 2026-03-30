using System;
using UnityEngine;

namespace AncientMemorial.Waves {
	[Serializable]
	public class Timeout : WaveEndCondition {
		[SerializeField]
		public double time = 0;
		
		public override bool Check() {
			return WaveManager.instance.timeElapsed >= time;
		}
	}
}