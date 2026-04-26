using System;
using UengSystem.Logic.UValues;
using UengSystem.Objects;
using UnityEngine;

namespace AncientMemorial.Entities {
	[Serializable]
	public class EntityByID : UValue<Entity> {
		public override bool getIsDynamic {
			get {
				ID.parent = this;
				return ID.isDynamicAuto;
			}
		}
		
		[SerializeReference][SubclassSelector]
		public UValue<string> ID;
		
		public override Entity getValue => (Entity)UObject.GetUObject(ID.value);
	}
}