using UengSystem.Logic.UValues;
using UengSystem.UI.UTexts;

namespace UengSystem.UI.UUIQueues {
	public record AnnounceQueueData(UValue<string> topTitle, UValue<string> insideTitle) : UUIQueueData {
		public override void Initialize(UUI ui) {
			ui.GetAction<UTextAction>("TopTitle").text = topTitle;
			ui.GetAction<UTextAction>("InsideTitle").text = insideTitle;
			ui.Initialize();
		}
	};
}