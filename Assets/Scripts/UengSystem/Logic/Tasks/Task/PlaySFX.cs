using System;
using UengSystem.Audio;
using UengSystem.Logic.UValues;
using UengSystem.Objects;
using UnityEngine;

namespace UengSystem.Logic.Tasks {
	[Serializable]
	public class PlaySFX : TaskComponent {
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