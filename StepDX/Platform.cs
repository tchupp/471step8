using System.Collections.Generic;
using Microsoft.DirectX;

namespace StepDX
{
    public class Platform : Polygon
    {
        /// <summary>
        ///     How high we go
        /// </summary>
        private const float MaxHeight = 1;

        /// <summary>
        ///     Speed in meters per second
        /// </summary>
        private const float Speed = 1;

        /// <summary>
        ///     Vertices after we move them
        /// </summary>
        protected List<Vector2> VerticesM = new List<Vector2>();

        /// <summary>
        ///     For saving the state
        /// </summary>
        private float _saveTime;

        /// <summary>
        ///     Current time for the object
        /// </summary>
        private float _time;

        /// <summary>
        ///     Vertices after they have been moved
        /// </summary>
        public override List<Vector2> Vertices
        {
            get { return VerticesM; }
        }

        /// <summary>
        ///     Advance the platform animation in time
        /// </summary>
        /// <param name="dt">The delta time in seconds</param>
        public override void Advance(float dt)
        {
            _time += dt;

            // I'm going to base my height entirely on the current time.
            // From 0 to speed, we are rising, speed to 2*speed we are 
            // falling.  So we need to know what step we are in.
            float h;

            var step = (int) (_time / Speed);
            if (step % 2 == 0)
            {
                // Even, rising
                h = MaxHeight * (_time - step * Speed) / Speed;
            }
            else h = 1 - MaxHeight * (_time - step * Speed) / Speed;

            // Move it
            VerticesM.Clear();
            foreach (var v in VerticesB)
            {
                VerticesM.Add(v + new Vector2(0, h));
            }
        }

        /// <summary>
        ///     Restore the current platform position state
        /// </summary>
        public void RestoreState()
        {
            _time = _saveTime;
        }

        /// <summary>
        ///     Save the current platform position state
        /// </summary>
        public void SaveState()
        {
            _saveTime = _time;
        }
    }
}