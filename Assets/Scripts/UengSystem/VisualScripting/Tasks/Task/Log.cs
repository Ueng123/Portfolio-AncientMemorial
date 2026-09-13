using System;
using UengSystem.UDebug;
using UengSystem.VisualScripting.UValues;
using UnityEngine;

namespace UengSystem.VisualScripting.Tasks {
	[Serializable]
	public class Log : TaskComponent {

		// 인스턴스 프로퍼티
		[SerializeReference][SubclassSelector]
		public UValue<string> text;

		// 오버라이드 메서드
		public override void Execute(ITaskable self) {
			DebugManager.Log(text.value);
		}
	}
}