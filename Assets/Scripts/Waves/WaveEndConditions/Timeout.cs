using System;
using UnityEngine;

namespace AncientMemorial.Waves {
	[Serializable]
	public class Timeout : WaveCondition {
		[SerializeField]
		public double time = 0;

		private bool elapsed;
		
		public override bool Check() {
			bool result = WaveManager.instance.timeElapsed >= time && !elapsed;
			elapsed = result;
			return result;
		}
	}
}