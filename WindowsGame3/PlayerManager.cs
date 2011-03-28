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
    public class PlayerManager : ModelManager
    {
        public int shipTypeIndex;
        public shipTypes playerShipData;
        //public float playerShipHealth;

        public float thrustAmount = 0.25f;
        private const float ThrustForce = 500.0f;

        /// <summary>
        /// Velocity scalar to approximate drag.
        /// </summary>
        private const float DragFactor = 0.98f;

        public PlayerManager(Game game)
            : base(game)
        {
            // TODO: Construct any child components here
        }

        public void weaponPylonInit()
        {
            Vector3 plyonPoint = (modelRotation.Right * 6) - (modelRotation.Up * 5);
            shipWeaponPylons.Add(plyonPoint);
            plyonPoint = (modelRotation.Right * -6) - (modelRotation.Up * 5);
            shipWeaponPylons.Add(plyonPoint);

        }

        public void updateShipMovement(GameTime gameTime, Camera ourCamera, float gameSpeed,KeyboardState keyboardState,
                                        Vector3 vecToTarget)
        {
            float turningSpeed = (float)gameTime.ElapsedGameTime.TotalMilliseconds / 1000.0f;
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
            float roll = 0;
            turningSpeed *= objectAgility * gameSpeed * 2.0f;
            Vector2 rotationAmount = Vector2.Zero;
            // Keyboard checks
            //Vector2 rotationAmount = -gamePadState.ThumbSticks.Left;
            if (keyboardState.IsKeyDown(Keys.A))
            {
                rotationAmount.X = 2.0f;
              //  if (right.Z < maxTurningAngle)
             //   {
             //   roll = -0.033f;
             //   }
            }
            if (keyboardState.IsKeyDown(Keys.D))
            {
                rotationAmount.X = -2.0f;
           //     if (right.Z > -1 * maxTurningAngle)
           //     {
          //      roll = 0.033f;
           //     }
            }
            //if (keyboardState.IsKeyDown(Keys.S))
            //    rotationAmount.Y = 1.0f;

            //if (keyboardState.IsKeyDown(Keys.W))
            //    rotationAmount.Y = -1.0f;

            //if (keyboardState.IsKeyDown(Keys.Z))
            //    roll = -0.033f;
            //if (keyboardState.IsKeyDown(Keys.X))
            //    roll = 0.033f;

            //if (Up.Y < 0)
            //    rotationAmount.X = +rotationAmount.X;

            // Scale rotation amount to radians per second
            rotationAmount = rotationAmount * turningSpeed * elapsed;

            Matrix rotationMatrix =
                Matrix.CreateFromAxisAngle(Right, rotationAmount.Y) *
                ((Matrix.CreateFromAxisAngle(Direction, roll) *
                Matrix.CreateFromAxisAngle(Up, rotationAmount.X)));

            Direction = Vector3.TransformNormal(Direction, rotationMatrix);
            Up = Vector3.TransformNormal(Up, rotationMatrix);
            Direction.Normalize();
            Up.Normalize();
            right = Vector3.Cross(Direction, Up);
            Up = Vector3.Cross(right, Direction);

            thrustAmount = 0.75f;

            // Calculate force from thrust amount
            Vector3 force = Direction * thrustAmount * objectThrust;

            // Apply acceleration
            Vector3 acceleration = force / objectMass;
            Velocity += acceleration * elapsed;

            // Apply psuedo drag
            Velocity *= DragFactor;

            // Apply velocity
            modelPosition += Velocity * elapsed;
            modelRotation = modelRotation * rotationMatrix;
            worldMatrix = (modelRotation * rotationMatrix) *
                          Matrix.CreateTranslation(modelPosition);
            modelBoundingSphere.Center = modelPosition; modelBoundingSphere.Radius = radius;
            viewMatrix = Matrix.CreateLookAt(modelPosition, modelRotation.Down , Up);

            modelFrustum.Matrix = Matrix.Transpose(viewMatrix) * projectionMatrix;
         }


        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            // TODO: Add your update code here

            base.Update(gameTime);
        }
    }
}