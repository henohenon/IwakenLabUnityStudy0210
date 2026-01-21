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
        private GameObject _colliderContainer;

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
            // ノード全体の中心を計算
            var bounds = new Bounds(nodes[0], Vector3.zero);
            foreach (var node in nodes)
            {
                bounds.Encapsulate(node);
            }
            transform.position = bounds.center;

            // コライダー用のコンテナを作成
            _colliderContainer = new GameObject("Colliders");
            _colliderContainer.transform.SetParent(transform);
            _colliderContainer.transform.localPosition = Vector3.zero;

            // 各セグメントにCapsuleColliderを配置
            for (int i = 0; i < nodes.Length - 1; i++)
            {
                CreateSegmentCollider(nodes[i], nodes[i + 1], i);
            }

            // Rigidbodyを追加（トリガー検出用）
            var rb = gameObject.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        private void CreateSegmentCollider(Vector3 start, Vector3 end, int index)
        {
            var segmentObj = new GameObject($"Segment_{index}");
            segmentObj.transform.SetParent(_colliderContainer.transform);

            // セグメントの中点に配置
            var midPoint = (start + end) / 2f;
            segmentObj.transform.position = midPoint;

            // セグメントの方向に回転
            var direction = end - start;
            var length = direction.magnitude;
            if (length > 0.001f)
            {
                segmentObj.transform.rotation = Quaternion.FromToRotation(Vector3.up, direction.normalized);
            }

            // CapsuleColliderを追加
            var capsule = segmentObj.AddComponent<CapsuleCollider>();
            capsule.height = length + lineWidth;
            capsule.radius = lineWidth / 2f;
            capsule.direction = 1; // Y軸方向
            capsule.isTrigger = true;

            // 当たり判定をこのオブジェクトに転送するコンポーネントを追加
            var forwarder = segmentObj.AddComponent<ColliderForwarder>();
            forwarder.Initialize(this);
        }

        public void NotifyHit(Collider other)
        {
            _onHit.OnNext(other);
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

        private void OnDestroy()
        {
            _onHit.Dispose();
        }
    }
}
