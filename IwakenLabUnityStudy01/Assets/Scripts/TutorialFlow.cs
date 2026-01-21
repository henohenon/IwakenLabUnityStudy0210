using UnityEngine;
using System;
using R3;
using TMPro;

namespace IwakenLabUnityStudy
{
    public class TutorialFlow : MonoBehaviour
    {
            [SerializeField] private MouseInputObserver mouseInputObserver;
            [SerializeField] private DrawWeapon drawWeapon;
            [SerializeField] private TMP_Text text;
            [SerializeField] private BattleWeapon battleWeaponPrefab;
            [SerializeField] private FallingBall fallingBallPrefab;
            [SerializeField] private float ballSpawnInterval = 6f;
            [SerializeField] private Vector3 ballSpawnPosition = new Vector3(0, 5, 0);

            private readonly Subject<Unit> _finishTutorial = new ();
            public Observable<Unit> FinishTutorial => _finishTutorial;

            private WeaponDrawSequencer _ovrWeaponDraw;
            private BattleWeapon _battleWeaponInstance;
            private IDisposable _drawSubscription;
            private IDisposable _startSubscription;
            private IDisposable _ballSpawnSubscription;

            private void Awake()
            {
                _ovrWeaponDraw = new WeaponDrawSequencer(mouseInputObserver, drawWeapon);

                _drawSubscription = _ovrWeaponDraw.OnDrawEnd.Subscribe(nodes =>
                {
                    _drawSubscription?.Dispose();

                    text.text = "ぶった斬れ！";
                    drawWeapon.gameObject.SetActive(false);

                    _battleWeaponInstance = Instantiate(battleWeaponPrefab);
                    _battleWeaponInstance.Initialize(nodes);

                    //tutorialLineController.To = startObject.gameObject.transform;
                    //tutorialLineController.From = _battleWeaponInstance.transform;
                    //tutorialLineController.gameObject.SetActive(true);

                    // ボールを定期的に生成
                    StartBallSpawning();

                    _startSubscription?.Dispose();
                    _startSubscription = _battleWeaponInstance.OnHit.Subscribe(col =>
                    {
                        if(!col.gameObject.CompareTag("StartObject")) return;
                        //tutorialLineController.gameObject.SetActive(false);
                        _ballSpawnSubscription?.Dispose();
                        Destroy(_battleWeaponInstance.gameObject);
                        _ovrWeaponDraw?.Dispose();
                        _ovrWeaponDraw = null;
                        text.text = "チュートリアルクリア！";
                        Destroy(col.gameObject);
                        _finishTutorial.OnNext(Unit.Default);
                        _startSubscription?.Dispose();
                    }).AddTo(this);
                }).AddTo(this);
            }

            private void OnEnable()
            {
                text.text = "一筆書きで剣を描け！";
            }

            private void StartBallSpawning()
            {
                _ballSpawnSubscription?.Dispose();
                _ballSpawnSubscription = Observable
                    .Interval(TimeSpan.FromSeconds(ballSpawnInterval))
                    .Subscribe(_ => SpawnBall())
                    .AddTo(this);

                // 最初のボールをすぐに生成
                SpawnBall();
            }

            private void SpawnBall()
            {
                if (fallingBallPrefab == null) return;

                // ランダムなX位置で生成
                var spawnPos = ballSpawnPosition;
                spawnPos.x += UnityEngine.Random.Range(-3f, 3f);

                Instantiate(fallingBallPrefab, spawnPos, Quaternion.identity);
            }

            private void OnDisable()
            {
                _ballSpawnSubscription?.Dispose();
            }
    }
}
