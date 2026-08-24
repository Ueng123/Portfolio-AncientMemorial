using System;
using AncientMemorial.Entities;
using UnityEngine;

namespace UengSystem.VisualScripting.UValues.UObjects {
	[Serializable]
	public class UEntityByID : UValue<Entity> {
		protected override bool getIsDynamic {
			get {
				ID.parent = this;
				return ID.isDynamic;
			}
		}
		
		[SerializeReference][SubclassSelector]
		public UValue<string> ID;

		protected override Entity getValue => (Entity)Objects.UObject.GetUObject(ID.value);
	}
}