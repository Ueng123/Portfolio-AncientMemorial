using System;
using UengSystem.Objects;
using UengSystem.Tasks.Logic.String;
using UengSystem.Tasks.Logic.Value;
using UnityEngine;

namespace AncientMemorial.Interactions {
	[Serializable]
	public class InteractProgressValue : UNumber {
		[Header("id or obj")]
		[SerializeReference][SubclassSelector]
		public UString id;
		public Interaction obj;
		
		public override float GetValue() {
			float? result = null;
			
			if (obj) {
				result = obj.GetProgress();
			}

			if (id != null) {
				result = (UObject.GetUObject(id.GetText()) as Interaction)?.GetProgress();
			}

			if (result != null) return (float)result;
			else throw new Exception();
		}
	}
}