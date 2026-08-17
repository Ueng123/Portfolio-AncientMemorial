using System.Collections;
using AncientMemorial.Cameras;
using AncientMemorial.Map;
using UengSystem.Logic.UValues.UBools;
using UengSystem.Logic.UValues.UFloats;
using UengSystem.ObjectPool;
using UengSystem.Objects;
using UengSystem.UI;
using UengSystem.Utility;
using UnityEngine;
using Random = UnityEngine.Random;

namespace AncientMemorial.Entities {
    public class CrystalPhase1 : CrystalBoss {

       public GameObject[] hideOnDeath;
       
       private Vector2 mapSize;
       
       private static readonly int  Spawning = Animator.StringToHash("spawning");
       protected override      void OnGrounded() { }
       
       public override    void        OnHit(Entity    attacker,   float    damage, Vector2? pushDir = null) {
          if (Random.Range(0, 3)==0) ShootMissile();
          
          base.OnHit(attacker, damage, pushDir);
       }

       private void ShootMissile(int shootN = 1) {
          for (int i = 0; i < shootN; i++) {
             Vector2 spawnPos = transform.position + new Vector3(Random.Range(-1f, 1f), 1.5f + Random.Range(-0.5f, 0.5f));
             float rotation = Random.Range(-40f, 40f);
             float speed = Random.Range(10f, 12f);
             float timeBeforeLockOn = Random.Range(0.8f, 1.5f);
             float lockOnDuration   = Random.Range(2f,   4f);
             ShootMissile(spawnPos, rotation, speed, timeBeforeLockOn, lockOnDuration);
          }
       }

       private void ShootMissile2(int shootN = 1) {
          for (int i = 0; i < shootN; i++) {
             Vector2 spawnPos = new Vector2(
                transform.position.x + Random.Range(-(mapSize.x - 1), mapSize.x - 1),
                transform.position.y + Random.Range(4,                mapSize.y - 4));
             float rotation         = Random.Range(0f, 360f);
             float speed            = Random.Range(8f, 15f);
             float timeBeforeLockOn = 0;
             float lockOnDuration   = Random.Range(1f,   1.5f);
             ShootMissile(spawnPos, rotation, speed, timeBeforeLockOn, lockOnDuration);
          }
       }
       
       private void ClearMissiles() {
          foreach (UObject obj in GetUObjects("crystalMissile")) {
             new DelayedAction(Random.Range(0f, 0.5f), () => {
                if (obj.isReleased) return;
                UObjectPool.instance.Release(obj.gameObject);
             }).ExecuteDA();
          }
       }
       
       protected override IEnumerator AttackEnumerator() {
          animator.SetBool(Spawning, true);
          PlaySFX("crystalRoar");
          
          ForceInvincibleTrue(true);
          
          InfoUUI.instance.AddInfoMessage("크리스탈이 기억속 잔재를 불러옵니다.");

          GameManager.UValueFloatVariables["EnemyDead"] = new UPureFloat { number = 0 };

          int entityToSpawn = Random.Range(6, 10);
          
          Debug.Log($"[CrystalAttack] entityToSpawn = {entityToSpawn+1}");
          
          for (int i = 0; i < entityToSpawn; i++) {
             Debug.Log($"[CrystalAttack] spawning entity {i}");
             string  spawnEntityID = Random.Range(0, 2) == 0 ? "SkeletonWarriorC" : "SkeletonArcherC";
             Vector3 spawnPosOffset = new (Random.Range(2, 6) * (Random.Range(0, 2) == 0 ? 1 : -1), Random.Range(1, 1.5f));
             UObjectPool.instance.Get(spawnEntityID, transform.position + spawnPosOffset, 2.5f);
             yield return CacheManager.WaitForSeconds(1);
          }
          
          yield return new WaitUntil(()=>GameManager.UValueFloatVariables["EnemyDead"].value >= entityToSpawn);

          groggy         = true;
          groggyedEffect = true;
          SendAttackMultiplyEvent(this, 1, true);
       }

