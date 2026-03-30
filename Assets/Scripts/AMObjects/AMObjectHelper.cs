using AncientMemorial.AMObjects;
using UnityEngine;

namespace AncientMemorial.AMObjects {
	public class AMObjectHelper : MonoBehaviour {
		private AdvancedAMObject script;
		
		[Header("InitialObject")]
		public bool   InitialObject;
		public string ID;
		public string Category;
		
		public void Start() {
			script = GetComponent<AdvancedAMObject>();
			
			if (!InitialObject) return;
			script.OnFirstGet();
			script.OnGet();
			script.Initialize();
			script.ID = ID;
			script.Category = Category;
		}
	}
}