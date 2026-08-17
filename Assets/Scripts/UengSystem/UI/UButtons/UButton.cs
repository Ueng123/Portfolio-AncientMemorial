using System;
using UengSystem.UDebug;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UengSystem.UI.UButtons {
	[Serializable]
	public class UButton : UUIComponent {
		[HideInInspector]
		public Button button;
		[HideInInspector]
		public bool   clicked;
		
		public virtual void OnButtonClicked() {
			DebugManager.Log("BUTTON CLICK DETECTED : UButton");
			clicked = true;
			EventSystem.current.SetSelectedGameObject(null);
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