using System;
using System.Collections.Generic;
using UengSystem.Managers;
using UnityEngine;
using UnityEngine.InputSystem;

namespace UengSystem.Inputs {
	public class InputManager : Manager<InputManager> {

		// 정적 프로퍼티
		private static Dictionary<ActionType, InputData>     inputData;

		public static Vector2 mousePosition => GetVector(ActionType.MousePosition);

		// 인스턴스 프로퍼티
		public        Camera                                 mainCamera;
		public        List<InputActions>                     inputActions;

		// 정적 메서드
		public static bool GetInput(ActionType actionType, InputState inputState) {
			return (inputData[actionType].inputState & inputState)!=0;
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

		public static void Clear() {
			inputData?.Clear();
		}
		
		// 인스턴스 메서드
		private void UpdateMousePosition() {
			inputData[ActionType.MousePosition].valueV = mainCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
		}
		
		public void UpdateInputs() {
			foreach (InputActions action in inputActions) {
				InputData currInput = inputData[action.actionType];
				
				switch (action.inputType) {
					case InputType.Button:
						bool Now = action.inputAction.IsPressed();
						bool Pre = currInput.valueB;

						if (Now  && Pre)  currInput.inputState = InputState.Hold;
						if (Now  && !Pre) currInput.inputState = InputState.Down;
						if (!Now && Pre)  currInput.inputState = InputState.Up;
						if (!Now && !Pre) currInput.inputState = InputState.None;
						
						currInput.valueB = Now;
						
						break;
					
					case InputType.Axis1D:
						float valueF = action.inputAction.ReadValue<float>();
						
						currInput.valueF = valueF;
							
						break;
					
					case InputType.Axis2D:
						Vector2 valueV = action.inputAction.ReadValue<Vector2>();

						currInput.valueV = valueV;
						
						break;
					
					default:
						throw new ArgumentOutOfRangeException();
				}
			}

			UpdateMousePosition();
		}

		// 오버라이드 메서드
		public override void Initialize() {
			inputData  = new Dictionary<ActionType, InputData>();

			for (int i = 0; i < inputActions.Count; i++) {
				InputActions action = inputActions[i];
				action.inputAction.Enable();

				InputData initInput = new InputData {
					inputState = (action.inputType == InputType.Button) ? InputState.None : InputState.Value
				};

				inputData[action.actionType] = initInput;
			}

			inputData[ActionType.MousePosition] = new InputData {
				inputState = InputState.Value
			};
		}
		
		public override void Uninitialize() {
			for (int i = 0; i < inputActions.Count; i++) {
				InputActions action = inputActions[i];
				action.inputAction.Disable();
			}
		}
	}
}