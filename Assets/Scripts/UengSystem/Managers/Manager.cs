using System;
using System.Collections.Generic;
using UengSystem.Objects;
using UengSystem.Tasks;
using UnityEngine;

namespace UengSystem.Managers {
	public abstract class Manager<T> : MonoBehaviour, IInitializable, IManager, ITaskable where T : Manager<T> {
		public static T instance;
		
		public virtual void Awake() {
			instance = (T)this;
			
			IManager.instances ??= new List<IManager>();
			IManager.instances.Add(instance);
		}

		private void OnDestroy() {
			Uninitialize();
		}

		public virtual void Initialize()   { }
		public virtual void Uninitialize() { }

		public abstract void ManagerUpdate();
		public abstract void ManagerFixedUpdate();
	}
}