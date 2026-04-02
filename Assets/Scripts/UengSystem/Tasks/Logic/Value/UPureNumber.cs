using System;

namespace UengSystem.Tasks.Logic.Value {
	[Serializable]
	public class UPureNumber : UNumber {
		public          float value;
		public override float GetValue() => value;
	}
}