using UnityEngine;
using R3;

namespace IwakenLabUnityStudy
{
    /// <summary>
    /// 描画されたノードデータを受け取り、武器としてGameObject化するクラス
    /// </summary>
    public class BattleWeapon : MonoBehaviour
    {
        [SerializeField] private LineRenderer lineRenderer;
        [SerializeField] private float lineWidth = 0.1f;
        [SerializeField] private Material lineMaterial;

        private readonly Subject<Collider> _onHit = new();
        public Observable<Collider> OnHit => _onHit;

        private Vector3[] _nodes;
        private BoxCollider _collider;

        /// <summary>
        /// ノードデータを受け取り、武器を初期化する
        /// </summary>
        /// <param name="nodes">描画されたノードの座標配列</param>
        public void Initialize(Vector3[] nodes)
        {
            _nodes = nodes;

            if (nodes == null || nodes.Length < 2)
            {
                Debug.LogWarning("BattleWeapon: ノードが不足しています");
                return;
            }

            // LineRendererの設定
            SetupLineRenderer(nodes);

            // Colliderの設定
            SetupCollider(nodes);
        }

        private void SetupLineRenderer(Vector3[] nodes)
        {
            if (lineRenderer == null)
            {
                lineRenderer = gameObject.AddComponent<LineRenderer>();
            }

            lineRenderer.positionCount = nodes.Length;
            lineRenderer.SetPositions(nodes);
            lineRenderer.startWidth = lineWidth;
            lineRenderer.endWidth = lineWidth;
            lineRenderer.useWorldSpace = true;

            if (lineMaterial != null)
            {
                lineRenderer.material = lineMaterial;
            }
        }

        private void SetupCollider(Vector3[] nodes)
        {
            // ノード全体を囲むBoundsを計算
            var bounds = new Bounds(nodes[0], Vector3.zero);
            foreach (var node in nodes)
            {
                bounds.Encapsulate(node);
            }

            // 位置を中心に設定
            transform.position = bounds.center;

            // BoxColliderを追加
            _collider = gameObject.AddComponent<BoxCollider>();
            _collider.size = bounds.size + Vector3.one * lineWidth;
            _collider.center = Vector3.zero;
            _collider.isTrigger = true;

            // Rigidbodyを追加（トリガー検出用）
            var rb = gameObject.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        private void OnTriggerEnter(Collider other)
        {
            _onHit.OnNext(other);
        }

        private void OnDestroy()
        {
            _onHit.Dispose();
        }
    }
}
