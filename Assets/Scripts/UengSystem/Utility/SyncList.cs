using System.Collections.Generic;
using System.Linq;

namespace UengSystem.Utility {
	public class SyncList<T> {
		
		private List<T> mainList;
		private List<T> actualList;
		
		public SyncList(List<T> mainList) {
			this.mainList = mainList;
			actualList    = new List<T>(mainList);
		}

		public SyncList(T[] mainList) {
			this.mainList = mainList.ToList();
			actualList    = new List<T>(mainList);
		}

		public SyncList(int bufferSize) {
			mainList   = new List<T>(bufferSize);
			actualList = new List<T>(bufferSize);
		}
		
		public SyncList() {
			mainList   = new List<T>(20);
			actualList = new List<T>(20);
		}

		public T this[int index] => mainList[index];

		public List<T> GetList() => mainList;
		
		public void Synchronize() {
			mainList.Clear();
			mainList.AddRange(actualList);
		}

		public void Add   (T item) {
			actualList.Add(item);
		}

		public void Remove(T item) {
			actualList.Remove(item);
		}

		public void Clear() {
			actualList.Clear();
		}

		public void ClearImmediately() {
			mainList.Clear();
			actualList.Clear();
		}

		public bool Contains(T item) {
			return actualList.Contains(item);
		}
		
		public int Count => mainList.Count;

		public int PotentialCount => actualList.Count;
	}
}