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
    public class Camera : Microsoft.Xna.Framework.GameComponent
    {
        //Camera variables
        Vector3 cameraReference = new Vector3(0, 0, 1);
        public Vector3 cameraOffset = new Vector3(0.0f,0.0f,0.0f);
        public Vector3 cameraOffset2 = new Vector3(0.0f,1000.0f,500);
        public Vector3 campos = new Vector3(500f, 0.0f, 1.0f);
        public Vector3 campos2 = new Vector3(-2000.0f, 3000.0f, 1.0f);
        private float fieldOfView = MathHelper.ToRadians(120.0f);
        public Vector3 cameraLookAt2 = Vector3.Zero;
        public float zoomFactor = 1.0f;
        public Matrix cameraRotation = Matrix.Identity;

        public Matrix viewMatrix;
        public Matrix projectionMatrix;

        public float AspectRatio
        {
            get { return aspectRatio; }
            set { aspectRatio = value; }
        }
        private float aspectRatio = 4.0f / 3.0f;

        public Camera(Game game)
            : base(game)
        {
            // TODO: Construct any child components here
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            // TODO: Add your initialization code here

            base.Initialize();
        }

        public void SetUpCamera()
        {
            viewMatrix = Matrix.CreateLookAt(new Vector3(20, 13, -5), new Vector3(8, 0, -7), new Vector3(0, 1, 0));
            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, Game.GraphicsDevice.Viewport.AspectRatio, 0.5f, 500.0f);
        }
        public void UpdateFirstPersonCamera(Vector3 position, Matrix modelRotation)
        {
            cameraRotation = Matrix.Lerp(cameraRotation, modelRotation, 0.1f);
            campos = new Vector3(0.0f, 0.0f, 1.0f);
            campos = Vector3.Transform(campos, cameraRotation);
            campos += position;
            Vector3 camup = new Vector3(0, 1, 0);
            camup = Vector3.Transform(camup, cameraRotation);
            //campos = position - cameraOffset;
            Vector3 cameraLookAt = new Vector3(0,0,0);
            cameraLookAt = position;
            //cameraLookAt.Y += 2.0f;
            viewMatrix = Matrix.CreateLookAt(campos, cameraLookAt, camup);
            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(fieldOfView,
                AspectRatio, 0.5f, 50000.0f);
        }

        public void UpdateChaseCamera(Vector3 position, Matrix modelRotation)
        {
            cameraRotation = Matrix.Lerp(cameraRotation, modelRotation, 0.1f);
            //cameraRotation *= Matrix.CreateRotationY(MathHelper.ToRadians(90));
            //campos = cameraOffset;
            campos = Vector3.Transform(campos, cameraRotation);
            campos += position;
            Vector3 camup = new Vector3(0, 1, 0);
            camup = Vector3.Transform(camup, cameraRotation);
            //campos = position - cameraOffset;
            Vector3 cameraLookAt;
            cameraLookAt = position + (modelRotation.Forward * 1.5f) - (modelRotation.Down * 15.0f);
            cameraLookAt.Y += 14.0f;
            viewMatrix = Matrix.CreateLookAt(campos, cameraLookAt, camup);
            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(fieldOfView,
                AspectRatio, 0.5f, 50000.0f);
        }

        public void UpdateGodCam(Vector3 position, Matrix modelRotation)
        {
            cameraLookAt2 = Vector3.Zero;
            campos2 = position + cameraOffset2 * zoomFactor;
            //cameraRotation = Matrix.Lerp(cameraRotation, modelRotation, 0.1f);
            //campos2 = Vector3.Transform(campos2, cameraRotation);
            Vector3 camup = new Vector3(0, 1, 0);
            //camup = Vector3.Transform(camup, cameraRotation);
            //campos = position - cameraOffset;
            cameraLookAt2 = position;
            cameraLookAt2.Y += 14.0f;
            viewMatrix = Matrix.CreateLookAt(campos2, cameraLookAt2, Vector3.Up);
            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(fieldOfView,
                AspectRatio, 0.5f, 50000.0f);
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