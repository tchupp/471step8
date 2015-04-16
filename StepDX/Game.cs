using System;
using System.Collections.Generic;
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
        /// The DirectX device we will draw on
        /// </summary>
        private Device device;

        /// <summary>
        /// Height of our playing area (meters)
        /// </summary>
        private readonly float playingH = 4;

        /// <summary>
        /// Width of our playing area (meters)
        /// </summary>
        private readonly float playingW = 32;

        /// <summary>
        /// Vertex buffer for our drawing
        /// </summary>
        private VertexBuffer vertices;

        /// <summary>
        /// The background image class
        /// </summary>
        private readonly Background background;

        /// <summary>
        /// All of the polygons that make up our world
        /// </summary>
        private readonly List<Polygon> world = new List<Polygon>();

        /// <summary>
        /// What the last time reading was
        /// </summary>
        private long lastTime;

        /// <summary>
        /// A stopwatch to use to keep track of time
        /// </summary>
        private readonly Stopwatch stopwatch = new Stopwatch();

        /// <summary>
        /// Our player sprite
        /// </summary>
        private readonly GameSprite player = new GameSprite();

        /// <summary>
        /// The collision testing subsystem
        /// </summary>
        private readonly Collision collision = new Collision();

        public Game()
        {
            InitializeComponent();
            if (!InitializeDirect3D()) return;

            background = new Background(device, playingW, playingH);

            // Determine the last time
            stopwatch.Start();
            lastTime = stopwatch.ElapsedMilliseconds;

            var floor = new Polygon();
            floor.AddVertex(new Vector2(0, 1));
            floor.AddVertex(new Vector2(playingW, 1));
            floor.AddVertex(new Vector2(playingW, 0.9f));
            floor.AddVertex(new Vector2(0, 0.9f));
            floor.Color = Color.CornflowerBlue;
            world.Add(floor);

            AddObstacle(2, 3, 1.7f, 1.9f, Color.Crimson);
            AddObstacle(4, 4.2f, 1, 2.1f, Color.Coral);
            AddObstacle(5, 6, 2.2f, 2.4f, Color.BurlyWood);
            AddObstacle(5.5f, 6.5f, 3.2f, 3.4f, Color.PeachPuff);
            AddObstacle(6.5f, 7.5f, 2.5f, 2.7f, Color.Chocolate);

            var platform = new Platform();
            platform.AddVertex(new Vector2(3.2f, 2));
            platform.AddVertex(new Vector2(3.9f, 2));
            platform.AddVertex(new Vector2(3.9f, 1.8f));
            platform.AddVertex(new Vector2(3.2f, 1.8f));
            platform.Color = Color.CornflowerBlue;
            world.Add(platform);

            var texture = TextureLoader.FromFile(device, "../../../stone08.bmp");
            var pt = new PolygonTextured();
            pt.Tex = texture;
            pt.AddVertex(new Vector2(1.2f, 3.5f));
            pt.AddTex(new Vector2(0, 1));
            pt.AddVertex(new Vector2(1.9f, 3.5f));
            pt.AddTex(new Vector2(0, 0));
            pt.AddVertex(new Vector2(1.9f, 3.3f));
            pt.AddTex(new Vector2(1, 0));
            pt.AddVertex(new Vector2(1.2f, 3.3f));
            pt.AddTex(new Vector2(1, 1));
            pt.Color = Color.Transparent;
            world.Add(pt);

            var spritetexture = TextureLoader.FromFile(device, "../../../guy8.bmp");
            player.Tex = spritetexture;
            player.AddVertex(new Vector2(-0.2f, 0));
            player.AddTex(new Vector2(0, 1));
            player.AddVertex(new Vector2(-0.2f, 1));
            player.AddTex(new Vector2(0, 0));
            player.AddVertex(new Vector2(0.2f, 1));
            player.AddTex(new Vector2(0.125f, 0));
            player.AddVertex(new Vector2(0.2f, 0));
            player.AddTex(new Vector2(0.125f, 1));
            player.Color = Color.Transparent;
            player.Transparent = true;
            player.Pos = new Vector2(0.5f, 1);
        }

        public void Render()
        {
            if (device == null) return;

            device.Clear(ClearFlags.Target, Color.Blue, 1.0f, 0);

            var wid = Width; // Width of our display window
            var hit = Height; // Height of our display window.
            var aspect = wid / (float) hit; // What is the aspect ratio?

            device.RenderState.ZBufferEnable = false; // We'll not use this feature
            device.RenderState.Lighting = false; // Or this one...
            device.RenderState.CullMode = Cull.None; // Or this one...

            var widP = playingH * aspect; // Total width of window

            var winCenter = player.Pos.X;
            if (winCenter - widP / 2 < 0) winCenter = widP / 2;
            else if (winCenter + widP / 2 > playingW) winCenter = playingW - widP / 2;

            device.Transform.Projection = Matrix.OrthoOffCenterLH(winCenter - widP / 2,
                winCenter + widP / 2,
                0, playingH, 0, 1);

            // Begin the scene
            device.BeginScene();

            // Render the background
            background.Render();
            // Render the player
            player.Render(device);

            // Render the objects in the world
            foreach (var p in world)
            {
                p.Render(device);
            }

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

        private void AddObstacle(float left, float right, float top, float bot, Color color)
        {
            var obj = new Polygon();
            obj.AddVertex(new Vector2(left, top));
            obj.AddVertex(new Vector2(right, top));
            obj.AddVertex(new Vector2(right, bot));
            obj.AddVertex(new Vector2(left, bot));
            obj.Color = color;
            world.Add(obj);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            var v = player.Vel;
            switch (e.KeyCode)
            {
                case Keys.Escape:
                    Close();
                    break;

                case Keys.Right:
                    v.X = 1.5f;
                    player.Vel = v;
                    break;

                case Keys.Left:
                    v.X = -1.5f;
                    player.Vel = v;
                    break;

                case Keys.Space:
                    v.Y = 7;
                    player.Vel = v;
                    player.Acc = new Vector2(0, -9.8f);
                    break;

                default:
                    break;
            }
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Left:
                case Keys.Right:
                    var v = player.Vel;
                    v.X = 0;
                    player.Vel = v;
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        ///     Advance the game in time
        /// </summary>
        public void Advance()
        {
            // How much time change has there been?
            var time = stopwatch.ElapsedMilliseconds;
            var delta = (time - lastTime) * 0.001f; // Delta time in milliseconds
            lastTime = time;

            while (delta > 0)
            {
                var step = delta;
                if (step > 0.05f) step = 0.05f;

                var maxspeed = Math.Max(Math.Abs(player.Vel.X), Math.Abs(player.Vel.Y));
                if (maxspeed > 0) step = (float) Math.Min(step, 0.05 / maxspeed);

                player.Advance(step);

                foreach (var p in world)
                {
                    p.Advance(step);
                }

                foreach (var p in world)
                {
                    if (collision.Test(player, p))
                    {
                        var depth = collision.P1inP2 ? collision.Depth : -collision.Depth;
                        player.Pos = player.Pos + collision.N * depth;

                        var v = player.Vel;
                        if (collision.N.X != 0) v.X = 0;
                        if (collision.N.Y != 0) v.Y = 0;

                        player.Vel = v;
                        player.Advance(0);
                    }
                }
                delta -= step;
            }
        }
    }
}