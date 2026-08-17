using System;
using AncientMemorial.Entities;
using UnityEngine;

namespace UengSystem.Logic.UValues.UObjects {
	[Serializable]
	public class UEntityByUObject : UValue<Entity> {
		protected override bool getIsDynamic {
			get {
				obj.parent = this;
				return obj.isDynamic;
			}
		}
		
		[SerializeReference][SubclassSelector]
		public UValue<Objects.UObject> obj;

		protected override Entity getValue => (Entity)obj.value;
	}
}