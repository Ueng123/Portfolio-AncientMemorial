using System;
using UengSystem.UDebug;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UengSystem.UI.UButtons {
	[Serializable]
	public class UButton : UUIComponent {

		// 인스턴스 프로퍼티
		[HideInInspector]
		public Button button;
		[HideInInspector]
		public bool   clicked;

		// 인스턴스 메서드
		public virtual void OnButtonClicked() {
			DebugManager.Log("BUTTON CLICK DETECTED : UButton");
			clicked = true;
			EventSystem.current.SetSelectedGameObject(null);
		}

		// 오버라이드 메서드
		public override void Initialize() {
			button = GetComponent<Button>();
			button.onClick.AddListener(OnButtonClicked);
		}

		public override void Uninitialize() {
			button.onClick.RemoveAllListeners();
		}
	}
}