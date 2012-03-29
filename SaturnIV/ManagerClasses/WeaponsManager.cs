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
        List<Projectile> projectiles = new List<Projectile>();
        // Laser Setup info
        EffectParameter effect_center_to_viewer;
        EffectParameter effect_color;
        EffectParameter effect_matrices_combined;
        EffectTechnique effect_technique;
        Matrix[] shader_matrices_combined = new Matrix[2];
        Effect laserEffect;
        Vector3 turretDirection = Vector3.Zero;
        public ParticleEmitter trailEmitter;
        float thrustAmount;
        double elapsedTime;
        double currentTime;
        // The explosions effect works by firing projectiles up into the
        // air, so we need to keep track of all the active projectiles.

        /// <summary>
        /// Velocity scalar to approximate drag.
        /// </summary>
        private const float DragFactor = 0.99f;

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
      
        public void updateMissileMovement(GameTime gameTime, float gameSpeed, weaponStruct thisObject)
        {
            currentTime = gameTime.TotalGameTime.TotalMilliseconds;
            float turningSpeed = (float)gameTime.ElapsedGameTime.TotalMilliseconds / 1000.0f;
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;            
            turningSpeed *= 1.0f * gameSpeed;
          if (thisObject.isHoming && thisObject.distanceFromOrigin > rand.Next(100,200) && thisObject.missileTarget !=null)
          {
               thisObject.targetPosition = Vector3.Normalize(thisObject.missileTarget.modelPosition - thisObject.modelPosition);
               //thisObject.Direction = Vector3.Lerp(thisObject.Direction, thisObject.targetPosition, turningSpeed);
               thisObject.vecToTarget = Vector3.Normalize(thisObject.currentTarget.targetPosition - thisObject.modelPosition);// / 
          } 
          else
            thisObject.targetPosition = thisObject.Direction;

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
            thisObject.modelRotation.Forward = Vector3.SmoothStep(thisObject.Direction, thisObject.targetPosition, 0.14f);
            thrustAmount = 1.0f;
            thisObject.Direction = thisObject.modelRotation.Forward;
            Vector3 force = thisObject.Direction * thrustAmount * thisObject.objectThrust;
            // Apply acceleration
            Vector3 acceleration = force / thisObject.objectMass;
            thisObject.Velocity += acceleration * elapsed;
            // Apply psuedo drag
            //if (distanceFromPlayer > 1000)
            //thisObject.Velocity *= DragFactor;
            // Apply velocity
            thisObject.modelPosition += thisObject.Velocity * elapsed;
            //if (thisObject.objectClass != WeaponClassEnum.Beam)
                thisObject.worldMatrix = rotationMatrix;
            thisObject.distanceFromOrigin = Vector3.Distance(thisObject.modelPosition, thisObject.missileOrigin.modelPosition);
            if (thisObject.missileTarget != null)
                thisObject.distanceFromTarget = Vector3.Distance(thisObject.modelPosition, thisObject.missileTarget.modelPosition);
            thisObject.modelBoundingSphere.Center = thisObject.modelPosition;
            if (thisObject.projectile != null)
                if (thisObject.isProjectile)
                    thisObject.projectile.Update(gameTime, thisObject.modelPosition);
            thisObject.timer += currentTime - elapsedTime;
            elapsedTime = currentTime;
            
        }

        public void DrawLaser(GraphicsDevice device, Matrix view, Matrix projection,Color laserColor,weaponStruct weapon)
        {
                laserEffect.CurrentTechnique = effect_technique;
                if (activeWeaponList.Count > 0)
                {
                    Matrix wMatrix = Matrix.CreateScale(new Vector3(20, 1, weapon.objectScale* 50)) * weapon.worldMatrix;
                        //set the mesh on the GPU
                        set_mesh(weapon.shipModel.Meshes[0], device);
                        laserEffect.Begin();
                        laserEffect.CurrentTechnique.Passes[0].Begin();
                        shader_matrices_combined[0] = wMatrix;
                        shader_matrices_combined[1] = wMatrix * view * projection;
                        effect_matrices_combined.SetValue(shader_matrices_combined);
                        effect_color.SetValue(Color.Blue.ToVector4());
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
                if (activeWeaponList[i].objectClass != WeaponClassEnum.Beam)
                {
                    if (activeWeaponList[i].distanceFromOrigin > activeWeaponList[i].range
                        || activeWeaponList[i].timer > activeWeaponList[i].timeToLive)
                        //|| activeWeaponList[i].currentTarget == null)
                    {
                        ourExplosion.CreateExplosionVertices((float)gameTime.TotalGameTime.TotalMilliseconds, 
                            activeWeaponList[i].modelPosition, 1.25f);
                        activeWeaponList.Remove(activeWeaponList[i]);
                    }
                }
                else if (activeWeaponList[i].timer > activeWeaponList[i].timeToLive)
                {
                    ourExplosion.CreateExplosionVertices((float)gameTime.TotalGameTime.TotalMilliseconds, 
                        activeWeaponList[i].modelPosition, 1.50f);
                    activeWeaponList.Remove(activeWeaponList[i]);
                }
            }
            base.Update(gameTime);
        }

     
