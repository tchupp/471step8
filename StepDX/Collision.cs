/*! \brief Simple collision checking system.  Only does pairwise collision testing.
 * Assumptions: 
 * Namespace is StepDX.  Please change to suit your project.
 * Existence of a Polygon class with a member function Vertices that returns a
 * List containing Vector2 objects, each the coordinates of a vertex in 
 * clockwise order.
 * \author Charles B. Owen
 * \version 1.00 01-18-2006 Initial standalone version.
 * \version 1.01 11-25-2012 Documentation revisions
 */

using System;
using Microsoft.DirectX;

namespace StepDX
{
    internal class Collision
    {
        /// <summary>
        ///     First polygon collision test result (one with the vertex)
        /// </summary>
        private Polygon _poly1;

        /// <summary>
        ///     Second polygon collision test result (one with the edge)
        /// </summary>
        private Polygon _poly2;

        /// <summary>
        ///     The normal at the intersection
        /// </summary>
        private Vector2 _norm = new Vector2();

        /// <summary>
        ///     Intersection depth
        /// </summary>
        private float _depth;

        /// <summary>
        ///     True if polygon 1 has the vertex inside polygon 2 edge
        /// </summary>
        private bool _p1Inp2;

        /// <summary>
        ///     Intersection depth
        /// </summary>
        public float Depth
        {
            get { return _depth; }
        }

        /// <summary>
        ///     The normal at the intersection
        /// </summary>
        public Vector2 Norm
        {
            get { return _norm; }
        }

        /// <summary>
        ///     True if polygon 1 has the vertex inside polygon 2 edge
        /// </summary>
        public bool P1InP2
        {
            get { return _p1Inp2; }
        }

        /// <summary>
        ///     Performs a collision test for two polygons.
        /// </summary>
        /// <param name="p1">Polygon 1</param>
        /// <param name="p2">Polygon 2</param>
        /// <returns>true if a collision</returns>
        public bool Test(Polygon p1, Polygon p2)
        {
            _poly1 = null;
            _poly2 = null;
            _depth = 0;

            // Symmetrical search for an edge that segments the two...
            if (!TestLr(p1, p2))
                return false;

            if (!TestLr(p2, p1))
                return false;

            // We know we have a collision.  Determine the vertex and edge that 
            // are involved.
            _p1Inp2 = true;
            if (!TestVe(p1, p2))
            {
                if (!TestVe(p2, p1))
                    return false;

                _p1Inp2 = false;
            }

            if (_poly1 == null || _depth == 0)
            {
                // No actual collision...
                return false;
            }

            return true;
        }

        // Test of all vertices in c1 against all edges in c2.
        // Returns false if there is an edge that separates the two
        // True simply means potential for an overlap
        private bool TestLr(Polygon p1, Polygon p2)
        {
            var v1 = p1.Vertices;
            var v2 = p2.Vertices;

            // Last vertex in v2
            var v2A = v2[v2.Count - 1];

            foreach (var v2B in v2)
            {
                // Compute the edge line function
                var a = v2A.Y - v2B.Y;
                var b = v2B.X - v2A.X;
                var c = -a * v2A.X - b * v2A.Y;

                var possible = true;
                foreach (var v1A in v1)
                {
                    var r = a * v1A.X + b * v1A.Y + c;

                    // If r <= zero, we're on the wrong side of the line.
                    // This can't be a separator line.
                    if (r <= 0)
                    {
                        possible = false;
                        break;
                    }
                }

                // If the possibility that this is a separator line never got cleared,
                // it just me one.
                if (possible)
                {
                    // We have a separator line.  This is our witness, save it
                    return false;
                }

                // Make this the end point for the next pass
                v2A = v2B;
            }

            // If no separating edges have been found, we potentially have an overlap
            return true;
        }

        /// <summary>
        ///     Test of all vertices in p1 against all edges in p2.
        ///     This is looking for a vertex in c1 that is inside c2, a
        ///     symmetrical problem to the above tests.
        /// </summary>
        /// <param name="p1">Polygon 1</param>
        /// <param name="p2">Polygon 2</param>
        /// <returns>true if the collision point was found.</returns>
        private bool TestVe(Polygon p1, Polygon p2)
        {
            var v1 = p1.Vertices;
            var v2 = p2.Vertices;

            // 
            // Loop over all of the vertices.
            // We are looking for the one vertex that 
            // is most deeply embedded in the object.  
            //

            var anyVert = false; // Until we know
            var bestVertR = 0.0f; // Looking for deepest, so start low
            var bestVertN = new Vector2(); // Best intersection normal

            foreach (var v1A in v1)
            {
                var possible = true; // Candidate vertex

                //
                // Loop over all of the edges. Is this vertex contained by all?
                //

                // Last vertex in v2
                var v2A = v2[v2.Count - 1];

                var bestR = 1e10f; // Least penetration for this vertex
                var bestN = new Vector2(); // Best intersection normal

                foreach (var v2B in v2)
                {
                    // Compute the edge line function
                    var a = v2A.Y - v2B.Y;
                    var b = v2B.X - v2A.X;

                    // Normalize these values
                    var len = (float) Math.Sqrt(a * a + b * b);
                    a /= len;
                    b /= len;

                    // Compute c and r...
                    var c = -a * v2A.X - b * v2A.Y;
                    var r = a * v1A.X + b * v1A.Y + c;

                    // If r <= zero, we're on the inside side of the line.
                    if (r > 0)
                    {
                        possible = false;
                        break;
                    }
                    if (-r < bestR)
                    {
                        bestR = -r;
                        bestN.X = a;
                        bestN.Y = b;
                    }

                    // Make this the end point for the next pass
                    v2A = v2B;
                }

                // The possibility this vertex is inside never got cleared.  
                // It must be actually inside.
                if (possible && bestR > bestVertR)
                {
                    bestVertN = bestN;
                    bestVertR = bestR;
                    anyVert = true;
                }
            }

            if (anyVert)
            {
                _poly1 = p1;
                _poly2 = p2;
                _norm = bestVertN;
                _depth = bestVertR;
                return true;
            }

            // If no penetrating vertex is found.
            return false;
        }
    }
}