using System;
using System.Threading;
using System.Threading.Tasks;
using R3;
using UnityEngine;

namespace IwakenLabUnityStudy
{
    public class WeaponDrawSequencer : IDisposable
    {
        private IDisposable _touchSubscription;
        private CompositeDisposable _compositeDisposable = new();
        
        private readonly MouseInputObserver _mouseInput;
        private readonly DrawWeapon _weapon;
        private bool _isDrawing;
 
        public WeaponDrawSequencer(MouseInputObserver mouseInput, DrawWeapon weapon)
        {
            _mouseInput = mouseInput;
            _weapon = weapon;
            _isDrawing = false;
        }

        public async Task<Vector3[]> DrawSequence(CancellationToken token)
        {
            if (_isDrawing) return Array.Empty<Vector3>();
            await _mouseInput.LeftClick.Where(clicked => clicked == true).FirstAsync(token);

            var mousePosition = _mouseInput.MouseWorldPosition.CurrentValue;
            _isDrawing = _weapon.DrawStart(mousePosition);

            if (!_isDrawing) return Array.Empty<Vector3>();
            _touchSubscription = Observable.EveryUpdate().Subscribe(_ =>
            {
                var deltaTime = Time.deltaTime;
                var pos = _mouseInput.MouseWorldPosition.CurrentValue;

                _weapon.Draw(pos, deltaTime);
            });

            await _mouseInput.LeftClick.Where(clicked => clicked == false).FirstAsync(token);

            _isDrawing = false;
            _touchSubscription?.Dispose();
            return _weapon.DrawEnd();
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
