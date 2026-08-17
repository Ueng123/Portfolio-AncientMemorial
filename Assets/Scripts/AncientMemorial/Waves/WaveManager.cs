using System.Collections.Generic;
using UengSystem.Audio;
using UengSystem.Logic.Tasks;
using UengSystem.Logic.UValues.UFloats;
using UengSystem.Managers;
using UengSystem.UI;
using UnityEngine;

namespace AncientMemorial.Waves {
	public class WaveManager : Manager<WaveManager> {
		public  List<Wave>  waves     = new List<Wave>();
		private Queue<Wave> waveQueue = new Queue<Wave>();
		public  Wave        currentWave;
		private bool        isWavePlaying;
		
		public override void Initialize() {
			waveQueue = new Queue<Wave>(waves);
			
			PlayWave();
		}

		public void NextWave() {
			currentWave?.waveTasks.CancelTasks();
			if (GameManager.instance.Crystal) GameManager.instance.Crystal.UnInteractable();

			if (waveQueue.Count == 0) {
				GameEnd();
				return;
			}

			currentWave = waveQueue.Dequeue();
			PlayWave();
		}

		// 0에 NextWave있음
		// 결국 안끝나고 웨이브 지속 PlayWave [ NextWave f -> PlayWave [ ... ] t ] t -- ok
		// 끝남 PlayWave [ NextWave f -> PlayWave [ NextWave f -> CLEAR! ]] -- ok
		private void PlayWave() {
			GameManager.UValueFloatVariables["timeElapsed"] = new UPureFloat { number = 0 };
			currentWave.waveTasks.Execute(CoroutineRunner.instance);
		}
		
		public void GameEnd() {
			AudioManager.instance.PlaySFX("clear");
			GameManager.SetTimeScale(0);
			UUIPool.instance.Open("ClearUI", GameManager.instance.mainScreenCanvas);
		}
		
		public override void ManagerUpdate() {
			if (!currentWave) return;
			
			GameManager.UValueFloatVariables["timeElapsed"] = new UPureFloat {
				number = GameManager.UValueFloatVariables["timeElapsed"].value + Time.deltaTime
			};

			foreach (ConditionalTask condition in currentWave.alwaysConditionalTasks) { condition.Execute(this); }
		}
	}
}