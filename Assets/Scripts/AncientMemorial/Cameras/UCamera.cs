using AncientMemorial.Entities;
using AncientMemorial.Map;
using UengSystem.Objects;
using Unity.Cinemachine;
using UnityEngine;

namespace AncientMemorial.Cameras {
    public class UCamera : UObject {

       // 인스턴스 프로퍼티
       [Header("Camera Settings")]
       public CinemachineCamera cam;

       public CameraManager cameraManager;
       public MapManager     mapManager;
       public CinemachineBasicMultiChannelPerlin noiseComponent;

       public CameraAlignType  AlignTypeX;
       public CameraAlignType  AlignTypeY;
       public CameraSizingType SizingType;
       public float lensOffset;

       public Transform mainCameraTransform;

       public float baseLensSize { get; private set; }

       public void ChangeCameraTransform(Transform TargetTransform) {
          mainCameraTransform = TargetTransform;
       }

       // 오버라이드 메서드
       public override void Initialize() {
          base.Initialize();

          mainCameraTransform = GameObject.FindGameObjectWithTag("MainCameraHelper").transform;
       }

       protected override void EarlyRoutine() { }

       protected override void Routine() { }

       protected override void LateRoutine() {
          if (!cameraManager || !mapManager || !cam || !mainCameraTransform) return;

          Vector2 mapCenterPos = mapManager.MapCenterTransform.position;
          Vector2 playerPos    = Entity.player ? Entity.player.transform.position + Vector3.up * 0.75f : mapCenterPos;

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

          baseLensSize = (SizingType switch {
             CameraSizingType.matchX => mapManager.CurrCamLens.x,
             CameraSizingType.matchY => mapManager.CurrCamLens.y,
             CameraSizingType.maxXY  => Mathf.Max(mapManager.CurrCamLens.x, mapManager.CurrCamLens.y),
             CameraSizingType.minXY  => Mathf.Min(mapManager.CurrCamLens.x, mapManager.CurrCamLens.y),
             _                       => 4f
          }) + lensOffset;

          LensSettings lens = cam.Lens;
          lens.OrthographicSize = baseLensSize + cameraManager.currentZoomOffset;
          cam.Lens = lens;

          if (noiseComponent) noiseComponent.AmplitudeGain = cameraManager.currentShakeGain;

          mainCameraTransform.position = new Vector3(xPos, yPos, mainCameraTransform.position.z);
       }

       protected override void FixedRoutine() { }
    }
}
