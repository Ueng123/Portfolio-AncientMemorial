namespace UengSystem.ObjectPool {
	public interface IObjectPoolable {

		// 인스턴스 프로퍼티
		public bool isReleased { get; }

		// 인스턴스 메서드
		public void OnFirstGet();
		public void Get(bool PlayEffect = true);
		public void Release(bool PlayEffect = true);
	}
}
