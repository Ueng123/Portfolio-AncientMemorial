using System;
using UnityEngine;

namespace AncientMemorial.Tasks {
	[Serializable]
	public abstract class Task { public abstract void Execute(ITaskable self); }
}