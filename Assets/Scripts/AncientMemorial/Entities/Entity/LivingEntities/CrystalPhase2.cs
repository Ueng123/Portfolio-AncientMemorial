using System;
using System.Collections;
using System.Collections.Generic;
using AncientMemorial.Cameras;
using AncientMemorial.Map;
using AncientMemorial.Projectiles;
using UengSystem.Logic.UValues.UBools;
using UengSystem.ObjectPool;
using UengSystem.Objects;
using UengSystem.States;
using UengSystem.UI;
using UengSystem.Utility;
using UnityEngine;
using DelayedAction = UengSystem.Utility.DelayedAction;
using Random = UnityEngine.Random;

namespace AncientMemorial.Entities {
    public class CrystalPhase2 : CrystalBoss {

       public GameObject[] hideOnDeath;

       private Vector2 mapSize;
       
       private static readonly int  Spawning = Animator.StringToHash("spawning");
       protected override      void OnGrounded() { }
       
       public override    void        OnHit(Entity    attacker,   float    damage, Vector2? pushDir = null) {
          if (Random.Range(0, 3) ==0) ShootMissile();
          if (Random.Range(0, 3) ==0) ShootBigMissile();
          
          base.OnHit(attacker, damage, pushDir);
       } 

       private void ShootMissile(int shootN = 1, float duration = 0f) {
          Vector2[] posData   = new Vector2[shootN];
          float[]   rotData   = new float[shootN];
          float[]   speedData = new float[shootN];
          float[]   tblData   = new float[shootN]; // timeBeforeLockOn
          float[]   lodData   = new float[shootN]; // lockOnDuration
          
          for (int i = 0; i < shootN; i++) {
             posData[i] = transform.position + new Vector3(Random.Range(-1f, 1f), 1.5f + Random.Range(-0.5f, 0.5f));
             rotData[i]   = Random.Range(-60f, 60f);
             speedData[i] = Random.Range(10f,  12f);
             tblData[i] = Random.Range(0.8f, 1.5f);
             lodData[i]   = Random.Range(2f, 3f);
          }

          StartCoroutine(SpreadMissiles(duration, posData, rotData, speedData, tblData, lodData));
       }

       private void ShootBigMissile(int shootN = 1, float duration = 0f) {
          Vector2[] posData   = new Vector2[shootN];
          float[]   rotData   = new float[shootN];
          float[]   speedData = new float[shootN];
          float[]   lodData   = new float[shootN]; // lockOnDuration
          
          for (int i = 0; i < shootN; i++) {
             posData[i] = new Vector2(
                transform.position.x + Random.Range(-(mapSize.x / 2 - 1), mapSize.x / 2 - 1),
                Random.Range(mapSize.y / 3f, mapSize.y * 5 / 6f));
             rotData[i]   = Random.Range(0f,   360f);
             speedData[i] = Random.Range(8f,   10f);
             lodData[i]   = Random.Range(0.8f, 1.5f);
          }

          StartCoroutine(SpreadBigMissiles(duration, posData, rotData, speedData, lodData));
       }
       
       private IEnumerator SpreadMissiles(float duration, Vector2[] posData, float[] rotData, float[] speedData, float[] tblData, float[] lodData) {
          if (!(posData.Length == rotData.Length && rotData.Length == speedData.Length)) yield break;
          int missileCount = posData.Length;
          
          float[] spawnTimes = new float[missileCount];
          for (int i = 0; i < missileCount; i++) { spawnTimes[i] = Random.Range(0f, duration); }
          Array.Sort(spawnTimes);

          float elapsedTime = 0f;
          for (int i = 0; i < missileCount; i++) {
             float waitTime = spawnTimes[i] - elapsedTime;
             yield return CacheManager.WaitForSeconds(waitTime);
             elapsedTime += waitTime;
             
             ShootMissile(posData[i], rotData[i], speedData[i], tblData[i], lodData[i]);
          }
       }
       private IEnumerator SpreadBigMissiles(float duration, Vector2[] posData, float[] rotData, float[] speedData, float[] lodData) {
          if (!(posData.Length == rotData.Length && rotData.Length == speedData.Length)) yield break;
          int missileCount = posData.Length;
          
          float[] spawnTimes = new float[missileCount];
          for (int i = 0; i < missileCount; i++) { spawnTimes[i] = Random.Range(0f, duration); }
          Array.Sort(spawnTimes);

          float elapsedTime = 0f;
          for (int i = 0; i < missileCount; i++) {
             float waitTime = spawnTimes[i] - elapsedTime;
             yield return CacheManager.WaitForSeconds(waitTime);
             elapsedTime += waitTime;
             
             ShootBigMissile(posData[i], rotData[i], speedData[i], 0, lodData[i]);
          }
       }

