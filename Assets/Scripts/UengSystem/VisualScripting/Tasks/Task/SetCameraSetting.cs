using System;
using AncientMemorial.Cameras;
using AncientMemorial.Map;
using UengSystem.UDebug;
using UnityEngine;

namespace UengSystem.VisualScripting.Tasks {
	[Serializable]
	public class SetCameraSetting : TaskComponent {

		// 인스턴스 프로퍼티
		public bool  changeLensOffset;
		public float TargetCamLensOffset;

		public bool            changeCameraAlign;
		public CameraAlignType TargetAlignTypeX;
		public CameraAlignType TargetAlignTypeY;

		public bool             changeCameraSizing;
		public CameraSizingType SizingType;

		// 오버라이드 메서드
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
				GameManager.instance.uCamera.AlignTypeX = TargetAlignTypeX;
				GameManager.instance.uCamera.AlignTypeY = TargetAlignTypeY;
			}
			if (changeCameraSizing) {
				GameManager.instance.uCamera.SizingType = SizingType;
			}
		}
	}
}
