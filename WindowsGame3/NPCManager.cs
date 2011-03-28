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


namespace WindowsGame3
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class NPCManager : ModelManager
    {
        
        public bool isHostile = false;
        public int shipTypeIndex;
        public shipTypes NPCShipData;
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

        private void ai(GameTime gameTime, ref List<WeaponsManager> missileList,ParticleSystem projectileTrailParticles)
        {
            double currentTime = gameTime.TotalGameTime.TotalMilliseconds;

            if (isEvading)
            {
                //vecToTarget = (currentTargetObject.modelPosition * rand.Next(500, 10000)) - modelPosition;
                isEvading = false;
            }
            else
            {
                switch (npcDisposition)
                {
                    case disposition.pursue:
                        vecToTarget = currentTargetObject.modelPosition - modelPosition;
                        if (distanceFromTarget < 5000)
                            if (currentTime - lastWeaponFireTime > weaponTypes.regenTime[(int)currentWeaponIndex]) // &&
                            //Vector3.Dot(modelRotation.Forward, vecToTarget) < Math.Cos(MathHelper.ToRadians(45)))
                            {
                                currentWeaponIndex = weaponArray[0];
                                helperClass.fireWeapon(Game, currentTargetObject, this, ref missileList, modelRotation.Right,
                                                        projectileTrailParticles
                                    );
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
                        if (distanceFromTarget < 5000 && currentTargetObject != null)
                        {
                            if (currentTime - lastWeaponFireTime > weaponTypes.regenTime[(int)currentWeaponIndex])
                            {
                                helperClass.fireWeapon(Game, currentTargetObject, this, ref missileList,
                                currentTargetObject.modelPosition - modelPosition, projectileTrailParticles);
                                lastWeaponFireTime = currentTime;
                                isEngaging = true;
                            }
                        }

                        // thrustAmount = 0.20f;
                        break;
                }
            }
        }


        public void updateShipMovement(GameTime gameTime, Camera ourCamera, float gameSpeed)
        {
            // update models 2d coords
            float turningSpeed = (float)gameTime.ElapsedGameTime.TotalMilliseconds / 1000.0f;
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
            turningSpeed *= objectAgility * gameSpeed;
            Vector3 rotationAmount = Vector3.Zero;
            thrustAmount = 1.0f;
            int roll = 0;
            

            Vector3 newDirection = Vector3.Zero;
            isFirstRun = false;
            Matrix rot = modelRotation;
            Vector3 forward = rot.Right;
            Vector3 right = rot.Forward;
            Vector3 up = rot.Up;
            vecToTarget.Normalize();
            rotationAmount = rotationAmount * turningSpeed * elapsed;

            if (Vector3.Dot(forward, vecToTarget) > -0.99f)
            {
                forward = Vector3.SmoothStep(forward, vecToTarget, objectAgility * 0.05f);
            }
            else
            {
                forward = Vector3.SmoothStep(forward, right, objectAgility * 0.05f);
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
            modelRotation = m;
            Direction = modelRotation.Right;
            Vector3 force = Direction * thrustAmount * objectThrust;
            // Apply acceleration
            Vector3 acceleration = force / objectMass;
            Velocity += acceleration * elapsed;
            // Apply psuedo drag
            Velocity *= DragFactor;
            // Apply velocity
            modelPosition += Velocity * elapsed;
            worldMatrix = m * Matrix.CreateTranslation(modelPosition);
            modelBoundingSphere.Center = modelPosition;
            viewMatrix = Matrix.CreateLookAt(modelPosition, forward, up);
            modelFrustum.Matrix = viewMatrix * projectionMatrix;
            screenCords = get2dCoords(this, ourCamera);
            if (currentTargetObject != null) 
                 distanceFromTarget = Vector3.Distance(modelPosition, currentTargetObject.modelPosition);
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

        public void editModeUpdate(GameTime gameTime)
        {
            //vecToTarget = currentTargetObject.modelPosition - modelPosition;
            double currentTime = gameTime.TotalGameTime.TotalMilliseconds;
            // update models 2d coords
            float turningSpeed = 2.0f;
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
            turningSpeed *= objectAgility * turningSpeed;
            Vector3 rotationAmount = Vector3.Zero;
         
            Vector3 newDirection = Vector3.Zero;
            isFirstRun = false;
            Matrix rot = modelRotation;
            Vector3 forward = rot.Right;
            Vector3 right = rot.Forward;
            Vector3 up = rot.Up;
            rotationAmount = rotationAmount * turningSpeed * elapsed;
            forward = vecToTarget;
         //   if (Vector3.Dot(forward, vecToTarget) > -0.99f)
       //     {
      //          forward = Vector3.SmoothStep(forward, vecToTarget, objectAgility * 0.05f);
      //      }
      //      else
      //      {
      //          forward = Vector3.SmoothStep(forward, right, objectAgility * 0.05f);
       //     }

            right = Vector3.Cross(forward, Vector3.Up);
            up = Vector3.Cross(right, forward);

            forward.Normalize();
            right.Normalize();
            up.Normalize();

            Matrix m = Matrix.Identity;
            m.Forward = right;
            m.Right = forward;
            m.Up = up;
            modelRotation = m;
            Direction = modelRotation.Right;

            worldMatrix = m * Matrix.CreateTranslation(modelPosition);
            modelBoundingSphere.Center = modelPosition;
            viewMatrix = Matrix.CreateLookAt(modelPosition, forward, up);
            modelFrustum.Matrix = viewMatrix * projectionMatrix;
            //screenCords = get2dCoords(this, ourCamera);
            if (currentTargetObject != null)
                distanceFromTarget = Vector3.Distance(modelPosition, currentTargetObject.modelPosition);
        }

    }
}