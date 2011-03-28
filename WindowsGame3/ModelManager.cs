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
    public class ModelManager : Microsoft.Xna.Framework.DrawableGameComponent
    {
        public shipTypes NPCShipData;
        public Model myModel;
        public Vector3 modelPosition;
        public Vector3 modelVelocity = Vector3.Zero;
        public Matrix modelRotation = Matrix.Identity;// * Matrix.CreateRotationY(MathHelper.ToRadians(90));
        public Matrix worldMatrix = Matrix.Identity;
        public Matrix viewMatrix = Matrix.Identity;
        public Matrix projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(25.0f), 4.0f / 3.0f, .5f, 500f);
        public Vector3 modellightDirection = new Vector3(0, 0, 0);
        public Vector3 modelScale;
        public string modelFileName;
        public Texture2D modelTexture;
        public Matrix modelWorldMatrix;
        public Matrix modelScaleMatrix;
        public Matrix modelTranslationMatrix;
        public float objectAgility;
        public shipClasses.ClassesEnum objectClass;
        public Vector3 Direction = Vector3.Forward;
        public Vector3 Up = Vector3.Up;
        public Vector3 right;
        public float radius = 1.0f;
        public Vector3 Right
        {
            get { return right; }
        }
        public Vector3 Velocity = Vector3.Zero;
        public BoundingSphere modelBoundingSphere = new BoundingSphere();
        public BoundingFrustum modelFrustum = new BoundingFrustum(Matrix.Identity);
        public Vector3 screenCords = Vector3.Zero;
        public bool isVisable;
        public bool isSelected = false;
        public List<Vector3> shipWeaponPylons = new List<Vector3>();

        public string objectFilename;
        public float objectMass, objectThrust;
        public string objectDesc;
        public string objectName;
        public float objectArmorFactor;
        public weaponTypes.MissileType primaryWeaponIndex;
        public weaponTypes.MissileType secondaryWeaponIndex;
        public weaponTypes.MissileType currentWeaponIndex;
        public weaponTypes.MissileType[] weaponArray;
        public int missileCount;
        public int cMissileCount;
        public int weaponCnt;

        public float objectArmorLvl = 100;
        public int shipShieldHealth = 100;
        public float objectShieldLvl, objectShieldRegenTime;


        public Vector3[] turretArray;
        public int currentPylon = 1;
        public Vector3 targetPosition;
        public float distanceFromTarget;
        public ModelManager currentTargetObject;
        public disposition npcDisposition;
        public Vector3 destination = Vector3.Zero;
        public Vector3 vecToTarget = Vector3.Zero;
        public bool isFirstRun;
        Effect effect;

        public enum disposition
        {
            pursue = 0,
            patrol = 1,
            evade = 2,
            loiter = 3
        }

        // The aspect ratio determines how to scale 3d to 2d projection.
        public float aspectRatio;

        public ModelManager(Game game)
            : base(game)
        {
            // TODO: Construct any child components here
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public void Initialize(int shipTypesIndex)
        {
            NPCShipData = new shipTypes();
            isFirstRun = true;
            objectFilename = NPCShipData.ShipModelFileName[shipTypesIndex];
            objectMass = NPCShipData.mass[shipTypesIndex];
            objectThrust = NPCShipData.thrust[shipTypesIndex];
            modelScale = NPCShipData.scale[shipTypesIndex];
            objectDesc = NPCShipData.objectDesc[shipTypesIndex];
            objectAgility = NPCShipData.agility[shipTypesIndex];
            objectClass = NPCShipData.objectClass[shipTypesIndex];
            objectArmorFactor = NPCShipData.ShipArmorFactor[shipTypesIndex];
            objectArmorLvl = NPCShipData.ShipArmorLvl[shipTypesIndex];
            objectShieldLvl = NPCShipData.ShipShieldLvl[shipTypesIndex];
            objectShieldRegenTime = NPCShipData.ShipShieldRegenTime[shipTypesIndex];
            radius = NPCShipData.sphereRadius[shipTypesIndex];
            //Load Weapon information for ship
            weaponCnt = NPCShipData.shipWeapons[shipTypesIndex].Length;
            missileCount = NPCShipData.missileCount[shipTypesIndex];
            cMissileCount = missileCount;
            weaponArray = new weaponTypes.MissileType[weaponCnt];
            weaponArray = NPCShipData.shipWeapons[shipTypesIndex];
            primaryWeaponIndex = weaponArray[0];
            currentWeaponIndex = primaryWeaponIndex;
            //Load Weapon Firing Positions
            turretArray = new Vector3[NPCShipData.turretLocations[shipTypesIndex].Length];
            turretArray = NPCShipData.turretLocations[shipTypesIndex];
            LoadModel(objectFilename);
            base.Initialize();
        }


        protected override void LoadContent()
        {
            base.LoadContent();
        }

        public void updateModelBoundingSphere(Matrix worldMatrix)
        {
           // worldMatrix = modelRotation * Matrix.CreateTranslation(modelPosition);

            modelBoundingSphere = new BoundingSphere();
            foreach (ModelMesh mesh in myModel.Meshes)
            {
                modelBoundingSphere = BoundingSphere.CreateMerged(modelBoundingSphere, mesh.BoundingSphere);
            }
            
            modelBoundingSphere = new BoundingSphere(modelPosition, radius);
            modelBoundingSphere = modelBoundingSphere.Transform(worldMatrix);
        }

        public void updateModelBoundingSphere(Matrix worldMatrix, float extraScale)
        {
            //Matrix worldMatrix = Matrix.CreateScale(extraScale) * Matrix.CreateScale(modelScale) * modelRotation * Matrix.CreateTranslation(modelPosition);

            modelBoundingSphere = new BoundingSphere(modelPosition, radius);
            //modelBoundingSphere = modelBoundingSphere.Transform(worldMatrix);
        }


        public void initModelPosition(Vector3 initPosition)
        {
           modelPosition = initPosition;
           modelRotation = Matrix.Identity * Matrix.CreateRotationY(MathHelper.ToRadians(90));
           //Direction = HelperClass.RandomDirection();
           updateModelBoundingSphere(worldMatrix);

        }

        public void loadModelCustomEffects(String myModelFile, string  myEffects)
        {
            effect = Game.Content.Load<Effect>(myEffects);
            myModel = Game.Content.Load<Model>(myModelFile);
            //aspectRatio = Game.GraphicsDevice.Viewport.AspectRatio;
            foreach (ModelMesh mesh in myModel.Meshes)
                foreach (ModelMeshPart meshPart in mesh.MeshParts)
                    meshPart.Effect = effect.Clone(Game.GraphicsDevice);
        }

        public Model LoadModel(string assetName)
        {
            myModel = Game.Content.Load<Model>(assetName);
            return myModel;
        }

        public Vector3 get2dCoords(ModelManager ex, Camera ourCamera)
        {
            return Game.GraphicsDevice.Viewport.Project(ex.modelPosition, ourCamera.projectionMatrix, 
                    ourCamera.viewMatrix, Matrix.Identity);
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

        public void DrawModelWithTexture(Matrix worldMatrix, Camera myCamera, Texture2D myTexture)
        {

            worldMatrix = Matrix.CreateScale(modelScale) * modelRotation * Matrix.CreateTranslation(modelPosition);
            Matrix[] targetTransforms = new Matrix[myModel.Bones.Count];
            myModel.CopyAbsoluteBoneTransformsTo(targetTransforms);
            foreach (ModelMesh mesh in myModel.Meshes)
            {
                foreach (Effect currentEffect in mesh.Effects)
                {
                    currentEffect.CurrentTechnique = currentEffect.Techniques["Textured"];
                    currentEffect.Parameters["xWorld"].SetValue(targetTransforms[mesh.ParentBone.Index] * worldMatrix);
                    currentEffect.Parameters["xView"].SetValue(myCamera.viewMatrix);
                    currentEffect.Parameters["xProjection"].SetValue(myCamera.projectionMatrix);
                    currentEffect.Parameters["xEnableLighting"].SetValue(true);
                    currentEffect.Parameters["xLightDirection"].SetValue(modellightDirection);
                    currentEffect.Parameters["xAmbient"].SetValue(1.5f);
                    //currentEffect.Parameters["xTexture"].SetValue(myTexture);
                }
                mesh.Draw();
            }
            //base.Draw(gameTime);
        }

        public void DrawModel (Camera myCamera)
        {

            //Matrix worldMatrix = modelScale * modelRotation * Matrix.CreateTranslation(modelPosition);
            // Copy any parent transforms.
            Matrix[] transforms = new Matrix[myModel.Bones.Count];
            myModel.CopyAbsoluteBoneTransformsTo(transforms);

            // Draw the model. A model can have multiple meshes, so loop.
            foreach (ModelMesh mesh in myModel.Meshes)
            {
                // This is where the mesh orientation is set, as well 
                // as our camera and projection.
                foreach (BasicEffect effect in mesh.Effects)
                {

                    effect.EnableDefaultLighting();
                    effect.DirectionalLight0.DiffuseColor = new Vector3(0.5f, 0.5f, 0.5f); // a red light
                    if(isSelected) effect.AmbientLightColor = Color.Green.ToVector3();
                    effect.DirectionalLight0.Direction = modelRotation.Forward;  // coming along the x-axis
                    effect.DirectionalLight0.SpecularColor = new Vector3(0, 1, 0); // with green highlights
                   // effect.AmbientLightColor = Color.White.ToVector3();
                    effect.World = transforms[mesh.ParentBone.Index] * worldMatrix;
                    effect.View = myCamera.viewMatrix;
                    effect.Projection = myCamera.projectionMatrix;
                }
                // Draw the mesh, using the effects set above.
                GraphicsDevice.RenderState.DepthBufferEnable = true; 
                mesh.Draw();
            }
            //base.Draw(gameTime);
        }
    }
}