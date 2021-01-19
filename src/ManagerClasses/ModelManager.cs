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
    public class ModelManager : Microsoft.Xna.Framework.DrawableGameComponent
    {
        public Model myModel;

        public Matrix viewMatrix = Matrix.Identity;
        //public Matrix projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(25.0f), 1.0f / 1.0f, .5f, 500f);
        public Texture2D modelTexture;
        public Vector3 screenCords = Vector3.Zero;

        // The aspect ratio determines how to scale 3d to 2d projection.
        public float aspectRatio;

        public ModelManager(Game game)
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
            base.Initialize();
        }

        protected override void LoadContent()
        {
            base.LoadContent();
        }

        public Model LoadModel(string assetName)
        {          
            myModel = Game.Content.Load<Model>(assetName);
            return myModel;
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
        public Vector3 get2dCoords(Vector3 modelPos, CameraNew ourCamera)
        {
            return Game.GraphicsDevice.Viewport.Project(modelPos, ourCamera.projectionMatrix,
                    ourCamera.viewMatrix, Matrix.Identity);
        }

        public void DrawModelWithTexture(Vector3 position, CameraNew myCamera, Texture2D myTexture, Model myModel1)
        {
            Matrix worldMatrix = Matrix.CreateScale(500) * Matrix.CreateTranslation(position);
            Matrix[] transforms = new Matrix[myModel1.Bones.Count];
            myModel1.CopyAbsoluteBoneTransformsTo(transforms);

            // Draw the model. A model can have multiple meshes, so loop.
            foreach (ModelMesh mesh in myModel1.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.TextureEnabled = true;
                    effect.Texture = myTexture;
                    effect.World = transforms[mesh.ParentBone.Index] * worldMatrix;
                    effect.View = viewMatrix;
                    effect.Projection = myCamera.projectionMatrix;
                }
                // Draw the mesh, using the effects set above.
                GraphicsDevice.RenderState.DepthBufferEnable = true;
                mesh.Draw();
            }
            //base.Draw(gameTime);
        }

        public void DrawModel (CameraNew myCamera,Model shipModel,Matrix worldMatrix, Color shipColor, bool isEdit)
        {
            Matrix[] transforms = new Matrix[shipModel.Bones.Count];
            shipModel.CopyAbsoluteBoneTransformsTo(transforms);
            //GraphicsDevice.RenderState.FillMode = FillMode.WireFrame;
            // Draw the model. A model can have multiple meshes, so loop.
            foreach (ModelMesh mesh in shipModel.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();
                    Color mColor = Color.Sienna;                       
                    //if (isEdit)
                    //    effect.EmissiveColor = shipColor.ToVector3();
                    effect.AmbientLightColor = Color.White.ToVector3();
                    //effect.EmissiveColor = Color.White.ToVector3();
                    effect.World = transforms[mesh.ParentBone.Index] * worldMatrix;
                    effect.View = myCamera.viewMatrix;
                    effect.Projection = myCamera.projectionMatrix;
                }
                // Draw the mesh, using the effects set above.
                GraphicsDevice.RenderState.DepthBufferEnable = true; 
                mesh.Draw();
            }
            //base.Draw(gameTime);
        }
    }
}