       // protected IEnumerator Attack2Enumerator() {
       //     animator.SetBool(Spawning, true);
       //     ForceInvincibleTrue(true);
       //
       //     InfoUUI.instance.AddInfoMessage("크리스탈이 기억속 강력한 잔재를 꺼내옵니다.");
       //     
       //     GameManager.UValueFloatVariables["skeletonTankDead"] = new UPureNumber { number = 0 };
       //
       //     int entityToSpawn = 1;
       //     Debug.Log($"[CrystalAttack] entityToSpawn = {entityToSpawn+1}");
       //     
       //     for (int i = 0; i < entityToSpawn; i++) {
       //        Debug.Log($"[CrystalAttack] spawning entity {i}");
       //        new DelayedAction(Random.Range(1, 6), () => {
       //           UObjectPool.instance.Get(
       //              "SkeletonTankC",
       //              (Vector2)transform.position
       //              + new Vector2(Random.Range(2, 6) * (Random.Range(0, 2) == 0 ? 1 : -1),
       //                         Random.Range(1, 1.5f)),
       //              2.5f);
       //        }).Execute();
       //     }
       //     
       //     yield return new WaitUntil(()=>GameManager.UValueFloatVariables["skeletonTankDead"].value >= entityToSpawn);
       //
       //     groggy         = true;
       //     groggyedEffect = true;
       //     AddProcessToUpdate(()=>SendEvent(UengSystem.Events.EventType.Entity_Behaviour_Hit, 10, new EntityHitData(
       //                                 this,
       //                                 null,
       //                                 this,
       //                                 0,
       //                                 Vector2.zero
       //                              )));
       // }
       
       // protected IEnumerator Attack3Enumerator() {
       //     animator.SetBool(Spawning, true);
       //     groggy         = true;
       //     
       //     for (int i = 0; i < 10; i++) {
       //        ShootMissile(Random.Range(0, 2));
       //        ShootMissile2(Random.Range(0, 2));
       //        yield return CacheManager.WaitForSeconds(2f);
       //     }
       //
       //     yield return CacheManager.WaitForSeconds(3f);
       //     
       //     for (int i = 0; i < 10; i++) {
       //        ShootMissile();
       //        ShootMissile2(Random.Range(0, 2));
       //        yield return CacheManager.WaitForSeconds(1.5f);
       //     }
       //     
       //     yield return CacheManager.WaitForSeconds(3f);
       // }

       protected override void OnAttackDone() {
          animator.SetBool(Spawning, false);
          ForceInvincibleTrue(false);
          
          // state                  = EnemyState.Alert;
          attackable             = false;
       }

       // 지금까진 이론상 그런거 없음
       protected override void OnAttackCancel() {
          animator.SetBool(Spawning, false);
          ForceInvincibleTrue(false);
          
          // state                  = EnemyState.Alert;
          attackable             = false;
       }

       StopWatch missileStopwatch = new StopWatch();
       public override void AlertRoutine() {
          if (!missileStopwatch.Check(4f)) {
             ShootMissile(Random.Range(0, 3));

             missileStopwatch.Tick();
          }
          
          base.AlertRoutine();
       }

       // public override void AttackRoutine() {
       //    ForceInvincibleTrue(true);
       //
       //    if (missileStopwatch.Check(2f)) return;
       //    ShootMissile(Random.Range(0, 3));
       //       
       //    missileStopwatch.Tick();
       // }
       
       protected override void EarlyRoutine() {
          base.EarlyRoutine();
          
          mapSize = MapManager.instance.GetMapSize();
       }

       public override void Initialize() {
          foreach (GameObject obj in hideOnDeath) {
             obj.SetActive(true);
          }
          
          GameObject sEff      = UObjectPool.instance.Get("SlashEffect", transform.position);
          sEff.transform.rotation   = Quaternion.Euler(0, 0, Random.Range(-5f, 5f));
          sEff.transform.localScale = new Vector3(2f, 10f);
          
          GameObject sEff2      = UObjectPool.instance.Get("SlashEffect", transform.position);
          sEff2.transform.rotation   = Quaternion.Euler(0, 0, 90+Random.Range(-5f, 5f));
          sEff2.transform.localScale = new Vector3(2f, 10f);
          
          CameraBrain.instance.ShakeLerp(5, 10);
          CameraBrain.instance.ZoomLerp(-2f);
          
          base.Initialize();
       }

       protected override void Death() {
          foreach (GameObject obj in hideOnDeath) {
             obj.SetActive(false);
          }
          
          PlaySFX("crystalHit2");

          GameManager.UValueBoolVariables["crystalPhase1End"]   = new UPureBool {boolValue = true};
          
          ClearMissiles();

          for (int i = 0; i < 10; i++) {
             UObjectPool.instance.Get("crystalDebris", (Vector2)transform.position + new Vector2(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f)));
          }
          
          base.Death();
          
          CrystalPhase2 phase2 = UObjectPool.instance.Get("CrystalPhase2", transform.position).GetComponent<CrystalPhase2>();
          phase2.ID = "Crystal";
          
          UUI bossBar = UUI.GetUUI("CrystalBossBar");
          if (!bossBar) return;

          UUIPool.instance.Close(bossBar.gameObject);
          UUI newBossBar = UUIPool.instance.Open("CrystalP2UI", GameManager.instance.mainScreenCanvas).GetComponent<UUI>();
          newBossBar.ID = "CrystalBossBar";
       }
    }
}