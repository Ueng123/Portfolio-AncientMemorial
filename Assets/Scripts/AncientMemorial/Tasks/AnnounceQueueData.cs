using UengSystem.Logic.UValues;
using UengSystem.Logic.UValues.UStrings;
using UengSystem.Managers;
using UengSystem.UI;
using UengSystem.UI.UTexts;
using UengSystem.UI.UUIQueues;

namespace AncientMemorial.Tasks {
	public record AnnounceQueueData(UValue<string> topT, UValue<string> insideT) : UUIQueueData() {
		public override void PreGet() {
			GameManager.UValueStringVariables["AnnounceUITopTitle"   ]  = topT;
			GameManager.UValueStringVariables["AnnounceUIInsideTitle"] = insideT;
		}

		public override void Initialize(UUI obj) { }
	};
}