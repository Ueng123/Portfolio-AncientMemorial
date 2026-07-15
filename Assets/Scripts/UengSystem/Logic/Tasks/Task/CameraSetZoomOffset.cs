using System;
using AncientMemorial.Cameras;
using UengSystem.Logic.UValues;
using UnityEngine;

namespace UengSystem.Logic.Tasks {
	[Serializable]
	public class CameraSetZoomOffset : TaskComponent {
		
		[SerializeReference] [SubclassSelector]
		private UValue<float> zoomOffset;
		
		[SerializeReference] [SubclassSelector]
		private UValue<float> speed;
		
		public override void Execute(ITaskable self) {
			CameraBrain.instance.ZoomLerp(zoomOffset.value, speed.value);
		}
	}
}