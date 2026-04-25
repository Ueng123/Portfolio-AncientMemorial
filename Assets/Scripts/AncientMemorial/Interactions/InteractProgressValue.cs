using System;
using UengSystem.Objects;
using UengSystem.Tasks.Logic.UValues;
using UengSystem.Tasks.Logic.UValues.UStrings;
using UnityEngine;

namespace AncientMemorial.Interactions {
	[Serializable]
	public class InteractProgressValue : UValue<float> {
		public override bool getIsDynamic => true;

		[Header("id or obj")] [SerializeReference] [SubclassSelector]
		public UValue<string> id;

		public Interaction obj;

		public override float getValue {
			get {
				float ? result = null;

				if (obj) {
					result = obj.GetProgress();
				}

				if (id != null) {
					Debug.Log(id.value);
					result = (UObject.GetUObject(id.value) as Interaction)?.GetProgress();
				}

				if (result != null) return (float) result;
				else throw new Exception();
			}
		}
	}
}