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

		// 정적 프로퍼티
		public static Queue<AnnounceQueueData> announceQueue = new ();

		// 인스턴스 프로퍼티
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

		// 오버라이드 메서드
		public override void Execute(ITaskable self) {
			UUIPool.instance.AddUUIQueue("AnnounceUI", new UUIQueueItem(
												   marginFront?.value??0,
												   marginBack?.value??0,
												   duration?.value??0,
												   new AnnounceQueueData(topT, insideT)));
		}
	}
}