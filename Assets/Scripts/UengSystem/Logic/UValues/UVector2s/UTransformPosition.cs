using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace UengSystem.Logic.UValues.UVector2s {
	[Serializable]
	public class UTransformPosition : UValue<Vector2> {
		protected override bool getIsDynamic => true;
		
		[FormerlySerializedAs("targetObject")] public Transform targetTransform;

		protected override Vector2 getValue => targetTransform.position;
	}
}