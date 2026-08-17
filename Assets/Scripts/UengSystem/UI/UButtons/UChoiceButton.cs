using UengSystem.UDebug;

namespace UengSystem.UI.UButtons {
	public class UChoiceButton : UButton {
		public UChoiceButton[] buttonGroups;
		
		public override void OnButtonClicked() {
			base.OnButtonClicked();

			foreach (UChoiceButton currButton in buttonGroups) {
				DebugManager.Log("YEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEES");
				currButton.button.enabled = false;
			}
		}
	}
}