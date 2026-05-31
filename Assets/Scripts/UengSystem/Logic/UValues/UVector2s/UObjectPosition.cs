using System;
using UengSystem.Objects;
using UnityEngine;

namespace UengSystem.Logic.UValues.UVector2s {
	[Serializable]
	public class UObjectPosition : UValue<Vector2> {
		public override bool getIsDynamic => true;

		[SerializeReference] [SubclassSelector]
		public UValue<UObject> targetObject;

		public override Vector2 getValue => targetObject.value.transform.position;
	}
}