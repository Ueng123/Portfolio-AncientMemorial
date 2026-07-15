using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UengSystem.UI.UInputFields {
	[Serializable]
	public class UInputField : UUIComponent {
		[HideInInspector]
		public TMP_InputField inputField;
		private string     text = "";

		[HideInInspector]
		public bool textChanged = false;
		[HideInInspector]
		public bool endEdit     = false;

		public virtual void OnTextChanged(string changed) {
			textChanged = true;
		}

		public virtual void OnEndEdit(string changed) {
			text = changed;
			endEdit = true;
		}
		
		public string GetLiveText() => inputField.text;
		public string GetText() => text;

		public void SetText(string textToChange) {
			inputField.text = textToChange;
			text            = textToChange;
		}
		
		public override void Initialize() {
			inputField = GetComponent<TMP_InputField>();
			inputField.onValueChanged.AddListener(OnTextChanged);
			inputField.onEndEdit.AddListener(OnEndEdit);
		}
	}
}