       private void ClearMissiles() {
          List<UObject> objs = GetUObjects("crystalMissile");
          for (int i = objs.Count - 1; i >= 0; i--) {
             UObject obj = objs[i];
             UObjectPool.instance.Release(obj.gameObject);
          }

          foreach (DelayedAction da in MissileAttackAreas) {
             da.Cancel();
          }

          MissileAttackAreas.Clear();
       }

       private List<DelayedAction> MissileAttackAreas = new List<DelayedAction>();
       
       // -1 : <- | 0 : v | 1 : ->
       private void ShootHugeMissile(float where) {
          GameObject obj = UObjectPool.instance.Get("CrystalHugeMissile", new Vector2(where*mapSize.x/3, mapSize.y-2));
          BasicProjectile hugeMissile = obj.GetComponent<BasicProjectile>();
          hugeMissile.Category = "crystalMissile";
          
          float      timeToFall = Mathf.Sqrt(20 * (obj.transform.position.y-0.5f) / Physics2D.gravity.y*-1);

          Vector2 hitboxPos  = new (mapSize.x*where/3f, (mapSize.y +1));
          Vector2 hitboxSize = new (mapSize.x/3f, (mapSize.y +1)*2);
          MissileAttackAreas.Add(AttackArea(Random.Range(90, 100), timeToFall, hitboxPos, hitboxSize));
       }
       
       private void ShootHugeMissile(float where, Vector2 hitboxPos, Vector2 hitboxSize) {
          GameObject obj = UObjectPool.instance.Get("CrystalHugeMissile", new Vector2(where*mapSize.x/4, mapSize.y-2));
          float      timeToFall = Mathf.Sqrt(20 * (obj.transform.position.y-0.5f) / Physics2D.gravity.y*-1);
          
          AttackArea(Random.Range(90, 100), timeToFall, hitboxPos, hitboxSize);
       }

       private       int[] attackIndexes;
       public        int   attackPhase    = 0;
       private const int   attackPhaseNum = 4;
       public override void Attack() {
          if (!attackable) return;
          attackable    =   false;
          attackIndexes ??= Shuffle.NewShuffledArray(attackPhaseNum);

          PlaySFX("crystalRoar");
          
          ClearMissiles();
          
          // random ONLY at first
          // ((UObject)this).state = attackIndexes[attackPhase] switch {
          //    0 => Attacking,
          //    1 => AttackVing,
          //    2 => Slash,
          //    3 => SlashHorizontal,
          //    _ => throw new Exception("NO BRO THAT'S NOT WHAT I WANT :(")
          // };
          
          attackPhase++;
          
          if (attackPhase != attackPhaseNum) return;
          attackPhase   = 0;
          attackIndexes = Shuffle.ShuffleArray(attackIndexes);
       }
       
       // private UState AttackVing => new (AttackVEnumerator(), OnAttackCancel, OnAttackDone, 0, this);
       //
       // private UState Slash => new (SlashEnumerator(), OnSlashCancel, OnSlashDone, 0, this);
       // private UState SlashHorizontal => new (SlashHorizontalEnumerator(), OnSlashCancel, OnSlashDone, 0, this);
       
       protected override IEnumerator AttackEnumerator() {
          animator.SetBool(Spawning, true);
          groggy = true;
          
          List<int> orders = new() { -1, 0, 1 };
          for (int i = orders.Count - 1; i > 0; i--)
          {
             int rnd  = Random.Range(0, i + 1);
             (orders[i], orders[rnd]) = (orders[rnd], orders[i]);
          }
          
          ShootMissile (Random.Range(2, 5), 5f);
          ShootBigMissile(Random.Range(2, 5), 5f);

          yield return CacheManager.WaitForSeconds(1f);
          
          ShootHugeMissile(orders[0]);
          
          yield return CacheManager.WaitForSeconds(2f);
          
          ShootHugeMissile(orders[1]);
          
          yield return CacheManager.WaitForSeconds(2f);
          
          ShootHugeMissile(orders[2]);

          yield return CacheManager.WaitForSeconds(9f);
       }
       
       protected IEnumerator AttackVEnumerator() {
          animator.SetBool(Spawning, true);
          groggy = true;
          
          for (int i = Random.Range(0, 2); i < Random.Range(5, 9); i++) {
             ShootHugeMissile(
                Random.Range(-0.1f, 0.1f) + (i % 2 == 0 ? 1 : -1),
                new Vector2(mapSize.x/4f * (i % 2 == 0 ? 1 : -1), mapSize.y/2),
                new Vector2(mapSize.x/2f, mapSize.y)
                );
             yield return CacheManager.WaitForSeconds(1.2f);
          }
          
          yield return CacheManager.WaitForSeconds(7.8f);
       }


       // wallN
       // 0 : < | 1 : > | 2 : v | 3 : ^ //
       
