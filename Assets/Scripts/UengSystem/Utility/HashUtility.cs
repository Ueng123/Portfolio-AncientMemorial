using System.Collections.Generic;
using UnityEngine;

namespace UengSystem.Utility {
	public static class HashUtility {

		// 정적 메서드
		public static int GetHash(this string text) => Animator.StringToHash(text);
	}
}