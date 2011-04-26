using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace SaturnIV
{
    class AIClass
    {
        // xxx solely for annotation
        Vector3 xxxThreatPositionAtNearestApproach = Vector3.Zero;
        Vector3 xxxOurPositionAtNearestApproach = Vector3.Zero;

        // return component of vector parallel to a unit basis vector
        // IMPORTANT NOTE: assumes "basis" has unit magnitude (length == 1)
        public static Vector3 ParallelComponent(Vector3 vector, Vector3 unitBasis)
        {
            float projection = Vector3.Dot(vector, unitBasis);
            return unitBasis * projection;
        }

        // return component of vector perpendicular to a unit basis vector
        // IMPORTANT NOTE: assumes "basis" has unit magnitude(length==1)
        public static Vector3 PerpendicularComponent(Vector3 vector, Vector3 unitBasis)
        {
            return (vector - ParallelComponent(vector, unitBasis));
        }

        // Flee behavior
        public static Vector3 EvadeVector(Vector3 Position,Vector3 target,Vector3 Velocity)
        {
            Vector3 desiredVelocity = Position - target;
            return desiredVelocity - Velocity;
        }

        // ------------------------------------------------------------------------
        // Unaligned collision avoidance behavior: avoid colliding with other
        // nearby vehicles moving in unconstrained directions.  Determine which
        // (if any) other other this we would collide with first, then steers
        // to avoid the site of that potential collision.  Returns a steering
        // force vector, which is zero length if there is no impending collision.
        public static Vector3 AvoidVector (float minTimeToCollision,newShipStruct thisShip,newShipStruct otherShip)   
        {
            // first priority is to prevent immediate interpenetration
            Vector3 separation = SteerToAvoidCloseNeighbors(minTimeToCollision, thisShip, otherShip);
            if (separation != Vector3.Zero) return separation;

            // otherwise, go on to consider potential future collisions
            float steer = 0;
            bool threat = false;

            // Time (in seconds) until the most immediate collision threat found
            // so far.  Initial value is a threshold: don't look more than this
            // many frames into the future.
            float minTime = minTimeToCollision;

            // for each of the other vehicles, determine which (if any)
            // pose the most immediate threat of collision.
                    // avoid when future positions are this close (or less)
                    float collisionDangerThreshold = thisShip.radius * 2;

                    // predicted time until nearest approach of "this" and "other"
                    float time = PredictNearestApproachTime(thisShip,otherShip);

                    // If the time is in the future, sooner than any other
                    // threatened collision...
                    if ((time >= 0) && (time < minTime))
                    {
                        // if the two will be close enough to collide,
                        // make a note of it
                        if (ComputeNearestApproachPositions(thisShip,otherShip, time) < collisionDangerThreshold)
                        {
                            minTime = time;
                            threat = true;
                        }
                    }

            // if a potential collision was found, compute steering to avoid
            if (threat)
            {
                // parallel: +1, perpendicular: 0, anti-parallel: -1
                float parallelness = Vector3.Dot(thisShip.Direction, otherShip.Direction);
                float angle = 0.707f;

                if (parallelness < -angle)
                {
                    // anti-parallel "head on" paths:
                    // steer away from future threat position
                    Vector3 offset = otherShip.modelPosition - thisShip.modelPosition;
                    float sideDot = Vector3.Dot(offset, thisShip.modelRotation.Right);
                    steer = (sideDot > 0) ? -1.0f : 1.0f;
                }
                else
                {
                    if (parallelness > angle)
                    {
                        // parallel paths: steer away from threat
                        Vector3 offset = otherShip.modelPosition - thisShip.modelPosition;
                        float sideDot = Vector3.Dot(offset, thisShip.modelRotation.Right);
                        steer = (sideDot > 0) ? -1.0f : 1.0f;
                    }
                    else
                    {
                        // perpendicular paths: steer behind threat
                        // (only the slower of the two does this)
                        if (otherShip.speed <= thisShip.speed)
                        {
                            float sideDot = Vector3.Dot(thisShip.modelRotation.Right, otherShip.Velocity);
                            steer = (sideDot > 0) ? -1.0f : 1.0f;
                        }
                    }
                }        
            }
            return thisShip.modelRotation.Right * steer;
        }

        // Given the time until nearest approach (predictNearestApproachTime)
        // determine position of each this at that time, and the distance
        // between them
        public static float ComputeNearestApproachPositions(newShipStruct thisShip, newShipStruct other, float time)
        {
            Vector3 myTravel = thisShip.Direction * thisShip.speed * time;
            Vector3 otherTravel = other.Direction * other.speed * time;

            Vector3 myFinal = thisShip.modelPosition + myTravel;
            Vector3 otherFinal = other.modelPosition + otherTravel;

            return Vector3.Distance(myFinal, otherFinal);
        }

        // Given two vehicles, based on their current positions and velocities,
        // determine the time until nearest approach
        public static float PredictNearestApproachTime(newShipStruct thisShip,newShipStruct other)
        {
            // imagine we are at the origin with no velocity,
            // compute the relative velocity of the other this
            Vector3 myVelocity = thisShip.Velocity;
            Vector3 otherVelocity = other.Velocity;
            Vector3 relVelocity = otherVelocity - myVelocity;
            float relSpeed = relVelocity.Length();

            // for parallel paths, the vehicles will always be at the same distance,
            // so return 0 (aka "now") since "there is no time like the present"
            if (relSpeed == 0) return 0;

            // Now consider the path of the other this in this relative
            // space, a line defined by the relative position and velocity.
            // The distance from the origin (our this) to that line is
            // the nearest approach.

            // Take the unit tangent along the other this's path
            Vector3 relTangent = relVelocity / relSpeed;

            // find distance from its path to origin (compute offset from
            // other to us, find length of projection onto path)
            Vector3 relPosition = thisShip.modelPosition - other.modelPosition;
            float projection = Vector3.Dot(relTangent, relPosition);

            return projection / relSpeed;
        }

        // ------------------------------------------------------------------------
        // avoidance of "close neighbors" -- used only by steerToAvoidNeighbors
        //
        // XXX  Does a hard steer away from any other agent who comes withing a
        // XXX  critical distance.  Ideally this should be replaced with a call
        // XXX  to steerForSeparation.
        public static Vector3 SteerToAvoidCloseNeighbors(float minSeparationDistance, newShipStruct thisShip,newShipStruct otherShip)
        {
            // for each of the other vehicles...
            
                    float sumOfRadii = thisShip.radius + otherShip.radius;
                    float minCenterToCenter = minSeparationDistance + sumOfRadii;
                    Vector3 offset = otherShip.modelPosition - thisShip.modelPosition;
                    float currentDistance = offset.Length();

                    if (currentDistance < minCenterToCenter)
                        return PerpendicularComponent(-offset, thisShip.Direction);
            // otherwise return zero
            return Vector3.Zero;
        }

        // ------------------------------------------------------------------------
        // used by boid behaviors
        public static bool IsInBoidNeighborhood(newShipStruct thisShip,newShipStruct otherShip, float minDistance, float maxDistance, float cosMaxAngle)
        {
                Vector3 offset = otherShip.modelPosition - thisShip.modelPosition;
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

        // Alignment behavior
        public static Vector3 SteerForAlignment(newShipStruct thisShip, float maxDistance, float cosMaxAngle, newShipStruct other)
        {
            // steering accumulator and count of neighbors, both initially zero
            Vector3 steering = Vector3.Zero;
            int neighbors = 1;

            // for each of the other vehicles...
           // for (int i = 0; i < flock.Count; i++)
          //  {
               
                if (IsInBoidNeighborhood(thisShip,other, thisShip.radius * 3, maxDistance, cosMaxAngle))
                {
                    // accumulate sum of neighbor's heading
                    steering += other.Direction;

                    // count neighbors
                    neighbors++;
                }
         //   }

            // divide by neighbors, subtract off current heading to get error-
            // correcting direction, then normalize to pure direction
            if (neighbors > 0)
            {
                steering = ((steering / (float)neighbors) - thisShip.Direction);
                steering.Normalize();
            }

            return steering;
        }


        // ------------------------------------------------------------------------
        // Cohesion behavior
        public static Vector3 SteerForCohesion(newShipStruct thisShip, float maxDistance, float cosMaxAngle,newShipStruct other)
        {
            // steering accumulator and count of neighbors, both initially zero
            Vector3 steering = Vector3.Zero;
            int neighbors = 1;

            // for each of the other vehicles...
            //for (int i = 0; i < flock.Count; i++)
            //{
                //newShipStruct other = flock[i];
                if (IsInBoidNeighborhood(thisShip,other, thisShip.radius * 2, thisShip.radius * 3, cosMaxAngle))
                {
                    // accumulate sum of neighbor's positions
                    steering += other.modelPosition;

                    // count neighbors
                    neighbors++;
                }
            //}

            // divide by neighbors, subtract off current position to get error-
            // correcting direction, then normalize to pure direction
            if (neighbors > 0)
            {
                steering = ((steering / (float)neighbors) - thisShip.modelPosition);
                steering.Normalize();
            }

            return steering;
        }
    }
}
