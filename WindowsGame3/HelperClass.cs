using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    class HelperClass
    {
        private float _FPS = 0f, _TotalTime = 0f, _DisplayFPS = 0f;
        Random rand = new Random();
        /// <summary>
        /// Returns a number between two values.
        /// </summary>
        /// <param name="min">Lower bound value</param>
        /// <param name="max">Upper bound value</param>
        /// <returns>Random number between bounds.</returns>
        public static float RandomBetween(double min, double max)
        {
            Random random = new Random();
            return (float)(min + (float)random.NextDouble() * (max - min));
        }

        public static Vector3 RandomPosition(float minBoxPos, float maxBoxPos)
        {
            Random random = new Random();

            return new Vector3(
                     RandomBetween(minBoxPos, maxBoxPos),
                     0.0f,
                     RandomBetween(minBoxPos, maxBoxPos));
        }

        public static Vector3 RandomDirection()
        {
            Random random = new Random();

            Vector3 direction = new Vector3(
                    RandomBetween(-1.0f, 1.0f),
                    RandomBetween(-1.0f, 1.0f),
                    RandomBetween(-1.0f, 1.0f));
            direction.Normalize();

            return direction;
        }


        public void fireWeapon(Game game, newShipStruct targetObject, newShipStruct weaponOrigin,
                               ref List<weaponStruct> missileList,ParticleSystem projectileTrailParticles, ref ModelManager modelManager,ref List<weaponData> weaponDefList)
        {
            weaponStruct tempData;
            ParticleEmitter trailEmitter;
            tempData = new weaponStruct();
            
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
            Vector3 vecToTarget = targetObject.modelPosition - weaponOrigin.modelPosition;
            tempData = InitializeWeapon(0,ref modelManager, ref weaponDefList);

            //Calculate path
            //tempData.calcInitalPath(originDirection);
            tempData.missileTarget = targetObject;
            tempData.missileOrigin = weaponOrigin.modelPosition;
            tempData.Velocity = weaponOrigin.Velocity;
            tempData.modelRotation = weaponOrigin.modelRotation;
            tempData.Up = Vector3.Up;
            tempData.Direction = Vector3.Forward;

            if (tempData.isProjectile)
            {
                trailEmitter = new ParticleEmitter(projectileTrailParticles,
                                               200, weaponOrigin.modelPosition, weaponOrigin.Velocity);
                tempData.trailEmitter = trailEmitter;
                //weaponOrigin.cMissileCount -= 1;
            }
            //if (weaponOrigin.cMissileCount >0)
                    missileList.Add(tempData);
            //isMissileHit = true;
        }

        public weaponStruct InitializeWeapon(int weaponTypeIndex,ref ModelManager modelManager,ref List<weaponData> weaponDefList)
        {
            weaponStruct newWeapon = new weaponStruct();
            newWeapon.objectFileName = weaponDefList[0].shipFileName;
            newWeapon.radius = weaponDefList[0].shipSphereRadius;
            newWeapon.shipModel = modelManager.LoadModel(newWeapon.objectFileName);
            newWeapon.objectAgility = weaponDefList[0].shipAgility;
            newWeapon.isProjectile = weaponDefList[0].isProjectile;
            newWeapon.objectColor = weaponDefList[0].weaponColor;
            newWeapon.modelBoundingSphere = new BoundingSphere(newWeapon.modelPosition, newWeapon.radius);
            return newWeapon;
        }

        public bool CheckForCollision(GameTime gameTime, List<newShipStruct> shipList, List<weaponStruct> missileBSList, 
                                       ref List<weaponStruct> missileList, ref ExplosionClass ourExplosion)
        {
            foreach (newShipStruct ship in shipList)
            {
                foreach (weaponStruct missile in missileBSList)
                {
                    if (ship.modelBoundingSphere.Contains(missile.modelBoundingSphere) == ContainmentType.Contains 
                        && missile.distanceFromOrigin > 200)
                    {
                        Vector3 currentExpLocation = missile.modelPosition;
                        missileList.Remove(missile);
                        ourExplosion.CreateExplosionVertices((float)gameTime.TotalGameTime.TotalMilliseconds,
                                                        currentExpLocation);
                        ship.objectArmorLvl -= (ship.objectArmorFactor / 100) * missile.damageFactor;
                        return true;
                    }
                }
            }
            return false;
        }

        public bool CheckForCollision(GameTime gameTime, newShipStruct player, List<weaponStruct> missileBSList,
                               ref List<weaponStruct> missileList, ref ExplosionClass ourExplosion)
        {
                foreach (weaponStruct missile in missileBSList)
                {
                    //if (player.modelBoundingSphere.Intersects(missile.modelBoundingSphere))
                    if (player.modelBoundingSphere.Contains(missile.modelBoundingSphere) == ContainmentType.Contains
                        && missile.distanceFromOrigin > 200)
                    {
                        Vector3 currentExpLocation = missile.modelPosition;
                        missileList.Remove(missile);
                        ourExplosion.CreateExplosionVertices((float)gameTime.TotalGameTime.TotalMilliseconds,
                                                        currentExpLocation);
                        player.objectArmorLvl -= (player.objectArmorFactor / 100) * missile.damageFactor;
                        return true;
                    }
            }
            return false;
        }

        public bool CheckForCollision(GameTime gameTime, NPCManager myShip, List<NPCManager> shipList2)
        {
                foreach (NPCManager ship2 in shipList2)
                {
                    if (myShip.modelBoundingSphere.Intersects(ship2.modelBoundingSphere))
                    {
                        return true;
                    }
                }
            return false;
        }

        public void DrawFPS(GameTime gameTime, GraphicsDevice device, SpriteBatch spriteBatch, SpriteFont spriteFont)
        {
            // Calculate the Frames Per Second
            float ElapsedTime = (float)gameTime.ElapsedRealTime.TotalSeconds;
            _TotalTime += ElapsedTime;

            if (_TotalTime >= 1)
            {
                _DisplayFPS = _FPS;
                _FPS = 0;
                _TotalTime = 0;
            }
            _FPS += 1;

            // Format the string appropriately
            string FpsText = _DisplayFPS.ToString() + " FPS";
            Vector2 FPSPos = new Vector2((device.Viewport.Width - spriteFont.MeasureString(FpsText).X) - 15, 10);
            spriteBatch.Begin();
            spriteBatch.DrawString(spriteFont, FpsText, FPSPos, Color.White);
            spriteBatch.End();
        }

    }

   
        
}
