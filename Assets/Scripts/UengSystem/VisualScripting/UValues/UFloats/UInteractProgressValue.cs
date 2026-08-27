using System;
using AncientMemorial.Interactions;
using UengSystem.Objects;
using UnityEngine;

namespace UengSystem.VisualScripting.UValues.UFloats {
	[Serializable]
	public class UInteractProgressValue : UValue<float> {
		protected override bool getIsDynamic => true;

		[SerializeReference] [SubclassSelector]
		public UValue<UObject> interactObject;

		protected override float getValue {
			get {
				float? result = null;

				if (interactObject != null) {
					result = (interactObject.value as Interaction)?.GetProgress();
				}

				if (result != null) return (float)result;
				else throw new Exception();
			}
		}
	}
}