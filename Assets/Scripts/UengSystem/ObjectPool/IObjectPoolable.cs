namespace UengSystem.ObjectPool {
	public interface IObjectPoolable {
		public bool isReleased { get; }

		public void OnFirstGet();
		public void Get(bool PlayEffect = true);
		public void Release(bool PlayEffect = true);
	}
}
