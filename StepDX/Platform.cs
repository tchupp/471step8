using System.Collections.Generic;
using Microsoft.DirectX;

namespace StepDX
{
    public class Platform : Polygon
    {
        /// <summary>
        /// How high we go
        /// </summary>
        private readonly float maxHeight = 1;

        /// <summary>
        /// For saving the state
        /// </summary>
        private float saveTime;

        /// <summary>
        /// Speed in meters per second
        /// </summary>
        private readonly float speed = 1;

        /// <summary>
        /// Current time for the object
        /// </summary>
        private float time;

        /// <summary>
        /// Vertices after we move them
        /// </summary>
        protected List<Vector2> verticesM = new List<Vector2>();

        /// <summary>
        /// Vertices after they have been moved
        /// </summary>
        public override List<Vector2> Vertices
        {
            get { return verticesM; }
        }

        /// <summary>
        /// Save the current platform position state
        /// </summary>
        public void SaveState() { saveTime = time; }

        /// <summary>
        /// Restore the current platform position state
        /// </summary>
        public void RestoreState() { time = saveTime; }

        /// <summary>
        /// Advance the platform animation in time
        /// </summary>
        /// <param name="dt">The delta time in seconds</param>
        public override void Advance(float dt)
        {
            time += dt;

            // I'm going to base my height entirely on the current time.
            // From 0 to speed, we are rising, speed to 2*speed we are 
            // falling.  So we need to know what step we are in.
            float h;

            var step = (int) (time / speed);
            if (step % 2 == 0)
            {
                // Even, rising
                h = maxHeight * (time - step * speed) / speed;
            }
            else h = 1 - maxHeight * (time - step * speed) / speed;

            // Move it
            verticesM.Clear();
            foreach (var v in verticesB)
            {
                verticesM.Add(v + new Vector2(0, h));
            }
        }
    }
}