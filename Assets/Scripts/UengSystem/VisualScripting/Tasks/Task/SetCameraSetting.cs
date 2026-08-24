using System;
using AncientMemorial.Cameras;
using AncientMemorial.Map;
using UengSystem.UDebug;
using UnityEngine;

namespace UengSystem.VisualScripting.Tasks {
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
			DebugManager.Log($"[TaskLog : {GetType()} - {Time.time}s - execute : {self}]");
			DebugManager.Log($" > Camera Lens Offset = {TargetCamLensOffset}\n" +
					  $" > Camera Sizing Type = {SizingType}\n"          +
					  $" > Align Type X = {TargetAlignTypeX}\n"          +
					  $" > Align Type Y = {TargetAlignTypeY}\n"          +
					  $"");

			if (changeLensOffset) {
				MapManager.instance.CamLensOffset = TargetCamLensOffset;
			}
			if (changeCameraAlign) {
				GameManager.instance.mainCamera.AlignTypeX = TargetAlignTypeX;
				GameManager.instance.mainCamera.AlignTypeY = TargetAlignTypeY;
			}
			if (changeCameraSizing) {
				GameManager.instance.mainCamera.SizingType = SizingType;
			}
		}
	}
}