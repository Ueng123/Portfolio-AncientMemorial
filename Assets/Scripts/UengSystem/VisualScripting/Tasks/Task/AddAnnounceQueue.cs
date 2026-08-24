using System;
using System.Collections.Generic;
using UengSystem.UI;
using UengSystem.UI.UUIQueues;
using UengSystem.VisualScripting.UValues;
using UengSystem.VisualScripting.UValues.UStrings;
using UnityEngine;

namespace UengSystem.VisualScripting.Tasks {
	[Serializable]
	public class AddAnnounceQueue : TaskComponent {
		public static Queue<AnnounceQueueData> announceQueue = new ();

		[SerializeReference][SubclassSelector]
		public UValue<string> topT;
		
		[SerializeReference][SubclassSelector]
		public UValue<string> insideT;

		[SerializeReference][SubclassSelector]
		public UValue<float> marginFront;
		[SerializeReference][SubclassSelector]
		public UValue<float> marginBack;
		[SerializeReference][SubclassSelector]
		public UValue<float> duration;
		
		public override void Execute(ITaskable self) {
			UUIPool.instance.AddUUIQueue("AnnounceUI", new UUIQueueItem(
												   marginFront?.value??0,
												   marginBack?.value??0,
												   duration?.value??0,
												   new AnnounceQueueData(topT, insideT)));
		}
	}
}