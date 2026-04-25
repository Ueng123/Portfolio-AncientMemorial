using System;
using UnityEngine;

namespace UengSystem.Tasks.Logic.UValues.UStrings {
	[Serializable]
	public class UColorText : UValue<string> {
		public override bool getIsDynamic => text.isDynamic || color.isDynamic;
		
		[SerializeReference] [SubclassSelector] public UString       text;
		[SerializeReference] [SubclassSelector] public UValue<Color> color;
		
		public override string getValue => $"<color=#{ColorUtility.ToHtmlStringRGB(color.getValue)}>{text.getValue}</color>";
	}
}