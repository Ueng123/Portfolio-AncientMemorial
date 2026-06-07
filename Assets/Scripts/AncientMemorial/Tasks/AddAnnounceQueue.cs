using System;
using System.Collections.Generic;
using UengSystem.Logic.Tasks;
using UengSystem.Logic.UValues;
using UengSystem.Objects;
using UengSystem.UI;
using UengSystem.UI.UUIQueues;
using UnityEngine;
using UnityEngine.Serialization;

namespace AncientMemorial.Tasks {
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
			UUIObjectPool.instance.AddUUIQueue("AnnounceUI", new UUIQueueItem(
												   marginFront.value,
												   marginBack.value,
												   duration.value,
												   new AnnounceQueueData(topT, insideT)));
		}
	}
}