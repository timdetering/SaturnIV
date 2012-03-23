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
    public struct VertexExplosion
    {
        public Vector3 Position;
        public Vector4 TexCoord;
        public Vector4 AdditionalInfo;
        public VertexExplosion(Vector3 Position, Vector4 TexCoord, Vector4 AdditionalInfo)
        {
            this.Position = Position;
            this.TexCoord = TexCoord;
            this.AdditionalInfo = AdditionalInfo;
        }
        public static readonly VertexElement[] VertexElements = new VertexElement[]
            {
                new VertexElement(0, 0, VertexElementFormat.Vector3, VertexElementMethod.Default, VertexElementUsage.Position, 0),
                new VertexElement(0, 12, VertexElementFormat.Vector4, VertexElementMethod.Default, VertexElementUsage.TextureCoordinate, 0),
                new VertexElement(0, 28, VertexElementFormat.Vector4, VertexElementMethod.Default, VertexElementUsage.TextureCoordinate, 1),
            };
        public static readonly int SizeInBytes = sizeof(float) * (3 + 4 + 4);
    }

    public class ExplosionClass
    {
        Texture2D myTexture;
        public VertexExplosion[] explosionVertices;
        VertexDeclaration myVertexDeclaration;
        Effect expEffect;
        float time = 0;
        Random rand;
        public bool isAlive = true;
        public List<VertexExplosion[]> expList;

        public void initExplosionClass(Game game)
        {
             expList = new List<VertexExplosion[]>();
            myTexture = game.Content.Load<Texture2D>("textures//explosion");
            expEffect = game.Content.Load<Effect>("Effects//explosionEffect");
            myVertexDeclaration = new VertexDeclaration(game.GraphicsDevice, VertexExplosion.VertexElements);
        }

        public void CreateExplosionVertices(float time, Vector3 expPosition,float eSize)
        {
            int particles = 30;
            explosionVertices = new VertexExplosion[particles * 6];
            rand = new Random();
            int i = 0;
            for (int partnr = 0; partnr < particles; partnr++)
            {
                Vector3 startingPos = expPosition;

                float z = (float)rand.NextDouble();
                float f = (float)Math.Sqrt(1.0 - z * z);
                float azimuth = (float)rand.Next(0, (int)MathHelper.TwoPi);

                Vector3 moveDirection = new Vector3((float)Math.Cos(azimuth) * f, (float)Math.Sin(azimuth) * f, z);
                //Vector3 moveDirection = new Vector3(r1, r2, r3);
                moveDirection.Normalize();

                float r4 = (float)rand.NextDouble();
                //r4 = r4 / 4.0f * 3.0f + 0.25f;
                r4 = eSize*25;
                explosionVertices[i++] = new VertexExplosion(startingPos, new Vector4(1, 1, time, 250), new Vector4(moveDirection, r4));
                explosionVertices[i++] = new VertexExplosion(startingPos, new Vector4(0, 0, time, 250), new Vector4(moveDirection, r4));
                explosionVertices[i++] = new VertexExplosion(startingPos, new Vector4(1, 0, time, 250), new Vector4(moveDirection, r4));

                explosionVertices[i++] = new VertexExplosion(startingPos, new Vector4(1, 1, time, 250), new Vector4(moveDirection, r4));
                explosionVertices[i++] = new VertexExplosion(startingPos, new Vector4(0, 1, time, 250), new Vector4(moveDirection, r4));
                explosionVertices[i++] = new VertexExplosion(startingPos, new Vector4(0, 0, time, 250), new Vector4(moveDirection, r4));
            }

            expList.Add(explosionVertices);
        }

        public void DrawExp(GameTime gameTime, CameraNew myCamera, GraphicsDevice device)
        {
            Matrix worldMatrix = Matrix.Identity;

                //draw billboards
                expEffect.CurrentTechnique = expEffect.Techniques["Explosion"];
                expEffect.Parameters["xWorld"].SetValue(worldMatrix);
                expEffect.Parameters["xProjection"].SetValue(myCamera.projectionMatrix);
                expEffect.Parameters["xView"].SetValue(myCamera.viewMatrix);

                expEffect.Parameters["xCamPos"].SetValue(CameraNew.position);
                expEffect.Parameters["xExplosionTexture"].SetValue(myTexture);
                expEffect.Parameters["xCamUp"].SetValue(Vector3.Up);
                expEffect.Parameters["xTime"].SetValue((float)gameTime.TotalGameTime.TotalMilliseconds);

                device.RenderState.AlphaBlendEnable = true;
                device.RenderState.SourceBlend = Blend.SourceAlpha;
                device.RenderState.DestinationBlend = Blend.One;
                device.RenderState.DepthBufferWriteEnable = false;


                foreach (VertexExplosion[] myVertexEx in expList)
                {
                    expEffect.Begin();
                    foreach (EffectPass pass in expEffect.CurrentTechnique.Passes)
                    {
                        pass.Begin();
                        device.VertexDeclaration = myVertexDeclaration;
                        device.DrawUserPrimitives<VertexExplosion>(PrimitiveType.TriangleList, myVertexEx, 0, myVertexEx.Length / 3);
                        pass.End();
                    }
                    expEffect.End();
                }         
        }
    }
}
