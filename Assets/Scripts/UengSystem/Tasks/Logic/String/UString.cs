using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace UengSystem.Tasks.Logic.String {
	[Serializable]
	public class UString {
		[SerializeReference]
		public List<UStringComponent> strings;
		private static StringBuilder stringBuilder = new StringBuilder();
		
		public string GetText() {
			stringBuilder.Clear();
			
			foreach (UStringComponent component in strings) {
				stringBuilder.Append(component.GetText());
			}
			
			return stringBuilder.ToString();
		}
	}
}