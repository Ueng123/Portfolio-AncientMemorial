using AncientMemorial;
using UnityEngine;
using UnityEngine.Audio;

namespace UengSystem.UI.USliders {
	public class UVolumeSlider : USlider {
		
		public string     volumeName;
		
		private void SetVolume(float value) {
			GameManager.instance.audioMixer.SetFloat(volumeName, GameManager.valueToDB(value));
		}
		
		public override void Initialize() {
			base.Initialize();
			
			slider.onValueChanged.AddListener(SetVolume);
		}
		
		public override void Uninitialize() {
			base.Uninitialize();
			
			slider.onValueChanged.RemoveAllListeners();
		}
	}
}