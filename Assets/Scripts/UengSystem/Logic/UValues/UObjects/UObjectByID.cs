using System;
using UengSystem.Objects;
using UnityEngine;

namespace UengSystem.Logic.UValues.UObjects {
	[Serializable]
	public class UObjectByID : UValue<Objects.UObject> {
		public override bool getIsDynamic {
			get {
				ID.parent = this;
				return ID.isDynamicAuto;
			}
		}
		
		[SerializeReference][SubclassSelector]
		public UValue<string> ID;
		
		public override Objects.UObject getValue => Objects.UObject.GetUObject(ID.value);
	}
}