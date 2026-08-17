using System;
using System.Text;
using UnityEngine;

namespace UengSystem.Logic.UValues.UStrings {
	[Serializable]
	public class UString : UValue<string> {
		protected override bool getIsDynamic {
			get {
				bool result = false;
				
				foreach (UValue<string> str in strings) {
					str.parent = this;
					result     = result || str.isDynamic;
				}

				return result;
			}
		}
		
		[SerializeReference][SubclassSelector]
		public UValue<string>[] strings;

		private readonly StringBuilder _stringBuilder = new StringBuilder(256);
		protected override string getValue {
			get {
				if (strings == null || strings.Length == 0) return string.Empty;
				
				_stringBuilder.Clear();

				foreach (UValue<string> item in strings) {
					_stringBuilder.Append(item.value);
				}

				return _stringBuilder.ToString();
			}
		}
	}
}