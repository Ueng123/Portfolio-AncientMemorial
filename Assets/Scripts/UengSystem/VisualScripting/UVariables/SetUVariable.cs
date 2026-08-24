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
		
		public  string variableName;

		private int? _variableID;
		protected int variableID {
			get {
				_variableID ??= variableName.GetHash();
				return _variableID.Value;
			}
		}

		[SerializeReference] [SubclassSelector]
		public UValue<T> value;
		
		public override void OnBeforeSerialize() {
			base.OnBeforeSerialize();
			_variableID = null;
		}

		public override void OnAfterDeserialize() {
			base.OnAfterDeserialize();
			_variableID = null;
		}

		public override void Execute(ITaskable self) {
			UVariable<T>.Set(variableID, value.OnSet());
		}
	}
}