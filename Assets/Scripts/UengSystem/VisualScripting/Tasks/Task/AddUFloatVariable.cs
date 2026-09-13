using System;
using UengSystem.VisualScripting.UVariables;
// using UengSystem.VisualScripting.UValues.UFloats;

namespace UengSystem.VisualScripting.Tasks {
	[Serializable]
	public class AddUFloatVariable : SetUVariable<float> {

		// 오버라이드 메서드
		public override void Execute(ITaskable self) {
			UVariable<float>.Set(variableID, value);
		}
	}
}