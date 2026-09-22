using System;
using UengSystem.UDebug;
using UengSystem.Utility;
using UengSystem.VisualScripting.Tasks;
using UengSystem.VisualScripting.UValues;
using UengSystem.VisualScripting.UValues.UBools;
using UengSystem.VisualScripting.UValues.UColors;
using UengSystem.VisualScripting.UValues.UFloats;
using UengSystem.VisualScripting.UValues.UStrings;
using UengSystem.VisualScripting.UValues.UVector2s;
using UnityEngine;

namespace UengSystem.VisualScripting.UVariables {
	[Serializable]
	public abstract class SetUVariable<T> : TaskComponent {

		// 인스턴스 프로퍼티
		public  string variableName;

		private int? VariableId;
		protected int variableID {
			get {
				VariableId ??= variableName.GetHash();
				return VariableId.Value;
			}
		}

		public override void OnAfterDeserialize() {
			base.OnAfterDeserialize();
			VariableId = null;
		}
		
		[SerializeReference] [SubclassSelector]
		public UValue<T> value;

		public override void Execute(ITaskable self) {
			UVariable<T>.Set(variableID, value.OnSet());
		}
	}
}
