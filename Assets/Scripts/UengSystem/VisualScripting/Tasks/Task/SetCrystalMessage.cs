using System;
using UengSystem.VisualScripting.UValues;
using UnityEngine;

namespace UengSystem.VisualScripting.Tasks {
	[Serializable]
	public class SetCrystalMessage : TaskComponent {

		// 인스턴스 프로퍼티
		[SerializeReference] [SubclassSelector]
		public UValue<string> newMessage;

		// 오버라이드 메서드
		public override void Execute(ITaskable self) {
			GameManager.instance.Crystal.interactionText = newMessage;
		}
	}
}