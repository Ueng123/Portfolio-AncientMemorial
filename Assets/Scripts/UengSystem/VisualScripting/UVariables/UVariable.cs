using System.Collections.Generic;
using UengSystem.VisualScripting.UValues;

namespace UengSystem.VisualScripting.UVariables {
	public abstract class UVariable<T> : IUValueVariable {
		private static Dictionary<int, UValue<T>> vars;
		private static void Initialize() {
			if (vars != null) return;
			vars = new Dictionary<int, UValue<T>>();
			IUValueVariable.clearActions.Add(vars.Clear);
		}
		
		public static T GetValue(int key, T defaultValue = default) {
			Initialize();
			if (vars.TryGetValue(key, out UValue<T> value)) return value.value;
			return defaultValue;
		}
		
		public  static UValue<T> Get(int key, UValue<T> defaultValue = null) {
			Initialize();
			return vars.GetValueOrDefault(key, defaultValue);
		}

		public static bool Get(int key, out UValue<T> value) {
			Initialize();
			return vars.TryGetValue(key, out value);
		}
		
		public static void Set(int key, UValue<T> value) {
			Initialize();
			vars[key] = value;
		}
		
		public static void TrySet(int key, UValue<T> value) {
			Initialize();
			vars.TryAdd(key, value);
		}
	}
}