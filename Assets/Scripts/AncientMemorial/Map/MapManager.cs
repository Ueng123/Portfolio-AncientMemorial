using AncientMemorial.Map;
using UnityEngine;
using UengSystem.Managers;

namespace AncientMemorial.Map {
	public class MapManager : Manager<MapManager> {

		// 정적 프로퍼티
		public static float rightWall => instance.CurrMapSize.x / 2;
		public static float leftWall => -instance.CurrMapSize.x / 2;

		// 인스턴스 프로퍼티
		[SerializeField] private Vector2 TargetMapSize;
		private                  Vector2 CurrMapSize;
		
		[Header("Camera Values")]
		[SerializeField ] private Vector2 CamSizeMargin;
		[HideInInspector] public  Vector2 TargetCamLens;
		[HideInInspector] public  Vector2 CurrCamLens;
		public                    float   CamLensOffset;
		
		[HideInInspector] public Vector2 TargetCamPos;
		[HideInInspector] public Vector2 CurrCamPos;
		
		[Header("Map Values")]
		private float   mapChangeSpeed;
		public MapObjects mapObjects;
		public Transform         MapCenterTransform;

		// 인스턴스 메서드
		public void SetMapSize(Vector2 size, float? speed = null, bool instant = false) {
			TargetMapSize = size;
			TargetCamLens = new Vector2(
				(3  *(TargetMapSize.x + CamSizeMargin.x) + 1) /11,
				(16 *(TargetMapSize.y + CamSizeMargin.y) + 3) /33);
			TargetCamPos = new Vector2(0, TargetMapSize.y / 2 + 0.5f);
			
			if (speed.HasValue) {mapChangeSpeed = speed.Value;}

			if (!instant) return;
			CurrMapSize = TargetMapSize;
			CurrCamPos  = TargetCamPos;
			CurrCamLens = TargetCamLens;
			MapCenterTransform.position = CurrCamPos;
		}
		
		public Vector2 GetMapSize()        => CurrMapSize;
		public Vector2 GetTargetMapSize() => TargetMapSize;

		// 오버라이드 메서드
		public override void Initialize() {
			TargetCamLens = new Vector2(
				(3  *(TargetMapSize.x + CamSizeMargin.x) + 1) /11,
				(16 *(TargetMapSize.y + CamSizeMargin.y) + 3) /33);
			TargetCamPos = new Vector2(0, TargetMapSize.y / 2 + 0.5f);
			
			CurrMapSize = TargetMapSize;
			CurrCamLens = TargetCamLens;
			CurrCamPos  = TargetCamPos;
		}
		
		public override void ManagerFixedUpdate() {
			CurrMapSize = Vector2.Lerp(CurrMapSize, TargetMapSize, mapChangeSpeed);
			
			CurrCamLens = Vector2.Lerp(CurrCamLens, TargetCamLens+(Vector2.one*CamLensOffset), mapChangeSpeed);
			CurrCamPos  = Vector2.Lerp(CurrCamPos,  TargetCamPos,                   mapChangeSpeed*2);
			
			MapCenterTransform.position = CurrCamPos;
			
			mapObjects.Move(CurrMapSize);
		}
	}
}