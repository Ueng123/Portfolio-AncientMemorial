using System;
using UengSystem.Objects;
using UengSystem.UAction;
using UengSystem.Utility;
using UengSystem.VisualScripting.UValues;
using UnityEngine;

namespace UengSystem.UI.UTexts {
	[Serializable]
	public class UTextAction : UUIAction {

		// 인스턴스 프로퍼티
		private UText textComponent;

		[SerializeReference] [SubclassSelector]
		public UValue<string> text;

		// 인스턴스 메서드
		public void SetText(UValue<string> newText, float duration) {
			UValue<string> oldText = text;

			text = newText;

			DelayedAction act = new (duration, () => text = oldText);
			act.ExecuteDA();
		}

		// 오버라이드 메서드
		public override void Initialize(UObject self) {
			textComponent = GetComponent<UText>();
			textComponent.Initialize();
			
			textComponent.SetValue(text?.value??"");
		}

		public override void Uninitialize(UObject self) {
			textComponent.SetValue(text?.value??"");
		}
		
		public override void Routine(UObject self) {
			textComponent.SetValue(text?.value??"");
		}
	}
}