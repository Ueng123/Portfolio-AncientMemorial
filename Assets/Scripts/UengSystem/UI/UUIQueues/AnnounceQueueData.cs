using UengSystem.UI.UTexts;
using UengSystem.VisualScripting.UValues;

namespace UengSystem.UI.UUIQueues {
	public record AnnounceQueueData(UValue<string> topTitle, UValue<string> insideTitle) : UUIQueueData {

		// 오버라이드 메서드
		public override void Initialize(UUI ui) {
			ui.GetAction<UTextAction>("TopTitle").text = topTitle;
			ui.GetAction<UTextAction>("InsideTitle").text = insideTitle;
		}
	};
}
