using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;


namespace SaturnIV
{
    public class DrawQuadClass
    {
        
        public void DrawQuad(VertexDeclaration quadVertexDecl, BasicEffect quadEffect, Matrix View, Matrix Projection, Quad quad, Texture2D texture,Vector3 pos)
        {
            Game1.graphics.GraphicsDevice.RenderState.DepthBufferEnable = true;
            quadEffect.EnableDefaultLighting();            
            quadEffect.World = Matrix.CreateRotationZ(MathHelper.ToRadians(-90)) * Matrix.CreateWorld(pos, Vector3.Forward, Vector3.Up);
            quadEffect.View = View;
            quadEffect.Projection = Projection;
            quadEffect.TextureEnabled = true;
            quadEffect.Texture = texture;

            Game1.graphics.GraphicsDevice.VertexDeclaration = quadVertexDecl;
            quadEffect.Begin();
            foreach (EffectPass pass in quadEffect.CurrentTechnique.Passes)
            {
                pass.Begin();
                Game1.graphics.GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionNormalTexture>(
                    PrimitiveType.TriangleList, quad.Vertices, 0, 4, quad.Indexes, 0, 2);

                pass.End();
            }
            quadEffect.End();
        }

    }
}