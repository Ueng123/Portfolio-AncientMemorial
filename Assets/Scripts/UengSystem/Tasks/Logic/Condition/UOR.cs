using UnityEngine;

namespace UengSystem.Tasks.Logic {
	public class UOR : UCondition {
		[SerializeReference] [SubclassSelector]
		public UCondition A;

		[SerializeReference] [SubclassSelector]
		public UCondition B;
		
		public override bool Check() {
			return A.Check() || B.Check();
		}
	}
}