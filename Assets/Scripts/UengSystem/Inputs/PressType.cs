using System;

namespace UengSystem.Inputs {
	[Flags]
	public enum PressType {
		Down = 1, // the actual frame when value changed to 1
		Hold = 2, // when input value is 1 (not actual frame)
		Up   = 4,   // the actual frame when input changed to 0
		None = 8, // when input value is 0 (not actual frame)
		Value = 16 // having NOT-BOOL-Value
	}
}