using UnityEngine;

namespace UengSystem.Objects {
	public class UObjectHelper : MonoBehaviour {
		private AdvancedUObject script;
		
		[Header("InitialObject")]
		public bool   InitialObject;
		public string ID;
		public string Category;
		
		private bool initialized = false;
		
		public void Update() {
			if (initialized) return;
			initialized = true;
			
			script = GetComponent<AdvancedUObject>();
			
			if (!InitialObject) return;
			script.OnFirstGet();
			script.OnGet();
			script.Initialize();
			script.ID = ID;
			script.Category = Category;
		}
	}
}