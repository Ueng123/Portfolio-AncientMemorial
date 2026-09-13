using System;
using TMPro;
using UnityEngine;

namespace UengSystem.UI.UInputFields {
	[Serializable]
	public class UInputField : UUIComponent {

		// 인스턴스 프로퍼티
		[HideInInspector]
		public TMP_InputField inputField;
		private string     text = "";

		[HideInInspector]
		public bool textChanged = false;
		[HideInInspector]
		public bool endEdit     = false;

		// 인스턴스 메서드
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

		// 오버라이드 메서드
		public override void Initialize() {
			inputField = GetComponent<TMP_InputField>();
			inputField.onValueChanged.AddListener(OnTextChanged);
			inputField.onEndEdit.AddListener(OnEndEdit);
		}

		public override void Uninitialize() {
			inputField.onValueChanged.RemoveAllListeners();
			inputField.onEndEdit.RemoveAllListeners();
		}
	}
}