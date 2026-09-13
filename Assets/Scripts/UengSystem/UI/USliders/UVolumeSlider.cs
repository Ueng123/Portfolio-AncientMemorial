using AncientMemorial;

namespace UengSystem.UI.USliders {
	public class UVolumeSlider : USlider {

		// 인스턴스 프로퍼티
		public string     volumeName;

		// 인스턴스 메서드
		private void SetVolume(float value) {
			GameManager.instance.audioMixer.SetFloat(volumeName, GameManager.valueToDB(value));
		}

		// 오버라이드 메서드
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