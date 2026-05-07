using UengSystem.Logic.UValues.UStrings;
using UengSystem.UI;
using UengSystem.UI.UTexts;
using UengSystem.UI.UUIQueues;

namespace AncientMemorial.Tasks {
	public record AnnounceQueueData(string topT, string insideT) : UUIQueueData() {
		public override void Initialize(UUI obj) {
			obj.GetAction<UTextAction>("TopTitle").text = new UPureString { Text = topT };
			obj.GetAction<UTextAction>("InsideTitle").text = new UPureString { Text = insideT };
		}
	};
}