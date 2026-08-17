using System;
using UengSystem.Managers;
using UnityEngine.Serialization;

namespace UengSystem.Logic.Tasks {
	[Serializable]
	public class SetStage : TaskComponent {
		[FormerlySerializedAs("stageName")] public UScenes uScenes;
		
		public override void Execute(ITaskable self) {
			USceneManager.SetStage(uScenes);
		}
	}
}