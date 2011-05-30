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
    public class PlayerManager : ModelManager
    {
        public int shipTypeIndex;
        public Line3D fArc1,fArc2;

        //public float playerShipHealth;

        public float thrustAmount = 0.0f;

        /// <summary>
        /// Velocity scalar to approximate drag.
        /// </summary>
        private const float DragFactor = 0.97f;

        public PlayerManager(Game game)
            : base(game)
        {
            // TODO: Construct any child components here
        }

       public void updateShipMovement(GameTime gameTime, float gameSpeed,KeyboardState keyboardState,newShipStruct playerShip,Camera ourCamera)
        {
            float turningSpeed = (float)gameTime.ElapsedGameTime.TotalMilliseconds / 1000.0f;
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
            float roll = 0;
            turningSpeed *= playerShip.objectAgility * gameSpeed;
            Vector2 rotationAmount = Vector2.Zero;
            // Keyboard checks
            //Vector2 rotationAmount = -gamePadState.ThumbSticks.Left;
            if (keyboardState.IsKeyDown(Keys.A))
            {
                rotationAmount.X = 4.0f;
            }
            if (keyboardState.IsKeyDown(Keys.D))
            {
                rotationAmount.X = -4.0f;
            }

            if (keyboardState.IsKeyDown(Keys.W))
            {
                playerShip.ThrusterEngaged = true;
                thrustAmount = 1.0f;
            }
            else
            {
                playerShip.ThrusterEngaged = false;
                thrustAmount = 0.0f;
            }

            // Scale rotation amount to radians per second
            rotationAmount = rotationAmount * turningSpeed * elapsed;

            Matrix rotationMatrix =
                Matrix.CreateFromAxisAngle(playerShip.Right, rotationAmount.Y) *
                ((Matrix.CreateFromAxisAngle(playerShip.Direction, roll) *
                Matrix.CreateFromAxisAngle(playerShip.Up, rotationAmount.X)));

            playerShip.Direction = Vector3.TransformNormal(playerShip.Direction, rotationMatrix);
            playerShip.Up = Vector3.TransformNormal(playerShip.Up, rotationMatrix);
            playerShip.Direction.Normalize();
            playerShip.Up.Normalize();
            playerShip.right = Vector3.Cross(playerShip.Direction, playerShip.Up);
            playerShip.Up = Vector3.Cross(playerShip.right, playerShip.Direction);
            Vector3 force = playerShip.Direction * thrustAmount * playerShip.objectThrust;
            // Apply acceleration
            Vector3 acceleration = force / playerShip.objectMass;
            playerShip.Velocity += acceleration * thrustAmount * elapsed;
            // Apply psuedo drag
            playerShip.Velocity *= DragFactor;
            // Apply velocity
            playerShip.modelPosition += playerShip.Velocity * elapsed;
            playerShip.modelRotation = playerShip.modelRotation * rotationMatrix;
            playerShip.worldMatrix = (playerShip.modelRotation * rotationMatrix) *
                          Matrix.CreateTranslation(playerShip.modelPosition);

            playerShip.modelBoundingSphere.Center = playerShip.modelPosition; playerShip.modelBoundingSphere.Radius = 17;
            playerShip.viewMatrix = Matrix.CreateLookAt(playerShip.modelPosition, playerShip.modelPosition 
                                                        + playerShip.Direction * 2.0f, playerShip.Up);
            if (playerShip.ThrusterEngaged)
            {
                playerShip.shipThruster.update(playerShip.modelPosition + (playerShip.modelRotation.Forward)
                                        - (playerShip.modelRotation.Up) + (playerShip.modelRotation.Right * -20),
                                        playerShip.Direction, new Vector3(6, 6, 6), 40.0f, 10.0f,
                                        Color.White, Color.Blue, Camera.position);

                playerShip.shipThruster.heat = 1.5f;
            }

            playerShip.modelFrustum.Matrix = playerShip.viewMatrix * playerShip.projectionMatrix;
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