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
    public class Quad
    {
        VertexDeclaration quadVertexDecl;
        public List<Quad> beamList;
        public Vector3 Origin;
        public Vector3 UpperLeft;
        public Vector3 LowerLeft;
        public Vector3 UpperRight;
        public Vector3 LowerRight;
        public Vector3 Normal;
        public Vector3 Up;
        public Vector3 Left;

        public VertexPositionColor[] Vertices;
        public int[] Indices;

        Texture2D texture;
    BasicEffect quadEffect;

    public void BeamClassInit(ContentManager Content)
    {
            texture = Content.Load<Texture2D>("textures//redtexture");
            quadEffect = new BasicEffect( Game1.graphics.GraphicsDevice, null );
            quadEffect.EnableDefaultLighting();
            quadEffect.TextureEnabled = false;
            quadVertexDecl = new VertexDeclaration(Game1.graphics.GraphicsDevice,
            VertexPositionColor.VertexElements);
    }

        public Quad(ContentManager Content,Vector3 origin, Vector3 normal,
            Vector3 up, float width, float height, Color color)
        {
            BeamClassInit(Content);
            Vertices = new VertexPositionColor[4];
            Indices = new int[6];
            Origin = origin;
            Normal = normal;
            Up = -up;
            // Calculate the quad corners
            Left = Vector3.Cross(normal, Up);
            Vector3 uppercenter = (Up * height / 2) + origin;
            UpperLeft = uppercenter + (Left * width / 2);
            UpperRight = uppercenter - (Left * width / 2);
            LowerLeft = UpperLeft - (Up * height);
            LowerRight = UpperRight - (Up * height);

            FillVertices(color);
        }

        private void FillVertices(Color color)
        {
            // Provide a normal for each vertex
            for (int i = 0; i < Vertices.Length; i++)
            {
               // Vertices[i].Normal = Normal;
            }
            color = Color.White;
            // Set the position and texture coordinate for each
            // vertex
            Vertices[0].Position = LowerLeft;
            Vertices[0].Color = color;
            //Vertices[0].TextureCoordinate = textureLowerLeft;
            Vertices[1].Position = UpperLeft;
            Vertices[1].Color = color;
            //Vertices[1].TextureCoordinate = textureUpperLeft;
            Vertices[2].Position = LowerRight;
            Vertices[2].Color = color;
            //Vertices[2].TextureCoordinate = textureLowerRight;
            Vertices[3].Position = UpperRight;
            Vertices[3].Color = color;
            //Vertices[3].TextureCoordinate = textureUpperRight;

            // Set the index buffer for each vertex, using
            // clockwise winding
            Indices[0] = 0;
            Indices[1] = 1;
            Indices[2] = 2;
            Indices[3] = 2;
            Indices[4] = 1;
            Indices[5] = 3;
        }

        public void DrawQuad(Matrix world,Matrix View, Matrix Projection,Quad quad)
        {
            Game1.graphics.GraphicsDevice.RenderState.DepthBufferEnable = true;
            quadEffect.World = world;
            quadEffect.View = View;
            quadEffect.Projection = Projection;
            //quadEffect.EnableDefaultLighting();

            Game1.graphics.GraphicsDevice.VertexDeclaration = quadVertexDecl;
            quadEffect.Begin();
            foreach (EffectPass pass in quadEffect.CurrentTechnique.Passes)
            {
                pass.Begin();
                Game1.graphics.GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionColor>(
                    PrimitiveType.TriangleList, quad.Vertices, 0, 4, quad.Indices, 0, 2);

                pass.End();
            }
            quadEffect.End();
        }

    }
}