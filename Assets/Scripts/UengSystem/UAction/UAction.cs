using System.Collections;

namespace UengSystem.UAction {
	public abstract class UAction {

		// 정적 프로퍼티
		public static int  runningActionCount;

		// 인스턴스 프로퍼티
		public bool Executing;

		// 인스턴스 메서드
		public void Execute(IActionable executor) {
			executor?.RegisterAction(this);
		}
		public    abstract void        Done();
		public    abstract void        Cancel();
		protected abstract IEnumerator ActionEnumerator();
	}
}