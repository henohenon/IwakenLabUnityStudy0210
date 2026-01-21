using System.Collections.Generic;
using UnityEngine;

namespace IwakenLabUnityStudy
{
    public class DrawWeapon : MonoBehaviour
    {
        [SerializeField] private LineRenderer lineRendererPrefab;
        [SerializeField] private float lineWidth = 0.1f;
        [SerializeField] private Color lineColor = Color.black;
        [SerializeField] private float minDistance = 0.01f;

        private LineRenderer _currentLine;
        private List<Vector3> _currentPoints = new();
        private List<LineRenderer> _allLines = new();
        private bool _isDrawing = false;

        public bool DrawStart(Vector3 position)
        {
            if (_isDrawing) return false;

            _isDrawing = true;
            _currentPoints.Clear();

            // 新しいLineRendererを作成
            if (lineRendererPrefab != null)
            {
                _currentLine = Instantiate(lineRendererPrefab, transform);
            }
            else
            {
                var go = new GameObject("Line");
                go.transform.SetParent(transform);
                _currentLine = go.AddComponent<LineRenderer>();
                _currentLine.material = new Material(Shader.Find("Sprites/Default"));
            }

            // LineRendererの設定
            _currentLine.startWidth = lineWidth;
            _currentLine.endWidth = lineWidth;
            _currentLine.startColor = lineColor;
            _currentLine.endColor = lineColor;
            _currentLine.positionCount = 0;
            _currentLine.useWorldSpace = true;

            // 最初の点を追加
            AddPoint(position);
            _allLines.Add(_currentLine);
            return true;
        }

        public void Draw(Vector3 position, float deltaTime = 0f)
        {
            if (!_isDrawing || _currentLine == null) return;

            // 前の点から一定距離離れていたら新しい点を追加
            if (_currentPoints.Count == 0 ||
                Vector3.Distance(position, _currentPoints[^1]) > minDistance)
            {
                AddPoint(position);
            }
        }

        private void AddPoint(Vector3 position)
        {
            // Z座標を固定（2D描画のため）
            position.z = 0;
            _currentPoints.Add(position);
            _currentLine.positionCount = _currentPoints.Count;
            _currentLine.SetPositions(_currentPoints.ToArray());
        }

        public Vector3[] DrawEnd()
        {
            if (!_isDrawing) return null;
            _isDrawing = false;
            var result = _currentPoints.ToArray();
            _currentLine = null;
            return result;
        }

        // 全ての線をクリア
        public void ClearAll()
        {
            foreach (var line in _allLines)
            {
                if (line != null)
                {
                    Destroy(line.gameObject);
                }
            }
            _allLines.Clear();
            _currentPoints.Clear();
        }
    }
}
