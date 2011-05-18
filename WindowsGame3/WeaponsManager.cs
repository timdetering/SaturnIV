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
    public class WeaponsManager : ModelManager
    {
        Random rand = new Random();

        public List<weaponStruct> activeWeaponList = new List<weaponStruct>();

        // Laser Setup info
        EffectParameter effect_center_to_viewer;
        EffectParameter effect_color;
        EffectParameter effect_matrices_combined;
        EffectTechnique effect_technique;
        Matrix[] shader_matrices_combined = new Matrix[2];
        Effect laserEffect;
        Vector3 turretDirection = Vector3.Zero;
        public ParticleEmitter trailEmitter;
        SpriteBatch spriteBatch;
        float thrustAmount;
        double elapsedTime;
        double currentTime;
        /// <summary>
        /// Velocity scalar to approximate drag.
        /// </summary>
        private const float DragFactor = 0.97f;

        public static void set_mesh(ModelMesh mesh, GraphicsDevice device)
        {
            device.VertexDeclaration = mesh.MeshParts[0].VertexDeclaration;
            device.Vertices[0].SetSource(mesh.VertexBuffer, mesh.MeshParts[0].StreamOffset,
                                         mesh.MeshParts[0].VertexStride);
            device.Indices = mesh.IndexBuffer;
        }

        public static void draw_set_mesh(ModelMesh mesh, GraphicsDevice device)
        {

            device.DrawIndexedPrimitives(PrimitiveType.TriangleList, mesh.MeshParts[0].BaseVertex, 0,
                                         mesh.MeshParts[0].NumVertices, mesh.MeshParts[0].StartIndex,
                                         mesh.MeshParts[0].PrimitiveCount);
        }

        public WeaponsManager(Game game)
            : base(game)
        {
            // TODO: Construct any child components here
        }

        public override void Initialize()
        {
            //Initialize laser effect
            laserEffect = Game.Content.Load<Effect>("Effects//laser_shader"); // load effect before laserbolt
            effect_color = laserEffect.Parameters["laser_bolt_color"];
            effect_center_to_viewer = laserEffect.Parameters["center_to_viewer"];
            effect_technique = laserEffect.Techniques["laserbolt_technique"];
            effect_matrices_combined = laserEffect.Parameters["world_matrices"];
        }

        public Model LaserModelLoad(string modelFileName)
        {
            
            myModel = Game.Content.Load<Model>(modelFileName);

            foreach (ModelMesh mesh in myModel.Meshes)
                foreach (ModelMeshPart part in mesh.MeshParts)
                    part.Effect = laserEffect;
            return myModel;
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        
        public void updateMissileMovement(GameTime gameTime, float gameSpeed, weaponStruct thisObject)
        {
            currentTime = gameTime.TotalGameTime.TotalMilliseconds;
            float turningSpeed = (float)gameTime.ElapsedGameTime.TotalMilliseconds / 1000.0f;
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
            
            turningSpeed *= thisObject.objectAgility * gameSpeed;

          if (thisObject.isHoming && thisObject.distanceFromOrigin > rand.Next(100,400) && thisObject.missileTarget !=null)
          {
               thisObject.targetPosition = Vector3.Normalize(thisObject.missileTarget.modelPosition - thisObject.modelPosition);
               //thisObject.Direction = Vector3.Lerp(thisObject.Direction, thisObject.targetPosition, turningSpeed);
               thisObject.vecToTarget = Vector3.Normalize(thisObject.currentTarget.targetPosition - thisObject.modelPosition);// / 
          } 
          else
            thisObject.targetPosition = thisObject.Direction;
         //  if (thisObject.isHoming)
         //       thisObject.angleOfAttack = NPCManager.GetSignedAngleBetweenTwoVectors(thisObject.modelPosition, thisObject.currentTarget.modelPosition,
         //                                  thisObject.currentTarget.modelRotation.Right);
          // if (thisObject.angleOfAttack < -0 || thisObject.angleOfAttack > .99)
          //      thisObject.targetPosition = thisObject.Direction;

            Vector3 scale, translation;
            Quaternion rotation;
            Matrix rotationMatrix = Matrix.CreateWorld(thisObject.modelPosition, thisObject.targetPosition, Vector3.Up);
            rotationMatrix.Decompose(out scale, out rotation, out translation);
            thisObject.Up = Vector3.TransformNormal(thisObject.Up, rotationMatrix);
            thisObject.Up.Normalize();
            thisObject.right = Vector3.Cross(thisObject.targetPosition, thisObject.Up);
            thisObject.Up = Vector3.Cross(thisObject.right, thisObject.modelRotation.Forward);
            thisObject.modelRotation = Matrix.CreateFromQuaternion(rotation);
            //Direction = modelRotation.Forward;
            thisObject.modelRotation.Forward = Vector3.SmoothStep(thisObject.Direction, thisObject.targetPosition, turningSpeed * 2);
            thrustAmount = 1.0f;
            thisObject.Direction = thisObject.modelRotation.Forward;
            Vector3 force = thisObject.Direction * thrustAmount * thisObject.objectThrust;
            // Apply acceleration
            Vector3 acceleration = force / thisObject.objectMass;
            thisObject.Velocity += acceleration * elapsed;
            // Apply psuedo drag
            //if (distanceFromPlayer > 1000)
            thisObject.Velocity *= DragFactor;
            // Apply velocity
            thisObject.modelPosition += thisObject.Velocity * elapsed;
            thisObject.worldMatrix = Matrix.CreateScale(thisObject.objectScale) * rotationMatrix;

            //thisObject.distanceFromOrigin = Vector3.Distance(thisObject.modelPosition, thisObject.missileOrigin.modelPosition);
           // thisObject.distanceFromTarget = Vector3.Distance(thisObject.modelPosition, thisObject.missileTarget.modelPosition);
            if (thisObject.trailEmitter != null)
                thisObject.trailEmitter.Update(gameTime, thisObject.modelPosition);
            thisObject.modelBoundingSphere.Center = thisObject.modelPosition;
          //  if (thisObject.isProjectile)
            //    thisObject.trailEmitter.Update(gameTime, thisObject.modelPosition);
            thisObject.timer += currentTime - elapsedTime;
            elapsedTime = currentTime;
         //   if (thisObject.shipThruster != null)
          //   {
         //    thisObject.shipThruster.update(thisObject.modelPosition,
         //    -thisObject.Direction, new Vector3(2, 50, 2), 80.0f, 10.0f,
         //    Color.White, Color.Orange, Camera.position);
////
        //     thisObject.shipThruster.heat = 1.5f;
         //    }
        }

        public void DrawLaser(GraphicsDevice device, Matrix view, Matrix projection,Color laserColor,weaponStruct weapon)
        {
                laserEffect.CurrentTechnique = effect_technique;
                if (activeWeaponList.Count > 0)
                {
                        //set the mesh on the GPU
                        set_mesh(weapon.shipModel.Meshes[0], device);
                        laserEffect.Begin();
                        laserEffect.CurrentTechnique.Passes[0].Begin();
                        shader_matrices_combined[0] = weapon.worldMatrix;
                        shader_matrices_combined[1] = weapon.worldMatrix * view * projection;
                        effect_matrices_combined.SetValue(shader_matrices_combined);
                        effect_color.SetValue(laserColor.ToVector4());
                        effect_center_to_viewer.SetValue(Vector3.Up);
                        laserEffect.CommitChanges();
                        draw_set_mesh(weapon.shipModel.Meshes[0], device);
                        laserEffect.CurrentTechnique.Passes[0].End();
                        laserEffect.End();
                }
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public void Update(GameTime gameTime, float gameSpeed, ExplosionClass ourExplosion)
        {
            // TODO: Add your update code here
            for (int i=0; i < activeWeaponList.Count; i++)
            {
                updateMissileMovement(gameTime, gameSpeed, activeWeaponList[i]);
                if (activeWeaponList[i].distanceFromOrigin > activeWeaponList[i].range || activeWeaponList[i].timer > activeWeaponList[i].timeToLive 
                    || activeWeaponList[i].currentTarget == null)
                {
                    ourExplosion.CreateExplosionVertices((float)gameTime.TotalGameTime.TotalMilliseconds,activeWeaponList[i].modelPosition,0.25f);                                                      
                    activeWeaponList.Remove(activeWeaponList[i]);
                }
            }
            base.Update(gameTime);
        }

        public void fireWeapon(newShipStruct targetObject, newShipStruct weaponOrigin, 
                               ParticleSystem projectileTrailParticles, ref List<weaponData> weaponDefList,int pylon)
        {
            weaponStruct tempData;
            ParticleEmitter trailEmitter;
            tempData = new weaponStruct();
            tempData.targetPosition = Vector3.Zero;
            
            if (targetObject != null)
                tempData.targetPosition = targetObject.modelPosition - weaponOrigin.modelPosition;

            //Calculate path
            //tempData.calcInitalPath(originDirection);
            tempData.objectFileName = weaponDefList[(int)weaponOrigin.currentWeapon.weaponType].FileName;
            tempData.radius = weaponDefList[(int)weaponOrigin.currentWeapon.weaponType].SphereRadius;
            tempData.isProjectile = weaponDefList[(int)weaponOrigin.currentWeapon.weaponType].isProjectile;
            tempData.range = weaponDefList[(int)weaponOrigin.currentWeapon.weaponType].range;
            tempData.objectColor = Color.White; // weaponDefList[0].weaponColor;
            tempData.objectScale = weaponDefList[(int)weaponOrigin.currentWeapon.weaponType].Scale;
            if (tempData.isProjectile)
                tempData.shipModel = LoadModel(tempData.objectFileName);
            else
                tempData.shipModel = LaserModelLoad(tempData.objectFileName);
            //tempData.shipModel = LaserModelLoad(tempData.objectFileName);
            tempData.objectAgility = weaponDefList[(int)weaponOrigin.currentWeapon.weaponType].Agility;
            tempData.objectMass = weaponDefList[(int)weaponOrigin.currentWeapon.weaponType].Mass;
            tempData.objectThrust = weaponDefList[(int)weaponOrigin.currentWeapon.weaponType].Thrust;           
            tempData.modelBoundingSphere = new BoundingSphere(tempData.modelPosition, tempData.radius);
            tempData.missileTarget = targetObject;
            tempData.missileOrigin = weaponOrigin;
            tempData.Velocity = weaponOrigin.Velocity;
            Vector3 plyonVector3 = new Vector3(weaponOrigin.currentWeapon.ModulePositionOnShip[pylon].X, 
                                               weaponOrigin.currentWeapon.ModulePositionOnShip[pylon].Y, 
                                               weaponOrigin.currentWeapon.ModulePositionOnShip[pylon].Z);
            tempData.modelPosition = weaponOrigin.modelPosition + plyonVector3;
            tempData.modelRotation = Matrix.Identity;
            switch ((int)weaponOrigin.currentWeapon.ModulePositionOnShip[pylon].W)
            {
                case 0:
                    tempData.Direction = weaponOrigin.modelRotation.Forward;
                    tempData.targetPosition = tempData.Direction;
                    break;
                case 1:
                    tempData.Direction = -weaponOrigin.modelRotation.Forward;
                    tempData.targetPosition = tempData.Direction;
                    break;
                case 2:
                    tempData.Direction = -weaponOrigin.modelRotation.Right;
                    tempData.targetPosition = tempData.Direction;
                    break;
                case 3:
                    tempData.Direction = -weaponOrigin.modelRotation.Right;
                    tempData.targetPosition = tempData.Direction;
                    break;
            }

            //tempData.Up = weaponOrigin.Up;
            tempData.range = weaponDefList[(int)weaponOrigin.currentWeapon.weaponType].range;
            tempData.Direction = weaponOrigin.Direction;
            tempData.targetPosition = weaponOrigin.Direction;
            tempData.timeToLive = 100;

          if (tempData.isProjectile)
           {
                 tempData.shipThruster = new Athruster();
                 tempData.shipThruster.LoadContent(Game, spriteBatch);
                trailEmitter = new ParticleEmitter(projectileTrailParticles,
                                               200, weaponOrigin.modelPosition, weaponOrigin.Velocity);
                tempData.trailEmitter = trailEmitter;
                //weaponOrigin.cMissileCount -= 1;
            }
            //if (weaponOrigin.cMissileCount >0)
            activeWeaponList.Add(tempData);
            //isMissileHit = true;
        }

//////////////////////// NEW FIRE WEAPON CLASS
        public void fireWeapon(newShipStruct targetObject, newShipStruct weaponOrigin,
                               ParticleSystem projectileTrailParticles, ref List<weaponData> weaponDefList, 
                               WeaponModule thisWeapon,int modIndex)
        {
            weaponStruct tempData;
            ParticleEmitter trailEmitter;
            tempData = new weaponStruct();
            tempData.targetPosition = Vector3.Zero;

            if (targetObject != null)
                tempData.targetPosition = targetObject.modelPosition - weaponOrigin.modelPosition;

            //Calculate path
            //tempData.calcInitalPath(originDirection);
            tempData.objectFileName = weaponDefList[(int)thisWeapon.weaponType].FileName;
            tempData.radius = weaponDefList[(int)thisWeapon.weaponType].SphereRadius*2;
            tempData.isProjectile = weaponDefList[(int)thisWeapon.weaponType].isProjectile;
            tempData.isHoming = weaponDefList[(int)thisWeapon.weaponType].isHoming;
            tempData.range = weaponDefList[(int)thisWeapon.weaponType].range;
            tempData.objectColor = Color.Blue; // weaponDefList[0].weaponColor;
            tempData.objectScale = weaponDefList[(int)thisWeapon.weaponType].Scale;
            tempData.damageFactor = weaponDefList[(int)thisWeapon.weaponType].damageFactor;
            if (tempData.isProjectile)
                tempData.shipModel = LoadModel(tempData.objectFileName);
            else
                tempData.shipModel = LaserModelLoad(tempData.objectFileName);
            tempData.objectAgility = weaponDefList[(int)thisWeapon.weaponType].Agility;
            tempData.objectMass = weaponDefList[(int)thisWeapon.weaponType].Mass;
            tempData.objectThrust = weaponDefList[(int)thisWeapon.weaponType].Thrust;
            tempData.modelBoundingSphere = new BoundingSphere(tempData.modelPosition, tempData.radius*4);
            tempData.missileTarget = targetObject;
            tempData.currentTarget = targetObject;
            tempData.missileOrigin = weaponOrigin;
            tempData.range = weaponDefList[(int)thisWeapon.weaponType].range;
            Vector3 plyonVector3 = new Vector3(thisWeapon.ModulePositionOnShip[modIndex].X,
                                   thisWeapon.ModulePositionOnShip[modIndex].Y,
                                   thisWeapon.ModulePositionOnShip[modIndex].Z);

            switch ((int)thisWeapon.ModulePositionOnShip[modIndex].W)
            {
                case 0:
                    tempData.Direction = weaponOrigin.Direction;
                    tempData.targetPosition = tempData.Direction;
                    break;
                case 1:
                    tempData.Direction = -weaponOrigin.Direction;
                    tempData.targetPosition = tempData.Direction;
                    break;
                case 2:
                    tempData.Direction = -weaponOrigin.modelRotation.Right;
                    tempData.targetPosition = tempData.Direction;
                    break;
                case 3:
                    tempData.Direction = weaponOrigin.modelRotation.Right;
                    tempData.targetPosition = tempData.Direction;
                    break;
            }

            //tempData.missileOrigin = weaponOrigin.modelPosition + plyonVector3;
            tempData.Velocity = weaponOrigin.Velocity;
            tempData.modelPosition = weaponOrigin.modelPosition + plyonVector3;
            tempData.modelRotation = Matrix.Identity;
        //    tempData.modelRotation.Forward = tempData.targetPosition;
           // tempData.Direction = tempData.Direction;

            if (tempData.isProjectile)
            {
                //tempData.shipThruster = new Athruster();
                //tempData.shipThruster.LoadContent(Game, spriteBatch);
                trailEmitter = new ParticleEmitter(projectileTrailParticles,
                                               200, weaponOrigin.modelPosition, weaponOrigin.Velocity/2);
               
                tempData.trailEmitter = trailEmitter;
            }
            tempData.timeToLive = 700;
            //if (weaponOrigin.cMissileCount >0)
            activeWeaponList.Add(tempData);
            //isMissileHit = true;
        }
    }
}