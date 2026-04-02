using System;
using UnityEngine.UI;

namespace UengSystem.UI.UButtons {
	[Serializable]
	public class UButton : UIComponent {
		private Button button;
		public  bool   clicked;
		
		public void OnButtonClicked() {
			clicked = true;
		}

		public override void Initialize() { button.onClick.AddListener(OnButtonClicked); }
	}
}