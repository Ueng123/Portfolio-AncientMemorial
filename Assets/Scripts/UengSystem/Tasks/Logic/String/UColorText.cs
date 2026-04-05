using System;
using UengSystem.Tasks.Logic.Color;
using UnityEngine;

namespace UengSystem.Tasks.Logic.String {
	[Serializable]
	public class UColorText : UStringComponent {
		[SerializeReference] [SubclassSelector]
		public UString text;
		[SerializeReference] [SubclassSelector]
		public UColor  color;
		
		public override string GetText() {
			return $"<color=#{ColorUtility.ToHtmlStringRGB(color.GetColor())}>{text.GetText()}</color>";
		}
	}
}