using System.Collections.Generic;
using UengSystem.Utility;
using UnityEngine;

namespace UengSystem.Managers {
	public interface IManager {

		// 정적 프로퍼티
		public static List<IManager> instances = new List<IManager>(16);

		// 인스턴스 메서드
		public void ManagerRoutine();
		public void ManagerFixedUpdate();
	}
}
