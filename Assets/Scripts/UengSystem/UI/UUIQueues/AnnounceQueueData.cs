using UengSystem.UI.UTexts;
using UengSystem.VisualScripting.UValues;

namespace UengSystem.UI.UUIQueues {
	public class AnnounceQueueData : UUIQueueData {
		public UValue<string> topTitle;
		public UValue<string> insideTitle;

		// 생성자
		public AnnounceQueueData(UValue<string> topTitle, UValue<string> insideTitle) {
			this.topTitle = topTitle;
			this.insideTitle = insideTitle;
		}

		// 오버라이드 메서드
		public override void Initialize(UUI ui) {
			ui.GetAction<UTextAction>("TopTitle").text = topTitle;
			ui.GetAction<UTextAction>("InsideTitle").text = insideTitle;
		}
	}
}
