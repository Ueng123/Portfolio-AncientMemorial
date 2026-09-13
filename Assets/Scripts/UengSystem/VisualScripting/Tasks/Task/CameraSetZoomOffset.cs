using System;
using AncientMemorial.Cameras;
using UengSystem.VisualScripting.UValues;
using UnityEngine;

namespace UengSystem.VisualScripting.Tasks {
	[Serializable]
	public class CameraSetZoomOffset : TaskComponent {

		// 인스턴스 프로퍼티
		[SerializeReference] [SubclassSelector]
		private UValue<float> zoomOffset;
		
		[SerializeReference] [SubclassSelector]
		private UValue<float> speed;

		// 오버라이드 메서드
		public override void Execute(ITaskable self) {
			CameraBrain.instance.ZoomLerp(zoomOffset.value, speed.value);
		}
	}
}