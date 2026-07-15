using System;
using AncientMemorial.Cameras;
using UengSystem.Logic.UValues;
using UnityEngine;

namespace UengSystem.Logic.Tasks {
	[Serializable]
	public class CameraShakeLerp : TaskComponent {
		
		[SerializeReference] [SubclassSelector]
		private UValue<float> intensity;
		
		[SerializeReference] [SubclassSelector]
		private UValue<float> speed;
		
		public override void Execute(ITaskable self) {
			CameraBrain.instance.ShakeLerp(intensity.value, speed.value);
		}
	}
}