using System.Collections.Generic;
using UengSystem.Objects;
using UengSystem.VisualScripting.Tasks;
using UnityEngine;

namespace UengSystem.Managers {
	public abstract class Manager<T> : MonoBehaviour, IManager, ITaskable where T : Manager<T> {
		public static  T    instance;
		
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

		public virtual void ManagerUpdate() { }

		public virtual void ManagerFixedUpdate() { }
	}
}