using System;
using UengSystem.Audio;
using UengSystem.Objects;
using UengSystem.VisualScripting.UValues;
using UnityEngine;

namespace UengSystem.VisualScripting.Tasks {
	[Serializable]
	public class PlaySFX : TaskComponent {

		// 인스턴스 프로퍼티
		public AudioClip       clip;

		public bool            playOnThis;
		
		[SerializeReference] [SubclassSelector]
		public UValue<UObject> audioPlayer;
		[SerializeReference] [SubclassSelector]
		public UValue<float> volume;
		[SerializeReference] [SubclassSelector]
		public UValue<float> pitch;
		[SerializeReference] [SubclassSelector]
		public UValue<float> pan;
		[SerializeReference] [SubclassSelector]
		public UValue<float> spread;
		[SerializeReference] [SubclassSelector]
		public UValue<bool>  loop;

		// 오버라이드 메서드
		public override void Execute(ITaskable self) {
			UObject audioPlayObject = playOnThis ? (UObject)self : audioPlayer?.value;

			if (audioPlayObject)
				audioPlayObject.PlaySFX(
					clip,
					volume:volume?.value??1,
					pitch:pitch?.value??1,
					pan:pan?.value??0,
					spread:spread?.value??0,
					loop:loop?.value??false
				);
			else AudioManager.instance.PlaySFX(clip);
		}
	}
}