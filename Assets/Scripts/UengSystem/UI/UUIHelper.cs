using UnityEngine;

namespace UengSystem.UI {
	public class UUIHelper : MonoBehaviour {
		private UUI script;
		
		[Header("InitialObject")]
		public bool   InitialObject;
		public string ID;
		public string Category;
		
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
