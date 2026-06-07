using System;
using System.Collections.Generic;
using UengSystem.Managers;
using UnityEngine;

namespace UengSystem.Inputs {
	public class InputManager : Manager<InputManager> {

		public        Camera                                 mainCamera;
		public        List<InputActions>                     inputActions;
		public static Dictionary<InputActionType, InputData> inputData;
		
		public override void Initialize() {
			inputData = new Dictionary<InputActionType, InputData>();
			
			foreach (InputActions action in inputActions) {
				action.inputAction.Enable();

				InputData initInput = new InputData {
					inputType = action.inputType,
					pressType = (action.inputType==InputType.Button)?
									InputPressType.None:
									InputPressType.Value
				};

				inputData[action.actionType] = initInput;
			}

			inputData[InputActionType.MousePosition] = new InputData {
				inputType = InputType.Axis2D,
				pressType = InputPressType.Value
			};
		}
		
		public override void Uninitialize() {
			foreach (InputActions action in inputActions) {
				action.inputAction.Disable();
			}
		}

		private void UpdateMousePosition() {
			inputData[InputActionType.MousePosition].valueV = mainCamera.ScreenToWorldPoint(Input.mousePosition);
		}
		
		public void UpdateInputs() {
			foreach (InputActions action in inputActions) {
				
				InputData currInput = inputData[action.actionType];
				
				switch (action.inputType) {
					case InputType.Button:
						bool Now = Mathf.Approximately(action.inputAction.ReadValue<float>(), 1);
						bool Pre = currInput.valueB;
						
						// 시점     | 현프레임 | 전프레임 | InputPressType
						// 눌림여부  |   O     |   O    |  Hold
						// 눌림여부  |   O     |   X    |  Down
						// 눌림여부  |   X     |   O    |  Up
						// 눌림여부  |   X     |   X    |  None

						if (Now  && Pre)  currInput.pressType = InputPressType.Hold;
						if (Now  && !Pre) currInput.pressType = InputPressType.Down;
						if (!Now && Pre)  currInput.pressType = InputPressType.Up;
						if (!Now && !Pre) currInput.pressType = InputPressType.None;
						
						currInput.valueB = Now;
						
						//Debug.Log($"{action.actionType} ({action.inputType}) = {valueB}");
						
						break;
					
					case InputType.Axis1D:
						float valueF = action.inputAction.ReadValue<float>();
						
						currInput.valueF = valueF;
						
						//Debug.Log($"{action.actionType} ({action.inputType}) = {valueF}");
							
						break;
					
					case InputType.Axis2D:
						Vector2 valueV = action.inputAction.ReadValue<Vector2>();

						currInput.valueV = valueV;
						
						//Debug.Log($"{action.actionType} ({action.inputType}) = {valueV}");
						
						break;
					
					default:
						throw new ArgumentOutOfRangeException();
				}
			}

			UpdateMousePosition();
		}
		
		public override void ManagerUpdate()      { }
		public override void ManagerFixedUpdate() { }
	}
}