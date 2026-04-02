using UengSystem.Objects;
using UnityEngine;

namespace UengSystem.UI {
	public abstract class UUI : AdvancedUObject {
		[SerializeReference][SubclassSelector]
		public UIAction[] actions;
		
		public override void OnGet() {
			base.OnGet();
		}

		public override void OnRelease() {
			base.OnRelease();
		}
	}
}