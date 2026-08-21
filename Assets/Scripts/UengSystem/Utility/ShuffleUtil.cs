using System.Collections.Generic;
using Random = UnityEngine.Random;

namespace UengSystem.Utility {
	public static class ShuffleUtil {
		public static int[] NewShuffledArray(int n) {
			int[] array = new int[n];
			for (int i = 0; i < n; i++) { array[i] = i; }

			for (int i = n-1; i > 0; i--) {
				int index = Random.Range(0, i+1);
				(array[i], array[index]) = (array[index], array[i]);
			}

			return array;
		}
		
		public static List<int> NewShuffledList(int n) {
			List<int> list = new List<int>(n);
			for (int i = 0; i < n; i++) { list.Add(i); }

			for (int i = n-1; i > 0; i--) {
				int index = Random.Range(0, i+1);
				(list[i], list[index]) = (list[index], list[i]);
			}

			return list;
		}
		
		public static T[] ShuffleArray<T> (T[] array) {
			int n = array.Length;
			
			for (int i = n-1; i > 0; i--) {
				int index = Random.Range(0, i+1);
				(array[i], array[index]) = (array[index], array[i]);
			}

			return array;
		}
		
		public static List<T> ShuffleList<T> (List<T> list) {
			int n = list.Count;
			
			for (int i = n-1; i > 0; i--) {
				int index = Random.Range(0, i+1);
				(list[i], list[index]) = (list[index], list[i]);
			}

			return list;
		}

		public static T[] Shuffle<T>(this T[] array) {
			array = ShuffleArray<T>(array);
			return array;
		}
	}
}