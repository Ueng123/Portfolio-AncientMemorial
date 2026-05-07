using System;
using AncientMemorial.Cameras;
using AncientMemorial.Map;
using UengSystem.Logic.Tasks;
using UnityEngine;

namespace AncientMemorial.Tasks {
	[Serializable]
	public class SetCameraSetting : TaskComponent {

		public bool  changeLensOffset;
		public float TargetCamLensOffset;

		public bool            changeCameraAlign;
		public CameraAlignType TargetAlignTypeX;
		public CameraAlignType TargetAlignTypeY;

		public bool             changeCameraSizing;
		public CameraSizingType SizingType;
		
		public override void Execute(ITaskable self) {
			Debug.Log($"[TaskLog : {GetType()} - {Time.time}s - execute : {self}]");
			Debug.Log($" > Camera Lens Offset = {TargetCamLensOffset}\n" +
					  $" > Camera Sizing Type = {SizingType}\n"          +
					  $" > Align Type X = {TargetAlignTypeX}\n"          +
					  $" > Align Type Y = {TargetAlignTypeY}\n"          +
					  $"");
			
			MapManager.instance.CamLensOffset = TargetCamLensOffset;
		}
	}
}