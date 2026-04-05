using System;
using UnityEngine;

namespace UengSystem.UI {
	[Serializable]
	public class UUIActionListItem {
		public string    key;
		
		[SerializeReference] [SubclassSelector]
		public UUIAction action;
	}
}