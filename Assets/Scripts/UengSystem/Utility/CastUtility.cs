namespace UengSystem.Utility {
	public static class CastUtility {
		public static T To<T>(this object obj) {
			return (T)obj;
		}
	}
}