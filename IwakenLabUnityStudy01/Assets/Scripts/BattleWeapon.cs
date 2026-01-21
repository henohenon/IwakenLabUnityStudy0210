using UnityEngine;
using UnityEngine.InputSystem;
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
        [SerializeField] private float moveSpeed = 5f;

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

        private void Update()
        {
            // 十字キー/WASDで移動（新Input System使用）
            var keyboard = Keyboard.current;
            if (keyboard == null) return;

            float horizontal = 0f;
            float vertical = 0f;

            // WASD
            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) horizontal -= 1f;
            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) horizontal += 1f;
            if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) vertical -= 1f;
            if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) vertical += 1f;

            if (horizontal == 0f && vertical == 0f) return;

            Vector3 movement = new Vector3(horizontal, vertical, 0f) * (moveSpeed * Time.deltaTime);
            transform.Translate(movement, Space.World);

            // LineRendererのノード位置も更新（useWorldSpace=trueのため）
            if (_nodes != null && lineRenderer != null)
            {
                for (int i = 0; i < _nodes.Length; i++)
                {
                    _nodes[i] += movement;
                }
                lineRenderer.SetPositions(_nodes);
            }
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
