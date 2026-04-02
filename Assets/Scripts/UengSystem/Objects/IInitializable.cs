using System;
using System.Collections.Generic;

namespace UengSystem.Objects {
	public interface IInitializable {
		public static List<Action> initializeActions = new List<Action>();
		public        void         Initialize();
		public        void         Uninitialize();
	}
}