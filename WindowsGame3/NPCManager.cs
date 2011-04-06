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
        bool isEngaging, isEvading, hasEvadeVector;
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
            {
            // First check for Evading state and act on that first and for most!
                if ((Vector3.Distance(thisShip.modelPosition, otherShip.modelPosition)) < rand.Next(10, 2200) + otherShip.EvadeDist[(int)otherShip.objectClass])
                {
                    if (!isEvading)
                        thisShip.vecToTarget = new Vector3(thisShip.Direction.X * rand.Next(10, 2200), 0, thisShip.Direction.Z * rand.Next(10, 2200));
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
                                isEngaging = true;
                            }
                            thrustAmount = (float)rand.NextDouble();
                        }
                        break;
                     case disposition.patrol:
                            if (thisShip.modelFrustum.Intersects(otherShip.modelFrustum))
                            {
                                thisShip.currentTarget = otherShip;
                                thisShip.currentDisposition = disposition.pursue;
                                if (currentTime - lastWeaponFireTime > weaponDefList[(int)thisShip.currentWeapon.weaponType].regenTime)
                                {
                                    weaponsManager.fireWeapon(thisShip.currentTarget, thisShip, projectileTrailParticles, ref weaponDefList, thisShip.pylonIndex);
                                    thisShip.pylonIndex++;
                                    if (thisShip.pylonIndex > thisShip.currentWeapon.ModulePositionOnShip.GetLength(0) - 1)
                                        thisShip.pylonIndex = 0;
                                    lastWeaponFireTime = currentTime;
                                    isEngaging = true;
                                }
                            }
                        // thrustAmount = 0.20f;
                        break;
                }
            }
        }

        public void updateShipMovement(GameTime gameTime, float gameSpeed, newShipStruct thisShip,
                               ref List<weaponData> weaponDefList, ref List<shipData> shipDefList,
                                Camera ourCamera)
        {
            // update models 2d coords
            float turningSpeed = (float)gameTime.ElapsedGameTime.TotalMilliseconds / 1000.0f;
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
            turningSpeed *= thisShip.objectAgility * gameSpeed;
            Vector3 rotationAmount = Vector3.Zero;
            int roll = 0;
            Vector3 newDirection = Vector3.Zero;
            Matrix rot = thisShip.modelRotation;
            Vector3 forward = rot.Right;
            Vector3 right = rot.Forward;
            Vector3 up = rot.Up;
            thisShip.vecToTarget.Normalize();
            rotationAmount = rotationAmount * turningSpeed * elapsed;

            if (Vector3.Dot(forward, thisShip.vecToTarget) > -0.99f)
            {
                forward = Vector3.SmoothStep(forward, thisShip.vecToTarget, thisShip.objectAgility * 0.05f);
            }
            else
            {
                forward = Vector3.SmoothStep(forward, right, thisShip.objectAgility * 0.05f);
            }

            right = Vector3.Cross(forward, Vector3.Up);
            up = Vector3.Cross(right, forward);

            forward.Normalize();
            right.Normalize();
            up.Normalize();

            Matrix m = Matrix.Identity;
            m.Forward = right;
            m.Right = forward;
            m.Up = up;
            thisShip.modelRotation = m;
            thisShip.Direction = thisShip.modelRotation.Right;
            Vector3 force = thisShip.Direction * thrustAmount * thisShip.objectThrust;
            // Apply acceleration
            Vector3 acceleration = force / thisShip.objectMass;
            thisShip.Velocity += acceleration * elapsed;
            // Apply psuedo drag
            thisShip.Velocity *= DragFactor;
            // Apply velocity
            thisShip.modelPosition += thisShip.Velocity * elapsed;
            thisShip.worldMatrix = rot * Matrix.CreateTranslation(thisShip.modelPosition);
           
            thisShip.modelBoundingSphere.Center = thisShip.modelPosition;
            thisShip.viewMatrix = Matrix.CreateLookAt(thisShip.modelPosition, thisShip.modelPosition + 
                                                      thisShip.Direction * 2.0f, thisShip.Up);
            thisShip.projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(25.0f), 4.0f / 3.0f, .5f, 500f);
            thisShip.modelFrustum.Matrix = thisShip.viewMatrix * thisShip.projectionMatrix;
          
            //screenCords = get2dCoords(this, ourCamera);
           // if (thisShip.ThrusterEngaged)
         //   {
                thisShip.shipThruster.update(thisShip.modelPosition + (thisShip.modelRotation.Forward)
                                        - (thisShip.modelRotation.Up) + (thisShip.modelRotation.Right * -20),
                                        thisShip.Direction, new Vector3(6, 6, 6), 40.0f, 10.0f,
                                        Color.White, Color.Blue, ourCamera.position);

                thisShip.shipThruster.heat = 1.5f;
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
            Vector3 rotationAmount = Vector3.Zero;
            thisShip.worldMatrix = Matrix.Identity;
            //Vector3 newDirection = Vector3.Zero;
            Matrix rot = thisShip.modelRotation;
            Vector3 forward = rot.Right;
            Vector3 right = rot.Forward;
            Vector3 up = rot.Up;
            rotationAmount = rotationAmount * turningSpeed * elapsed;
            //forward = thisShip.vecToTarget;

            right = Vector3.Cross(forward, Vector3.Up);
            up = Vector3.Cross(right, forward);

            forward.Normalize();
            right.Normalize();
            up.Normalize();

            Matrix m = Matrix.Identity;
            m.Forward = right;
            m.Right = forward;
            m.Up = up;
            //thisShip.modelRotation = m;
            thisShip.Direction = thisShip.modelRotation.Right;
            //thisShip.worldMatrix = (thisShip.modelRotation * m) * Matrix.CreateTranslation(thisShip.modelPosition);

            thisShip.modelBoundingSphere.Center = thisShip.modelPosition;
            viewMatrix = Matrix.CreateLookAt(thisShip.modelPosition, forward, up);
            //thisShip.modelFrustum.Matrix = viewMatrix * projectionMatrix;
            //thisShip.screenCords = get2dCoords(thisShip.modelPosition, ourCamera);
            if (thisShip.currentTarget != null)
                thisShip.distanceFromTarget = Vector3.Distance(thisShip.modelPosition, thisShip.currentTarget.modelPosition);
        }

    }
}