using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

namespace StepDX
{
    public partial class Game : Form
    {
        /// <summary>
        ///     The background image class
        /// </summary>
        private readonly Background background;

        private readonly float playerMaxX = 31.6f; // Maximum x allowed
        private readonly float playerMinX = 0.4f; // Minimum x allowed

        /// <summary>
        ///     Height of our playing area (meters)
        /// </summary>
        private readonly float playingH = 4;

        /// <summary>
        ///     Width of our playing area (meters)
        /// </summary>
        private readonly float playingW = 32;

        /// <summary>
        ///     A stopwatch to use to keep track of time
        /// </summary>
        private readonly Stopwatch stopwatch = new Stopwatch();

        /// <summary>
        ///     Vertex buffer for our drawing
        /// </summary>
        private readonly VertexBuffer vertices;

        /// <summary>
        ///     The DirectX device we will draw on
        /// </summary>
        private Device device;

        /// <summary>
        ///     What the last time reading was
        /// </summary>
        private long lastTime;

        private Vector2 playerLoc = new Vector2(0.4f, 1); // Where our player is
        private Vector2 playerSpeed = new Vector2(0, 0); // How fast we are moving

        public Game()
        {
            InitializeComponent();
            if (!InitializeDirect3D()) return;

            vertices = new VertexBuffer(typeof (CustomVertex.PositionColored), // Type of vertex
                4, // How many
                device, // What device
                0, // No special usage
                CustomVertex.PositionColored.Format,
                Pool.Managed);

            background = new Background(device, playingW, playingH);

            // Determine the last time
            stopwatch.Start();
            lastTime = stopwatch.ElapsedMilliseconds;
        }

        public void Render()
        {
            if (device == null) return;

            device.Clear(ClearFlags.Target, Color.Blue, 1.0f, 0);

            var wid = Width; // Width of our display window
            var hit = Height; // Height of our display window.
            var aspect = wid/(float) hit; // What is the aspect ratio?

            device.RenderState.ZBufferEnable = false; // We'll not use this feature
            device.RenderState.Lighting = false; // Or this one...
            device.RenderState.CullMode = Cull.None; // Or this one...

            var widP = playingH*aspect; // Total width of window
            var winCenter = playerLoc.X;
            if (winCenter - widP/2 < 0)
                winCenter = widP/2;
            else if (winCenter + widP/2 > playingW)
                winCenter = playingW - widP/2;

            device.Transform.Projection = Matrix.OrthoOffCenterLH(winCenter - widP/2,
                winCenter + widP/2,
                0, playingH, 0, 1);

            //Begin the scene
            device.BeginScene();

            // Render the background
            background.Render();

            // We'll add rendering code here...

            // Render the triangle (later a rectangle)
            var gs = vertices.Lock(0, 0, 0); // Lock the vertex list
            var clr = Color.FromArgb(255, 0, 0).ToArgb();

            gs.Write(new CustomVertex.PositionColored(playerLoc.X - 0.1f, playerLoc.Y, 0, clr));
            gs.Write(new CustomVertex.PositionColored(playerLoc.X - 0.1f, playerLoc.Y + 0.5f, 0, clr));
            gs.Write(new CustomVertex.PositionColored(playerLoc.X + 0.1f, playerLoc.Y + 0.5f, 0, clr));
            gs.Write(new CustomVertex.PositionColored(playerLoc.X + 0.1f, playerLoc.Y, 0, clr));

            vertices.Unlock();

            device.SetStreamSource(0, vertices, 0);
            device.VertexFormat = CustomVertex.PositionColored.Format;
            device.DrawPrimitives(PrimitiveType.TriangleFan, 0, 2);

            //End the scene
            device.EndScene();
            device.Present();
        }

        /// <summary>
        ///     Initialize the Direct3D device for rendering
        /// </summary>
        /// <returns>true if successful</returns>
        private bool InitializeDirect3D()
        {
            try
            {
                // Now let's setup our D3D stuff
                var presentParams = new PresentParameters();
                presentParams.Windowed = true;
                presentParams.SwapEffect = SwapEffect.Discard;

                device = new Device(0, DeviceType.Hardware, this, CreateFlags.SoftwareVertexProcessing, presentParams);
            }
            catch (DirectXException)
            {
                return false;
            }

            return true;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Escape:
                    Close();
                    break;
                case Keys.Right:
                    playerSpeed.X = 5;
                    break;
                case Keys.Left:
                    playerSpeed.X = -5;
                    break;
                case Keys.Space:
                    break;
                default:
                    break;
            }
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Right || e.KeyCode == Keys.Left)
                playerSpeed.X = 0;
        }

        /// <summary>
        ///     Advance the game in time
        /// </summary>
        public void Advance()
        {
            // How much time change has there been?
            var time = stopwatch.ElapsedMilliseconds;
            var delta = (time - lastTime)*0.001f; // Delta time in milliseconds
            lastTime = time;

            playerLoc += playerSpeed*delta;

            if (playerLoc.X < playerMinX)
                playerLoc.X = playerMinX;
            else if (playerLoc.X > playerMaxX)
                playerLoc.X = playerMaxX;
        }
    }
}