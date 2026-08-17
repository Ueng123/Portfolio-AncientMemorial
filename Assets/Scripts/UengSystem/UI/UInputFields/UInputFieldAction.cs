using System;
using AncientMemorial;
using UengSystem.Logic.Tasks;
using UengSystem.Logic.UValues;
using UengSystem.Logic.UValues.UStrings;
using UengSystem.Managers;
using UengSystem.Objects;
using UnityEngine;

namespace UengSystem.UI.UInputFields {
	[Serializable]
	public class UInputFieldAction : UUIAction {
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
			
			if (GameManager.UValueStringVariables.TryGetValue("InputFieldValue", out UValue<string> v)) {
				inputFieldTextVariable = (UPureString)v;
			} 
			else {
				GameManager.UValueStringVariables["InputFieldValue"] = new UPureString {
					dynamicType = DynamicType.Dynamic,
					Text        = ""
				};

				inputFieldTextVariable = (UPureString)GameManager.UValueStringVariables["InputFieldValue"];
			}
		}

		public override void Uninitialize(UObject self) {
			component.Uninitialize();
		}

		public override void Routine(UObject self) {
			if (taskOnTextChanged!=null&&inputField.textChanged) {
				inputField.textChanged      = false;
				inputFieldTextVariable.Text = inputField.GetLiveText();
				taskOnTextChanged.Execute(CoroutineRunner.instance);
			}
			
			if (taskOnEndEdit!=null && inputField.endEdit) {
				inputField.endEdit          = false;
				inputFieldTextVariable.Text = inputField.GetText();
				taskOnEndEdit.Execute(CoroutineRunner.instance);
			}
		}
	}
}