       public void GetTwoRandomNumbers(out int first, out int second)
       {
          int[] numbers = { 0, 1, 2, 3 };
          
          int idx1 = Random.Range(0, 4);
          int temp = numbers[0];
          numbers[0]    = numbers[idx1];
          numbers[idx1] = temp;
          
          int idx2 = Random.Range(1, 4);
          temp          = numbers[1];
          numbers[1]    = numbers[idx2];
          numbers[idx2] = temp;

          first  = numbers[0];
          second = numbers[1];
       }
       
       public Vector2 GetWallPoint(int wallN) {
          return wallN switch {
             0 => new Vector2(-mapSize.x, Random.Range(1f,  mapSize.x /2f - 1)),
             1 => new Vector2(mapSize.x,  Random.Range(1f,  mapSize.x /2f - 1)),
             2 => new Vector2(Random.Range(-(mapSize.y -1), (mapSize.y -1)), 0),
             3 => new Vector2(Random.Range(-(mapSize.y -1), (mapSize.y -1)), mapSize.y),
             _ => Vector2.zero
          };
       }  
       
       protected IEnumerator SlashEnumerator() {
          animator.SetBool(Spawning, true);
          groggy = true;
          
          yield return CacheManager.WaitForSeconds(1f);
          int   patternNum = Random.Range(4, 7);

          for (int j = 0; j < patternNum; j++) {
             int       slashNum        = Random.Range(8,  12);
             float     patternDuration = Random.Range(3f, 5f);
             Vector4[] slashData       = new Vector4[slashNum];

             for (int i = 0; i < slashNum; i++) {
                float duration = Random.Range(2, patternDuration);

                GetTwoRandomNumbers(out int s, out int e);
                Vector2 startPos = GetWallPoint(s);
                Vector2 endPos   = GetWallPoint(e);
                Vector2 dir      = endPos                                    - startPos;
                float   angle    = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg + 90;
                
                Vector2 center = (startPos + endPos) * 0.5f;
                float   length = dir.magnitude;
                
                slashData[i] = new Vector4(center.x, center.y, length, angle);
                new DelayedAction(patternDuration - duration, () => {
                   PlaySFX("crystalRoar");
                   AttackArea(2, duration, center, new Vector2(0.5f, length+5f), angle);
                }, () => { }, this).ExecuteDA();
             }

             new DelayedAction(patternDuration, () => {
                PlaySFX("crystalSlash");
                
                foreach (Vector4 data in slashData) {
                   Vector2    slashPos = new(data.x, data.y);
                   GameObject obj      = UObjectPool.instance.Get("SlashEffect", slashPos);
                   obj.transform.rotation   = Quaternion.Euler(0, 0, data.w);
                   obj.transform.localScale = new Vector3(1f, data.z + 0.5f, 1f);
                }

                GameManager.SetTimeScale(0, 0.1f);
                CameraBrain.instance.ShakeLerp(5, 3);
                CameraBrain.instance.ZoomLerp(-2f);
             }, () => { }, this).ExecuteDA();

             ShootMissile(Random.Range(2,  5), patternDuration);
             ShootBigMissile(Random.Range(2, 5), patternDuration);

             yield return CacheManager.WaitForSeconds(patternDuration + 1f);
          }
          
          yield return CacheManager.WaitForSeconds(1f);
       }
       
       protected IEnumerator SlashHorizontalEnumerator() {
          animator.SetBool(Spawning, true);
          groggy = true;
          
          yield return CacheManager.WaitForSeconds(1f);

          int   patternNum      = Random.Range(4, 7);
          int[] slashNums       = new int[patternNum];
          for (int i = 0; i < patternNum; i++) { slashNums[i] = Random.Range(5,  10); }

          for (int j = 0; j < patternNum; j++) {
             int       slashNum        = slashNums[j];
             float     patternDuration = Random.Range(3f, 4f);
             Vector4[] slashData       = new Vector4[slashNum];

             for (int i = 0; i < slashNum; i++) {
                float duration = Random.Range(2, patternDuration);

                Vector2 startPos = GetWallPoint(2);
                Vector2 endPos   = new(startPos.x, mapSize.y);
                Vector2 dir   = endPos - startPos;
                float   angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg + 90;
                
                Vector2 center = (startPos + endPos) * 0.5f;
                float   length = dir.magnitude;
                
                slashData[i] = new Vector4(center.x, center.y, length, angle);
                new DelayedAction(patternDuration - duration, () => {
                   PlaySFX("crystalRoar");
                   AttackArea(2, duration, center, new Vector2(2f, length+5f), angle);
                }).ExecuteDA();
             }// da + slashNum

             new DelayedAction(patternDuration, () => {
                PlaySFX("crystalSlash");
                
                foreach (Vector4 data in slashData) {
                   GameObject obj      = UObjectPool.instance.Get("SlashEffect", new Vector2(data.x, data.y+Random.Range(-2f, 2f)));
                   obj.transform.rotation   = Quaternion.Euler(0, 0, Random.Range(-5f, 5f));
                   obj.transform.localScale = new Vector3(3f, mapSize.y+Random.Range(-1f, 1f), 1f);
                }

                GameManager.SetTimeScale(0, 0.05f);
                CameraBrain.instance.ShakeLerp(5, 3);
                CameraBrain.instance.ZoomLerp(-2f);
             }).ExecuteDA(); // da + 1

             ShootMissile(Random.Range(2,  5), patternDuration);
             ShootBigMissile(Random.Range(2, 5), patternDuration);

             yield return CacheManager.WaitForSeconds(patternDuration + 1f);
          }
          
          yield return CacheManager.WaitForSeconds(1f);
       }

