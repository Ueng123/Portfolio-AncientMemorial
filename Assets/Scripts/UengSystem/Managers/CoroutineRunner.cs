using UengSystem.Objects;

namespace UengSystem.Managers {
	public class CoroutineRunner : UObject {
		public static CoroutineRunner instance;

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