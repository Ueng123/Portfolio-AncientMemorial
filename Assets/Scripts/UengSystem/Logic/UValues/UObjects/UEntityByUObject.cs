using System;
using AncientMemorial.Entities;
using UengSystem.Objects;
using UnityEngine;

namespace UengSystem.Logic.UValues.UObjects {
	[Serializable]
	public class UEntityByUObject : UValue<Entity> {
		public override bool getIsDynamic {
			get {
				obj.parent = this;
				return obj.isDynamicAuto;
			}
		}
		
		[SerializeReference][SubclassSelector]
		public UValue<Objects.UObject> obj;
		
		public override Entity getValue => (Entity)obj.value;
	}
}