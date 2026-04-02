using System;

namespace UengSystem.UI {
	[Serializable]
	public abstract class UIComponent {
		public UUI parent;
		
		public abstract void Initialize();
	}
}