using System;
using UengSystem.StageManager;
using UnityEngine.Serialization;

namespace UengSystem.VisualScripting.Tasks {
	[Serializable]
	public class SetStage : TaskComponent {
		[FormerlySerializedAs("stageName")] public UScenes uScenes;
		
		public override void Execute(ITaskable self) {
			USceneManager.SetStage(uScenes);
		}
	}
}