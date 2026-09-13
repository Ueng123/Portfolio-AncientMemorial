using UengSystem.Objects;

namespace UengSystem.Managers {
	public class GlobalObject : UObject {

		// 정적 프로퍼티
		public static GlobalObject instance;

		// 오버라이드 메서드
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