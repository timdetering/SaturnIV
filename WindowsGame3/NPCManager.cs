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
        float thrustAmount;
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
                               ref List<weaponData> weaponDefList,newShipStruct thisShip)
        {
            double currentTime = gameTime.TotalGameTime.TotalMilliseconds;

            if (isEvading)
            {
                //vecToTarget = (currentTargetObject.modelPosition * rand.Next(500, 10000)) - modelPosition;
                isEvading = false;
            }
            else
            {
                switch (thisShip.currentDisposition)
                {
                    case disposition.pursue:
                        thisShip.vecToTarget = thisShip.currentTarget.modelPosition - thisShip.modelPosition;
                        if (thisShip.distanceFromTarget < 5000)
                            if (currentTime - lastWeaponFireTime > weaponDefList[(int)thisShip.currentWeapon].regenTime)
                            //Vector3.Dot(modelRotation.Forward, vecToTarget) < Math.Cos(MathHelper.ToRadians(45)))
                            {
                                //currentWeaponIndex = weaponArray[0];
                                //weaponsManager.fireWeapon(thisShip.currentTarget, thisShip, projectileTrailParticles, ref weaponDefList);

                                lastWeaponFireTime = currentTime;
                                isEngaging = true;
                                isEvading = false;
                            }
                        break;
                    case disposition.evade:
                        if (isEvading)
                        {
                            //vecToTarget = (currentTargetObject.modelPosition * rand.Next(500, 10000)) - modelPosition;
                            isEvading = true;
                        }
                        // thrustAmount = 0.25f;
                        isEngaging = false;
                        break;

                    case disposition.patrol:
                        if (thisShip.distanceFromTarget < 5000 && thisShip.currentTarget != null)
                        {
                            if (currentTime - lastWeaponFireTime > weaponDefList[(int)thisShip.currentWeapon].regenTime)
                            {
                                weaponsManager.fireWeapon(thisShip.currentTarget, thisShip, projectileTrailParticles, ref weaponDefList);
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
            thrustAmount = 0.50f;
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
            thisShip.worldMatrix = m * Matrix.CreateTranslation(thisShip.modelPosition);
            thisShip.modelBoundingSphere.Center = thisShip.modelPosition;
            //viewMatrix = Matrix.CreateLookAt(modelPosition, forward, up);
            //modelFrustum.Matrix = viewMatrix * projectionMatrix;
             //screenCords = get2dCoords(this, ourCamera);
            if (thisShip.ThrusterEngaged)
            {
                thisShip.shipThruster.update(thisShip.modelPosition + (thisShip.modelRotation.Forward)
                                        - (thisShip.modelRotation.Up) + (thisShip.modelRotation.Right * -20),
                                        thisShip.Direction, new Vector3(6, 6, 6), 40.0f, 10.0f,
                                        Color.White, Color.Blue, ourCamera.position);

                thisShip.shipThruster.heat = 1.5f;
            }

            if (thisShip.currentTarget != null)
                thisShip.distanceFromTarget = Vector3.Distance(thisShip.modelPosition, thisShip.currentTarget.modelPosition);
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

        public void editModeUpdate(GameTime gameTime,newShipStruct thisShip)
        {
            //vecToTarget = currentTargetObject.modelPosition - modelPosition;
            double currentTime = gameTime.TotalGameTime.TotalMilliseconds;
            // update models 2d coords
            float turningSpeed = 2.0f;
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
            turningSpeed *= thisShip.objectAgility * turningSpeed;
            Vector3 rotationAmount = Vector3.Zero;
         
            Vector3 newDirection = Vector3.Zero;
            Matrix rot = thisShip.modelRotation;
            Vector3 forward = rot.Right;
            Vector3 right = rot.Forward;
            Vector3 up = rot.Up;
            rotationAmount = rotationAmount * turningSpeed * elapsed;
            forward = thisShip.vecToTarget;

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
            
            thisShip.worldMatrix = m * Matrix.CreateTranslation(thisShip.modelPosition);
            thisShip.modelBoundingSphere.Center = thisShip.modelPosition;
            //viewMatrix = Matrix.CreateLookAt(modelPosition, forward, up);
            //modelFrustum.Matrix = viewMatrix * projectionMatrix;
            //screenCords = get2dCoords(this, ourCamera);
            if (thisShip.currentTarget != null)
                thisShip.distanceFromTarget = Vector3.Distance(thisShip.modelPosition, thisShip.currentTarget.modelPosition);
        }

    }
}