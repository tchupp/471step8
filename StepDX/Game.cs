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
        ///     Height of our playing area (meters)
        /// </summary>
        private const float PlayingH = 4;

        /// <summary>
        ///     Width of our playing area (meters)
        /// </summary>
        private const float PlayingW = 32;

        /// <summary>
        ///     The DirectX device we will draw on
        /// </summary>
        private Device _device;

        /// <summary>
        ///     The background image class
        /// </summary>
        private readonly Background _background;

        /// <summary>
        ///     All of the polygons that make up our world
        /// </summary>
        private readonly List<Polygon> _world = new List<Polygon>();

        /// <summary>
        ///     What the last time reading was
        /// </summary>
        private long _lastTime;

        /// <summary>
        ///     A stopwatch to use to keep track of time
        /// </summary>
        private readonly Stopwatch _stopwatch = new Stopwatch();

        /// <summary>
        ///     Our player sprite
        /// </summary>
        private readonly GameSprite _player = new GameSprite();

        /// <summary>
        ///     The collision testing subsystem
        /// </summary>
        private readonly Collision _collision = new Collision();

        public Game()
        {
            InitializeComponent();
            if (!InitializeDirect3D()) return;

            _background = new Background(_device, PlayingW, PlayingH);

            // Determine the last time
            _stopwatch.Start();
            _lastTime = _stopwatch.ElapsedMilliseconds;

            var floor = new Polygon();
            floor.AddVertex(new Vector2(0, 1));
            floor.AddVertex(new Vector2(PlayingW, 1));
            floor.AddVertex(new Vector2(PlayingW, 0.9f));
            floor.AddVertex(new Vector2(0, 0.9f));
            floor.Color = Color.CornflowerBlue;
            _world.Add(floor);

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
            _world.Add(platform);

            var texture = TextureLoader.FromFile(_device, "../../../stone08.bmp");
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
            _world.Add(pt);

            var spritetexture = TextureLoader.FromFile(_device, "../../../guy8.bmp");
            _player.Tex = spritetexture;
            _player.AddVertex(new Vector2(-0.2f, 0));
            _player.AddTex(new Vector2(0, 1));
            _player.AddVertex(new Vector2(-0.2f, 1));
            _player.AddTex(new Vector2(0, 0));
            _player.AddVertex(new Vector2(0.2f, 1));
            _player.AddTex(new Vector2(0.125f, 0));
            _player.AddVertex(new Vector2(0.2f, 0));
            _player.AddTex(new Vector2(0.125f, 1));
            _player.Color = Color.Transparent;
            _player.Transparent = true;
            _player.Pos = new Vector2(0.5f, 1);
        }

        /// <summary>
        ///     Advance the game in time
        /// </summary>
        public void Advance()
        {
            // How much time change has there been?
            var time = _stopwatch.ElapsedMilliseconds;
            var delta = (time - _lastTime) * 0.001f; // Delta time in milliseconds
            _lastTime = time;

            while (delta > 0)
            {
                var step = delta;
                if (step > 0.05f) step = 0.05f;

                var maxspeed = Math.Max(Math.Abs(_player.Vel.X), Math.Abs(_player.Vel.Y));
                if (maxspeed > 0) step = (float) Math.Min(step, 0.05 / maxspeed);

                _player.Advance(step);

                foreach (var p in _world)
                {
                    p.Advance(step);
                }

                foreach (var p in _world)
                {
                    if (_collision.Test(_player, p))
                    {
                        var depth = _collision.P1InP2 ? _collision.Depth : -_collision.Depth;
                        _player.Pos = _player.Pos + _collision.Norm * depth;

                        var v = _player.Vel;
                        if (_collision.Norm.X != 0) v.X = 0;
                        if (_collision.Norm.Y != 0) v.Y = 0;

                        _player.Vel = v;
                        _player.Advance(0);
                    }
                }
                delta -= step;
            }
        }

        public void Render()
        {
            if (_device == null) return;

            _device.Clear(ClearFlags.Target, Color.Blue, 1.0f, 0);

            var wid = Width; // Width of our display window
            var hit = Height; // Height of our display window.
            var aspect = wid / (float) hit; // What is the aspect ratio?

            _device.RenderState.ZBufferEnable = false; // We'll not use this feature
            _device.RenderState.Lighting = false; // Or this one...
            _device.RenderState.CullMode = Cull.None; // Or this one...

            var widP = PlayingH * aspect; // Total width of window

            var winCenter = _player.Pos.X;
            if (winCenter - widP / 2 < 0) winCenter = widP / 2;
            else if (winCenter + widP / 2 > PlayingW) winCenter = PlayingW - widP / 2;

            _device.Transform.Projection = Matrix.OrthoOffCenterLH(winCenter - widP / 2,
                winCenter + widP / 2,
                0, PlayingH, 0, 1);

            // Begin the scene
            _device.BeginScene();

            // Render the background
            _background.Render();

            // Render the objects in the world
            foreach (var p in _world)
            {
                p.Render(_device);
            }

            // Render the player
            _player.Render(_device);

            //End the scene
            _device.EndScene();
            _device.Present();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            var v = _player.Vel;
            switch (e.KeyCode)
            {
                case Keys.Escape:
                    Close();
                    break;

                case Keys.Right:
                    v.X = 1.5f;
                    _player.Vel = v;
                    break;

                case Keys.Left:
                    v.X = -1.5f;
                    _player.Vel = v;
                    break;

                case Keys.Space:
                    _player.Jump();
                    break;
            }
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Left:
                case Keys.Right:
                    var v = _player.Vel;
                    v.X = 0;
                    _player.Vel = v;
                    break;
            }
        }

        private void AddObstacle(float left, float right, float top, float bot, Color color)
        {
            var obj = new Polygon();
            obj.AddVertex(new Vector2(left, top));
            obj.AddVertex(new Vector2(right, top));
            obj.AddVertex(new Vector2(right, bot));
            obj.AddVertex(new Vector2(left, bot));
            obj.Color = color;
            _world.Add(obj);
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

                _device = new Device(0, DeviceType.Hardware, this, CreateFlags.SoftwareVertexProcessing, presentParams);
            }
            catch (DirectXException)
            {
                return false;
            }
            return true;
        }
    }
}