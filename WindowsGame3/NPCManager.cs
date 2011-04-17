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
        double lastWeaponFireTime;
        bool isEvading, hasEvadeVector;
        float currentTargetLevel;
        Vector3 isFacing;
        Vector3 isRight;
        int moduleCount = 0;
        //disposition predisposition = new disposition();
        Random rand = new Random();

        HelperClass helperClass = new HelperClass();

        /// <summary>
        /// Velocity scalar to approximate drag.
        /// </summary>
        private const float DragFactor = 0.98f;

        public NPCManager(Game game)
            : base(game)
        {
            // TODO: Construct any child components here
        }

        public void performAI(GameTime gameTime, ref WeaponsManager weaponsManager, ParticleSystem projectileTrailParticles,
                               ref List<weaponData> weaponDefList,newShipStruct thisShip, newShipStruct otherShip)
        {
            double currentTime = gameTime.TotalGameTime.TotalMilliseconds;
            Random rand = new Random();
            moduleCount = 0;
            // First check for Evading state and act on that first and for most!
                if ((Vector3.Distance(thisShip.modelPosition, otherShip.modelPosition)) < rand.Next(10, 100) + otherShip.EvadeDist[(int)otherShip.objectClass])
                {
                    if (!isEvading)
                        thisShip.vecToTarget = thisShip.Direction * (float)rand.NextDouble();
                    isEvading = true;
                    thrustAmount = 1.0f;
                }
                else
                    isEvading = false;
                if (!isEvading)
                {
                    switch (thisShip.currentDisposition)
                    {
                        case disposition.pursue:
                            thisShip.angleOfAttack = MathHelper.ToDegrees((float)GetSignedAngleBetweenTwoVectors(thisShip.Direction, thisShip.vecToTarget, thisShip.modelRotation.Right));
                            thisShip.vecToTarget = Vector3.Normalize(thisShip.currentTarget.modelPosition - thisShip.modelPosition);// *(float)rand.NextDouble();
                            if (Vector3.Distance(thisShip.modelPosition, otherShip.modelPosition) < 200) //rand.Next(10, 100) + weaponDefList[(int)thisShip.currentWeapon.weaponType].range)
                            {
                                thisShip.currentDisposition = disposition.patrol;
                                thisShip.isEngaging = false;
                                thisShip.vecToTarget = (thisShip.currentTarget.modelPosition - thisShip.modelPosition) * (float)rand.NextDouble();
                                break;
                            } else
                            // Cycle Through Weapons
                                foreach (WeaponModule thisWeapon in thisShip.weaponArray)
                                {
                                    for (int i = 0; i < thisWeapon.ModulePositionOnShip.Count(); i++)
                                    {
                                        if (thisShip.moduleFrustum[moduleCount].Intersects(otherShip.modelFrustum))
                                        {
                                            if (currentTime - thisShip.lastWeaponFireTime > weaponDefList[(int)thisWeapon.weaponType].regenTime)
                                            {
                                                weaponsManager.fireWeapon(thisShip.currentTarget, thisShip, projectileTrailParticles, ref weaponDefList, thisWeapon,i);
                                                thisShip.lastWeaponFireTime = currentTime;
                                                thisShip.isEngaging = true;
                                                break;
                                            }
                                       }
                                        moduleCount++;
                                   }
                            }                            
                            break;
                        case disposition.patrol:
                            //thisShip.vecToTarget = thisShip.Direction;
                            if (thisShip.currentTarget != null)
                                currentTargetLevel = thisShip.TargetPrefs[(int)thisShip.currentTarget.objectClass];
                            if (Vector3.Distance(thisShip.modelPosition, otherShip.modelPosition) < rand.Next(10, 100) + weaponDefList[(int)thisShip.currentWeapon.weaponType].range)
                            {
                                //Decide weather or Not to Pursue based on this ships TargetPrefs values;  Ex. A capitalship is not going to chase a fighter!
                              //  if (thisShip.TargetPrefs[(int)otherShip.objectClass] > 0)
                     //           {
                                    thisShip.isEngaging = true;
                                    thisShip.currentDisposition = disposition.pursue;
                                    thisShip.currentTarget = otherShip;
                    //            }

                                if (currentTime - lastWeaponFireTime > weaponDefList[(int)thisShip.currentWeapon.weaponType].regenTime)
                                {
                                    weaponsManager.fireWeapon(thisShip.currentTarget, thisShip, projectileTrailParticles, ref weaponDefList, thisShip.pylonIndex);
                                    thisShip.pylonIndex++;
                                    if (thisShip.pylonIndex > thisShip.currentWeapon.ModulePositionOnShip.GetLength(0) - 1)
                                        thisShip.pylonIndex = 0;
                                    lastWeaponFireTime = currentTime;
                                }
                            }
                            else
                                thisShip.isEngaging = false;
                            // thrustAmount = 0.20f;
                            break;
                    }
                }
        }

        public void updateShipMovement(GameTime gameTime, float gameSpeed, newShipStruct thisShip,
                                       Camera ourCamera, bool isEdit)
        {
            // update models 2d coords
            float turningSpeed = (float)gameTime.ElapsedGameTime.TotalMilliseconds / 1000.0f;
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
            turningSpeed *= thisShip.objectAgility * gameSpeed;
            Vector2 rotationAmount = new Vector2(0,0);
            int roll = 0;
            // Scale rotation amount to radians per second

            if (GetSignedAngleBetweenTwoVectors(thisShip.Direction, Vector3.Normalize(thisShip.vecToTarget - thisShip.modelPosition), thisShip.modelRotation.Right)
                < 1)
            {
                rotationAmount.X -= 2.0f;
            }
            else
                rotationAmount.X += 2.0f;

            rotationAmount = rotationAmount * turningSpeed * elapsed;
            Matrix rotationMatrix =
                Matrix.CreateFromAxisAngle(thisShip.Right, rotationAmount.Y) *
                ((Matrix.CreateFromAxisAngle(thisShip.Direction, roll) *
                Matrix.CreateFromAxisAngle(thisShip.Up, rotationAmount.X)));

            thisShip.Direction = Vector3.TransformNormal(thisShip.Direction, rotationMatrix);
            thisShip.Up = Vector3.TransformNormal(thisShip.Up, rotationMatrix);
            thisShip.Direction.Normalize();
            thisShip.Up.Normalize();
            thisShip.right = Vector3.Cross(thisShip.Direction, thisShip.Up);
            thisShip.Up = Vector3.Cross(thisShip.right, thisShip.Direction);

            Vector3 force = thisShip.Direction * thrustAmount * thisShip.objectThrust;
            // Apply acceleration
            Vector3 acceleration = force / thisShip.objectMass;
            thisShip.Velocity += acceleration * thrustAmount * elapsed;
            // Apply psuedo drag
            thisShip.Velocity *= DragFactor;
            // Apply velocity
            thisShip.modelPosition += thisShip.Velocity * elapsed;
            thisShip.modelRotation = thisShip.modelRotation * rotationMatrix;
            thisShip.worldMatrix = thisShip.modelRotation * rotationMatrix * Matrix.CreateTranslation(thisShip.modelPosition);

            thisShip.modelBoundingSphere.Center = thisShip.modelPosition;
            thisShip.viewMatrix = Matrix.CreateLookAt(thisShip.modelPosition, thisShip.modelPosition +
                                                      thisShip.Direction * 2.0f, thisShip.Up);
            thisShip.modelFrustum.Matrix = thisShip.viewMatrix * thisShip.projectionMatrix;

            //Update all Weapon Module Firing Frustums
            moduleCount = 0;
            foreach (WeaponModule thisWeapon in thisShip.weaponArray)
            {
                for (int i=0;i<thisWeapon.ModulePositionOnShip.Count();i++)
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
                            isFacing = thisShip.right;
                           isRight = thisShip.modelRotation.Right;
                            break;
                        case 3:
                            isFacing = -thisShip.right;
                            isRight = thisShip.modelRotation.Left;
                            break;
                    }
                    thisShip.moduleFrustum[moduleCount].Matrix = Matrix.CreateLookAt(new Vector3(thisShip.modelPosition.X,
                                                        thisShip.modelPosition.Y, thisShip.modelPosition.Z),thisShip.modelPosition + isFacing,thisShip.Up) *
                                                        Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(thisWeapon.FiringEnvelopeAngle),
                                                        4.0f / 3.0f, .5f, 500f);
                    moduleCount++;
                }
            } 

                
            // if (thisShip.ThrusterEngaged)
            //   {
            //      thisShip.shipThruster.update(thisShip.modelPosition + (thisShip.modelRotation.Forward)
            //                              - (thisShip.modelRotation.Up) + (thisShip.modelRotation.Right * -20),
            //                              thisShip.Direction, new Vector3(6, 6, 6), 40.0f, 10.0f,
            //                              Color.White, Color.Blue, ourCamera.position);

            //      thisShip.shipThruster.heat = 1.5f;
            //  }

            if (thisShip.currentTarget != null)
                thisShip.distanceFromTarget = Vector3.Distance(thisShip.modelPosition, thisShip.currentTarget.modelPosition);
            thisShip.screenCords = get2dCoords(thisShip.modelPosition, ourCamera);
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

        public double GetSignedAngleBetweenTwoVectors(Vector3 Source, Vector3 Dest, Vector3 DestsRight)
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
    }
}