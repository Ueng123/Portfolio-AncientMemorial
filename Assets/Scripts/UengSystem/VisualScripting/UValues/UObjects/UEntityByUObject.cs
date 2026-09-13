using System;
using AncientMemorial.Entities;
using UnityEngine;

namespace UengSystem.VisualScripting.UValues.UObjects {
	[Serializable]
	public class UEntityByUObject : UValue<Entity> {

		// 인스턴스 프로퍼티
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