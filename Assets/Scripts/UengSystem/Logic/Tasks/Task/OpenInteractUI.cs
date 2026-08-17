using UengSystem.Logic.Tasks;
using UengSystem.Logic.UValues;
using UengSystem.Logic.UValues.UStrings;
using UengSystem.Objects;

namespace AncientMemorial.Tasks {
	public class OpenInteractUI : TaskComponent {
		public UValue<UObject> interact;
		
		public override void Execute(ITaskable self) {
			GameManager.UValueStringVariables["InteractTargetObject"] = new UPureString { Text = interact.value.ID };
		}
	}
}