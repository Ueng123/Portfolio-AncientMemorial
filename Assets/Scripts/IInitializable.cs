using System;
using System.Collections.Generic;


namespace AncientMemorial {
	public interface IInitializable {
		public static List<Action> initializeActions = new List<Action>();
		public        void         Initialize();
		public        void         Uninitialize();
	}
}