using UnityEngine;

namespace IwakenLabUnityStudy
{
    public class DrawFlow : MonoBehaviour
    {
        [SerializeField] private XrObjectResolver xrObjectResolver;
        [SerializeField] private OvrInputObserver ovrInputObserver;
        [SerializeField] private DrawWeapon drawSword;
        [SerializeField] private DrawWeapon drawGuard;
        [SerializeField] private BattleSword battleSwordPrefab;
        [SerializeField] private BattleGuard battleGuardPrefab;
        [SerializeField] private WeaponIconsView weaponIconsSwordView;
        [SerializeField] private WeaponIconsView weaponIconsShieldView;
        [FormerlySerializedAs("timerSlider")] [SerializeField]
        private Slider limitSlider;

        [Header("指示文")]
        // 単一 -> 複数（2枚）に変更
        [SerializeField] private Image mainImage1;
        [SerializeField] private Image mainImage2;
        // 剣/盾 それぞれ mainImage1/2 用スプライト
        [SerializeField] private Sprite mainImageSword1;
        [SerializeField] private Sprite mainImageSword2;
        [SerializeField] private Sprite mainImageShield1;
        [SerializeField] private Sprite mainImageShield2;

        [Header("武器設定")]
        [SerializeField] private int swordCount = 3;
        [SerializeField] private int shieldCount = 3;
        [SerializeField] private int inkAmount = 30;

        private int TotalWeaponCount => swordCount + shieldCount;

        private readonly Subject<BattlePlayerData> _finishDraw = new();
        public Observable<BattlePlayerData> FinishDraw => _finishDraw;

        private WeaponDrawSequencer _weaponDrawSequencer;
        private IDisposable _drawSubscription;
        private List<SplineNode[]> _swordNodeList = new();
        private List<SplineNode[]> _guardNodeList = new();
        private List<BattleWeapon> _weaponInstanceList = new();

        // タイムアップによる完了かどうかを管理する
        private bool _isTimeout = false;

        // Model
        private readonly ReactiveProperty<int> _progress = new(0);
        private readonly ReactiveProperty<float> _limitRate = new(1);

        private IDisposable _progressSubscription;

        private bool IsSword(int progress)
        {
            return progress < swordCount;
        }

        private void Awake()
        {
            // View
            _progress.Subscribe(progress =>
            {
                if (IsSword(progress))
                {
                    if (mainImage1 && mainImageSword1) mainImage1.sprite = mainImageSword1;
                    if (mainImage2 && mainImageSword2) mainImage2.sprite = mainImageSword2;

                    weaponIconsSwordView.gameObject.SetActive(true);
                    weaponIconsShieldView.gameObject.SetActive(false);
                    weaponIconsSwordView.UpdateCount(swordCount, progress);
                }
                else
                {
                    if (mainImage1 && mainImageShield1) mainImage1.sprite = mainImageShield1;
                    if (mainImage2 && mainImageShield2) mainImage2.sprite = mainImageShield2;

                    weaponIconsSwordView.gameObject.SetActive(false);
                    weaponIconsShieldView.gameObject.SetActive(true);
                    weaponIconsShieldView.UpdateCount(shieldCount, progress - swordCount);
                }
            }).AddTo(this);
            _limitRate.Subscribe(value => limitSlider.value = value).AddTo(this);

            // Presenterは自分自身。多分きっとコンナカンジ


            _progressSubscription = _progress.Subscribe(progress =>
            {
                if (progress >= TotalWeaponCount)
                {
                    _progressSubscription?.Dispose();
                    _finishDraw.OnNext(new BattlePlayerData(_swordNodeList.ToArray(), _guardNodeList.ToArray()));
                    return;
                }

                _weaponDrawSequencer?.Dispose();
                _drawSubscription?.Dispose();
                drawSword.gameObject.SetActive(false);
                drawGuard.gameObject.SetActive(false);
                _limitRate.Value = 1;
                // 新しい描画ステップを開始するためにタイムアウトフラグをリセットする
                _isTimeout = false;

                if (IsSword(progress))
                {
                    drawSword.gameObject.SetActive(true);
                    _drawSubscription = (_weaponDrawSequencer =
                            new WeaponDrawSequencer(ovrInputObserver, drawSword, xrObjectResolver))
                        .OnDrawEnd.Subscribe(nodes =>
                        {
                            // 正常完了時のみ描画完了の効果音を鳴らす
                            if (!_isTimeout)
                            {
                                SEManager.Instance.Play(SEPath.DRAW_COMP);
                            }

                            // nullの場合は空配列を使用
                            var safeNodes = nodes ?? Array.Empty<SplineNode>();
                            _swordNodeList.Add(safeNodes);
                            /*
                            var instance = Instantiate(battleSwordPrefab, xrObjectResolver.RightHandObj.transform);
                            _weaponInstanceList.Add(instance);
                            instance.Initialize(data, Utils.SelfLayer);
                            */

                            _progress.Value++;
                        }).AddTo(this);
                }
                else
                {
                    drawGuard.gameObject.SetActive(true);
                    _drawSubscription = (_weaponDrawSequencer =
                            new WeaponDrawSequencer(ovrInputObserver, drawGuard, xrObjectResolver))
                        .OnDrawEnd.Subscribe(nodes =>
                        {
                            // 正常完了時のみ描画完了の効果音を鳴らす
                            if (!_isTimeout)
                            {
                                SEManager.Instance.Play(SEPath.DRAW_COMP);
                            }

                            // nullの場合は空配列を使用
                            var safeNodes = nodes ?? Array.Empty<SplineNode>();
                            _guardNodeList.Add(safeNodes);
                            /*
                            var instance = Instantiate(battleGuardPrefab, xrObjectResolver.RightHandObj.transform);
                            _weaponInstanceList.Add(instance);
                            instance.Initialize(data, Utils.SelfLayer);
                            */

                            _progress.Value++;
                        }).AddTo(this);
                }
            }).AddTo(this);
        }

        private void Update()
        {
            if (_weaponDrawSequencer == null) return;
            _limitRate.Value = 1 - Mathf.Clamp(_weaponDrawSequencer.UsedAmount / inkAmount, 0, 1);
            if (_limitRate.CurrentValue == 0)
            {
                // 時間切れの場合はフラグを立て、タイムアップ効果音を再生してから強制完了させる
                _isTimeout = true;
                SEManager.Instance.Play(SEPath.TIMEUP);
                _weaponDrawSequencer?.ForceDrawComplete();
            }
        }

        private void OnEnable()
        {
            _swordNodeList.Clear();
            _guardNodeList.Clear();

            _limitRate.OnNext(1);
            _progress.OnNext(0);
        }
    }
}