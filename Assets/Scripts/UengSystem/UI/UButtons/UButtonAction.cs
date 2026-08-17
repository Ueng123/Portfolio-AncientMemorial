using System;
using System.Collections.Generic;
using UengSystem.Inputs;
using UengSystem.Logic.Tasks;
using UengSystem.Objects;
using UnityEngine.UI;

namespace UengSystem.UI.UButtons {
	[Serializable]
	public class UButtonAction : UUIAction {
		public static Dictionary<ActionType, List<int>> actionPriority = new ();
		public static int                                    currentBID     = 0;
		private int                                    BID;
		
		public ActionType actionType;
		public PressType  pressType;
		public Task            taskOnClicked;

		private UButton button;
		private Button  buttonObject;

		public override void Initialize(UObject self) {
			BID = currentBID++;
			List<int> BIDList;
			if (!actionPriority.TryGetValue(actionType, out BIDList)) {
				actionPriority[actionType] = BIDList = new List<int>();
			}
			
			BIDList.Add(BID);
			
			if (!component) return;
			button       = (UButton)component;
			button.Initialize();
			buttonObject = button.button;
		}

		public override void Uninitialize(UObject self) {
			actionPriority[actionType].Remove(BID);
			
			if (!component) return;
			component.Uninitialize();
		}

		public bool checkKeyPressed() {
			if (!actionPriority[actionType].Contains(BID)) return false;
			
			bool input           = InputManager.GetInput(actionType, pressType);
			bool isMainInput     = actionPriority[actionType][^1]               == BID;
			bool buttonClickable = !component || (buttonObject.enabled && buttonObject.interactable);
			bool result          = input && buttonClickable && isMainInput;
			
			if (result) button?.OnButtonClicked();
 			return result;
		}
		
		public override void Routine(UObject self) {
			bool needAction = checkKeyPressed() || (button&&button.clicked);
			
			if (needAction) {
                if (button) button.clicked = false;
                taskOnClicked.Execute(self);
			}
		}
	}
}