       protected override void OnAttackDone() {
          animator.SetBool(Spawning, false);
          ForceInvincibleTrue(false);
          groggy = false;
          
          ClearMissiles();
          
          // state                  = EnemyState.Alert;
          attackable             = false;
       }
       
       protected override void OnAttackCancel() {
          animator.SetBool(Spawning, false);
          ForceInvincibleTrue(false);
          groggy = false;
          
          ClearMissiles();
          
          // state                  = EnemyState.Alert;
          attackable             = false;
       }

       protected void OnSlashDone() {
          animator.SetBool(Spawning, false);
          ForceInvincibleTrue(false);
          groggy = false;
          
          ClearMissiles();
          
          // state                  = EnemyState.Alert;
          attackable             = false;
       }
       
       protected void OnSlashCancel() {
          animator.SetBool(Spawning, false);
          ForceInvincibleTrue(false);
          groggy = false;
          
          ClearMissiles();
          
          // state                  = EnemyState.Alert;
          attackable             = false;
       }

       StopWatch missileStopwatch = new StopWatch();
       public override void AlertRoutine() {
          if (!missileStopwatch.Check(1f)) {
             ShootMissile(Random.Range(0,  2), duration:2f);
             ShootBigMissile(Random.Range(0, 2), duration:2f);

             missileStopwatch.Tick();
          }
          
          base.AlertRoutine();
       }
       
       // public override void AttackRoutine() { }

       protected override void EarlyRoutine() {
          
          mapSize = MapManager.instance.GetMapSize();
          
          base.EarlyRoutine();
       }

       public override void Initialize() {
          base.Initialize();
          mapSize = MapManager.instance.GetMapSize();
          
          foreach (GameObject obj in hideOnDeath) {
             obj.SetActive(true);
          }
          
          StartCoroutine(SpawnEffect());
       }

       private IEnumerator SpawnEffect() {
          float elapsed  = 0f;
          const float duration = 10f;

          while (elapsed < duration) {
             elapsed += Time.deltaTime;
             transform.Translate(Vector3.up * (0.1f * Time.deltaTime));
             yield return null;
          }
          
          PlaySFX("crystalRoar");
          PlaySFX("crystalSlash", volume:1f);
          
          GameObject sEff      = UObjectPool.instance.Get("SlashEffect", transform.position);
          sEff.transform.rotation   = Quaternion.Euler(0, 0, Random.Range(-5f, 5f));
          sEff.transform.localScale = new Vector3(2f, 10f);
          
          GameObject sEff2      = UObjectPool.instance.Get("SlashEffect", transform.position);
          sEff2.transform.rotation   = Quaternion.Euler(0, 0, 90+Random.Range(-5f, 5f));
          sEff2.transform.localScale = new Vector3(2f, 10f);
          
          CameraBrain.instance.ShakeLerp(5, 10);
          CameraBrain.instance.ZoomLerp(-2f, 3);
          
          attackable = false;
       }

       protected override void Death() {
          foreach (GameObject obj in hideOnDeath) {
             obj.SetActive(false);
          }
          
          PlaySFX("crystalHit2");

          GameManager.UValueBoolVariables["crystalPhase2End"]   = new UPureBool {boolValue = true};

          ClearMissiles();

          for (int i = 0; i < 10; i++) {
             UObjectPool.instance.Get("crystalDebris", (Vector2)transform.position + new Vector2(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f)));
          }
          
          base.Death();
          
          CrystalPhase3 phase3 = UObjectPool.instance.Get("CrystalPhase3", transform.position).GetComponent<CrystalPhase3>();
          phase3.ID = "Crystal";
          
          UUI bossBar = UUI.GetUUI("CrystalBossBar");
          if (!bossBar) return;

          UUIPool.instance.Close(bossBar.gameObject);
          UUI newBossBar = UUIPool.instance.Open("CrystalP3UI", GameManager.instance.mainScreenCanvas).GetComponent<UUI>();
          newBossBar.ID = "CrystalBossBar";
       }
    }
}