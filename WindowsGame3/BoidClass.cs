using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SaturnIV
{
    class BoidClass
    {
        // ------------------------------------------------------------------------
        // used by boid behaviors
        public static bool IsInBoidNeighborhood(newShipStruct thisShip,newShipStruct other, float minDistance, float maxDistance, float cosMaxAngle)
        {
            if (other == thisShip)
            {
                return false;
            }
            else
            {
                Vector3 offset = other.modelPosition - thisShip.modelPosition;
                float distanceSquared = offset.LengthSquared();

                // definitely in neighborhood if inside minDistance sphere
                if (distanceSquared < (minDistance * minDistance))
                {
                    return true;
                }
                else
                {
                    // definitely not in neighborhood if outside maxDistance sphere
                    if (distanceSquared > (maxDistance * maxDistance))
                    {
                        return false;
                    }
                    else
                    {
                        // otherwise, test angular offset from forward axis
                        Vector3 unitOffset = offset / (float)Math.Sqrt(distanceSquared);
                        float forwardness = Vector3.Dot(thisShip.Direction, unitOffset);
                        return forwardness > cosMaxAngle;
                    }
                }
            }
        }
        ///
        // ------------------------------------------------------------------------
        // Separation behavior -- determines the direction away from nearby boids
        public static Vector3 SteerForSeparation(newShipStruct thisShip, float maxDistance, float cosMaxAngle, squadClass flock)
        {
            // steering accumulator and count of neighbors, both initially zero
            Vector3 steering = Vector3.Zero;
            int neighbors = 0;

            // for each of the other vehicles...
            for (int i = 0; i < flock.squadmate.Count; i++)
            {
                newShipStruct other = flock.squadmate[i];

                    Vector3 offset = other.modelPosition - thisShip.modelPosition;
                    float distanceSquared = Vector3.Dot(offset, offset);
                    steering += (offset / -distanceSquared);

                    // count neighbors
                    neighbors++;
          }

                steering = (steering / (float)neighbors);
                steering.Normalize();

            return steering;
        }

        // ------------------------------------------------------------------------
        // Alignment behavior
        public static Vector3 SteerForAlignment(newShipStruct thisShip, float maxDistance, float cosMaxAngle, squadClass flock)
        {
            // steering accumulator and count of neighbors, both initially zero
            Vector3 steering = Vector3.Zero;

            // for each of the other vehicles...
            for (int i = 0; i < flock.squadmate.Count; i++)
            {
                newShipStruct other = flock.squadmate[i];
                if (other == flock.leader)
                    steering = (other.Direction - thisShip.Direction) * 0.10f;
                    steering.Normalize();
            }

            return steering;
        }

        // ------------------------------------------------------------------------
        // Cohesion behavior
        public static Vector3 SteerForCohesion(newShipStruct thisShip, float maxDistance, float cosMaxAngle, squadClass flock)
        {
            // steering accumulator and count of neighbors, both initially zero
            Vector3 steering = Vector3.Zero;
            int neighbors = 0;

            // for each of the other vehicles...
            for (int i = 0; i < flock.squadmate.Count; i++)
            {
                newShipStruct other = flock.squadmate[i];
                if (other == flock.leader)
                    steering += other.modelPosition;

                    // count neighbors
                    neighbors++;
            }

            // divide by neighbors, subtract off current position to get error-
            // correcting direction, then normalize to pure direction

                steering = (steering - thisShip.modelPosition);
                steering.Normalize();

            return steering;
        }
    }
}
