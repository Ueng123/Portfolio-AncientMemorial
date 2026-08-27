using System;
using UnityEngine;

namespace UengSystem.VisualScripting.UValues.UStrings {
	[Serializable]
	public class UColorString : UValue<string> {
		protected override bool getIsDynamic {
			get {
				text.parent  = this;
				color.parent = this;
				return text.isDynamic || color.isDynamic;
			}
		}
		
		[SerializeReference] [SubclassSelector] public UValue<string> text;
		[SerializeReference] [SubclassSelector] public UValue<Color>  color;

		protected override string getValue => $"<color=#{ColorUtility.ToHtmlStringRGB(color.value)}>{text.value}</color>";
	}
}