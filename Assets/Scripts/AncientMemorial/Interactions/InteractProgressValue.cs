using System;
using UengSystem.Logic.UValues;
using UengSystem.Objects;
using UnityEngine;

namespace AncientMemorial.Interactions {
	[Serializable]
	public class InteractProgressValue : UValue<float> {
		public override bool getIsDynamic => true;

		[SerializeReference] [SubclassSelector]
		public UValue<UObject> interactObject;

		public override float getValue {
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