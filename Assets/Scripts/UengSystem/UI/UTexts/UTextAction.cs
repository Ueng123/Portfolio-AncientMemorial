using System;
using UengSystem.Tasks;
using UengSystem.Tasks.Logic.UValues;
using UengSystem.Tasks.Logic.UValues.UStrings;
using UengSystem.Utility;

namespace UengSystem.UI.UTexts {
	[Serializable]
	public class UTextAction : UUIAction {
		public void SetText(UValue<string> newText) {
			((UText)component).SetValue(newText.value);
		}
		
		public void SetText(UValue<string> newText, float duration) {
			string currText = ((UText)component).GetValue();
			
			((UText)component).SetValue(newText.value);
			
			new DelayedAction(duration, () => ((UText)component).SetValue(currText)).Execute();
		}
		
		public override void Routine(ITaskable self) { }
	}
}