using System;
using UengSystem.Audio;
using UengSystem.Logic.UValues;
using UnityEngine;

namespace UengSystem.Logic.Tasks {
	[Serializable]
	public class PlaySFX : TaskComponent {
		public AudioClip     clip;
		
		public override void Execute(ITaskable self) {
			AudioManager.instance.PlaySFX(clip);
		}
	}
}