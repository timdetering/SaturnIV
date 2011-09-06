using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;

namespace SaturnIV
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class NPCManager : ModelManager
    {
        public bool isHostile = false;
        public int shipTypeIndex;
        public float distanceFromPlayer;
        public bool isTargeted = false;
        float thrustAmount = 0.75f;
        private const float ThrustForce = 500.0f;
        bool isEvading, hasEvadeVector,isSquad;
        float currentTargetLevel;
        Vector3 isFacing;
        Vector3 isRight;
        int moduleCount = 0;
        public double[] regentime;
        double speedTime;
        float lastTime;
        float projection;
        Random rand = new Random();
        //disposition predisposition = new disposition();
       
        HelperClass helperClass = new HelperClass();

        /// <summary>
        /// Velocity scalar to approximate drag.
        /// </summary>
        private const float DragFactor = 0.99f;

        public NPCManager(Game game)
            : base(game)
        {
                regentime = new double[500];
        }

        public void performAI(GameTime gameTime, ref WeaponsManager weaponsManager, ref ParticleSystem projectileTrailParticles,
                               ref List<weaponData> weaponDefList,newShipStruct thisShip, newShipStruct otherShip, int targetIndex,squadClass boidList)
        {
            isSquad = false;
            double currentTime = gameTime.TotalGameTime.TotalMilliseconds;
            moduleCount = 0;            
            float distance = Vector3.Distance(thisShip.modelPosition, otherShip.modelPosition);
            thisShip.thrustAmount = 1.0f;
            thisShip.angleOfAttack = (float)GetSignedAngleBetweenTwoVectors(thisShip.Direction, otherShip.Direction, otherShip.Right);            

              // Squad AI Stuff
            thisShip.thrustAmount = 0.95f;
           
                if (thisShip.currentTarget != null && thisShip.currentTarget.hullLvl < 0)
                {
                    thisShip.currentTarget = null;
                    thisShip.currentTargetLevel = 0;
                }

                //float isBehind = Vector3.Dot(thisShip.modelPosition, otherShip.modelPosition);                  
                if (distance < thisShip.EvadeDist[(int)otherShip.objectClass]
                    && thisShip.angleOfAttack > 0.75 && thisShip.angleOfAttack < 2.99 && !thisShip.isEvading)
                {
                    thisShip.targetPosition = thisShip.modelPosition + -thisShip.Direction + 
                            HelperClass.RandomDirection() * thisShip.modelLen * 50;
                    thisShip.thrustAmount = 1.0f;
                    thisShip.timer = currentTime;
                    thisShip.isEvading = true;
                }
            
                if (thisShip.currentTarget != null && !thisShip.isEvading && thisShip.ChasePrefs[(int)thisShip.currentTarget.objectClass] > 0)
                    thisShip.targetPosition = thisShip.currentTarget.modelPosition + thisShip.currentTarget.Direction
                        * rand.Next(-50, 50);

                        if (thisShip.objectAgility < 2.0)
                        {
                            if (thisShip.isEvading && currentTime - thisShip.timer > rand.Next(8000, 9000))
                                thisShip.isEvading = false;
                        }
                        else
                            if (thisShip.isEvading && currentTime - thisShip.timer > rand.Next(3000, 4000))
                                thisShip.isEvading = false;

                /// Engaging
                if (thisShip.team != otherShip.team && !thisShip.isEvading &&  thisShip.currentDisposition != disposition.moving)
                {
                    if (thisShip.TargetPrefs[(int)otherShip.objectClass] >= thisShip.currentTargetLevel 
                        && distance < thisShip.engageDist[(int)otherShip.objectClass]*4)
                    {
                        thisShip.currentTargetLevel = thisShip.TargetPrefs[(int)otherShip.objectClass];
                        thisShip.currentTarget = otherShip;
                        thisShip.currentDisposition = disposition.engaging;
                    } //else
                    if (thisShip.objectClass == ClassesEnum.Capitalship && otherShip.objectClass != ClassesEnum.Capitalship
                          && distance < 5000)
                    {
                        //thisShip.currentTargetLevel = thisShip.TargetPrefs[(int)otherShip.objectClass];
                        thisShip.currentTarget = otherShip;
                        thisShip.currentDisposition = disposition.engaging;
                    }
                }
                /// Too do Optimize
                if (thisShip.currentTarget != null && thisShip.currentTarget == otherShip)
                    cycleWeapons(thisShip, thisShip.currentTarget, currentTime, weaponsManager, projectileTrailParticles,
                        weaponDefList);

            if (thisShip.currentDisposition == disposition.moving)
                thisShip.targetPosition = thisShip.wayPointPosition;

            if (thisShip.currentDisposition == disposition.defensive)
                thisShip.thrustAmount = 0.10f;            

            if (thisShip.modelBoundingSphere.Intersects(new BoundingSphere(thisShip.wayPointPosition,200)))
                thisShip.currentDisposition = disposition.engaging;
        }

        public void updateShipMovement(GameTime gameTime, float gameSpeed, newShipStruct thisShip,
                                      Camera ourCamera, bool isEdit)
        {
            thisShip.vecToTarget = Vector3.Normalize(thisShip.targetPosition - thisShip.modelPosition);
            float turningSpeed = (float)gameTime.ElapsedGameTime.TotalMilliseconds / 1000.0f;
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
            turningSpeed *= thisShip.objectAgility * gameSpeed;

            if (isEdit)
                thisShip.thrustAmount = 0.0f;
            if (isEdit)
                thisShip.Direction = thisShip.vecToTarget;
            else
                thisShip.Direction = Vector3.Normalize(Vector3.Lerp(thisShip.Direction, thisShip.vecToTarget, 
                                      turningSpeed * 0.15f));

            thisShip.modelRotation.Forward = thisShip.Direction;

            thisShip.modelRotation.Right = Vector3.Cross(thisShip.Direction,Vector3.Up);
            thisShip.modelRotation.Up = Vector3.Up;

            Vector3 force = thisShip.Direction * thisShip.thrustAmount * thisShip.objectThrust;
            // Apply acceleration
            Vector3 acceleration = force / thisShip.objectMass;
            thisShip.Velocity += acceleration * thisShip.thrustAmount * elapsed;
            // Apply psuedo drag
            thisShip.Velocity *= DragFactor;
            //Calc speed per second
            if ((float)gameTime.TotalGameTime.TotalMilliseconds - lastTime > 600)
                thisShip.speed = Vector3.Distance(thisShip.modelPosition, thisShip.modelPosition + thisShip.Velocity * elapsed) * 100;
            lastTime = elapsed;

            // Apply velocity
            thisShip.modelPosition += thisShip.Velocity * elapsed;
            //thisShip.worldMatrix = thisShip.modelRotation * Matrix.CreateTranslation(thisShip.modelPosition);
            thisShip.worldMatrix = Matrix.CreateRotationY(MathHelper.ToRadians(90)) * Matrix.CreateWorld(thisShip.modelPosition, thisShip.Direction, Vector3.Up);

            thisShip.modelBoundingSphere.Center = thisShip.modelPosition;
            thisShip.viewMatrix = Matrix.CreateLookAt(thisShip.modelPosition, thisShip.modelPosition +
                                                      thisShip.Direction * 2.0f, thisShip.Up);
            thisShip.modelFrustum.Matrix = thisShip.viewMatrix * thisShip.projectionMatrix;
            Matrix cMatrix = Matrix.CreateLookAt(thisShip.modelPosition, thisShip.modelPosition + thisShip.modelRotation.Right * thisShip.modelWidth, Vector3.Up);
            thisShip.portFrustum = new BoundingFrustum(cMatrix * Matrix.CreatePerspectiveFieldOfView(2.00f,
                                                    16.0f / 9.0f, .5f, thisShip.modelWidth / 2));

            cMatrix = Matrix.CreateLookAt(thisShip.modelPosition, thisShip.modelPosition + -thisShip.modelRotation.Right * thisShip.modelWidth, Vector3.Up);
            thisShip.starboardFrustum = new BoundingFrustum(cMatrix * Matrix.CreatePerspectiveFieldOfView(2.00f,
                                                    16.0f / 9.0f, .5f, thisShip.modelWidth / 2));

            //Update all Weapon Module Firing Frustums
            moduleCount = 0;
            foreach (WeaponModule thisWeapon in thisShip.weaponArray)
            {
                for (int i = 0; i < thisWeapon.ModulePositionOnShip.Count(); i++)
                {
                    switch ((int)thisWeapon.ModulePositionOnShip[i].W)
                    {
                        case 0:
                            isFacing = thisShip.Direction;
                            isRight = thisShip.modelRotation.Forward;
                            break;
                        case 1:
                            isFacing = -thisShip.Direction;
                            isRight = thisShip.modelRotation.Backward;
                            break;
                        case 2:
                            isFacing = -thisShip.modelRotation.Right;
                            isRight = thisShip.modelRotation.Forward;
                            break;
                        case 3:
                            isFacing = thisShip.modelRotation.Right;
                            isRight = -thisShip.modelRotation.Forward;
                            break;
                    }
                    Vector3 tVec = thisShip.modelPosition;
                    tVec = thisShip.modelPosition + (thisShip.modelRotation.Forward)
                        - (thisShip.modelRotation.Up) + (thisShip.modelRotation.Right * 1);
             
                    thisShip.moduleFrustum[moduleCount].Matrix = Matrix.CreateLookAt(thisShip.modelPosition, thisShip.modelPosition + isFacing, Vector3.Up) *
                                                        Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(thisWeapon.FiringEnvelopeAngle),
                                                        16.0f / 9.0f, .5f, thisWeapon.weaponRange);
                    moduleCount++;
                }
            }

            if (thisShip.currentTarget != null)
                thisShip.distanceFromTarget = Vector3.Distance(thisShip.modelPosition, thisShip.currentTarget.modelPosition);
            thisShip.screenCords = get2dCoords(thisShip.modelPosition, ourCamera);
        }

        public Vector3 findFacing(int W,newShipStruct thisShip)
        {
            switch (W)
            {
                case 0:
                    isFacing = thisShip.Direction;
                    isRight = thisShip.modelRotation.Forward;
                    break;
                case 1:
                    isFacing = -thisShip.Direction;
                    isRight = thisShip.modelRotation.Backward;
                    break;
                case 2:
                    isFacing = -thisShip.modelRotation.Right;
                    isRight = thisShip.modelRotation.Forward;
                    break;
                case 3:
                    isFacing = thisShip.modelRotation.Right;
                    isRight = -thisShip.modelRotation.Forward;
                    break;
            }
            return isFacing;
        }

        public Vector3 findRight(int W, newShipStruct thisShip)
        {
            switch (W)
            {
                case 0:
                    isFacing = thisShip.Direction;
                    isRight = thisShip.modelRotation.Forward;
                    break;
                case 1:
                    isFacing = -thisShip.Direction;
                    isRight = thisShip.modelRotation.Backward;
                    break;
                case 2:
                    isFacing = -thisShip.modelRotation.Right;
                    isRight = thisShip.modelRotation.Forward;
                    break;
                case 3:
                    isFacing = thisShip.modelRotation.Right;
                    isRight = -thisShip.modelRotation.Forward;
                    break;
            }
            return isRight;
        }

        /// Find the angle between two vectors. This will not only give the angle difference, but the direction.
        /// For example, it may give you -1 radian, or 1 radian, depending on the direction. Angle given will be the 
        /// angle from the FromVector to the DestVector, in radians.
        /// </summary>
        /// <param name="FromVector">Vector to start at.</param>
        /// <param name="DestVector">Destination vector.</param>
        /// <param name="DestVectorsRight">Right vector of the destination vector</param>
        /// <returns>Signed angle, in radians</returns>        
        /// <remarks>All three vectors must lie along the same plane.</remarks>

        public static double GetSignedAngleBetweenTwoVectors(Vector3 Source, Vector3 Dest, Vector3 DestsRight)
        {
            // We make sure all of our vectors are unit length
            Source.Normalize();
            Dest.Normalize();
            DestsRight.Normalize();

            float forwardDot = Vector3.Dot(Source, Dest);
            float rightDot = Vector3.Dot(Source, DestsRight);

            // Make sure we stay in range no matter what, so Acos doesn't fail later
            forwardDot = MathHelper.Clamp(forwardDot, -1.0f, 1.0f);

            double angleBetween = Math.Acos((float)forwardDot);

            if (rightDot < 0.0f)
                angleBetween *= -1.0f;

            return angleBetween;
        }


        public void selectTarget(newShipStruct thisShip, newShipStruct otherShip, float currentTargetLevel, 
                                  ref List<weaponData> weaponDefList)
        {
            if (Vector3.Distance(thisShip.modelPosition, otherShip.modelPosition) < 1000)
            {
                //Decide weather or Not to Pursue based on this ships TargetPrefs values;  Ex. A capitalship is not going to chase a fighter!
                if (thisShip.currentTarget != null)
                    if (thisShip.TargetPrefs[(int)otherShip.objectClass] > currentTargetLevel)
                        thisShip.currentTarget = otherShip;
            }
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        /// 
        public override void Update(GameTime gameTime)
        {
            // TODO: Add your update code here

            base.Update(gameTime);
        }

        public void cycleWeapons(newShipStruct thisShip, newShipStruct otherShip, double currentTime, WeaponsManager weaponsManager,
           ParticleSystem projectileTrailParticles, List<weaponData> weaponDefList)
        {
            bool noFire = false;
            foreach (WeaponModule thisWeapon in thisShip.weaponArray)
            {
                if (thisWeapon.weaponType == WeaponTypeEnum.MassDriver && thisShip.currentTarget.objectClass == ClassesEnum.Fighter)
                    noFire = true;
                else
                    noFire = false;
                if (!noFire)
                for (int i = 0; i < thisWeapon.ModulePositionOnShip.Count(); i++)
                {
                    if (thisShip.moduleFrustum[moduleCount].Intersects(otherShip.modelBoundingSphere) && thisShip.team != otherShip.team)
                    {
                        if (currentTime - thisShip.regenTimer[moduleCount] > weaponDefList[(int)thisWeapon.weaponType].regenTime)
                        {
                            weaponsManager.fireWeapon(thisShip.currentTarget, thisShip, ref projectileTrailParticles, ref weaponDefList, thisWeapon, i);
                            thisShip.regenTimer[moduleCount] = currentTime;
                            thisShip.isEngaging = true;
                            break;
                        }
                    }
                    moduleCount++;
                }
            }
        }
    }
}