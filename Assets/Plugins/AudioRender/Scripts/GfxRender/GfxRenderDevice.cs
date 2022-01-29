using System;
using System.Collections.Generic;
using UnityEngine;

namespace AudioRender
{
    public class GfxRenderDevice : MonoBehaviour, IRenderDevice
    {
        public void Dispose()
        {
        }
        void Start() {
            Camera.main.clearFlags = CameraClearFlags.Nothing;
            Camera.onPostRender += OnPostRenderCallback;
        }

        void OnDestroy() {
            Camera.onPostRender -= OnPostRenderCallback;
        }

        public bool WaitSync() {
            return true;
        }

        public void Begin() {
            this.lines = new List<GfxLine>();
            this.submittedLines = new List<GfxLine>();
            this.gfxRenderer = new GfxRenderer();
            this.position = Vector2.zero;
            this.intensity = 1.0f;
        }

        public void Submit() {
            var temp = lines;
            this.lines = submittedLines;
            this.submittedLines = temp;
            lines.Clear();
        }

        public Rect GetViewPort()
        {
            return new Rect(0, 0, 1, 1);
        }

        public void SetPoint(Vector2 point)
        {
            this.position = point;
        }

        public void SetIntensity(float intensity) {
            this.intensity = intensity;
        }

        public void DrawCircle(float radius)
        {
            // TODO
        }

        public void DrawLine(Vector2 point, float intensity = -1) {
            if (intensity >= 0.0f) {
                this.intensity = intensity;
            }
            lines.Add(new GfxLine(this.position, point, this.intensity));
            this.position = point;
        }

        public void SyncPoint(IntPtr device, Vector2 point) {
            this.position = point;
        }

        private void OnPostRenderCallback(Camera camera) {
            gfxRenderer.Render(submittedLines, decayMaterial, lineMaterial);
        }

        public Material decayMaterial;
        public Material lineMaterial;
        private GfxRenderer gfxRenderer;
        private List<GfxLine> lines;
        private List<GfxLine> submittedLines;
        private Vector2 position;
        private float intensity;
    }
}