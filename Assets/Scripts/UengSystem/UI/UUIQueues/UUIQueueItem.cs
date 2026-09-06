namespace UengSystem.UI.UUIQueues {
	public readonly struct UUIQueueItem {
		public float marginFront { get; }
		public float marginBack { get; }
		public float duration { get; }
		public UUIQueueData data { get; }

		public UUIQueueItem(float MarginFront, float MarginBack, float Duration, UUIQueueData Data) {
			marginFront = MarginFront;
			marginBack = MarginBack;
			duration = Duration;
			data = Data;
		}
	}
}