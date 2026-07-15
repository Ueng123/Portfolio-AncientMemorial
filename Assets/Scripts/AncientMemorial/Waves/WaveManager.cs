using System.Collections.Generic;
using UengSystem.Logic.Tasks;
using UengSystem.Logic.UValues.UFloats;
using UengSystem.Managers;
using UengSystem.UI;
using UnityEngine;

namespace AncientMemorial.Waves {
	public class WaveManager : Manager<WaveManager> {
		public  List<Wave>  waves = new List<Wave>();
		private Queue<Wave> waveQueue = new Queue<Wave>();
		public  Wave        currentWave;
		
		public override void Initialize() {
			waveQueue = new Queue<Wave>(waves);
		}

		public void NextWave() {
			currentWave?.waveTasks.CancelTasks();
			if (GameManager.instance.Crystal) GameManager.instance.Crystal.interactable = false;

			if (waveQueue.Count == 0) {
				GameEnd();
				return;
			}

			GameManager.UValueFloatVariables["timeElapsed"] = new UPureNumber { number = 0 };
			
			currentWave = waveQueue.Dequeue();
			currentWave.waveTasks.Execute(GlobalCoroutineRunner.instance);
		}
		
		public void GameEnd() {
			UUIObjectPool.instance.Open("ClearUI", GameManager.instance.mainScreenCanvas);
		}
		
		public override void ManagerUpdate() {
			if (!currentWave) NextWave();
			
			GameManager.UValueFloatVariables["timeElapsed"] = new UPureNumber {
				number = GameManager.UValueFloatVariables["timeElapsed"].value + GlobalCoroutineRunner.instance.DeltaTime
			};

			foreach (ConditionalTask condition in currentWave.alwaysConditionalTasks) { condition.Execute(this); }
		}

		public override void ManagerFixedUpdate() {
			// there's nothing to do yay
		}
	}
}