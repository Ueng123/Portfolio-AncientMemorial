using System;
using UnityEngine;
using UnityEngine.UI;

namespace UengSystem.UI.UButtons {
	[Serializable]
	public class UButton : UUIComponent {
		[HideInInspector]
		public Button button;
		[HideInInspector]
		public bool   clicked;
		
		public virtual void OnButtonClicked() {
			Debug.Log("BUTTON CLICK DETECTED : UButton");
			clicked = true;
		}

		public override void Initialize() {
			button = GetComponent<Button>();
			button.onClick.AddListener(OnButtonClicked);
		}

		public override void Uninitialize() {
			button.onClick.RemoveAllListeners();
		}
	}
}