using System;
using UnityEngine.UI;

namespace UengSystem.UI.UButtons {
	[Serializable]
	public class UButton : UUIComponent {
		private Button button;
		public  bool   clicked;
		
		public void OnButtonClicked() {
			clicked = true;
		}

		public override void Initialize() {
			button = GetComponent<Button>();
			button.onClick.AddListener(OnButtonClicked);
		}
	}
}