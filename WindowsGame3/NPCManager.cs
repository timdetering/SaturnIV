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
            // First check for Evading state and act on that first and for most!
                if ((Vector3.Distance(thisShip.modelPosition, otherShip.modelPosition)) < rand.Next(10, 100) + otherShip.EvadeDist[(int)otherShip.objectClass])
                {
                    //if (!isEvading)
                        //thisShip.vecToTarget = new Vector3(thisShip.Direction.X * rand.Next(10, 2200), 0, thisShip.Direction.Z * rand.Next(10, 2200));
                    isEvading = true;
                    thrustAmount = (float)rand.NextDouble();
                }
                else
                    isEvading = false;
                switch (thisShip.currentDisposition)
                {   
                    case disposition.pursue:
                        thisShip.vecToTarget = (thisShip.currentTarget.modelPosition - thisShip.modelPosition) * (float)rand.NextDouble();
                        if (thisShip.modelFrustum.Intersects(otherShip.modelFrustum))
                        {
                            if (currentTime - thisShip.lastWeaponFireTime > weaponDefList[(int)thisShip.currentWeapon.weaponType].regenTime)
                            {
                                if (thisShip.pylonIndex > thisShip.currentWeapon.ModulePositionOnShip.GetLength(0))
                                    thisShip.pylonIndex = 0;
                                
                                weaponsManager.fireWeapon(thisShip.currentTarget, thisShip, projectileTrailParticles, ref weaponDefList,thisShip.pylonIndex);
                                thisShip.pylonIndex++;
                                if (thisShip.pylonIndex > thisShip.currentWeapon.ModulePositionOnShip.GetLength(0) - 1)
                                    thisShip.pylonIndex = 0;
                                thisShip.lastWeaponFireTime = currentTime;
                                thisShip.isEngaging = true;
                            }
                            thrustAmount = (float)rand.NextDouble();
                        }
                        break;
                     case disposition.patrol:
                        //thisShip.vecToTarget = thisShip.Direction;
                        if (thisShip.modelFrustum.Intersects(otherShip.modelFrustum))
                        {
                            thisShip.isEngaging = true;
                            thisShip.currentTarget = otherShip;
                            thisShip.currentDisposition = disposition.pursue;
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

        public void updateShipMovement(GameTime gameTime, float gameSpeed, newShipStruct thisShip,
                                       Camera ourCamera, bool isEdit)
        {
            // update models 2d coords
            float turningSpeed = (float)gameTime.ElapsedGameTime.TotalMilliseconds / 1000.0f;
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
            turningSpeed *= thisShip.objectAgility * gameSpeed;
            Vector3 rotationAmount = Vector3.Zero;
            int roll = 0;
            thisShip.vecToTarget.Normalize();
            rotationAmount = rotationAmount * turningSpeed * elapsed;

            Vector3 scale, translation;
            Quaternion rotation;
            Matrix rotationMatrix = Matrix.CreateWorld(thisShip.modelPosition, thisShip.vecToTarget, Vector3.Up);
            rotationMatrix.Decompose(out scale, out rotation, out translation);
            thisShip.Up = Vector3.TransformNormal(thisShip.Up, rotationMatrix);
            thisShip.Up.Normalize();
            thisShip.right = Vector3.Cross(thisShip.Direction, thisShip.Up);
            thisShip.Up = Vector3.Cross(thisShip.right, thisShip.Direction);
            thisShip.modelRotation = Matrix.CreateFromQuaternion(rotation);
            thisShip.modelRotation.Forward = Vector3.Lerp(thisShip.modelRotation.Forward, thisShip.vecToTarget, turningSpeed * 0.50f); ;
            if (!isEdit)
                thrustAmount = 1.0f;
            else
                thrustAmount = 0.0f;
            thisShip.Direction = thisShip.modelRotation.Right;
            Vector3 force = thisShip.Direction * thrustAmount * thisShip.objectThrust;
            // Apply acceleration
            Vector3 acceleration = force / thisShip.objectMass;
            thisShip.Velocity += acceleration * elapsed;
            // Apply psuedo drag
            thisShip.Velocity *= DragFactor;
            // Apply velocity
            thisShip.modelPosition += thisShip.Velocity * elapsed;
            thisShip.worldMatrix = rotationMatrix;
           
            thisShip.modelBoundingSphere.Center = thisShip.modelPosition;
            thisShip.viewMatrix = Matrix.CreateLookAt(thisShip.modelPosition, thisShip.modelPosition + 
                                                      thisShip.Direction * 2.0f, thisShip.Up);
            thisShip.modelFrustum.Matrix = thisShip.viewMatrix * thisShip.projectionMatrix;
          
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

        public void editModeUpdate(GameTime gameTime,newShipStruct thisShip,Camera ourCamera)
        {
            //vecToTarget = currentTargetObject.modelPosition - modelPosition;
            double currentTime = gameTime.TotalGameTime.TotalMilliseconds;
            // update models 2d coords
            float turningSpeed = 2.0f;
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
            turningSpeed *= thisShip.objectAgility * turningSpeed;
            Vector3 scale, translation;
            Quaternion rotation;
            Matrix rotationMatrix = Matrix.CreateWorld(thisShip.modelPosition, thisShip.vecToTarget, Vector3.Up);
            rotationMatrix.Decompose(out scale, out rotation, out translation);
            thisShip.Up = Vector3.TransformNormal(thisShip.Up, rotationMatrix);
            thisShip.Up.Normalize();
            thisShip.right = Vector3.Cross(thisShip.vecToTarget, thisShip.Up);
            thisShip.Up = Vector3.Cross(thisShip.right, thisShip.modelRotation.Forward);
            thisShip.modelRotation = Matrix.CreateFromQuaternion(rotation); 
            thisShip.modelRotation.Forward = thisShip.vecToTarget;
            // thisShip.modelRotation.Forward = Vector3.Lerp(thisShip.modelRotation.Forward, thisShip.vecToTarget, turningSpeed * 0.5f); ;
           
            thisShip.Direction = thisShip.modelRotation.Right;
            
            thisShip.worldMatrix = rotationMatrix;

            thisShip.modelBoundingSphere.Center = thisShip.modelPosition;
            thisShip.viewMatrix = Matrix.CreateLookAt(thisShip.modelPosition, thisShip.modelPosition +
                                                      thisShip.Direction * 2.0f, thisShip.Up);
            thisShip.projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(25.0f), 1.0f / 1.0f, .5f, 500f);
            thisShip.modelFrustum.Matrix = thisShip.viewMatrix * thisShip.projectionMatrix;

            thisShip.modelBoundingSphere.Center = thisShip.modelPosition;
            thisShip.viewMatrix = Matrix.CreateLookAt(thisShip.modelPosition, thisShip.modelPosition +
                                                       thisShip.Direction * 2.0f, thisShip.Up);
            thisShip.projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(25.0f), 4.0f / 3.0f, .5f, 500f);
            thisShip.modelFrustum.Matrix = thisShip.viewMatrix * thisShip.projectionMatrix;
            thisShip.screenCords = get2dCoords(thisShip.modelPosition, ourCamera);
            if (thisShip.currentTarget != null)
                thisShip.distanceFromTarget = Vector3.Distance(thisShip.modelPosition, thisShip.currentTarget.modelPosition);
        }

    }
}