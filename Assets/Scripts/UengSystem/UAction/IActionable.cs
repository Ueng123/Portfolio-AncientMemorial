namespace UengSystem.UAction {
	public interface IActionable {
		public void RegisterAction(UAction action);
		public void UnregisterAction(UAction action);
		public void StopAllUActions();
	}
}