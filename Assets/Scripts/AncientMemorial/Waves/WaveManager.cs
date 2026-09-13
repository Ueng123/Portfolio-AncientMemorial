using System.Collections.Generic;
using UengSystem;
using UengSystem.Audio;
using UengSystem.Managers;
using UengSystem.UI;
using UengSystem.Utility;
using UengSystem.VisualScripting.Tasks;
using UengSystem.VisualScripting.UValues.UFloats;
// using UengSystem.VisualScripting.UValues.UFloats;
using UnityEngine;

namespace AncientMemorial.Waves {
	public class WaveManager : Manager<WaveManager> {

		// 정적 프로퍼티
		private static readonly int ClearUIPrefabId = "ClearUI".GetHash();
		private static readonly int DieUIPrefabId = "DieUI".GetHash();
		private static readonly int ClearClipId = "clear".GetHash();
		private static readonly int DeadClipId = "Dead".GetHash();

		// 인스턴스 프로퍼티
		private int TIME_ELAPSED = "timeElapsed".GetHash();
		
		public  List<Wave>  waves     = new List<Wave>();
		private Queue<Wave> waveQueue = new Queue<Wave>();
		public  Wave        currentWave;
		private bool        isWavePlaying;

		// 인스턴스 메서드
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
		
		private void PlayWave() {
			UPureFloat.SetValue(TIME_ELAPSED, 0);
			currentWave.waveTasks.Execute(GlobalObject.instance);
		}
		
		public void GameEnd() {
			AudioManager.instance.PlaySFX(ClearClipId);
			currentWave.waveTasks.CancelTasks();
			currentWave = null;
			GameManager.SetTimeScale(0);
			AudioManager.instance.StopAllSFX();
			UUI.Get(ClearUIPrefabId, GameManager.instance.mainScreenCanvas);
		}
		
		public void GameOver() {
			UUI.Get(DieUIPrefabId, GameManager.instance.mainScreenCanvas);
			currentWave.waveTasks.CancelTasks();
			currentWave = null;
			AudioManager.instance.SetBGM(DeadClipId);
			AudioManager.instance.StopAllSFX();
			GameManager.SetTimeScale(0);
		}

		// 오버라이드 메서드
		public override void Initialize() {
			waveQueue = new Queue<Wave>(waves);
			
			PlayWave();
		}
		
		public override void ManagerUpdate() {
			if (!currentWave) return;
			
			UPureFloat.AddValue(TIME_ELAPSED, Time.deltaTime);

			foreach (ConditionalTask condition in currentWave.alwaysConditionalTasks) { condition.Execute(this); }
		}
	}
}
