using System;
using UnityEngine;

namespace UengSystem.Tasks.Logic.String {
	[Serializable]
	public class UStringListItem {
		[SerializeReference] [SubclassSelector]
		public UStringComponent component;
	}
}