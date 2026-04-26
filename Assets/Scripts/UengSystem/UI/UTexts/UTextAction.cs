using System;
using UengSystem.Logic.Tasks;
using UengSystem.Logic.UValues;
using UengSystem.Utility;
using UnityEngine;

namespace UengSystem.UI.UTexts {
	[Serializable]
	public class UTextAction : UUIAction {
		public void SetText(UValue<string> newText, float duration) {
			string currText = ((UText)component).GetValue();
			
			((UText)component).SetValue(newText.value);
			
			new DelayedAction(duration, () => ((UText)component).SetValue(currText)).Execute();
		}
		
		public override void Routine(ITaskable self) { }
	}
}