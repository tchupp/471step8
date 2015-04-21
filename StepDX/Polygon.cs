using System.Collections.Generic;
using System.Drawing;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

namespace StepDX
{
    public class Polygon
    {
        /// <summary>
        ///     The color we will draw this polygon
        /// </summary>
        protected Color PolyColor = Color.AntiqueWhite;

        //
        // The vertices of the polygon
        //

        /// <summary>
        ///     The raw vertex values we are supplied
        /// </summary>
        protected List<Vector2> VerticesB = new List<Vector2>();

        /// <summary>
        ///     Vertex buffer used for drawing
        /// </summary>
        protected VertexBuffer VerticesV;

        /// <summary>
        ///     A property: The color we will draw the polygon
        /// </summary>
        public Color Color
        {
            set { PolyColor = value; }
            get { return PolyColor; }
        }

        /// <summary>
        ///     Access to the list of vertices
        /// </summary>
        public virtual List<Vector2> Vertices
        {
            get { return VerticesB; }
        }

        /// <summary>
        ///     Add a vertex to the polygon.  Must be called before the
        ///     first rendering of the polygon.
        /// </summary>
        /// <param name="vertex">The vertex to add</param>
        public void AddVertex(Vector2 vertex)
        {
            if (VerticesV == null) VerticesB.Add(vertex);
        }

        /// <summary>
        ///     Support for advancing an animation
        /// </summary>
        /// <param name="dt">A delta time amount in seconds</param>
        public virtual void Advance(float dt)
        {
        }

        /// <summary>
        ///     Draw the polygon
        /// </summary>
        /// <param name="device">The device to draw on</param>
        public virtual void Render(Device device)
        {
            // Get the vertices we will draw
            var vertices = Vertices;

            // Ensure we have at least a triangle
            if (vertices.Count < 3) return;

            if (VerticesV == null)
            {
                VerticesV = new VertexBuffer(typeof (CustomVertex.PositionColored), // Type
                    vertices.Count, // How many
                    device, // What device
                    0, // No special usage
                    CustomVertex.PositionColored.Format,
                    Pool.Managed);
            }

            var gs = VerticesV.Lock(0, 0, 0); // Lock the background vertex list
            var clr = PolyColor.ToArgb();

            foreach (var v in vertices)
            {
                gs.Write(new CustomVertex.PositionColored(v.X, v.Y, 0, clr));
            }

            VerticesV.Unlock();

            device.SetStreamSource(0, VerticesV, 0);
            device.VertexFormat = CustomVertex.PositionColored.Format;
            device.DrawPrimitives(PrimitiveType.TriangleFan, 0, vertices.Count - 2);
        }
    }
}