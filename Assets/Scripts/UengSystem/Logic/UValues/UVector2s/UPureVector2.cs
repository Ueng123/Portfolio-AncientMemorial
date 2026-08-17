using System;
using UnityEngine;

namespace UengSystem.Logic.UValues.UVector2s {
	[Serializable]
	public class UPureVector2 : UValue<Vector2> {
		protected override bool    getIsDynamic => false;

		public Vector2 vector2;

		protected override Vector2 getValue => vector2; 
	}
}