using System;
using UnityEngine;

namespace UengSystem.VisualScripting.UValues.UFloats {
	[Serializable]
	public class VolumeToDecibel : UValue<float> {
		protected override bool getIsDynamic {
			get {
				volume.parent = this;
				return volume.isDynamic;
			}
		}

		[SerializeReference] [SubclassSelector]
		public UValue<float> volume;

		protected override float getValue => Mathf.Log10(Mathf.Clamp(volume.value, 0.0001f, 1f)) * 20f;
	}
}