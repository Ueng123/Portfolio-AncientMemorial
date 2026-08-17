using System;
using System.Collections.Generic;
using UengSystem.Managers;
using UnityEngine;
using UnityEngine.InputSystem;

namespace UengSystem.Inputs {
	public class InputManager : Manager<InputManager> {

		public        Camera                                 mainCamera;
		public        List<InputActions>                     inputActions;
		private static Dictionary<ActionType, InputData>     inputData;
		
		public static void Clear() {
			inputData?.Clear();
		}

		public static Vector2 mousePosition => inputData[ActionType.MousePosition].valueV;
		
		public static bool GetInput(ActionType actionType, PressType pressType) {
			return (inputData[actionType].pressType & pressType)!=0;
		}

		public static bool GetInput(ActionType actionType) {
			return inputData[actionType].valueB;
		}
		
		public static float GetValue(ActionType actionType) {
			return inputData[actionType].valueF;
		}
		
		public static Vector2 GetVector(ActionType actionType) {
			return inputData[actionType].valueV;
		}
		
		public override void Initialize() {
			inputData  = new Dictionary<ActionType, InputData>();

			for (int i = 0; i < inputActions.Count; i++) {
				InputActions action = inputActions[i];
				action.inputAction.Enable();

				InputData initInput = new InputData {
					inputType = action.inputType,
					pressType = (action.inputType == InputType.Button) ? PressType.None : PressType.Value
				};

				inputData[action.actionType] = initInput;
			}

			inputData[ActionType.MousePosition] = new InputData {
				inputType = InputType.Axis2D,
				pressType = PressType.Value
			};
		}
		
		public override void Uninitialize() {
			for (int i = 0; i < inputActions.Count; i++) {
				InputActions action = inputActions[i];
				action.inputAction.Disable();
			}
		}

		private void UpdateMousePosition() {
			inputData[ActionType.MousePosition].valueV = mainCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
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

						if (Now  && Pre)  currInput.pressType = PressType.Hold;
						if (Now  && !Pre) currInput.pressType = PressType.Down;
						if (!Now && Pre)  currInput.pressType = PressType.Up;
						if (!Now && !Pre) currInput.pressType = PressType.None;
						
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
	}
}