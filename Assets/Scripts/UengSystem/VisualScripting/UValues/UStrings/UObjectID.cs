using System;
using UengSystem.Objects;
using UnityEngine;

namespace UengSystem.VisualScripting.UValues.UStrings {
	[Serializable]
	public class UObjectID : UValue<string> {

		// 인스턴스 프로퍼티
		protected override bool   getIsDynamic => false;

		[SerializeReference][SubclassSelector]
		public UValue<UObject> obj;

		protected override string getValue => obj.value.ID;
	}
}