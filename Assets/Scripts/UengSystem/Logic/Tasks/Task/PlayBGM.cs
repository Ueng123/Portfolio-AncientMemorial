using System;
using UengSystem.Audio;
using UengSystem.Logic.UValues;
using UnityEngine;

namespace UengSystem.Logic.Tasks {
	[Serializable]
	public class PlayBGM : TaskComponent {
		public AudioClip     clip;
		
		[SerializeReference] [SubclassSelector]
		public UValue<float> duration;
		
		public override void Execute(ITaskable self) {
			if (duration==null) AudioManager.instance.SetBGM(clip);
			else                AudioManager.instance.SetBGM(clip, duration.value);
		}
	}
}