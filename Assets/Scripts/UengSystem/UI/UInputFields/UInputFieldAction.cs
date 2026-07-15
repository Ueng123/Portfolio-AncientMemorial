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
			component.Initialize();
			
			inputField = (UInputField)component;
			if (initialText!=null) inputField.SetText(initialText.value);
			
			if (GameManager.UValueStringVariables.TryGetValue("InputFieldValue", out UValue<string> v)) {
				inputFieldTextVariable = (UPureString)v;
			} 
			else {
				GameManager.UValueStringVariables["InputFieldValue"] = new UPureString {
					dynamicType = DynamicType.Dynamic,
					Text        = ""
				};
			}
		}

		public override void Routine(UObject self) {
			if (taskOnTextChanged!=null&&inputField.textChanged) {
				inputFieldTextVariable.Text = inputField.GetLiveText();
				taskOnTextChanged.Execute(GlobalCoroutineRunner.instance);
			}
			
			if (taskOnEndEdit!=null && inputField.endEdit) {
				inputFieldTextVariable.Text = inputField.GetText();
				taskOnEndEdit.Execute(GlobalCoroutineRunner.instance);
			}
		}
	}
}