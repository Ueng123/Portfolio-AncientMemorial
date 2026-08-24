using System.Collections.Generic;
using UnityEngine;

namespace UengSystem.Utility {
	public static class HashUtility {
		public static int GetHash(this string text) => Animator.StringToHash(text);
	}
}