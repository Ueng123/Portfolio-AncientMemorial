using System;
using System.Collections.Generic;

namespace UengSystem.VisualScripting.UVariables {
	public interface IUValueVariable {

		// 정적 프로퍼티
		public static List<Action> clearActions = new(10);

		// 정적 메서드
		public static void Clear() {
			foreach (Action action in clearActions) {
				action.Invoke();
			}
		}
	}
}