using AncientMemorial.Entities;
using AncientMemorial.Map;
using UengSystem.Objects;
using Unity.Cinemachine;
using UnityEngine;

namespace AncientMemorial.Cameras {
    public class MainCamera : UObject {

       [Header("Camera Settings")]
       public CinemachineCamera cam;
       
       public CameraAlignType  AlignTypeX;
       public CameraAlignType  AlignTypeY;
       public CameraSizingType SizingType;
       public float lensOffset;
       
       public Transform       mainCameraTransform;

       public override void Initialize() {
          base.Initialize();
          
          mainCameraTransform = GameObject.FindGameObjectWithTag("MainCameraHelper").transform;
       }

       public float BaseLensSize { get; private set; }

       protected override void EarlyRoutine() { }

       protected override void Routine() { }

       protected override void LateRoutine() {
          Vector2 mapCenterPos = MapManager.instance.MapCenterTransform.position;
          Vector2 playerPos    = Entity.player ? Entity.player.transform.position + Vector3.up*0.75f : mapCenterPos;

          float xPos = AlignTypeX switch {
             CameraAlignType.Center => mapCenterPos.x,
             CameraAlignType.Both   => (mapCenterPos.x + playerPos.x) / 2,
             CameraAlignType.Player => playerPos.x,
             _                      => 0
          };

          float yPos = AlignTypeY switch {
             CameraAlignType.Center => mapCenterPos.y,
             CameraAlignType.Both   => (mapCenterPos.y + playerPos.y) / 2,
             CameraAlignType.Player => playerPos.y,
             _                      => 0
          };

          // 맵과 해상도에 맞춰 계산된 순정 렌즈값 캐싱
          BaseLensSize = (SizingType switch {
             CameraSizingType.matchX => MapManager.instance.CurrCamLens.x,
             CameraSizingType.matchY => MapManager.instance.CurrCamLens.y,
             CameraSizingType.maxXY  => Mathf.Max(MapManager.instance.CurrCamLens.x, MapManager.instance.CurrCamLens.y),
             CameraSizingType.minXY  => Mathf.Min(MapManager.instance.CurrCamLens.x, MapManager.instance.CurrCamLens.y),
             _                       => 4f
          }) + lensOffset;
          
          // 위치만 타겟팅 갱신 완.
          mainCameraTransform.position = new Vector3(xPos, yPos, mainCameraTransform.position.z);
       }

       protected override void FixedRoutine() { }
    }
}