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
            //[SerializeField] private TMPro.TMP_Text text;
            //[SerializeField] private TutorialLineController tutorialLineController;

            private readonly Subject<Unit> _finishTutorial = new ();
            public Observable<Unit> FinishTutorial => _finishTutorial;

            private WeaponDrawSequencer _ovrWeaponDraw;
            private BattleWeapon _battleWeaponInstance;
            private IDisposable _drawSubscription;
            private IDisposable _startSubscription;

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


                    //_startSubscription?.Dispose();
                    //_startSubscription = _battleWeaponInstance.OnHit.Subscribe(col =>
                    //{
                    //    if(!col.gameObject.CompareTag("StartObject")) return;
                    //    tutorialLineController.gameObject.SetActive(false);
                    //    Destroy(_battleWeaponInstance.gameObject);
                    //    _ovrWeaponDraw?.Dispose();
                    //    _ovrWeaponDraw = null;
                    //    _finishTutorial.OnNext(Unit.Default);
                    //    _startSubscription?.Dispose();
                    //}).AddTo(this);
                }).AddTo(this);
            }

            private void OnEnable()
            {
                text.text = "一筆書きで剣を書け！";
            }
    }
}
