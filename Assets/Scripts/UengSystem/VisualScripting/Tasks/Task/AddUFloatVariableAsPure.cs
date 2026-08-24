using System;
using UengSystem.VisualScripting.UValues.UFloats;
using UengSystem.VisualScripting.UVariables;
// using UengSystem.VisualScripting.UValues.UFloats;

namespace UengSystem.VisualScripting.Tasks {
	[Serializable]
	public class AddUFloatVariableAsPure : SetUVariable<float> {
		public override void Execute(ITaskable self) {
			UPureFloat.AddValue(variableID, value.value);
		}
	}
}