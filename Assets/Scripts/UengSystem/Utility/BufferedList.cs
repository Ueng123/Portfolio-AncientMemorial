using System.Collections.Generic;
using System.Linq;

namespace UengSystem.Utility {
	public class BufferedList<T> {
		
		private List<T> mainList;
		private Queue<(T, ModifyType)> modifyQueue;
		
		public BufferedList(List<T> mainList) {
			this.mainList = mainList;
			modifyQueue = new Queue<(T, ModifyType)>(10);
		}

		public BufferedList(T[] mainList) {
			this.mainList = mainList.ToList();
			modifyQueue   = new Queue<(T, ModifyType)>(10);
		}

		public BufferedList(int bufferSize) {
			mainList    = new List<T>(bufferSize);
			modifyQueue = new Queue<(T, ModifyType)>(10);
		}
		
		public BufferedList(int bufferSize, int bufferSize2) {
			mainList    = new List<T>(bufferSize);
			modifyQueue = new Queue<(T, ModifyType)>(bufferSize2);
		}

		public BufferedList() {
			mainList    = new List<T>(20);
			modifyQueue = new Queue<(T, ModifyType)>(10);
		}

		public T this[int index] => mainList[index];

		public List<T> GetList() => mainList;
		
		public void Apply() {
			while (modifyQueue.Count > 0) {
				(T item, ModifyType type) request = modifyQueue.Dequeue();
				if (request.type == ModifyType.Add) mainList.Add(request.item);
				if (request.type == ModifyType.Remove) mainList.Remove(request.item);
				if (request.type == ModifyType.Clear) mainList.Clear();
			}

			countOffset = 0;
		}

		public void Add   (T item) {
			countOffset += 1;
			modifyQueue.Enqueue((item, ModifyType.Add));
		}

		public void Remove(T item) {
			countOffset -= 1;
			modifyQueue.Enqueue((item, ModifyType.Remove));
		}

		public void Clear() {
			countOffset = -mainList.Count;
			modifyQueue.Enqueue((default, ModifyType.Clear));
		}

		public void ClearImmediately() {
			mainList.Clear();
			modifyQueue.Clear();
			countOffset = 0;
		}
		
		public int Count => mainList.Count;

		public int countOffset = 0;
		public int PotentialCount => mainList.Count + countOffset;
	}
}