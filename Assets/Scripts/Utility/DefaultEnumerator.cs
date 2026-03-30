using System.Collections;
using System.Collections.Generic;

namespace AncientMemorial {
	public class DefaultEnumerator<T> : IEnumerator<T> {
		
		private List<T> L;

		public DefaultEnumerator(List<T> l) {
			this.L = l;
		}
		
		private int index = -1;
		
		public bool        MoveNext() {
			index++;
			return (index < L.Count);
		}
		
		public void        Reset() {
			index = -1;
		}
		
		public T Current => L[index];
		
		object IEnumerator.Current => Current;
		
		public void        Dispose() { }
	}
}