using System.Collections.Generic;
using Microsoft.DirectX;

namespace StepDX
{
    public class GameSprite : PolygonTextured
    {
        private const float SpriteRate = 6; // 6 per second
        protected List<Vector2> VerticesM = new List<Vector2>(); // The vertices
        private Vector2 _acc = new Vector2(0, 0); // Acceleration
        private Vector2 _pos = new Vector2(0, 0); // Position
        private Vector2 _vel = new Vector2(0, 0); // Velocity
        private Vector2 _aSave; // Acceleration
        private Vector2 _pSave; // Position
        private Vector2 _vSave; // Velocity
        private bool _jumping;
        private float _spriteTime;

        public Vector2 Pos
        {
            set { _pos = value; }
            get { return _pos; }
        }

        public Vector2 Vel
        {
            set { _vel = value; }
            get { return _vel; }
        }

        public Vector2 Acc
        {
            set { _acc = value; }
            get { return _acc; }
        }

        public override List<Vector2> Vertices
        {
            get { return VerticesM; }
        }

        public override void Advance(float dt)
        {
            // Euler steps
            _vel.X += _acc.X * dt;
            _vel.Y += _acc.Y * dt;
            _pos.X += _vel.X * dt;
            _pos.Y += _vel.Y * dt;

            if (_vel.Y == 0) // reset jumping if standing
                _jumping = false;

            int spriteNum;

            if (_vel.X == 0) // sprite not moving
            {
                spriteNum = 5;
                _spriteTime = 0;
            }
            else // sprite walking
            {
                _spriteTime += dt;
                spriteNum = (int) (_spriteTime * SpriteRate) % 4; // 4 images
            }

            if (_jumping) // sprite jumping/falling
                spriteNum = 7;

            // Create the texture vertices
            TextureC.Clear();
            if (_vel.X >= 0)
            {
                TextureC.Add(new Vector2(spriteNum * 0.125f, 1));
                TextureC.Add(new Vector2(spriteNum * 0.125f, 0));
                TextureC.Add(new Vector2((spriteNum + 1) * 0.125f, 0));
                TextureC.Add(new Vector2((spriteNum + 1) * 0.125f, 1));
            }
            else
            {
                // If moving in the negative direction, we draw our sprite 
                // as a mirror image.
                TextureC.Add(new Vector2((spriteNum + 1) * 0.125f, 1));
                TextureC.Add(new Vector2((spriteNum + 1) * 0.125f, 0));
                TextureC.Add(new Vector2(spriteNum * 0.125f, 0));
                TextureC.Add(new Vector2(spriteNum * 0.125f, 1));
            }

            // Move the vertices
            VerticesM.Clear();
            foreach (var x in VerticesB)
            {
                VerticesM.Add(new Vector2(x.X + _pos.X, x.Y + _pos.Y));
            }
        }

        public void Jump()
        {
            if (!_jumping)
            {
                _vel.Y = 7;
                _acc.Y = -9.8f;
                _jumping = true;
            }
        }

        public void RestoreState()
        {
            _pos = _pSave;
            _vel = _vSave;
            _acc = _aSave;
        }

        public void SaveState()
        {
            _pSave = _pos;
            _vSave = _vel;
            _aSave = _acc;
        }
    }
}