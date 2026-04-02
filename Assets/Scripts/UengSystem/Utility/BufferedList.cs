using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace UengSystem.Utility {
	public class BufferedList<T> : IEnumerable<T> {
		
		private List<T> mainList = new List<T>();
		private List<T> AddQueue = new List<T>();
		private List<T> RemoveQueue = new List<T>();
		
		public BufferedList(List<T> mainList) => this.mainList = mainList;
		public BufferedList(T[] mainList) => this.mainList = mainList.ToList();
		public BufferedList() { }

		public T this[int index] => mainList[index];

		public List<T> GetList() => mainList;

		public void ApplyAdd() {
			foreach (T item in AddQueue) {
				mainList.Add(item);
			}
			
			AddQueue.Clear();
		}

		public void ApplyRemove() {
			foreach (T item in RemoveQueue) {
				mainList.Remove(item);
			}
			
			RemoveQueue.Clear();
		}
		
		public void Apply() {
			ApplyAdd();
			ApplyRemove();
		}

		public void Add   (T item) => AddQueue   .Add(item);
		public void Remove(T item) => RemoveQueue.Add(item);
		
		public IEnumerator<T>   GetEnumerator() {
			return new DefaultEnumerator<T>(mainList);
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}
	}
}