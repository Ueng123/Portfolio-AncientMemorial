using System;
using AncientMemorial;
using UengSystem.Managers;
using UengSystem.Objects;
using UengSystem.Utility;
using UengSystem.VisualScripting.Tasks;
using UengSystem.VisualScripting.UValues;
using UengSystem.VisualScripting.UValues.UStrings;

// using UengSystem.VisualScripting.UValues.UStrings;
using UnityEngine;

namespace UengSystem.UI.UInputFields {
	[Serializable]
	public class UInputFieldAction : UUIAction {
		private int INPUT_FIELD_VALUE = "InputFieldValue".GetHash();
		
		public Task taskOnTextChanged;
		public Task taskOnEndEdit;

		[SerializeReference] [SubclassSelector]
		public UValue<string> initialText;

		private       UInputField inputField;
		public static UPureString inputFieldTextVariable;
		
		public override void Initialize(UObject self) {
			inputField = (UInputField)component;
			inputField.Initialize();
			inputField.SetText(initialText?.value??string.Empty);
			inputField.textChanged = false;
			
			// if (GameManager.UValueStringVariables.TryGetValue("InputFieldValue", out UValue<string> v)) {
			// 	inputFieldTextVariable = (UPureString)v;
			// } 
			// else {
			// 	GameManager.UValueStringVariables["InputFieldValue"] = new UPureString {
			// 		dynamicType = DynamicType.Dynamic,
			// 		pureValue   = ""
			// 	};
			//
			// 	inputFieldTextVariable = (UPureString)GameManager.UValueStringVariables["InputFieldValue"];
			// }
			inputFieldTextVariable ??= UPureString.GetPureValue(INPUT_FIELD_VALUE).SetDynamicCache(DynamicType.Dynamic).To<UPureString>();
		}

		public override void Uninitialize(UObject self) {
			component.Uninitialize();
		}

		public override void Routine(UObject self) {
			if (taskOnTextChanged!=null&&inputField.textChanged) {
				inputField.textChanged          = false;
				inputFieldTextVariable.pureValue = inputField.GetLiveText();
				taskOnTextChanged.Execute(GlobalObject.instance);
			}
			
			if (taskOnEndEdit!=null && inputField.endEdit) {
				inputField.endEdit              = false;
				inputFieldTextVariable.pureValue = inputField.GetText();
				taskOnEndEdit.Execute(GlobalObject.instance);
			}
		}
	}
}