using System;
using UengSystem.Logic.Tasks;
using UengSystem.Logic.UValues;
using UengSystem.Objects;
using UengSystem.Utility;
using UnityEngine;

namespace UengSystem.UI.UTexts {
	[Serializable]
	public class UTextAction : UUIAction {

		[SerializeReference] [SubclassSelector]
		public UValue<string> text;
		
		private UText textComponent;
		
		public void SetText(UValue<string> newText, float duration) {
			UValue<string> oldText = text;

			text = newText;

			DelayedAction act = new DelayedAction(duration, () => text = oldText);
			act.Execute();
		}

		public override void Initialize(UObject self) {
			component.Initialize();
			textComponent = (UText)component;
			textComponent.SetValue(text.value);
		}

		public override void Routine(UObject self) {
			textComponent.SetValue(text.value);
		}
	}
}