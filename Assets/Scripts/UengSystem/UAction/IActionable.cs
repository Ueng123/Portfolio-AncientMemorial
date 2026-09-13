namespace UengSystem.UAction {
	public interface IActionable {

		// 인스턴스 메서드
		public void RegisterAction(UAction action);
		public void UnregisterAction(UAction action);
		public void StopAllUActions();
	}
}