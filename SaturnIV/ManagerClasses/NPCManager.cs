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
        private const float ThrustForce = 500.0f;
        Vector3 isFacing;
        Vector3 isRight;
        int moduleCount = 0;
        public double[] regentime;
        float lastTime;
        double timeToEvade;
        Random rand = new Random();
        double loopTimer = -1;
        int c;
        //disposition predisposition = new disposition();
       
        HelperClass helperClass = new HelperClass();

        /// <summary>
        /// Velocity scalar to approximate drag.
        /// </summary>
        private const float DragFactor = 0.97f;

        public NPCManager(Game game)
            : base(game)
        {
                regentime = new double[500];
            
        }
       
        public void performAI(GameTime gameTime, ref WeaponsManager weaponsManager, ref ParticleSystem projectileTrailParticles,
                               ref List<weaponData> weaponDefList, newShipStruct thisShip, List<newShipStruct> shipList, int targetIndex, squadClass boidList)
        {
            double currentTime = gameTime.TotalGameTime.TotalMilliseconds;
            moduleCount = 0;                  
            thisShip.thrustAmount = 0.95f;
           
            if (loopTimer < 0)
                loopTimer = currentTime;

            if (thisShip.currentTarget == null)
            {
                //thisShip.currentTarget = null;
                thisShip.currentTargetLevel = 0;
                thisShip.currentDisposition = disposition.patrol;
            }

            List<newShipStruct> tmpList = new List<newShipStruct>();
            List<newShipStruct> tmpList2 = new List<newShipStruct>();

                foreach (newShipStruct iShip in shipList)
                    if (iShip != thisShip)
                        tmpList2.Add(iShip);
                /// Narrow down target list.
                /// 
                tmpList = shipList.Where(item => item.team != thisShip.team).ToList();
                //tmpList2 = shipList.Where(item => item.team != thisShip.team).ToList();
                float highestValueTarget = tmpList.Max(newShipStruct => thisShip.TargetPrefs[(int)newShipStruct.objectClass]);
                thisShip.currentTargetLevel = highestValueTarget;
            for (int i = 0; i < tmpList.Count; i++)
            {
                    if (thisShip.TargetPrefs[(int)tmpList[i].objectClass] < highestValueTarget)
                        tmpList.Remove(tmpList[i]);
                    
            }
               // for (int i = 0; i < tmpList.Count; i++)
               // {
                    //if (thisShip.engageDist[(int)tmpList[i].objectClass] > Vector3.Distance(thisShip.modelPosition, tmpList[i].modelPosition))
                    //    tmpList.Remove(tmpList[i]);
                //}

            /// End Target List Creation
            /// 
            /// Target Selection
            /// 
            if ((thisShip.currentTarget == null && thisShip.currentDisposition != disposition.idle) || thisShip.canEngageMultipleTargets)
            {
                c = tmpList.Count();
                if (c > 0)
                {
                    int b = rand.Next(c);
                    thisShip.currentTarget = tmpList[b];
                    thisShip.currentTarget.isAlreadyEngaged = true;
                    thisShip.currentTargetLevel = thisShip.TargetPrefs[(int)thisShip.currentTarget.objectClass];
                    thisShip.currentDisposition = disposition.engaging;
                    //thisShip.timer = currentTime;
                }
            }            
            
            //if (thisShip.currentTarget != null && !thisShip.isEvading && currentTime - thisShip.timer> 5000)
            //{
            //    c = tmpList.Count();
            //    if (c > 0)
            //    {
            //        int b = rand.Next(c);
            //        if (thisShip.currentTargetLevel >= thisShip.TargetPrefs[(int)tmpList[b].objectClass])
            //        {
            //            thisShip.currentTarget = tmpList[b];
            //            thisShip.currentTarget.isAlreadyEngaged = true;
            //            thisShip.currentTargetLevel = thisShip.TargetPrefs[(int)thisShip.currentTarget.objectClass];
            //            thisShip.currentDisposition = disposition.engaging;
            //        }
            //    }
              //  thisShip.timer = currentTime;
            //}
            /// End Target Selection
            /// 
            /// Begin Disposition Logic Switch
            /// 
            switch (thisShip.currentDisposition)
            {
                case disposition.engaging:
                    if ( thisShip.currentTarget != null)
                        cycleWeapons(thisShip, thisShip.currentTarget, currentTime, weaponsManager, projectileTrailParticles,
                                   weaponDefList);
                    //else
                    //    cycleWeapons(thisShip, tmpList, currentTime, weaponsManager, projectileTrailParticles,
                     //              weaponDefList);
                    thisShip.thrustAmount = 1.25f;
                    if (thisShip.currentTarget != null && !thisShip.isEvading && thisShip.ChasePrefs[(int)thisShip.currentTarget.objectClass] > 0)
                        thisShip.targetPosition = thisShip.currentTarget.modelPosition;// + 
                                //(thisShip.currentTarget.Direction * rand.Next(-250, 350));
                    break;
                case disposition.moving:
                    thisShip.targetPosition = thisShip.wayPointPosition;
                    break;
                case disposition.defensive:
                    thisShip.thrustAmount = 0.25f;
                    break;       
            }             
            /// End Disposition Logic Switch
            /// 
            /// Begin Evading Routine
            /// 
            foreach (newShipStruct iShip in tmpList2)
            {
                float distance = Vector3.Distance(thisShip.modelPosition, iShip.modelPosition);
                thisShip.angleOfAttack = (float)GetSignedAngleBetweenTwoVectors(thisShip.Direction, iShip.Direction, iShip.Right);    
                if (thisShip.currentDisposition != disposition.moving && (distance < thisShip.EvadeDist[(int)iShip.objectClass] / 4 && !thisShip.isEvading) &&
                    thisShip.ChasePrefs[(int)iShip.objectClass] > 0)
                    //|| (thisShip.angleOfAttack > 3.11 && !thisShip.isEvading) && distance < thisShip.EvadeDist[(int)iShip.objectClass] / 2)
                {
                    thisShip.targetPosition = thisShip.modelPosition +((thisShip.Direction +
                         (HelperClass.RandomDirection()) * thisShip.modelLen * 5));
                    thisShip.thrustAmount = 1.50f;
                    thisShip.isEvading = true;
                    thisShip.isPursuing = false;
                    // MARK!
                    thisShip.timer = currentTime;
                    if (thisShip.objectAgility > 1.9)
                        timeToEvade = rand.Next(2500, 4000);
                    else
                        timeToEvade = rand.Next(3500, 5000);
                    break;
                }
            }
                if (thisShip.isEvading && currentTime - thisShip.timer > timeToEvade)
                {
                    thisShip.isEvading = false;
                    thisShip.timer = 0;
                }
            /// End Evade Routine
            ///      
            //if (thisShip.modelBoundingSphere.Intersects(new BoundingSphere(thisShip.wayPointPosition,15000)))
            //    thisShip.currentDisposition = disposition.patrol;
        }

        public void updateShipMovement(GameTime gameTime, float gameSpeed, newShipStruct thisShip,
                                      CameraNew ourCamera, bool isEdit)
        {
            thisShip.vecToTarget = Vector3.Normalize(thisShip.targetPosition - thisShip.modelPosition);
            float turningSpeed = (float)gameTime.ElapsedGameTime.TotalMilliseconds / 1000.0f;
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
            turningSpeed *= thisShip.objectAgility * gameSpeed;

            if (isEdit)
                thisShip.thrustAmount = 0.0f;
            if (isEdit)
                thisShip.Direction = thisShip.vecToTarget + HelperClass.RandomDirection();
            else
                thisShip.Direction = Vector3.Normalize(Vector3.Lerp(thisShip.Direction, thisShip.vecToTarget, 
                                      turningSpeed * 0.045f));

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
            foreach (WeaponModule thisWeapon in thisShip.weaponArray)
            {
                moduleCount = 0;
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

//        public void cycleWeapons(newShipStruct thisShip, List<newShipStruct> otherShipList, double currentTime, WeaponsManager weaponsManager,
//           ParticleSystem projectileTrailParticles, List<weaponData> weaponDefList)
//        {
//            foreach (newShipStruct otherShip in otherShipList)
//            {
//                foreach (WeaponModule thisWeapon in thisShip.weaponArray)
//                {
//                    moduleCount = 0;
//                    for (int i = 0; i < thisWeapon.ModulePositionOnShip.Count(); i++)
//                    {
//                        if (thisShip.moduleFrustum[moduleCount].Intersects(otherShip.modelBoundingSphere) && thisShip.team != otherShip.team)
//                        {
//                            if (currentTime - thisShip.regenTimer[moduleCount] > weaponDefList[(int)thisWeapon.weaponType].regenTime)
//                            {
//                                weaponsManager.fireWeapon(otherShip, thisShip, ref projectileTrailParticles, ref weaponDefList, thisWeapon, i);
//                                thisShip.regenTimer[moduleCount] = currentTime;
//                                thisShip.isEngaging = true;
//                                break;
//                            }
//                        }
//                        moduleCount++;
//                    }
//                }
 //           }
 //       }
    }
}