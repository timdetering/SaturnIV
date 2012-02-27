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
    public static class BoundingSphereRenderer
    {
        static VertexBuffer vertBuffer;
        static VertexDeclaration vertDecl;
        static BasicEffect effect;
        static int sphereResolution;

        /// <summary>
        /// Initializes the graphics objects for rendering the spheres. If this method isn't
        /// run manually, it will be called the first time you render a sphere.
        /// </summary>
        /// <param name="graphicsDevice">The graphics device to use when rendering.</param>
        /// <param name="sphereResolution">The number of line segments 
        ///     to use for each of the three circles.</param>
        public static void InitializeGraphics(GraphicsDevice graphicsDevice, int sphereResolution, Color mColor)
        {
            BoundingSphereRenderer.sphereResolution = sphereResolution;
            //Color mColor = new Color(new Vector4(0f, 1f, 0f, 1.0f));
            graphicsDevice.RenderState.FillMode = FillMode.Solid;
            vertDecl = new VertexDeclaration(graphicsDevice, VertexPositionColor.VertexElements);
            effect = new BasicEffect(graphicsDevice, null);
            effect.LightingEnabled = false;
            effect.VertexColorEnabled = true;

            VertexPositionColor[] verts = new VertexPositionColor[(sphereResolution + 1) * 3];

            int index = 0;

            float step = MathHelper.TwoPi / (float)sphereResolution;

            //create the loop on the XY plane first
            for (float a = 0f; a <= MathHelper.TwoPi; a += step)
            {
                verts[index++] = new VertexPositionColor(
                    new Vector3((float)Math.Cos(a), (float)Math.Sin(a), 0f),
                    mColor);
            }

            //next on the XZ plane
            for (float a = 0f; a <= MathHelper.TwoPi; a += step)
            {
                verts[index++] = new VertexPositionColor(
                    new Vector3((float)Math.Cos(a), 0f, (float)Math.Sin(a)),
                    mColor);
            }

            //finally on the YZ plane
            for (float a = 0f; a <= MathHelper.TwoPi; a += step)
            {
                verts[index++] = new VertexPositionColor(
                    new Vector3(0f, (float)Math.Cos(a), (float)Math.Sin(a)),
                    mColor);
            }

            vertBuffer = new VertexBuffer(
                graphicsDevice,
                verts.Length * VertexPositionColor.SizeInBytes,
                BufferUsage.None);
            vertBuffer.SetData(verts);
        }

        /// <summary>
        /// Renders a bounding sphere using a single color for all three axis.
        /// </summary>
        /// <param name="sphere">The sphere to render.</param>
        /// <param name="graphicsDevice">The graphics device to use when rendering.</param>
        /// <param name="view">The current view matrix.</param>
        /// <param name="projection">The current projection matrix.</param>
        /// <param name="color">The color to use for rendering the circles.</param>
        public static void Render(
            BoundingSphere sphere,
            GraphicsDevice graphicsDevice,
            Matrix view,
            Matrix projection,
            Color color)
        {
            if (vertBuffer == null)
                InitializeGraphics(graphicsDevice, 30,color);
            graphicsDevice.VertexDeclaration = vertDecl;
            graphicsDevice.Vertices[0].SetSource(
                  vertBuffer,
                  0,
                  VertexPositionColor.SizeInBytes);

            effect.World =
                  Matrix.CreateScale(sphere.Radius) *
                  Matrix.CreateTranslation(sphere.Center);
            effect.View = view;
            effect.Projection = projection;
            effect.DiffuseColor = color.ToVector3();

            effect.Begin();
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Begin();

                //render each circle individually
                graphicsDevice.DrawPrimitives(
                      PrimitiveType.LineStrip,
                      0,
                      sphereResolution);
                graphicsDevice.DrawPrimitives(
                      PrimitiveType.LineStrip,
                      sphereResolution + 1,
                      sphereResolution);
                graphicsDevice.DrawPrimitives(
                      PrimitiveType.LineStrip,
                      (sphereResolution + 1) * 2,
                      sphereResolution);

                pass.End();
            }
            effect.End();
        }

        public static void Render3dCircle(
            Vector3 center, float radius,
            GraphicsDevice graphicsDevice,
            Matrix view,
            Matrix projection,
            Color color)
        {
            if (vertBuffer == null)
                InitializeGraphics(graphicsDevice, 30, color);

           graphicsDevice.RenderState.PointSize = 1.0f;
            graphicsDevice.VertexDeclaration = vertDecl;
            graphicsDevice.Vertices[0].SetSource(
                  vertBuffer,
                  0,
                  VertexPositionColor.SizeInBytes);
            effect.World =
                  Matrix.CreateScale(radius) *
                  Matrix.CreateTranslation(center);
            effect.VertexColorEnabled = true;
            effect.LightingEnabled = false;
            effect.View = view;
            effect.Projection = projection;
            effect.DiffuseColor = color.ToVector3();
            effect.AmbientLightColor = color.ToVector3();

            effect.Begin();
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Begin();
                graphicsDevice.DrawPrimitives(
                      PrimitiveType.LineStrip,
                      sphereResolution + 1,
                      sphereResolution);
                pass.End();
            }
            effect.End();

        }

    }
}