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
    public class WeaponsManager : ModelManager
    {

        public Vector3 missileOrigin;
        public Vector3 missileTarget;
        public int targetIndex;
        public int weaponRange;
        public int shipTypeIndex;
        public shipTypes NPCShipData;
        public float distanceFromPlayer;
        public float distanceFromTarget;
        public bool fromPlayer;
        Color laserColor;
        public bool isHoming;
        public bool isProjectile;
        public int regenTime;
        public int damageFactor;
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

        public void InitializeWeapon(int shipTypesIndex)
        {
            objectFilename = weaponTypes.ModelFileName[shipTypesIndex];
            objectMass = weaponTypes.mass[shipTypesIndex];
            objectThrust = weaponTypes.thrust[shipTypesIndex];
            weaponRange = weaponTypes.weaponMaxRange[shipTypesIndex];
            modelScale = weaponTypes.scale[shipTypesIndex];
            isHoming = weaponTypes.isGudied[shipTypesIndex];
            isProjectile = weaponTypes.isProjectile[shipTypesIndex];
            laserColor = weaponTypes.laserColor[shipTypesIndex];
            objectAgility = weaponTypes.agility[shipTypesIndex];
            regenTime = weaponTypes.regenTime[shipTypesIndex];
            damageFactor = weaponTypes.damage[shipTypeIndex];
            modelPosition = Vector3.Zero;
           // if (isProjectile)
          //      LoadModel(objectFilename);
          //  else
                LaserModelLoad(objectFilename);

            base.Initialize();
        }

        public void LaserModelLoad(string modelFileName)
        {
            laserEffect = Game.Content.Load<Effect>("Effects//laser_shader"); // load effect before laserbolt
            myModel = Game.Content.Load<Model>(modelFileName);

            foreach (ModelMesh mesh in myModel.Meshes)
                foreach (ModelMeshPart part in mesh.MeshParts)
                    part.Effect = laserEffect;

            effect_color = laserEffect.Parameters["laser_bolt_color"];
            effect_center_to_viewer = laserEffect.Parameters["center_to_viewer"];
            effect_technique = laserEffect.Techniques["laserbolt_technique"];
            effect_matrices_combined = laserEffect.Parameters["world_matrices"];
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        
        public void updateMissileMovement(GameTime gameTime, float gameSpeed, Camera ourCamera)
        {
            Vector3 vecToTarget = Vector3.Zero;
            float turningSpeed = (float)gameTime.ElapsedGameTime.TotalMilliseconds / 1000.0f;
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
            turningSpeed *= objectAgility * gameSpeed;

            if (isHoming) // && distanceFromPlayer > 1000)
            {
                //vecToTarget = missileTarget - missileOrigin;
                vecToTarget = currentTargetObject.modelPosition - modelPosition;
                //Direction = Vector3.Lerp(Direction, vecToTarget, 0.25f); ;
            } 
            else
                
                 vecToTarget = Direction;

            Vector3 scale, translation;
            Quaternion rotation;
            Matrix rotationMatrix = Matrix.CreateWorld(modelPosition, vecToTarget, Vector3.Up);
            rotationMatrix.Decompose(out scale, out rotation, out translation);
            Up = Vector3.TransformNormal(Up, rotationMatrix);
            Up.Normalize();
            right = Vector3.Cross(vecToTarget, Up);
            Up = Vector3.Cross(right, modelRotation.Forward);
            modelRotation = Matrix.CreateFromQuaternion(rotation);
            //Direction = modelRotation.Forward;
            modelRotation.Forward = Vector3.Lerp(modelRotation.Forward, vecToTarget, 0.01f); ;
            thrustAmount = 0.15f;
            Direction = modelRotation.Forward;
            Vector3 force = Direction * thrustAmount * objectThrust;
            // Apply acceleration
            Vector3 acceleration = force / objectMass;
            Velocity += acceleration * elapsed;
            // Apply psuedo drag
            //if (distanceFromPlayer > 1000)
           Velocity *= DragFactor;
            // Apply velocity
            modelPosition += Velocity * elapsed;
            worldMatrix = rotationMatrix;

            screenCords = get2dCoords(this, ourCamera);
            distanceFromPlayer = Vector3.Distance(modelPosition, missileOrigin);
            distanceFromTarget = Vector3.Distance(modelPosition, missileTarget);
            if (trailEmitter != null)
                trailEmitter.Update(gameTime, modelPosition);
            modelBoundingSphere.Center = modelPosition;
        }

        public void calcInitalPath(Vector3 mMissileTarget)
        {
            //missileTarget = originShipPosition + originShipDirection;
            missileTarget = mMissileTarget;
        }

        public void DrawLaser(GraphicsDevice device, Matrix view, Matrix projection)
        {
                // the previous method used a mesh.Draw() call for each laserbolt mesh.
                // that was pretty inefficient because it 1: Sets the effect, 2: sets the mesh on the gpu
                // 3: begins the effect, 4: begins the effect pass, 5: ends the effect pass and 6: ends the effect
                // for every single laserbolt.

                // 1 thru 6 only needs to be done once if you're drawing the same mesh over and over.
                // But you have to set the mesh on the GPU and use drawindexedprimitives to draw the 
                // meshes.  Thats what the two static functions set_mesh and draw_mesh do.

            worldMatrix = Matrix.CreateScale(modelScale) * worldMatrix;
                // set the effect
                laserEffect.CurrentTechnique = effect_technique;

                //set the mesh on the GPU
                set_mesh(myModel.Meshes[0], device);

                laserEffect.Begin();
                laserEffect.CurrentTechnique.Passes[0].Begin();
                        shader_matrices_combined[0] = worldMatrix;
                        shader_matrices_combined[1] = worldMatrix * view * projection;
                        effect_matrices_combined.SetValue(shader_matrices_combined);
                        effect_color.SetValue(laserColor.ToVector4());
                        effect_center_to_viewer.SetValue(modelRotation.Up);
                        laserEffect.CommitChanges();
                        draw_set_mesh(myModel.Meshes[0], device);
                laserEffect.CurrentTechnique.Passes[0].End();
                laserEffect.End();
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
    }
}