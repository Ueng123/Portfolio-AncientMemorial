using UnityEngine;

namespace UengSystem.Objects {
	public class UObjectHelper : MonoBehaviour {

		// 인스턴스 프로퍼티
		private UObject script;
		
		[Header("InitialObject")]
		public bool   InitialObject;
		public string ID;
		public string Category;
		
		private bool initialized = false;

		// 인스턴스 메서드
		public void Update() {
			if (initialized) return;
			initialized = true;
			
			script = GetComponent<UObject>();
			
			if (!InitialObject) return;
			if (ID       != "") script.ID       = ID;
			if (Category != "") script.Category = Category;
			script.Get(PlayEffect: false);
		}
	}
}
