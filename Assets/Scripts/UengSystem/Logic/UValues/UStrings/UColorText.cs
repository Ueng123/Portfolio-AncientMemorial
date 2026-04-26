using System;
using UnityEngine;

namespace UengSystem.Logic.UValues.UStrings {
	[Serializable]
	public class UColorText : UValue<string> {
		public override bool getIsDynamic {
			get {
				text.parent  = this;
				color.parent = this;
				return text.isDynamic || color.isDynamic;
			}
		}
		
		[SerializeReference] [SubclassSelector] public UValue<string> text;
		[SerializeReference] [SubclassSelector] public UValue<Color> color;
		
		public override string getValue => $"<color=#{ColorUtility.ToHtmlStringRGB(color.getValue)}>{text.getValue}</color>";
	}
}