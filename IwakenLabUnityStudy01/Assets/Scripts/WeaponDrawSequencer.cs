using System;
using R3;
using UnityEngine;

namespace IwakenLabUnityStudy
{
    public class WeaponDrawSequencer : IDisposable
    {
        private readonly Subject<Vector3[]> _onDrawEnd = new();
        public Observable<Vector3[]> OnDrawEnd => _onDrawEnd;
        private readonly Subject<Unit> _onDrawStart = new();
        public Observable<Unit> OnDrawStart => _onDrawStart;
        private IDisposable _touchSubscription;
        private CompositeDisposable _compositeDisposable = new();
        private bool _isDrawing;
        private DrawWeapon _weapon;
        private float _usedAmount;
        public float UsedAmount => _usedAmount;
        private Vector3? _previousWritePos;

        public WeaponDrawSequencer(MouseInputObserver mouseInput, DrawWeapon weapon)
        {
            _weapon = weapon;

            // マウス左クリックで描画
            mouseInput.LeftClick.Subscribe(value =>
            {
                if (value)
                {
                    var mousePosition = mouseInput.MouseWorldPosition.CurrentValue;
                    _isDrawing = weapon.DrawStart(mousePosition);
                    if (_isDrawing)
                    {
                        _onDrawStart.OnNext(Unit.Default);
                        _previousWritePos = null;
                        _touchSubscription = Observable.EveryUpdate().Subscribe(_ =>
                        {
                            var deltaTime = Time.deltaTime;
                            var pos = mouseInput.MouseWorldPosition.CurrentValue;

                            weapon.Draw(pos, deltaTime);

                            var diff = _previousWritePos.HasValue
                                ? (_previousWritePos.Value - pos).magnitude
                                : 0f;
                            _usedAmount += deltaTime + diff * 3;
                            _previousWritePos = pos;
                        });
                    }
                }
                else
                {
                    if (!_isDrawing) return;
                    _isDrawing = false;
                    _touchSubscription?.Dispose();
                    var data = weapon.DrawEnd();
                    _weapon = null;
                    if (data != null) _onDrawEnd.OnNext(data);
                }
            }).AddTo(_compositeDisposable);
        }

        public void ForceDrawComplete()
        {
            if (_weapon == null) return;
            _isDrawing = false;
            _touchSubscription?.Dispose();
            var data = _weapon.DrawEnd();
            if (data != null) _onDrawEnd.OnNext(data);
            _weapon = null;
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
