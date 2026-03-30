using System;
using System.Threading;
using AncientMemorial.Entities;
using AncientMemorial.Events;

namespace AncientMemorial.Buffs {
	public abstract class Buff {
		public BuffType type;
		public Entity   target;
		public float    time;
		public int      stack;
		public int      maxStack;

		protected Buff(int stack, float time, int maxStack, BuffType type) {
			if (stack > maxStack) throw new Exception($"Stack {stack} is too big");
			
			this.type  = type;
			this.time  = time;
			this.stack = stack;
			this.maxStack = maxStack;
		}

		public abstract void OnApply();
		public abstract void OnRemove();
		public abstract void OnExpired();
		
		public abstract void Routine();
		public abstract void OnEvent(Event e);
	}
}