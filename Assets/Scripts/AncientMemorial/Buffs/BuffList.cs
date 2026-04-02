using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Events_Event = UengSystem.Events.Event;

namespace AncientMemorial.Buffs {
	public class BuffList {
		private List<Buff> buffs = new List<Buff>();

		public void AddBuff(Buff newBuff) {
			Buff targetBuff = buffs
				.FirstOrDefault(buff => buff.type == newBuff.type);
			
			if      (targetBuff == null) buffs.Add(newBuff);
			else if (targetBuff.stack >= newBuff.stack) {
				targetBuff.OnRemove();
				
				targetBuff.stack = newBuff.stack;
				targetBuff.time  = newBuff.time;

				targetBuff.OnApply();
			}
		}

		public void RemoveBuff(BuffType buffType) {
			Buff removeBuff = buffs.FirstOrDefault(buff => buff.type == buffType);
			if (removeBuff != null) RemoveBuff(removeBuff);
			else throw new KeyNotFoundException($"Buff type {buffType} not found");
		}
		
		public void RemoveBuff(Buff removeBuff) {
			removeBuff.OnRemove();
			buffs.Remove(removeBuff);
		}

		public void RemoveAllBuffs() {
			foreach (Buff buff in buffs) {
				RemoveBuff(buff);
			}
		}

		public void Routine() {
			foreach (Buff buff in buffs) {
				
				buff.Routine();
				buff.time -= Time.deltaTime;
				
				if (!(buff.time <= 0)) continue;
				buff.OnExpired();
				RemoveBuff(buff);
				
			}
		}

		public void OnEvent(Events_Event e) {
			foreach (Buff buff in buffs) {
				buff.OnEvent(e);
			}
		}
	}
}