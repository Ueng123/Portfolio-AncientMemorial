using System.Collections.Generic;
using UengSystem.Utility;
using UnityEngine;

namespace AncientMemorial.Entities {
	public class EntityGroundChecker : MonoBehaviour {

		// 인스턴스 프로퍼티
		public bool isThereStandable {
			get {
				FilterStandables();
				return standables.Count > 0;
			}
		}

		private List<CanStandOn> standables = new ();

		// 인스턴스 메서드
		private void FilterStandables() {
			int count = standables.Count;
			if (count == 0) return;

			// 역순 순회로 가비지 없이 원소 제거 (상남자 국룰 구조)
			for (int i = count - 1; i >= 0; i--) {
				CanStandOn s = standables[i];
				if (!s || !s.gameObject.activeInHierarchy) {
					standables.RemoveAt(i);
				}
			}
		}
		
		private void OnTriggerEnter2D(Collider2D other) {
			if (other.TryGetComponent(out CanStandOn s)) {
				standables.Add(s);
			};
		}

		private void OnTriggerExit2D(Collider2D other) {
			if (other.TryGetComponent(out CanStandOn s)) {
				standables.Remove(s);
			};
		}
		
		private void OnDisable() {
			standables.Clear();
		}
	}
}