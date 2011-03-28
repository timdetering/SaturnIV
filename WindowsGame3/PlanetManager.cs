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
    public class PlanetManager : ModelManager
    {
        public int planetTypeIndex;
        public shipTypes planetData;
        public PlanetManager tempData;

        //Space Object Variables
        Matrix rotationMatrix = Matrix.Identity;
        int numberOfPlanets = 10;
        int planetXBoundry = 200;
        int planetYBoundry = 200;
        int planetZBoundry = 200;
        Vector3 rotationAmount = new Vector3();
        int planetRadiusBoundry = 5000;
        int planetRadius;
        public List<ModelManager> planetList = new List<ModelManager>();
        public Texture2D[] planetTextureArray;

        public PlanetManager(Game game)
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
            //planetData.Description = shipTypes.ShipModelFileName[planetTypeIndex];
            //loadModel(planetData.Description, "effects");
            base.Initialize();
        }

        public void loadPlanetTextures()
        {
            planetTextureArray = new Texture2D[5];
            planetTextureArray[0] = Game.Content.Load<Texture2D>("textures/saturn");
            planetTextureArray[1] = Game.Content.Load<Texture2D>("textures/planettexture2");
            planetTextureArray[2] = Game.Content.Load<Texture2D>("textures/planettexture3");

            //planetTextureArray[2] = Game.Content.Load<Texture2D>("earthplanettexture");
        }

        public void generatSpaceObjects(int numberOfPlanets)
        {
            Random Position = new Random();
            loadModelCustomEffects("Models/sphere", "effects");

            loadPlanetTextures();
            tempData = new PlanetManager(Game);
            double tX, tY, tZ, w, t;
            for (int i = 0; i < numberOfPlanets; i++)
            {
                tZ = 2.0 * Position.NextDouble() - 1.0; 
                t = 2.0 * MathHelper.Pi * Position.NextDouble(); 
                w = Math.Sqrt(1 - tZ * tZ); 
                tX = w * Math.Cos(t); 
                tY = w * Math.Sin(t); 
                tempData = new PlanetManager(Game);
                int tTextureIndex = Position.Next(2);
                tempData.planetRadius = Position.Next(200,planetRadiusBoundry);
                tempData.modelTexture = planetTextureArray[0];
                tempData.modelPosition = new Vector3(0,0,1);
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

        public void UpdatePlanetRotation()
        {
            
            foreach (PlanetManager planet in planetList)
            {
                rotationAmount.Y = rotationAmount.Y + 1.0f;
                rotationMatrix =
                Matrix.CreateFromAxisAngle(Right, rotationAmount.Y) *
                Matrix.CreateFromAxisAngle(Up, rotationAmount.X);
                rotationMatrix.Up += new Vector3(0, 10, 0);
                planet.modelRotation *= rotationMatrix;
            }
        }

        public void DrawPlanets(GameTime gameTime, Matrix viewMatrix, Matrix projectionMatrix)
        {
            foreach (PlanetManager planet in planetList)
            {
                float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
                Matrix worldMatrix = Matrix.CreateScale(planet.planetRadius) * 
                                     Matrix.CreateTranslation(planet.modelPosition);
                Matrix[] targetTransforms = new Matrix[myModel.Bones.Count];
                myModel.CopyAbsoluteBoneTransformsTo(targetTransforms);
                foreach (ModelMesh mesh in myModel.Meshes)
                {
                    foreach (Effect currentEffect in mesh.Effects)
                    {
                        currentEffect.CurrentTechnique = currentEffect.Techniques["Textured"];
                        currentEffect.Parameters["xWorld"].SetValue(targetTransforms[mesh.ParentBone.Index] * worldMatrix);
                        currentEffect.Parameters["xView"].SetValue(viewMatrix);
                        currentEffect.Parameters["xProjection"].SetValue(projectionMatrix);
                        currentEffect.Parameters["xEnableLighting"].SetValue(true);
                        currentEffect.Parameters["xLightDirection"].SetValue(modellightDirection);
                        currentEffect.Parameters["xAmbient"].SetValue(0.5f);
                        currentEffect.Parameters["xTexture"].SetValue(planet.modelTexture);
                    }
                    mesh.Draw();
                }
                //base.Draw(gameTime);
            }
        }
    }
}