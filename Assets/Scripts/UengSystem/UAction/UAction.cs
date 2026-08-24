using System.Collections;

namespace UengSystem.UAction {
	public abstract class UAction {
		public static int  runningActionCount;

		public bool Executing;

		public void Execute(IActionable executor) {
			executor?.RegisterAction(this);
		}
		public    abstract void        Done();
		public    abstract void        Cancel();
		protected abstract IEnumerator ActionEnumerator();
	}
}