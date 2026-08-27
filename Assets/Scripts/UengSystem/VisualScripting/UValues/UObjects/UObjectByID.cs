using System;
using UengSystem.Objects;
using UnityEngine;

namespace UengSystem.VisualScripting.UValues.UObjects {
	[Serializable]
	public class UObjectByID : UValue<UObject> {
		protected override bool getIsDynamic {
			get {
				ID.parent = this;
				return ID.isDynamic;
			}
		}
		
		[SerializeReference][SubclassSelector]
		public UValue<string> ID;

		protected override UObject getValue => UObject.GetUObject(ID.value);
	}
}