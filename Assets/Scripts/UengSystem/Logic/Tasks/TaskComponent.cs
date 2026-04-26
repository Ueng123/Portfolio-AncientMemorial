using System;

namespace UengSystem.Logic.Tasks {
	[Serializable]
	public abstract class TaskComponent { public abstract void Execute(ITaskable self); }
}