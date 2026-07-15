using System;
using System.Collections.Generic;
using UengSystem.Logic.Tasks;
using UengSystem.Objects;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UengSystem.Managers {
	public abstract class Manager<T> : MonoBehaviour, IInitializable, IManager, ITaskable where T : Manager<T> {
		public static T instance;
		
		public virtual void Awake() {
			instance = (T)this;
			
			IManager.instances ??= new List<IManager>();
			IManager.instances.Add(instance);
		}
		
		private void OnDestroy() {
			instance = null;
			Uninitialize();
		}
		
		public virtual void Initialize()   { }
		public virtual void Uninitialize() { }
		
		public abstract void ManagerUpdate();
		public abstract void ManagerFixedUpdate();
	}
}