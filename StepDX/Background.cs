using System.Drawing;
using Microsoft.DirectX.Direct3D;

namespace StepDX
{
    public class Background
    {
        /// <summary>
        ///     Background texture map
        /// </summary>
        private readonly Texture _backgroundT;

        /// <summary>
        ///     Background vertex buffer
        /// </summary>
        private readonly VertexBuffer _backgroundV;

        /// <summary>
        ///     The DirectX device we will draw on
        /// </summary>
        private readonly Device _device;

        public Background(Device device, float width, float height)
        {
            _device = device;

            // Load the background texture image
            _backgroundT = TextureLoader.FromFile(device, "../../../mars.bmp");

            // Create a vertex buffer for the background image we will draw
            _backgroundV = new VertexBuffer(typeof (CustomVertex.PositionColoredTextured), // Type
                4, // How many
                device, // What device
                0, // No special usage
                CustomVertex.PositionColoredTextured.Format,
                Pool.Managed);

            // Fill the vertex buffer with the corners of a rectangle that covers
            // the entire playing surface.
            var stm = _backgroundV.Lock(0, 0, 0); // Lock the background vertex list
            var clr = Color.Transparent.ToArgb();
            stm.Write(new CustomVertex.PositionColoredTextured(0, 0, 0, clr, 0, 1));
            stm.Write(new CustomVertex.PositionColoredTextured(0, height, 0, clr, 0, 0));
            stm.Write(new CustomVertex.PositionColoredTextured(width, height, 0, clr, 1, 0));
            stm.Write(new CustomVertex.PositionColoredTextured(width, 0, 0, clr, 1, 1));

            _backgroundV.Unlock();
        }

        /// <summary>
        ///     Render the background image
        /// </summary>
        public void Render()
        {
            _device.SetTexture(0, _backgroundT);
            _device.SetStreamSource(0, _backgroundV, 0);
            _device.VertexFormat = CustomVertex.PositionColoredTextured.Format;
            _device.DrawPrimitives(PrimitiveType.TriangleFan, 0, 2);
            _device.SetTexture(0, null);
        }
    }
}