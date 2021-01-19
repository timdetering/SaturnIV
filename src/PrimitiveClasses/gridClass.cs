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

    public class Grid
    {
        public int Width;
        public int Height;
        public int Rows;
        public int Columns;
        public Vector3 Position;
        List<VertexPositionColor> vertices;
        BasicEffect effect;
        Game game;

        public Grid(Vector3 Position, int Width, int Height, int Rows, int Columns, Game game)
        {
            this.game = game;
            Color gridColor = Color.Red;
            effect = new BasicEffect(game.GraphicsDevice, null);
            effect.VertexColorEnabled = true;
            this.Width = Width;
            this.Height = Height;
            this.Rows = Rows;
            this.Columns = Columns;
            this.Position = Position;
            vertices = new List<VertexPositionColor>();
            int xDiff = this.Width / this.Columns;
            int zDiff = this.Height / this.Rows;
            float xBase = this.Position.X - this.Width / 2f;
            float zBase = this.Position.Z - this.Height / 2f;
            float yBase = this.Position.Y;
            for (int i = 0; i <= this.Rows; i++)
            {
                vertices.Add(new VertexPositionColor(new Vector3(xBase + i * xDiff, yBase, zBase), gridColor)); vertices.Add(new VertexPositionColor(new Vector3(xBase + i * xDiff, yBase, zBase + this.Height), gridColor));
            }
            for (int i = 0; i <= this.Columns; i++)
            {
                vertices.Add(new VertexPositionColor(new Vector3(xBase, yBase, zBase + i * zDiff), gridColor));
                vertices.Add(new VertexPositionColor(new Vector3(xBase + this.Width, yBase, zBase + i * zDiff), gridColor));
            }
        }

        public void Draw(CameraNew Camera)
        {
            game.GraphicsDevice.VertexDeclaration = new VertexDeclaration(game.GraphicsDevice, VertexPositionColor.VertexElements);
            effect.Begin();
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Begin();
                effect.View = Camera.viewMatrix;
                effect.Projection = Camera.projectionMatrix;
                //Draw vertices as Primitive                  
                pass.End();
            }
            effect.End();
            game.GraphicsDevice.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.LineList, vertices.ToArray(), 0, vertices.Count / 2);
        }
    }
} 
	 