using System.Collections.Generic;

namespace UengSystem.UI.UUIQueues {
	public class UUIQueue {
		public Queue<UUIQueueItem> queue = new Queue<UUIQueueItem>();

		public void GetNextQueue() {
			if (queue.Count == 0) return;
			
			UUIQueueItem item = queue.Dequeue();
			
			
		}
	}
}