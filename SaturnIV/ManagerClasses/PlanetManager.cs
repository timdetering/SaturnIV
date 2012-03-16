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
    public class PlanetManager : ModelManager
    {
        public int planetTypeIndex;
        public PlanetManager tempData;

        //Space Object Variables
        Matrix rotationMatrix = Matrix.Identity;
        public static List<planetStruct> planetList = new List<planetStruct>();
        public Texture2D[] planetTextureArray;
        public Line3D line;
        public static BoundingSphere planetBS;
        public PlanetManager(Game game)
            : base(game)
        {
            // TODO: Construct any child components here
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public void Initialize()
        {
            loadPlanetTextures();
            base.Initialize();
        }

        public void loadPlanetTextures()
        {
            planetTextureArray = new Texture2D[5];
            planetTextureArray[0] = Game.Content.Load<Texture2D>("textures/planettexture1");
            planetTextureArray[1] = Game.Content.Load<Texture2D>("textures/planettexture2");
            planetTextureArray[2] = Game.Content.Load<Texture2D>("textures/planettexture3");

            //planetTextureArray[2] = Game.Content.Load<Texture2D>("earthplanettexture");
        }
        public void generatSpaceObjects(int numberOfPlanets)
        {
            planetBS = new BoundingSphere(Vector3.Zero, 2000);

            Random Position = new Random();
            loadPlanetTextures();
            double tX, tY, tZ, w, t;
            for (int i = 0; i < numberOfPlanets; i++)
            {
                tZ = 2.0 * Position.NextDouble() - 1.0;
                t = 2.0 * MathHelper.Pi * Position.NextDouble();
                w = Math.Sqrt(1 - tZ * tZ);
                tX = w * Math.Cos(t);
                tY = 0;//w * Math.Sin(t); 
                planetStruct tempData = new planetStruct();
                //int tTextureIndex = 1;
                tempData.planetModel = LoadModel("Models/planet");
                tempData.planetRadius = 225; // Position.Next(100, planetRadiusBoundry);
                tempData.planetPosition = HelperClass.RandomPosition(-90000, 90000);
                tempData.planetPosition.Y = -200000;
                tempData.planetTexture = planetTextureArray[1];
                planetList.Add(tempData);
            }
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            // TODO: Add your update code here
            //UpdatePlanetRotation();
            base.Update(gameTime);
        }

        public void DrawPlanets(GameTime gameTime, Matrix viewMatrix, Matrix projectionMatrix, CameraNew ourCamera)
        {
            foreach (planetStruct planet in planetList)
            {
               //BoundingSphereRenderer.Render(planetBS, Game.GraphicsDevice, viewMatrix, projectionMatrix, Color.Yellow);
                Matrix worldMatrix = Matrix.CreateScale(planet.planetRadius) * Matrix.CreateTranslation(planet.planetPosition);
                Matrix[] transforms = new Matrix[planet.planetModel.Bones.Count];
                planet.planetModel.CopyAbsoluteBoneTransformsTo(transforms);

                // Draw the model. A model can have multiple meshes, so loop.
                foreach (ModelMesh mesh in planet.planetModel.Meshes)
                {
                    foreach (BasicEffect effect in mesh.Effects)
                    {
                        effect.TextureEnabled = true;
                        effect.Texture = planet.planetTexture;
                        effect.World = transforms[mesh.ParentBone.Index] * worldMatrix;
                        effect.View = viewMatrix;
                        effect.Projection = projectionMatrix;
                    }
                    // Draw the mesh, using the effects set above.
                    GraphicsDevice.RenderState.DepthBufferEnable = true;
                    mesh.Draw();
                }
            }
        }

        /// <summary>
        /// Translates a point around an origin
        /// </summary>
        /// <param name="point">Point that is going to be translated</param>
        /// <param name="originPoint">Origin of rotation</param>
        /// <param name="rotationAxis">Axis to rotate around, this Vector should be a unit vector (normalized)</param>
        /// <param name="radiansToRotate">Radians to rotate</param>
        /// <returns>Translated point</returns>
        public Vector3 RotateAroundPoint(Vector3 point, Vector3 originPoint, Vector3 rotationAxis, float radiansToRotate)
        {
            Vector3 diffVect = point - originPoint;

            Vector3 rotatedVect = Vector3.Transform(diffVect, Matrix.CreateFromAxisAngle(rotationAxis, radiansToRotate));

            rotatedVect += originPoint;

            return rotatedVect;
        }
    }
}