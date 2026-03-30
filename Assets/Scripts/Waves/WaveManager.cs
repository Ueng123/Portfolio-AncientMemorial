using System.Collections.Generic;
using AncientMemorial;
using AncientMemorial.Interactions;
using UnityEngine;

namespace AncientMemorial.Waves {
	public class WaveManager : Manager<WaveManager> {
		public  List<Wave>  waves = new List<Wave>();
		private Queue<Wave> waveQueue = new Queue<Wave>();
		public  Wave        currentWave;

		public double       timeElapsed = 0;
		
		public override void Initialize() {
			waveQueue = new Queue<Wave>(waves);
		}

		public void NextWave() {
			currentWave?.waveTasks.CancelTasks();
			GameManager.instance.Crystal.interactable = false;

			if (waveQueue.Count == 0) {
				GameEnd();
				return;
			}
			
			timeElapsed = 0;
			
			currentWave = waveQueue.Dequeue();
			currentWave.waveTasks.ExecuteTasks(this);
		}

		public void EndWave() {
			Crystal crystal = GameManager.instance.Crystal;
			
			crystal.ChangeInteractText(currentWave.crystalInteractText);
			crystal.interactable = true;
		}

		public void GameEnd() {
			Debug.Log("Game End WOW!!!!");
		}
		
		public override void ManagerUpdate() {
			if (!currentWave) NextWave();
			
			timeElapsed += Time.deltaTime;

			foreach (WaveCondition condition in currentWave.condition) {
				if (condition.Check()) {
					condition.taskToDo.ExecuteTasks(this);
				}
			}
		}

		public override void ManagerFixedUpdate() {
			// there's nothing to do yay
		}
	}
}