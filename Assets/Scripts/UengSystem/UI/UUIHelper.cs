using UnityEngine;

namespace UengSystem.UI {
	public class UUIHelper : MonoBehaviour {

		// 인스턴스 프로퍼티
		private UUI script;
		
		[Header("InitialObject")]
		public bool   InitialObject;
		public string ID;
		public string Category;

		// 인스턴스 메서드
		public void Start() {
			script = GetComponent<UUI>();
			
			if (!InitialObject) return;
			
			script.ID        = ID;
			script.Category  = "UI";
			script.UCategory = Category;
			script.canvas = GetComponentInParent<UCanvas>();
			script.Get(PlayEffect: false);
		}
	}
}
