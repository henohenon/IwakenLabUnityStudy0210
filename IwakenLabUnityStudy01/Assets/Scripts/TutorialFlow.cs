using UnityEngine;
using System;
using R3;

namespace IwakenLabUnityStudy
{
    public class TutorialFlow : MonoBehaviour
    {
            [SerializeField] private XrObjectResolver xrObjectResolver;
            [SerializeField] private OvrInputObserver ovrInputObserver;
            [SerializeField] private GlobalData globalData;
            [SerializeField] private DrawWeapon drawWeapon;
            [SerializeField] private TMPro.TMP_Text text;
            [SerializeField] private Collider startObject;
            [SerializeField] private TutorialLineController tutorialLineController;

            private readonly Subject<Unit> _finishTutorial = new ();
            public Observable<Unit> FinishTutorial => _finishTutorial;

            private WeaponDrawSequencer _ovrWeaponDraw;
            private BattleWeapon _battleWeaponInstance;
            private IDisposable _drawSubscription;
            private IDisposable _startSubscription;

            private void Awake()
            {
                if(!xrObjectResolver)
                {
                    throw new Exception("No XrObjectResolver found");
                }

                _ovrWeaponDraw = new WeaponDrawSequencer(ovrInputObserver, drawWeapon, xrObjectResolver, tutorialLineController);

                _drawSubscription = _ovrWeaponDraw.OnDrawEnd.Subscribe(nodes =>
                {
                    _drawSubscription?.Dispose();

                    text.text = "ぶった斬れ！";
                    xrObjectResolver.PenController.EnablePen(false);
                    drawWeapon.gameObject.SetActive(false);
                    startObject.gameObject.SetActive(true);

                    _battleWeaponInstance = Instantiate(globalData.prefabDictionary[WeaponStyle.Sword], xrObjectResolver.BattlePlayer.RightHand.transform);
                    _battleWeaponInstance.Initialize(nodes, WeaponStyle.Sword, Utils.SelfLayer);

                    tutorialLineController.To = startObject.gameObject.transform;
                    tutorialLineController.From = _battleWeaponInstance.transform;
                    tutorialLineController.gameObject.SetActive(true);


                    _startSubscription?.Dispose();
                    _startSubscription = _battleWeaponInstance.OnHit.Subscribe(col =>
                    {
                        if(!col.gameObject.CompareTag("StartObject")) return;
                        tutorialLineController.gameObject.SetActive(false);
                        Destroy(_battleWeaponInstance.gameObject);
                        _ovrWeaponDraw?.Dispose();
                        _ovrWeaponDraw = null;
                        _finishTutorial.OnNext(Unit.Default);
                        _startSubscription?.Dispose();
                    }).AddTo(this);
                }).AddTo(this);
            }

            private void OnEnable()
            {
                text.text = "一筆書きで剣を書け！";
                startObject.gameObject.SetActive(false);
            }
    }
}