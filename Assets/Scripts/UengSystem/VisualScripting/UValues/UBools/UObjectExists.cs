using System;
using UnityEngine;

namespace UengSystem.VisualScripting.UValues.UBools {
	[Serializable]
	public class UObjectExists : UValue<bool> {

		// 인스턴스 프로퍼티
		protected override bool getIsDynamic => true;

		[SerializeReference] [SubclassSelector]
		public UValue<string> id;

		protected override bool getValue => Objects.UObject.UObjectExists(id.value);
	}
}