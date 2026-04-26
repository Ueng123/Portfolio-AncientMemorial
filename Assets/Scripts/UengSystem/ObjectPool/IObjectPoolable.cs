using UnityEngine;

namespace UengSystem.ObjectPool {
	public interface IObjectPoolable {
		public bool gettable { get; set; }

		public void OnFirstGet();
		public void Get(float time);
		public void Release(float time);
	}
}