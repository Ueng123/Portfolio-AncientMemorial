using System;
using UengSystem.Objects;
using UengSystem.UAction;
using UengSystem.Utility;
using UengSystem.VisualScripting.UValues;
using UnityEngine;

namespace UengSystem.UI.UTexts {
	[Serializable]
	public class UTextAction : UUIAction {
		private UText textComponent;

		[SerializeReference] [SubclassSelector]
		public UValue<string> text;
		
		
		public void SetText(UValue<string> newText, float duration) {
			UValue<string> oldText = text;

			text = newText;

			DelayedAction act = new (duration, () => text = oldText);
			act.ExecuteDA();
		}

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