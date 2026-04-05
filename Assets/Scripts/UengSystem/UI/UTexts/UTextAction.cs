using System;
using UengSystem.Tasks;
using UengSystem.Tasks.Logic.String;
using UengSystem.Utility;

namespace UengSystem.UI.UTexts {
	[Serializable]
	public class UTextAction : UUIAction {
		public void SetText(UString newText) {
			((UText)component).SetValue(newText.GetText());
		}
		
		public void SetText(UString newText, float duration) {
			string currText = ((UText)component).GetValue();
			
			((UText)component).SetValue(newText.GetText());
			
			new DelayedAction(duration, () => ((UText)component).SetValue(currText)).Execute();
		}
		
		public override void Routine(ITaskable self) { }
	}
}