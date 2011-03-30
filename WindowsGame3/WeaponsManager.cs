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
        float thrustAmount;
        /// <summary>
        /// Velocity scalar to approximate drag.
        /// </summary>
        private const float DragFactor = 0.98f;

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
            //Vector3 vecToTarget = Vector3.Zero;
            float turningSpeed = (float)gameTime.ElapsedGameTime.TotalMilliseconds / 1000.0f;
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
            turningSpeed *= thisObject.objectAgility * gameSpeed;

      //      if (thisObject.isHoming) // && distanceFromPlayer > 1000)
      //      {
                //vecToTarget = missileTarget - missileOrigin;
             //   thisObject.vecToTarget = thisObject.missileTarget.modelPosition - thisObject.modelPosition;
                //Direction = Vector3.Lerp(Direction, vecToTarget, 0.25f); ;
        //    } 
       //     else

                thisObject.vecToTarget = thisObject.Direction;

            Vector3 scale, translation;
            Quaternion rotation;
            Matrix rotationMatrix = Matrix.CreateWorld(thisObject.modelPosition, thisObject.vecToTarget, Vector3.Up);
            rotationMatrix.Decompose(out scale, out rotation, out translation);
            thisObject.Up = Vector3.TransformNormal(thisObject.Up, rotationMatrix);
            thisObject.Up.Normalize();
            thisObject.right = Vector3.Cross(thisObject.vecToTarget, thisObject.Up);
            thisObject.Up = Vector3.Cross(thisObject.right, thisObject.modelRotation.Forward);
            thisObject.modelRotation = Matrix.CreateFromQuaternion(rotation);
            //Direction = modelRotation.Forward;
            thisObject.modelRotation.Forward = Vector3.Lerp(thisObject.modelRotation.Forward, thisObject.vecToTarget, turningSpeed); ;
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
            thisObject.worldMatrix = rotationMatrix;

            thisObject.distanceFromOrigin = Vector3.Distance(thisObject.modelPosition, thisObject.missileOrigin);
            thisObject.distanceFromTarget = Vector3.Distance(thisObject.modelPosition, thisObject.missileTarget.modelPosition);
            if (trailEmitter != null)
                trailEmitter.Update(gameTime, thisObject.modelPosition);
            thisObject.modelBoundingSphere.Center = thisObject.modelPosition;
        }

        public void DrawLaser(GraphicsDevice device, Matrix view, Matrix projection,Color laserColor)
        {
                laserEffect.CurrentTechnique = effect_technique;
                if (activeWeaponList.Count > 0)
                {
                    foreach (weaponStruct weapon in activeWeaponList)
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
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public void Update(GameTime gameTime,float gameSpeed)
        {
            // TODO: Add your update code here
            foreach (weaponStruct weapon in activeWeaponList)
                updateMissileMovement(gameTime, gameSpeed, weapon);
            base.Update(gameTime);
        }

        public void fireWeapon(newShipStruct targetObject, newShipStruct weaponOrigin, 
                               ParticleSystem projectileTrailParticles, ref List<weaponData> weaponDefList)
        {
            weaponStruct tempData;
            ParticleEmitter trailEmitter;
            tempData = new weaponStruct();
            tempData.vecToTarget = Vector3.Zero;
            //tempData.InitializeWeapon((int)weaponOrigin.currentWeaponIndex);
            //if (weaponOrigin.currentPylon <= weaponOrigin.turretArray.Length)
            //{
            //     tempData.modelPosition = weaponOrigin.modelPosition - ((weaponOrigin.modelRotation.Right *
            //                          weaponOrigin.turretArray[weaponOrigin.currentPylon - 1].X)
            //                          - (weaponOrigin.modelRotation.Up * weaponOrigin.turretArray[weaponOrigin.currentPylon - 1].Y)
            //                          - (weaponOrigin.modelRotation.Forward * weaponOrigin.turretArray[weaponOrigin.currentPylon - 1].Z));
            //      weaponOrigin.currentPylon++;
            // }
            //if (weaponOrigin.currentPylon < weaponOrigin.turretArray.Length) weaponOrigin.currentPylon++;
            //  else
            //weaponOrigin.currentPylon = 1;
            if (targetObject != null)
                tempData.vecToTarget = targetObject.modelPosition - weaponOrigin.modelPosition;

            //Calculate path
            //tempData.calcInitalPath(originDirection);
            tempData.objectFileName = weaponDefList[0].FileName;
            tempData.radius = weaponDefList[0].SphereRadius;
            tempData.shipModel = LaserModelLoad(tempData.objectFileName);
            tempData.objectAgility = weaponDefList[0].Agility;
            tempData.objectMass = weaponDefList[0].Mass;
            tempData.objectThrust = weaponDefList[0].Thrust;
            tempData.isProjectile = weaponDefList[0].isProjectile;
            tempData.objectColor = Color.Blue; // weaponDefList[0].weaponColor;
            tempData.modelBoundingSphere = new BoundingSphere(tempData.modelPosition, tempData.radius);
            tempData.missileTarget = targetObject;
            tempData.missileOrigin = weaponOrigin.modelPosition;
            tempData.Velocity = weaponOrigin.Velocity;
            tempData.modelPosition = weaponOrigin.modelPosition;
            tempData.modelRotation = Matrix.Identity * Matrix.CreateRotationY(MathHelper.ToRadians(90));;
            tempData.Up = weaponOrigin.Up;
            tempData.Direction = weaponOrigin.Direction;
            if (tempData.isProjectile)
            {
                trailEmitter = new ParticleEmitter(projectileTrailParticles,
                                               200, weaponOrigin.modelPosition, weaponOrigin.Velocity);
                tempData.trailEmitter = trailEmitter;
                //weaponOrigin.cMissileCount -= 1;
            }
            //if (weaponOrigin.cMissileCount >0)
            activeWeaponList.Add(tempData);
            //isMissileHit = true;
        }
    }
}