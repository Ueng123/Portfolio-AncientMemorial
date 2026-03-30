using AncientMemorial.Tasks;
using UnityEngine;
using UnityEngine.Serialization;

namespace AncientMemorial.Waves {
	[CreateAssetMenu(fileName = "New Wave", menuName = "Waves/New Wave")]
	public class Wave : ScriptableObject {
		[SerializeField]                        
		public TaskList         waveTasks;
		
		[SerializeReference] [SubclassSelector]
		public WaveCondition[] condition;

		public bool instantNextWave;

		public string crystalInteractText;
	}
}