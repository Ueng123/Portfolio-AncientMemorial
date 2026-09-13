using System;
using UengSystem.UI.UUIs;
using UengSystem.VisualScripting.UValues;
using UnityEngine;

namespace UengSystem.VisualScripting.Tasks {
	[Serializable]
	public class AddInfoMessage : TaskComponent {

		// 인스턴스 프로퍼티
		[SerializeReference][SubclassSelector]
		public UValue<string> message;

		// 오버라이드 메서드
		public override void Execute(ITaskable self) {
			InfoUUI.instance.AddInfoMessage(message.value);
		}
	}
}