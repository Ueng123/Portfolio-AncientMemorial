using System;
using TMPro;

namespace UengSystem.UI.UTexts {
	[Serializable]
	public class UText : UUIComponent {
		private TMP_Text text;

		public override void Initialize() {
			text = GetComponent<TMP_Text>();
		}
		
		public string GetValue()             => text.text;
		public void   SetValue(string value) => text.text = value;
	}
}