using UnityEngine;

namespace IwakenLabUnityStudy
{
    /// <summary>
    /// 子オブジェクトのコライダーイベントを親のBattleWeaponに転送する
    /// </summary>
    public class ColliderForwarder : MonoBehaviour
    {
        private BattleWeapon _battleWeapon;

        public void Initialize(BattleWeapon battleWeapon)
        {
            _battleWeapon = battleWeapon;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_battleWeapon != null)
            {
                _battleWeapon.NotifyHit(other);
            }
        }
    }
}