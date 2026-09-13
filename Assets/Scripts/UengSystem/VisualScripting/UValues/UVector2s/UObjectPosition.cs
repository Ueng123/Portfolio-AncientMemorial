using System;
using UengSystem.Objects;
using UnityEngine;

namespace UengSystem.VisualScripting.UValues.UVector2s {
	[Serializable]
	public class UObjectPosition : UValue<Vector2> {

		// 인스턴스 프로퍼티
		protected override bool getIsDynamic => true;

		[SerializeReference] [SubclassSelector]
		public UValue<UObject> targetObject;

		protected override Vector2 getValue => targetObject.value.transform.position;
	}
}