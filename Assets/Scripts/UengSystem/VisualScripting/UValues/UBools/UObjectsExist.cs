using System;
using UnityEngine;

namespace UengSystem.VisualScripting.UValues.UBools {
	[Serializable]
	public class UObjectsExist : UValue<bool> {

		// 인스턴스 프로퍼티
		protected override bool getIsDynamic => true;

		[SerializeReference] [SubclassSelector]
		public UValue<string> category;

		protected override bool getValue => Objects.UObject.UObjectsExist(category.value);
	}
}