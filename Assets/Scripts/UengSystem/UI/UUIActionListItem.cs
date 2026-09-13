using System;
using UnityEngine;

namespace UengSystem.UI {
	[Serializable]
	public class UUIActionListItem {

		// 인스턴스 프로퍼티
		public string    key;
		
		[SerializeReference] [SubclassSelector]
		public UUIAction action;
	}
}