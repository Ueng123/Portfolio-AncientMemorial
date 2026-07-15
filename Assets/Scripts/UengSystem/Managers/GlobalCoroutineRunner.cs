using UengSystem.Objects;

namespace UengSystem.Managers {
	public class GlobalCoroutineRunner : UObject {
		public static GlobalCoroutineRunner instance;

		public override void Initialize() {
			base.Initialize();
			instance = this;
		}

		public override void Uninitialize() {
			base.Initialize();
			instance = null;
		}
	}
}