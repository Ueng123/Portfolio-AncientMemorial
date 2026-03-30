using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AncientMemorial.Inputs {
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
						bool valueB = Mathf.Approximately(action.inputAction.ReadValue<float>(), 1);

						currInput.pressType = valueB ? 
																  (currInput.valueB?
																	   InputPressType.Hold:
																	   InputPressType.Down): 
																  (currInput.valueB? 
																	   InputPressType.Up:
																	   InputPressType.None);
						currInput.valueB = valueB;
						
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