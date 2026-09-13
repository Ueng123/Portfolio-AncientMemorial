using UengSystem.VisualScripting.Tasks;
using UnityEngine;

namespace AncientMemorial.Waves {
	[CreateAssetMenu(fileName = "New Wave", menuName = "Waves/New Wave")]
	public class Wave : ScriptableObject {

		// 인스턴스 프로퍼티
		[SerializeField]                        
		public Task         waveTasks;

		[SerializeReference] [SubclassSelector]
		public ConditionalTask[] alwaysConditionalTasks;
	}
}