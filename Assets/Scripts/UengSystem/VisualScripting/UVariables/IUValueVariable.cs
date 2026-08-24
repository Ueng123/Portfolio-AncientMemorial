using System;
using System.Collections.Generic;

namespace UengSystem.VisualScripting.UVariables {
	public interface IUValueVariable {
		public static List<Action> clearActions = new(10);

		public static void Clear() {
			foreach (Action action in clearActions) {
				action.Invoke();
			}
		}
	}
}