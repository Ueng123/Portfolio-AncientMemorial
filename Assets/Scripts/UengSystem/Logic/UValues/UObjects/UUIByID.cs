using System;
using UengSystem.UI;
using UnityEngine;

namespace UengSystem.Logic.UValues.UObjects {
	[Serializable]
	public class UUIByID : UValue<UUI> {
		public override bool getIsDynamic {
			get {
				ID.parent = this;
				return ID.isDynamic;
			}
		}
		
		[SerializeReference][SubclassSelector]
		public UValue<string> ID;

		public override UUI getValue => UUI.GetUUI(ID.value);
	}
}