using System;
using UnityEngine;

namespace UengSystem.Tasks {
	[Serializable]
	public abstract class TaskComponent { public abstract void Execute(ITaskable self); }
}