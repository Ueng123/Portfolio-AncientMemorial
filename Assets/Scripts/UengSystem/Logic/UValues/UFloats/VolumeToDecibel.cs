using System;
using UnityEngine;

namespace UengSystem.Logic.UValues.UFloats {
	[Serializable]
	public class VolumeToDecibel : UValue<float> {
		public override bool getIsDynamic {
			get {
				volume.parent = this;
				return volume.getIsDynamic;
			}
		}

		[SerializeReference] [SubclassSelector]
		public UValue<float> volume;

		public override float getValue => Mathf.Log10(Mathf.Clamp(volume.value, 0.0001f, 1f)) * 20f;
	}
}