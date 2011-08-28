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
    public class RenderStarfield : Microsoft.Xna.Framework.GameComponent
    {

        Effect effect;
        Effect starEffects;
        RenderTarget2D cloudsRenderTarget;
        Texture2D cloudStaticMap;
        Texture2D cloudMap;
        VertexPositionTexture[] fullScreenVertices;
        VertexDeclaration fullScreenVertexDeclaration;
        Model SkySphereModel;
        VertexPositionColor[] starList;
        VertexDeclaration vertexPosColDecl;
        SpriteBatch spriteBatch;
        Texture2D blank;
       
        public RenderStarfield(Game game)
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

        public void LoadStarFieldAssets(Game game)
        {
            Random rand;
            rand = new Random();
            PresentationParameters pp = game.GraphicsDevice.PresentationParameters;
            effect = game.Content.Load<Effect>("effectslib");
            cloudsRenderTarget = new RenderTarget2D(game.GraphicsDevice, pp.BackBufferWidth, pp.BackBufferHeight, 
                                                    1, game.GraphicsDevice.DisplayMode.Format);

            fullScreenVertices = SetUpFullscreenVertices();
            fullScreenVertexDeclaration = new VertexDeclaration(game.GraphicsDevice, VertexPositionTexture.VertexElements);
            // create the SpriteBatch object
            SkySphereModel = game.Content.Load<Model>("Models/sphere");
            SkySphereModel.Meshes[0].MeshParts[0].Effect = effect.Clone(game.GraphicsDevice);
           
            //SkySphereEffect.Parameters["SkyboxTexture"].SetValue(
            //        cloudMap);
            // Set the Skysphere Effect to each part of the Skysphere model
            foreach (ModelMesh mesh in SkySphereModel.Meshes)
            {
                foreach (ModelMeshPart part in mesh.MeshParts)
                {
                    part.Effect = effect;
                }
            }

            spriteBatch = new SpriteBatch(game.GraphicsDevice);
            starEffects = game.Content.Load<Effect>("Effects/pointsprites");
            starEffects.Parameters["SpriteTexture"].SetValue(
                game.Content.Load<Texture2D>("textures/star"));
            //starList= new VertexPositionColor[30000];
            vertexPosColDecl = new VertexDeclaration(game.GraphicsDevice,
                VertexPositionColor.VertexElements);
            generateStarField(game, 2000);
        }

         private VertexPositionTexture[] SetUpFullscreenVertices()
        {
            VertexPositionTexture[] vertices = new VertexPositionTexture[4];

            vertices[0] = new VertexPositionTexture(new Vector3(-1, 1, 0f), new Vector2(0, 1));
            vertices[1] = new VertexPositionTexture(new Vector3(1, 1, 0f), new Vector2(1, 1));
            vertices[2] = new VertexPositionTexture(new Vector3(-1, -1, 0f), new Vector2(0, 0));
            vertices[3] = new VertexPositionTexture(new Vector3(1, -1, 0f), new Vector2(1, 0));

            return vertices;
        }

        private void generateStarField(Game game, int numberofStars)
        {
            Random rand = new Random();
            Color[] starColors;
            starColors = new Color[6];
            starColors[0] = Color.White;
            starColors[1] = Color.WhiteSmoke;
            starColors[2] = Color.Blue;
            starColors[3] = Color.LightGray;
            starColors[5] = Color.Red;

            starList = new VertexPositionColor[numberofStars];
            double x, y, z, w, t;
            for (int count = 0; count < numberofStars; count++)
            {
                z = 2.0 * rand.NextDouble() - 1.0;
                t = 2.0 * MathHelper.Pi * rand.NextDouble();
                w = Math.Sqrt(1 - z * z);
                x = w * Math.Cos(t);
                y = w * Math.Sin(t);
               starList[count] = new VertexPositionColor(new Vector3((float)x * rand.Next(-20000, 200000),
                                                        (float)y * rand.Next(-20000, 200000), (float)z * rand.Next(-20000, 200000)),
                                                         starColors[rand.Next(0, 5)]);
             }
        }

        public void DrawStars(Game game, Camera ourCamera)
        {
            game.GraphicsDevice.RenderState.PointSpriteEnable = true;
            game.GraphicsDevice.RenderState.PointSize = 2.0f;
            game.GraphicsDevice.RenderState.AlphaBlendEnable = true;
            game.GraphicsDevice.RenderState.SourceBlend = Blend.SourceAlpha;
            game.GraphicsDevice.RenderState.DestinationBlend = Blend.One;
            game.GraphicsDevice.RenderState.DepthBufferWriteEnable = false;

            game.GraphicsDevice.VertexDeclaration = vertexPosColDecl;
            Matrix WVPMatrix = Matrix.Identity *ourCamera.viewMatrix * ourCamera.projectionMatrix;
            starEffects.Parameters["WVPMatrix"].SetValue(WVPMatrix);

            starEffects.Begin();
            foreach (EffectPass pass in starEffects.CurrentTechnique.Passes)
            {
                pass.Begin();
                game.GraphicsDevice.DrawUserPrimitives
                    <VertexPositionColor>(
                        PrimitiveType.PointList,
                        starList,
                        0,
                        starList.Length);
                pass.End();
            }
            starEffects.End();

            game.GraphicsDevice.RenderState.PointSpriteEnable = false;
            game.GraphicsDevice.RenderState.DepthBufferWriteEnable = true;
            game.GraphicsDevice.RenderState.SourceBlend = Blend.SourceAlpha;
            game.GraphicsDevice.RenderState.DestinationBlend = Blend.InverseSourceAlpha;
        }

        public void DrawSkyDome(Game game, Camera ourCamera)
        {
            game.GraphicsDevice.RenderState.DepthBufferWriteEnable = false;

            Matrix[] modelTransforms = new Matrix[SkySphereModel.Bones.Count];
            SkySphereModel.CopyAbsoluteBoneTransformsTo(modelTransforms);
            Matrix wMatrix = Matrix.CreateTranslation(0, -0.3f, 5) * Matrix.CreateScale(5) *
                             Matrix.CreateTranslation(Camera.position);
            foreach (ModelMesh mesh in SkySphereModel.Meshes)
            {
                foreach (Effect currentEffect in mesh.Effects)
                {
                    Matrix worldMatrix = modelTransforms[mesh.ParentBone.Index] * wMatrix;
                    currentEffect.CurrentTechnique = currentEffect.Techniques["Textured"];
                    currentEffect.Parameters["xWorld"].SetValue(worldMatrix);
                    currentEffect.Parameters["xView"].SetValue(ourCamera.viewMatrix);
                    currentEffect.Parameters["xProjection"].SetValue(ourCamera.projectionMatrix);
                    currentEffect.Parameters["xTexture"].SetValue(cloudMap);
                    currentEffect.Parameters["xEnableLighting"].SetValue(false);
                }
                mesh.Draw();
            }
            game.GraphicsDevice.RenderState.DepthBufferWriteEnable = true;
        }

    }
}