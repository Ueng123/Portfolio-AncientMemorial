using System;

namespace UengSystem.Tasks.Logic.String {
	[Serializable]
	public class UPureString : UStringComponent {
		public          string Text;
		public override string GetText() => Text;
	}
}