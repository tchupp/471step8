using System.Collections.Generic;
using Microsoft.DirectX;

namespace StepDX
{
    public class GameSprite : PolygonTextured
    {
        private Vector2 _acc = new Vector2(0, 0); // Acceleration
        private Vector2 _pos = new Vector2(0, 0); // Position
        private Vector2 _vel = new Vector2(0, 0); // Velocity
        private Vector2 aSave; // Acceleration
        private Vector2 pSave; // Position
        private Vector2 vSave; // Velocity

        private float spriteTime = 0;
        private float spriteRate = 6;   // 6 per second

        protected List<Vector2> verticesM = new List<Vector2>(); // The vertices

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
            get { return verticesM; }
        }

        public void SaveState()
        {
            pSave = _pos;
            vSave = _vel;
            aSave = _acc;
        }

        public void RestoreState()
        {
            _pos = pSave;
            _vel = vSave;
            _acc = aSave;
        }

        public override void Advance(float dt)
        {
            // Euler steps
            _vel.X += _acc.X * dt;
            _vel.Y += _acc.Y * dt;
            _pos.X += _vel.X * dt;
            _pos.Y += _vel.Y * dt;

            int spriteNum;

            if (_vel.X == 0)
            {
                spriteNum = 5;
                spriteTime = 0;
            }
            else
            {
                spriteTime += dt;
                spriteNum = (int)(spriteTime * spriteRate) % 4;     // 4 images
            }

            if (_vel.Y != 0)
            {
                spriteNum = 7;
            }

            // Create the texture vertices
            textureC.Clear();
            if (_vel.X >= 0)
            {
                textureC.Add(new Vector2(spriteNum * 0.125f, 1));
                textureC.Add(new Vector2(spriteNum * 0.125f, 0));
                textureC.Add(new Vector2((spriteNum + 1) * 0.125f, 0));
                textureC.Add(new Vector2((spriteNum + 1) * 0.125f, 1));
            }
            else
            {
                // If moving in the negative direction, we draw our sprite 
                // as a mirror image.
                textureC.Add(new Vector2((spriteNum + 1) * 0.125f, 1));
                textureC.Add(new Vector2((spriteNum + 1) * 0.125f, 0));
                textureC.Add(new Vector2(spriteNum * 0.125f, 0));
                textureC.Add(new Vector2(spriteNum * 0.125f, 1));
            }

            // Move the vertices
            verticesM.Clear();
            foreach (var x in verticesB)
            {
                verticesM.Add(new Vector2(x.X + _pos.X, x.Y + _pos.Y));
            }
        }
    }
}