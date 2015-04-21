using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

namespace StepDX
{
    public class PolygonTextured : Polygon
    {
        /// <summary>
        ///     List of texture coordinates
        /// </summary>
        protected List<Vector2> TextureC = new List<Vector2>();

        /// <summary>
        ///     The texture map we use for this polygon
        /// </summary>
        private Texture _texture;

        /// <summary>
        ///     Indicates if the texture is transparent
        /// </summary>
        private bool _transparent;

        /// <summary>
        ///     The texture map we use for this polygon
        /// </summary>
        public Texture Tex
        {
            get { return _texture; }
            set { _texture = value; }
        }

        /// <summary>
        ///     Indicates if the texture is transparent
        /// </summary>
        public bool Transparent
        {
            set { _transparent = value; }
            get { return _transparent; }
        }

        /// <summary>
        ///     Add a texture coordinate
        /// </summary>
        /// <param name="v">Texture coordinate</param>
        public void AddTex(Vector2 v)
        {
            TextureC.Add(v);
        }

        /// <summary>
        ///     Render the textured polygon
        /// </summary>
        /// <param name="device">Device to render onto</param>
        public override void Render(Device device)
        {
            // Get the vertices we will draw
            var vertices = Vertices;

            // Ensure we have at least a triangle
            if (vertices.Count < 3) return;

            // Ensure the number of vertices and textures are the same
            Debug.Assert(TextureC.Count == vertices.Count);

            if (VerticesV == null)
            {
                VerticesV = new VertexBuffer(typeof (CustomVertex.PositionColoredTextured), // Type
                    vertices.Count, // How many
                    device, // What device
                    0, // No special usage
                    CustomVertex.PositionColoredTextured.Format,
                    Pool.Managed);
            }

            var gs = VerticesV.Lock(0, 0, 0); // Lock the background vertex list
            var clr = PolyColor.ToArgb();

            for (var i = 0; i < vertices.Count; i++)
            {
                var v = vertices[i];
                var t = TextureC[i];
                gs.Write(new CustomVertex.PositionColoredTextured(v.X, v.Y, 0, clr, t.X, t.Y));
            }

            VerticesV.Unlock();

            if (_transparent)
            {
                device.RenderState.AlphaBlendEnable = true;
                device.RenderState.SourceBlend = Blend.SourceAlpha;
                device.RenderState.DestinationBlend = Blend.InvSourceAlpha;
            }

            device.SetTexture(0, _texture);
            device.SetStreamSource(0, VerticesV, 0);
            device.VertexFormat = CustomVertex.PositionColoredTextured.Format;
            device.DrawPrimitives(PrimitiveType.TriangleFan, 0, vertices.Count - 2);
            device.SetTexture(0, null);

            if (_transparent) device.RenderState.AlphaBlendEnable = false;
        }
    }
}