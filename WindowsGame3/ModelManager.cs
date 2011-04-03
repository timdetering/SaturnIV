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
        public Matrix projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(25.0f), 1.0f / 1.0f, .5f, 500f);
        public Texture2D modelTexture;
        public Vector3 screenCords = Vector3.Zero;
        Effect effect;

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

        public void loadModelCustomEffects(String myModelFile, string  myEffects)
        {
            effect = Game.Content.Load<Effect>(myEffects);
            myModel = Game.Content.Load<Model>(myModelFile);
            //aspectRatio = Game.GraphicsDevice.Viewport.AspectRatio;
            foreach (ModelMesh mesh in myModel.Meshes)
                foreach (ModelMeshPart meshPart in mesh.MeshParts)
                    meshPart.Effect = effect.Clone(Game.GraphicsDevice);
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
        public Vector3 get2dCoords(Vector3 modelPos, Camera ourCamera)
        {
            return Game.GraphicsDevice.Viewport.Project(modelPos, ourCamera.projectionMatrix,
                    ourCamera.viewMatrix, Matrix.Identity);
        }

        public void DrawModelWithTexture(Matrix worldMatrix, Camera myCamera, Texture2D myTexture)
        {

           // worldMatrix = Matrix.CreateScale(modelScale) * modelRotation * Matrix.CreateTranslation(modelPosition);
            Matrix[] targetTransforms = new Matrix[myModel.Bones.Count];
            myModel.CopyAbsoluteBoneTransformsTo(targetTransforms);
            foreach (ModelMesh mesh in myModel.Meshes)
            {
                foreach (Effect currentEffect in mesh.Effects)
                {
                    currentEffect.CurrentTechnique = currentEffect.Techniques["Textured"];
                    currentEffect.Parameters["xWorld"].SetValue(targetTransforms[mesh.ParentBone.Index] * worldMatrix);
                    currentEffect.Parameters["xView"].SetValue(myCamera.viewMatrix);
                    currentEffect.Parameters["xProjection"].SetValue(myCamera.projectionMatrix);
                    currentEffect.Parameters["xEnableLighting"].SetValue(true);
                    //currentEffect.Parameters["xLightDirection"].SetValue(modellightDirection);
                    currentEffect.Parameters["xAmbient"].SetValue(1.5f);
                    //currentEffect.Parameters["xTexture"].SetValue(myTexture);
                }
                mesh.Draw();
            }
            //base.Draw(gameTime);
        }

        public void DrawModel (Camera myCamera,Model shipModel,Matrix worldMatrix)
        {
            Matrix[] transforms = new Matrix[shipModel.Bones.Count];
            shipModel.CopyAbsoluteBoneTransformsTo(transforms);

            // Draw the model. A model can have multiple meshes, so loop.
            foreach (ModelMesh mesh in shipModel.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();
                    effect.DirectionalLight0.DiffuseColor = new Vector3(0.5f, 0.5f, 0.5f); // a red light
                    //effect.AmbientLightColor = Color.Green.ToVector3();
                    //effect.DirectionalLight0.Direction = modelRotation.Forward;  // coming along the x-axis
                    effect.DirectionalLight0.SpecularColor = new Vector3(1, 1, 1); // with green highlights
                    //effect.AmbientLightColor = Color.White.ToVector3();
                    effect.World = transforms[mesh.ParentBone.Index] * worldMatrix;
                    effect.View = myCamera.viewMatrix;
                    effect.Projection = myCamera.projectionMatrix;
                    Vector3 lightPos = new Vector3(5000,15000,0);
                    //effect.CurrentTechnique = effect.Techniques["Complete"];
                    //effect.Parameters["light1Pos"].SetValue(lightPos);
                    //effect.Parameters["light1Dir"].SetValue(lightPos);
                    //effect.Parameters["wvp"].SetValue(w * myCamera.viewMatrix * myCamera.projectionMatrix);
                    //effect.Parameters["worldI"].SetValue(Matrix.Invert(w));
                    //effect.Parameters["worldIT"].SetValue(Matrix.Transpose(Matrix.Invert(w)));                   
                    //effect.Parameters["viewInv"].SetValue(Matrix.Invert(viewMatrix));
                    //effect.Parameters["world"].SetValue(w);
                }
                // Draw the mesh, using the effects set above.
                GraphicsDevice.RenderState.DepthBufferEnable = true; 
                mesh.Draw();
            }
            //base.Draw(gameTime);
        }

        /// <summary>
        ///     Draws the with custom effect.
        /// </summary>
        /// <param name = "model">The model.</param>
        /// <param name = "world">The world.</param>
        /// <param name = "viewMatrix">The view matrix.</param>
        /// <param name = "projectionMatrix">The projection matrix.</param>
        /// <param name = "offset">The offset.</param>
        public void DrawWithCustomEffect(Model model,
                                                Matrix world,
                                                Matrix viewMatrix,
                                                Matrix projectionMatrix,
                                                Vector3 offset)
        {
            var transforms = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(transforms);

            //var translation = Matrix.CreateTranslation(offset);
            foreach (var mesh in model.Meshes)
            {
                foreach (var effect in mesh.Effects)
                {
                    var w = transforms[mesh.ParentBone.Index] * world;// * translation;

                    effect.CurrentTechnique = effect.Techniques["Complete"];
                    effect.Parameters["wvp"].SetValue(w * viewMatrix * projectionMatrix);
                    effect.Parameters["worldI"].SetValue(Matrix.Invert(w));

                    //var s = effect.Parameters.GetParameterBySemantic("worldIT");
                    //if (s!= null)
                    //    s.SetValue(Matrix.Transpose(Matrix.Invert(w)));

                    effect.Parameters["worldIT"].SetValue(Matrix.Transpose(Matrix.Invert(w)));
                    //s = effect.Parameters.GetParameterBySemantic("viewInv");
                    //if (s != null)
                    //    s.SetValue(Matrix.Transpose(Matrix.Invert(camera.View)));

                    effect.Parameters["viewInv"].SetValue(Matrix.Invert(viewMatrix));
                    //s = effect.Parameters.GetParameterBySemantic("world");
                    //if (s != null)
                    //    s.SetValue(w);

                    effect.Parameters["world"].SetValue(w);

                    foreach (var pass in effect.CurrentTechnique.Passes)
                    {
                        //pass.Apply();
                        mesh.Draw();
                    }
                }
            }
            //_renderedModelCount = RenderedModelCount + 1;
        }
    }
}