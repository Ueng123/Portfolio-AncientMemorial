using UengSystem.Objects;

namespace UengSystem.Managers {
	public class GlobalObject : UObject {
		public static GlobalObject instance;

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