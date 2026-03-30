using UnityEngine;

namespace AncientMemorial.ObjectPool {
	public interface IObjectPoolable {
		public void OnFirstGet();
		public void Get(float time);
		public void Release(float time);
	}
}