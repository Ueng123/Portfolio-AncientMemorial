using UengSystem.UDebug;

namespace UengSystem.UI.UButtons {
	public class UChoiceButton : UButton {

		// 인스턴스 프로퍼티
		public UChoiceButton[] buttonGroups;

		// 오버라이드 메서드
		public override void OnButtonClicked() {
			base.OnButtonClicked();

			foreach (UChoiceButton currButton in buttonGroups) {
				DebugManager.Log("YEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEES");
				currButton.button.enabled = false;
			}
		}
	}
}