using UengSystem.States;

namespace AncientMemorial.States {
	public interface IStateObject {

		// 인스턴스 프로퍼티
		public UState entityState { get; set; }
	}
}