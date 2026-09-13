namespace UengSystem.Utility {
	public static class CastUtility {

		// 정적 메서드
		public static T To<T>(this object obj) {
			return (T)obj;
		}

		public static T As<T>(this object obj) where T : class {
			return obj as T;
		}
	}
}
