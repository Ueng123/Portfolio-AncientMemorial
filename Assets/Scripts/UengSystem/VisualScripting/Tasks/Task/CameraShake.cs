using System;
using AncientMemorial.Cameras;
using UengSystem.VisualScripting.UValues;
using UnityEngine;

namespace UengSystem.VisualScripting.Tasks {
	[Serializable]
	public class CameraShakeLerp : TaskComponent {

		// 인스턴스 프로퍼티
		[SerializeReference] [SubclassSelector]
		private UValue<float> intensity;
		
		[SerializeReference] [SubclassSelector]
		private UValue<float> speed;

		// 오버라이드 메서드
		public override void Execute(ITaskable self) {
			CameraManager.instance.ShakeLerp(intensity.value, speed.value);
		}
	}
}