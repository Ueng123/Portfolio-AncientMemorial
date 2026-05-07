using System;
using UengSystem.Logic.Tasks;
using UengSystem.Logic.UValues;
using UengSystem.Utility;
using UnityEngine;

namespace UengSystem.UI.UTexts {
	[Serializable]
	public class UTextAction : UUIAction {

		[SerializeReference] [SubclassSelector]
		public UValue<string> text;
		
		public void SetText(UValue<string> newText, float duration) {
			UValue<string> oldText = text;

			text = newText;

			DelayedAction act = new DelayedAction(duration, () => text = oldText);
			act.Execute();
		}

		public override void Initialize(ITaskable self) {
			component.Initialize();
			((UText)component).SetValue(text.value);
		}

		public override void Routine(ITaskable self) {
			((UText)component).SetValue(text.value);
		}
	}
}