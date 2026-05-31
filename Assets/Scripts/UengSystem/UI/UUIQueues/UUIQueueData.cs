namespace UengSystem.UI.UUIQueues {
	public abstract record UUIQueueData() {
		public abstract void PreGet();
		public abstract void Initialize(UUI obj);
	};
}