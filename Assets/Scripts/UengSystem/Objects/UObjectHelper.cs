using UnityEngine;

namespace UengSystem.Objects {
	public class UObjectHelper : MonoBehaviour {
		private UObject script;
		
		[Header("InitialObject")]
		public bool   InitialObject;
		public string ID;
		public string Category;
		
		private bool initialized = false;
		
		public void Update() {
			if (initialized) return;
			initialized = true;
			
			script = GetComponent<UObject>();
			
			if (!InitialObject) return;
			script.OnFirstGet();
			script.OnGet();
			script.Initialize();
			script.ID = ID;
			script.Category = Category;
		}
	}
}