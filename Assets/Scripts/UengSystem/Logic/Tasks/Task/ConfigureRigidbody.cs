using System;
using UengSystem.Logic.UValues;
using UengSystem.Objects;
using UnityEngine;

namespace UengSystem.Logic.Tasks {
	[Serializable]
	public class ConfigureRigidbody : TaskComponent {
		public bool throwErrorIfNoRigidbody;

		[SerializeReference] [SubclassSelector]
		public UValue<UObject> obj;

		public bool changeVelocity;
		[SerializeReference] [SubclassSelector]
		public UValue<Vector2> velocity;
		
		public bool addForce;
		[SerializeReference] [SubclassSelector]
		public UValue<Vector2> force;
		
		public bool addTorque;
		[SerializeReference] [SubclassSelector]
		public UValue<float> torque;
		
		public override void Execute(ITaskable self) {
			Debug.Log($"[TaskLog : {GetType()} - {Time.time}s - execute : {self}]");
			Debug.Log($" >>> Given Data\n"                                        +
					  $" > obj = {obj.value.name}\n"                              +
					  (changeVelocity ? $" > velocity = {velocity.value}\n" : "") + 
					  (addForce       ? $" > force    = {force.value}\n"    : "") +
					  (addTorque      ? $" > torque   = {torque.value}\n"   : "") +
					  $"");
			
			if (!obj.value.rigidbody2D && throwErrorIfNoRigidbody) throw new NullReferenceException("No rigidbody found");
			
			if (changeVelocity) obj.value.rigidbody2D.linearVelocity = velocity.value;
			if (addForce) obj.value.rigidbody2D.AddForce(force.value, ForceMode2D.Impulse);
			if (addTorque) obj.value.rigidbody2D.AddTorque(torque.value);
		}
	}
}