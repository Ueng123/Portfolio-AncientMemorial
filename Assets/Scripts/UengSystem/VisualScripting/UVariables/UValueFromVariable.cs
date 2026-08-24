using System;
using UengSystem.UDebug;
using UengSystem.Utility;
using UengSystem.VisualScripting.UValues;
using UnityEngine;

namespace UengSystem.VisualScripting.UVariables {
	[Serializable]
	public abstract class UValueFromVariable<T> : UValue<T> {
		protected override bool getIsDynamic => true;

		public  string variableName;

		private int? _variableID;
		private int variableID {
			get {
				_variableID ??= variableName.GetHash();
				return _variableID.Value;
			}
		}

		public override void Dirty() {
			 base.Dirty();
			 _variableID = null;
		}

		public override void OnBeforeSerialize() {
			base.OnBeforeSerialize();
			_variableID = null;
		}

		public override void OnAfterDeserialize() {
			base.OnAfterDeserialize();
			_variableID = null;
		}

		protected override T getValue => UVariable<T>.GetValue(variableID);
	}
}