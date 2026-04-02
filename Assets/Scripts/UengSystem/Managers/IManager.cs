using System.Collections.Generic;

namespace UengSystem.Managers {
	public interface IManager {
		public static List<IManager> instances;

		public void Initialize();
		public void Uninitialize();

		public void ManagerUpdate();
		public void ManagerFixedUpdate();
	}
}