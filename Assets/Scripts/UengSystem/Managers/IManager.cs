using System.Collections.Generic;
using UengSystem.Utility;
using UnityEngine;

namespace UengSystem.Managers {
	public interface IManager {
		public static List<IManager>         instances                   = new List<IManager>(16);

		public void Initialize();
		public void Uninitialize();

		public void ManagerUpdate();
		public void ManagerFixedUpdate();
	}
}