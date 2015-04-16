using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

namespace StepDX
{
    public class PolygonTextured : Polygon
    {
        /// <summary>
        /// The texture map we use for this polygon
        /// </summary>
        private Texture texture;

        /// <summary>
        /// List of texture coordinates
        /// </summary>
        protected List<Vector2> textureC = new List<Vector2>();

        /// <summary>
        /// Indicates if the texture is transparent
        /// </summary>
        private bool transparent;

        /// <summary>
        /// Indicates if the texture is transparent
        /// </summary>
        public bool Transparent
        {
            set { transparent = value; }
            get { return transparent; }
        }

        /// <summary>
        /// The texture map we use for this polygon
        /// </summary>
        public Texture Tex
        {
            get { return texture; }
            set { texture = value; }
        }

        /// <summary>
        /// Add a texture coordinate
        /// </summary>
        /// <param name="v">Texture coordinate</param>
        public void AddTex(Vector2 v) { textureC.Add(v); }

        /// <summary>
        /// Render the textured polygon
        /// </summary>
        /// <param name="device">Device to render onto</param>
        public override void Render(Device device)
        {
            // Get the vertices we will draw
            var vertices = Vertices;

            // Ensure we have at least a triangle
            if (vertices.Count < 3) return;

            // Ensure the number of vertices and textures are the same
            Debug.Assert(textureC.Count == vertices.Count);

            if (verticesV == null)
            {
                verticesV = new VertexBuffer(typeof (CustomVertex.PositionColoredTextured), // Type
                    vertices.Count, // How many
                    device, // What device
                    0, // No special usage
                    CustomVertex.PositionColoredTextured.Format,
                    Pool.Managed);
            }

            var gs = verticesV.Lock(0, 0, 0); // Lock the background vertex list
            var clr = color.ToArgb();

            for (var i = 0; i < vertices.Count; i++)
            {
                var v = vertices[i];
                var t = textureC[i];
                gs.Write(new CustomVertex.PositionColoredTextured(v.X, v.Y, 0, clr, t.X, t.Y));
            }

            verticesV.Unlock();

            if (transparent)
            {
                device.RenderState.AlphaBlendEnable = true;
                device.RenderState.SourceBlend = Blend.SourceAlpha;
                device.RenderState.DestinationBlend = Blend.InvSourceAlpha;
            }

            device.SetTexture(0, texture);
            device.SetStreamSource(0, verticesV, 0);
            device.VertexFormat = CustomVertex.PositionColoredTextured.Format;
            device.DrawPrimitives(PrimitiveType.TriangleFan, 0, vertices.Count - 2);
            device.SetTexture(0, null);

            if (transparent) device.RenderState.AlphaBlendEnable = false;
        }
    }
}