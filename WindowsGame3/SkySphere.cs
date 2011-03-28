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
    public class SkySphere : Microsoft.Xna.Framework.GameComponent
    {
        Model SkySphereModel;
        Effect SkySphereEffect;
        TextureCube SkyboxTexture;


        public SkySphere(Game game)
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

        public void LoadSkySphere(Game game)
        {

            //TextureCube SkyboxTexture =
            //    game.Content.Load<TextureCube>("cloudMap");
            SkyboxTexture = game.Content.Load<TextureCube>("textures/Sky128");
            SkySphereEffect = game.Content.Load<Effect>("Effects/SkySphere");
            SkySphereModel = game.Content.Load<Model>("Models/largeSphere"); 
            SkySphereModel.Meshes[0].MeshParts[0].Effect = SkySphereEffect.Clone(game.GraphicsDevice);
            SkySphereEffect.Parameters["SkyboxTexture"].SetValue(SkyboxTexture);
            
            // Set the Skysphere Effect to each part of the Skysphere model
            foreach (ModelMesh mesh in SkySphereModel.Meshes)
            {
                foreach (ModelMeshPart part in mesh.MeshParts)
                {
                    part.Effect = SkySphereEffect;
                }
            }
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

        public void DrawSkySphere(Game game, Camera ourCamera)
        {
             game.GraphicsDevice.Clear(Color.CornflowerBlue);
             SkySphereEffect.Parameters["ViewMatrix"].SetValue(ourCamera.viewMatrix);
             SkySphereEffect.Parameters["ProjectionMatrix"].SetValue(ourCamera.projectionMatrix);

            //Draw the model, a model can have multiple meshes, so loop
            foreach (ModelMesh mesh in SkySphereModel.Meshes)
            {
                //Draw the mesh, will use the effects set above.
                mesh.Draw();
            }
            game.GraphicsDevice.RenderState.CullMode = CullMode.CullCounterClockwiseFace;
            game.GraphicsDevice.RenderState.DepthBufferWriteEnable = true;

        }


        
    }
}