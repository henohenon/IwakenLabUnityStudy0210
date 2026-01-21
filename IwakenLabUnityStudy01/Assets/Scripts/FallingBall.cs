using UnityEngine;

namespace IwakenLabUnityStudy
{
    /// <summary>
    /// 上から落下するボール
    /// </summary>
    public class FallingBall : MonoBehaviour
    {
        [SerializeField] private float fallSpeed = 3f;

        private void Update()
        {
            // 下方向に移動
            transform.Translate(Vector3.down * (fallSpeed * Time.deltaTime), Space.World);
        }

        private void OnTriggerEnter(Collider other)
        {
            // BattleWeaponに当たったら消える
            if (other.GetComponent<BattleWeapon>() != null)
            {
                Destroy(gameObject);
            }
        }
    }
}