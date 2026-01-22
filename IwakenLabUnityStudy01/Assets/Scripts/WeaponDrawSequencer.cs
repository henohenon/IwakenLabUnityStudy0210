using System;
using R3;
using UnityEngine;

namespace IwakenLabUnityStudy
{
    public class WeaponDrawSequencer : IDisposable
    {
        private readonly Subject<Vector3[]> _onDrawEnd = new();
        public Observable<Vector3[]> OnDrawEnd => _onDrawEnd;
        private IDisposable _touchSubscription;
        private CompositeDisposable _compositeDisposable = new();
        private bool _isDrawing;

        public WeaponDrawSequencer(MouseInputObserver mouseInput, DrawWeapon weapon)
        {
            // マウス左クリックで描画
            mouseInput.LeftClick.Subscribe(value =>
            {
                if (value)
                {
                    var mousePosition = mouseInput.MouseWorldPosition.CurrentValue;
                    _isDrawing = weapon.DrawStart(mousePosition);
                    if (_isDrawing)
                    {
                        _touchSubscription = Observable.EveryUpdate().Subscribe(_ =>
                        {
                            var deltaTime = Time.deltaTime;
                            var pos = mouseInput.MouseWorldPosition.CurrentValue;

                            weapon.Draw(pos, deltaTime);
                        });
                    }
                }
                else
                {
                    if (!_isDrawing) return;
                    _isDrawing = false;
                    _touchSubscription?.Dispose();
                    var data = weapon.DrawEnd();
                    if (data != null) _onDrawEnd.OnNext(data);
                }
            }).AddTo(_compositeDisposable);
        }

        public void Dispose()
        {
            _touchSubscription?.Dispose();
            _compositeDisposable?.Dispose();

            _touchSubscription = null;
            _compositeDisposable = null;
        }
    }
}
