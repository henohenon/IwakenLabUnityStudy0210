using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using SketchKnights.Scripts.Controller;
using SketchKnights.Scripts.Manager;
using R3;
using Cysharp.Threading.Tasks;                 // 追加
using KanKikuchi.AudioManager;
using SketchKnights.Scripts.Controller.Battle;

namespace IwakenLabUnityStudy
{
    public class BattleFlow : MonoBehaviour
    {
                [SerializeField] private bool isDebug = false;
            [SerializeField] private XrObjectResolver xrObjectResolver;
            [SerializeField] private Enemy enemyPrefab;
            [SerializeField] private float battleTime = 60;
            [SerializeField] private float enemyLength = 3.5f;
            [SerializeField] private EnemySpawn[] spawnData;


            private readonly List<Enemy> _enemies = new();


            public Observable<bool> OnFinishBattle => _onFinishBattle; // onFinishBattle(bool)だと意味わからんから、ongameover/ongameclearのほうが良い

            private readonly Subject<bool> _onFinishBattle = new();

            private void Awake()
            {
                xrObjectResolver.XrBattleUi.SetActive(false);
                if (isDebug) Initialize(WeaponSampleData.PlayerSample);

                // bgm
                BGMManager.Instance.Play(BGMPath.BATTLE, isLoop: true);
            }

            public void Initialize(BattlePlayerData data)
            {
                // WaitFlowで初期化されているので、ここではコメントアウト。2度初期化するとプレイヤーデータの重複バグが起きて危険。
                // xrObjectResolver.BattlePlayer.Initialize(data, Utils.SelfLayer);
                if (isDebug) xrObjectResolver.BattlePlayer.Initialize(data, Utils.SelfLayer);

                gameObject.SetActive(true);

                _enemies.Clear();

                foreach (var s in spawnData)
                {
                    s.isSpawned = false;
                }


                xrObjectResolver.BattlePlayer.OnDead.Subscribe(_ =>
                {
                    gameObject.SetActive(false);
                    _onFinishBattle.OnNext(false);
                    // ゲームオーバー効果音を再生する
                    SEManager.Instance.Play(SEPath.GAMEOVER);
                }).AddTo(this);
                _deltaTime = 0;
            }
            private float _deltaTime = 0;
            public void Update()
            {
                _deltaTime += Time.deltaTime;
                if (_deltaTime >= battleTime)
                {
                    // クリア時の効果音を再生する
                    SEManager.Instance.Play(SEPath.CLEAR);
                    _onFinishBattle.OnNext(true);
                    foreach(var enemy in _enemies) enemy?.Death();
                    _enemies.Clear();
                    return;
                }

                foreach(var spawn in spawnData)
                {
                    if(spawn.isSpawned) continue;
                    if(spawn.time > _deltaTime) continue;
                    spawn.isSpawned = true;
                    Vector3 dir      = Quaternion.Euler(0f, spawn.angle, 0f) * Vector3.forward; // 角度分だけ前方ベクトルを回転
                    Vector3 position = dir * enemyLength;                                       // 半径 enemyLength の位置
                    position.y += spawn.yOffset;
                    var enemy = Instantiate(enemyPrefab, position, Quaternion.identity);
                    enemy.Initialize(xrObjectResolver.CameraTransform);
                    _enemies.Add(enemy);
                }
            }

            private void OnDisable()
            {
                xrObjectResolver.XrBattleUi.SetActive(false);
            }

            private void OnEnable()
            {
                xrObjectResolver.XrBattleUi.SetActive(true);
            }
    }
}