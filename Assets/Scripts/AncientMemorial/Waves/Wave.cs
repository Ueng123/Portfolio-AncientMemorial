using UengSystem.Tasks;
using UengSystem.Tasks.Logic;
using UengSystem.Tasks.Logic.UValues.UBools;
using UnityEngine;

namespace AncientMemorial.Waves {
	[CreateAssetMenu(fileName = "New Wave", menuName = "Waves/New Wave")]
	public class Wave : ScriptableObject {
		[SerializeField]                        
		public Task         waveTasks;

		[SerializeReference] [SubclassSelector]
		public ConditionalTask[] alwaysConditionalTasks;
	}
}