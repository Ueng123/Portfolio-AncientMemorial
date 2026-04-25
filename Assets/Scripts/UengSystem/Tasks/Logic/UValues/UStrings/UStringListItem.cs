using System;
using UnityEngine;

namespace UengSystem.Tasks.Logic.UValues.UStrings {
	[Serializable]
	public class UStringListItem {
		[SerializeReference] [SubclassSelector]
		public UValue<string> component;
	}
}