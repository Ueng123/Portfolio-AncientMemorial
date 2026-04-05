using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace UengSystem.Tasks.Logic.String {
	[Serializable]
	public class UString {
		public UStringListItem[] strings;
		
		public string GetText() {
			return strings.Aggregate("", (current, item) => current + item.component.GetText());
		}
	}
}