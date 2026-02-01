using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Masks
{
    /// <summary>
    /// Vertex-color gradient for Unity UI elements.
    /// Splits the mesh at gradient boundaries so start/end produce visible hard edges.
    /// </summary>
    [AddComponentMenu("UI/Effects/UI Gradient")]
    public class UIGradient : BaseMeshEffect
    {
        public enum Direction { TopToBottom, BottomToTop }

        [SerializeField] Direction direction = Direction.TopToBottom;
        [SerializeField] Color topColor = Color.white;
        [SerializeField] Color bottomColor = Color.black;

        [Header("Gradient Range")]
        [Tooltip("Normalized position where the gradient begins. Solid topColor before this.")]
        [SerializeField, Range(0f, 1f)] float gradientStart = 0f;
        [Tooltip("Normalized position where the gradient ends. Solid bottomColor after this.")]
        [SerializeField, Range(0f, 1f)] float gradientEnd = 1f;

        // reusable buffers
        static readonly List<UIVertex> _stream = new();
        static readonly List<UIVertex> _passA = new();
        static readonly List<UIVertex> _passB = new();

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            if (graphic) graphic.SetVerticesDirty();
        }
#endif

        public override void ModifyMesh(VertexHelper vh)
        {
            if (!IsActive() || vh.currentVertCount == 0)
                return;

            _stream.Clear();
            vh.GetUIVertexStream(_stream);
            if (_stream.Count < 3) return;

            // global Y bounds
            float minY = float.MaxValue, maxY = float.MinValue;
            for (int i = 0; i < _stream.Count; i++)
            {
                float y = _stream[i].position.y;
                if (y < minY) minY = y;
                if (y > maxY) maxY = y;
            }

            float range = maxY - minY;
            if (range < 0.001f) return;

            float lo = Mathf.Min(gradientStart, gradientEnd);
            float hi = Mathf.Max(gradientStart, gradientEnd);

            // convert gradient thresholds to Y positions
            float splitA, splitB;
            if (direction == Direction.TopToBottom)
            {
                // t=0 at top, t=1 at bottom  →  y = maxY − t*range
                splitA = maxY - lo * range;
                splitB = maxY - hi * range;
            }
            else
            {
                // t=0 at bottom, t=1 at top  →  y = minY + t*range
                splitA = minY + lo * range;
                splitB = minY + hi * range;
            }

            // pass 1 – split every triangle at the first boundary
            _passA.Clear();
            for (int i = 0; i + 2 < _stream.Count; i += 3)
                SplitTriAtY(_stream[i], _stream[i + 1], _stream[i + 2], splitA, _passA);

            // pass 2 – split at the second boundary
            _passB.Clear();
            for (int i = 0; i + 2 < _passA.Count; i += 3)
                SplitTriAtY(_passA[i], _passA[i + 1], _passA[i + 2], splitB, _passB);

            // colour every vertex
            bool collapsed = hi - lo < 0.0001f;
            for (int i = 0; i < _passB.Count; i++)
            {
                UIVertex v = _passB[i];

                float t = (v.position.y - minY) / range;          // 0 = bottom, 1 = top
                if (direction == Direction.TopToBottom) t = 1f - t; // 0 = top edge, 1 = bottom edge

                float gt;
                if (collapsed)
                    gt = t <= lo ? 0f : 1f;
                else
                    gt = Mathf.Clamp01((t - lo) / (hi - lo));

                Color gc = Color.Lerp(topColor, bottomColor, gt);
                v.color = (Color)v.color * gc;
                _passB[i] = v;
            }

            // rebuild
            vh.Clear();
            for (int i = 0; i + 2 < _passB.Count; i += 3)
            {
                int idx = vh.currentVertCount;
                vh.AddVert(_passB[i]);
                vh.AddVert(_passB[i + 1]);
                vh.AddVert(_passB[i + 2]);
                vh.AddTriangle(idx, idx + 1, idx + 2);
            }
        }

        // ── triangle split ──────────────────────────────────────────────

        static void SplitTriAtY(UIVertex v0, UIVertex v1, UIVertex v2, float splitY, List<UIVertex> output)
        {
            bool a0 = v0.position.y >= splitY;
            bool a1 = v1.position.y >= splitY;
            bool a2 = v2.position.y >= splitY;
            int above = (a0 ? 1 : 0) + (a1 ? 1 : 0) + (a2 ? 1 : 0);

            // nothing to split – all verts on the same side
            if (above == 0 || above == 3)
            {
                output.Add(v0); output.Add(v1); output.Add(v2);
                return;
            }

            // identify the lone vertex (opposite side from the other two)
            UIVertex lone, pA, pB;
            if (a0 != a1 && a0 != a2)      { lone = v0; pA = v1; pB = v2; }
            else if (a1 != a0 && a1 != a2)  { lone = v1; pA = v0; pB = v2; }
            else                             { lone = v2; pA = v0; pB = v1; }

            float dA = pA.position.y - lone.position.y;
            float dB = pB.position.y - lone.position.y;

            if (Mathf.Abs(dA) < 0.001f || Mathf.Abs(dB) < 0.001f)
            {
                output.Add(v0); output.Add(v1); output.Add(v2);
                return;
            }

            float tA = (splitY - lone.position.y) / dA;
            float tB = (splitY - lone.position.y) / dB;
            UIVertex sA = LerpVert(lone, pA, tA);
            UIVertex sB = LerpVert(lone, pB, tB);

            // lone-side triangle
            output.Add(lone); output.Add(sA); output.Add(sB);
            // pair-side quad  (two triangles)
            output.Add(sA); output.Add(pA); output.Add(pB);
            output.Add(sA); output.Add(pB); output.Add(sB);
        }

        static UIVertex LerpVert(UIVertex a, UIVertex b, float t)
        {
            UIVertex v = default;
            v.position = Vector3.Lerp(a.position, b.position, t);
            v.normal   = Vector3.Lerp(a.normal,   b.normal,   t);
            v.tangent  = Vector4.Lerp(a.tangent,  b.tangent,  t);
            v.color    = Color32.Lerp(a.color,    b.color,    t);
            v.uv0      = Vector4.Lerp(a.uv0,     b.uv0,      t);
            v.uv1      = Vector4.Lerp(a.uv1,     b.uv1,      t);
            v.uv2      = Vector4.Lerp(a.uv2,     b.uv2,      t);
            v.uv3      = Vector4.Lerp(a.uv3,     b.uv3,      t);
            return v;
        }
    }
}
