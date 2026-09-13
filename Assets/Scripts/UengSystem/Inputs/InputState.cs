using System;

namespace UengSystem.Inputs {
	[Flags]
	public enum InputState {
		Down = 1,  // 눌린 프레임
		Hold = 2,  // 눌려있음
		Up   = 4,  // 뗀 프레임
		None = 8,  // 떼져있음
		Value = 16 // 버튼 아님
	}
}