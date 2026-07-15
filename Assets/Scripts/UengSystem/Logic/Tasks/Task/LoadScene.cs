using System;
using UengSystem.Logic.Tasks;
using UengSystem.Logic.UValues;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UengSystem.Logic.Tasks {
	[Serializable]
	public class LoadScene : TaskComponent {
		
		[SerializeReference] [SubclassSelector]
		public UValue<string> sceneName;
		
		public override void Execute(ITaskable self) {
			SceneManager.LoadScene(sceneName.value);
		}
	}
}