using UengSystem.VisualScripting.Tasks;
using UnityEngine;

namespace UengSystem.Managers {
	public abstract class Manager<T> : MonoBehaviour, IManager, ITaskable where T : Manager<T> {

		// 정적 프로퍼티
		public static  T    instance;

		// 인스턴스 메서드
		public virtual void Awake() {
			instance = (T)this;
			
			IManager.instances.Add(instance);
		}
		
		private void OnDestroy() {
			instance = null;
			Uninitialize();
		}
		
		public virtual void Initialize()   { }
		public virtual void Uninitialize() { }

		public virtual void ManagerRoutine() { }
		public virtual void ManagerFixedUpdate() { }
	}
}
