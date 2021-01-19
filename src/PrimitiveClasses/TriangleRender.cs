using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace SaturnIV
{
    public class renderTriangle
    {
        VertexDeclaration vertDecl;
        VertexPositionColor[] PointList;
        VertexBuffer vertexBuffer;
        static BasicEffect effect;
        VertexPositionColor[] vertices;

        private void SetUpVertices(Vector3 pos1, Vector3 pos2, Vector3 pos3, Color color)
        {
            vertices = new VertexPositionColor[3];
            Vector3 color4 = color.ToVector3();
            vertices[0].Position = pos1;
            vertices[0].Color = new Color(color4);
            vertices[1].Position = pos2;
            vertices[0].Color = new Color(color4);
            vertices[2].Position = pos3;
            vertices[0].Color = new Color(color4);
        }

        public void Render(
            GraphicsDevice device,
            Matrix view,
            Matrix projection,
            Color color,
            Vector3 pos1, Vector3 pos2, Vector3 pos3
            )
        {

            SetUpVertices(pos1, pos2, pos3, color);
            if (effect == null)
            {
                effect = new BasicEffect(device, null);
                effect.VertexColorEnabled = true;
                //effect.LightingEnabled = true;
                vertDecl = new VertexDeclaration(device, VertexPositionColor.VertexElements);
            }
            device.RenderState.CullMode = CullMode.None;
            device.RenderState.DepthBufferEnable = true;
            device.RenderState.AlphaBlendEnable = false;
            device.VertexDeclaration = vertDecl;

            effect.View = view;
            effect.Projection = projection;
            effect.Begin();

            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Begin();
                device.DrawUserPrimitives(PrimitiveType.TriangleList, vertices, 0, 1);
                pass.End();
            }
            effect.End();

        }
    }
}