//////////////////////// NEW FIRE WEAPON CLASS
        public void fireWeapon(newShipStruct targetObject, newShipStruct weaponOrigin,
                               ref ParticleSystem projectileTrailParticles, ref List<weaponData> weaponDefList, 
                               WeaponModule thisWeapon,int modIndex)
        {
            weaponStruct tempData;
            tempData = new weaponStruct();
            tempData.targetPosition = Vector3.Zero;

            if (targetObject != null)
                tempData.targetPosition = targetObject.modelPosition - weaponOrigin.modelPosition;
            else
                tempData.targetPosition = weaponOrigin.Direction * 5000;

            //Calculate path
            //tempData.calcInitalPath(originDirection);
            tempData.objectFileName = weaponDefList[(int)thisWeapon.weaponType].FileName;
            tempData.radius = weaponDefList[(int)thisWeapon.weaponType].SphereRadius*2;
            tempData.isProjectile = weaponDefList[(int)thisWeapon.weaponType].isProjectile;
            tempData.isHoming = weaponDefList[(int)thisWeapon.weaponType].isHoming;
            tempData.objectClass = weaponDefList[(int)thisWeapon.weaponType].wClass;
            tempData.range = thisWeapon.weaponRange;//weaponDefList[(int)thisWeapon.weaponType].range;
            tempData.objectColor = Color.LightBlue; // weaponDefList[0].weaponColor;
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

            tempData.Velocity = weaponOrigin.Velocity;
            tempData.modelPosition = weaponOrigin.modelPosition + plyonVector3;
            tempData.modelRotation = Matrix.Identity;
            if (targetObject != null)
                tempData.Direction = Vector3.Normalize(targetObject.modelPosition - tempData.modelPosition);
            if (tempData.isProjectile)
                tempData.projectile = new Projectile(projectileTrailParticles,tempData.modelPosition,Vector3.Zero);            
            tempData.timeToLive = weaponDefList[(int)thisWeapon.weaponType].timeToLive;
            tempData.regenTime = weaponDefList[(int)thisWeapon.weaponType].regenTime;
            tempData.worldMatrix = Matrix.CreateWorld(tempData.modelPosition, weaponOrigin.Direction, Vector3.Up);        
            //tempData.beamQuad = new Quad(Game.Content,weaponOrigin.modelPosition + tempData.Direction 
            //    * weaponOrigin.distanceFromTarget/2, Vector3.UnitZ, tempData.Direction, 200, 
            //    weaponOrigin.distanceFromTarget,Color.Red);
            tempData.modIndex = modIndex;
            activeWeaponList.Add(tempData);
        }
    }
}