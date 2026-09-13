namespace UengSystem.UI.UUIQueues {
	public readonly struct UUIQueueItem {

		// 인스턴스 프로퍼티
		public float marginFront { get; }
		public float marginBack { get; }
		public float duration { get; }
		public UUIQueueData data { get; }

		// 인스턴스 메서드
		public UUIQueueItem(float MarginFront, float MarginBack, float Duration, UUIQueueData Data) {
			marginFront = MarginFront;
			marginBack = MarginBack;
			duration = Duration;
			data = Data;
		}
	}
}