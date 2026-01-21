using R3;
using UnityEngine;
using UnityEngine.InputSystem;

namespace IwakenLabUnityStudy
{
    public class MouseInputObserver : MonoBehaviour
    {
        private readonly ReactiveProperty<bool> _leftClick = new();
        private readonly ReactiveProperty<bool> _rightClick = new();
        private readonly ReactiveProperty<Vector3> _mousePosition = new();

        /// <summary>
        /// マウス左クリック状態（OvrInputObserver.RIndexTrigger の代替）
        /// </summary>
        public ReadOnlyReactiveProperty<bool> LeftClick => _leftClick;

        /// <summary>
        /// マウス右クリック状態
        /// </summary>
        public ReadOnlyReactiveProperty<bool> RightClick => _rightClick;

        /// <summary>
        /// マウスのワールド座標
        /// </summary>
        public ReadOnlyReactiveProperty<Vector3> MouseWorldPosition => _mousePosition;

        [SerializeField] private Camera targetCamera;

        private void Start()
        {
            if (targetCamera == null)
            {
                targetCamera = Camera.main;
            }
        }

        private void Update()
        {
            var mouse = Mouse.current;
            if (mouse == null) return;

            // マウスボタンの状態を更新
            _leftClick.Value = mouse.leftButton.isPressed;
            _rightClick.Value = mouse.rightButton.isPressed;

            // マウスのワールド座標を更新
            if (targetCamera != null)
            {
                Vector3 mousePos = mouse.position.ReadValue();
                mousePos.z = Mathf.Abs(targetCamera.transform.position.z);
                _mousePosition.Value = targetCamera.ScreenToWorldPoint(mousePos);
            }
        }

        /// <summary>
        /// OvrInputObserver.RIndexTrigger の代替プロパティ
        /// WeaponDrawSequencer との互換性のため
        /// </summary>
        public ReadOnlyReactiveProperty<bool> RIndexTrigger => _leftClick;
    }
}