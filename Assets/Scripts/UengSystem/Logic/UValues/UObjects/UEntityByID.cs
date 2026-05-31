using System;
using AncientMemorial.Entities;
using UengSystem.Objects;
using UnityEngine;

namespace UengSystem.Logic.UValues.UObjects {
	[Serializable]
	public class UEntityByID : UValue<Entity> {
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