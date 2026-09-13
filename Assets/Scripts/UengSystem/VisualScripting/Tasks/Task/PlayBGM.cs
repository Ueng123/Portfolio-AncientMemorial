using System;
using UengSystem.Audio;
using UengSystem.VisualScripting.UValues;
using UnityEngine;

namespace UengSystem.VisualScripting.Tasks {
	[Serializable]
	public class PlayBGM : TaskComponent {

		// 인스턴스 프로퍼티
		public AudioClip     clip;
		
		[SerializeReference] [SubclassSelector]
		public UValue<float> duration;

		// 오버라이드 메서드
		public override void Execute(ITaskable self) {
			if (duration==null) AudioManager.instance.SetBGM(clip);
			else                AudioManager.instance.SetBGM(clip, duration.value);
		}
	}
}