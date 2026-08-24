using System;
using UengSystem.Objects;
using UengSystem.UDebug;
using UengSystem.VisualScripting.UValues;
using UnityEngine;

namespace UengSystem.VisualScripting.Tasks {
	[Serializable]
	public class ConfigureRigidbody : TaskComponent {
		public bool throwErrorIfNoRigidbody;

		public bool thisObject;
		[SerializeReference] [SubclassSelector]
		public UValue<UObject> obj;
		
		[SerializeReference] [SubclassSelector]
		public UValue<Vector2> velocity;
		
		[SerializeReference] [SubclassSelector]
		public UValue<Vector2> force;
		
		[SerializeReference] [SubclassSelector]
		public UValue<float> torque;
		
		public override void Execute(ITaskable self) {
			UObject uObject = thisObject ? (UObject)self : obj.value;
			
			DebugManager.Log($"[TaskLog : {GetType()} - {Time.time}s - execute : {self}]");
			DebugManager.Log($" >>> Given Data\n"                                          +
							 $" > obj = {uObject.name}\n"                                  +
							 (velocity != null ? $" > velocity = {velocity.value}\n" : "") +
							 (force    != null ? $" > force    = {force.value}\n" : "")    +
							 (torque   != null ? $" > torque   = {torque.value}\n" : "")   +
							 $"");
			
			if (!uObject.rigidbody2D && throwErrorIfNoRigidbody) throw new NullReferenceException("No rigidbody found");
			
			if (velocity != null) uObject.rigidbody2D.linearVelocity = velocity.value;
			if (force    != null) uObject.rigidbody2D.AddForce(force.value, ForceMode2D.Impulse);
			if (torque   != null) uObject.rigidbody2D.AddTorque(torque.value);
		}
	}
}