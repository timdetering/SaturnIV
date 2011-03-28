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

namespace WindowsGame3
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


        public void fireWeapon(Game game, ModelManager targetObject, ModelManager weaponOrigin,
                               ref List<WeaponsManager> missileList, Vector3 originDirection, 
                                ParticleSystem projectileTrailParticles)
        {
            WeaponsManager tempData;
            ParticleEmitter trailEmitter;
            tempData = new WeaponsManager(game);
            tempData.InitializeWeapon((int)weaponOrigin.currentWeaponIndex);
            if (weaponOrigin.currentPylon <= weaponOrigin.turretArray.Length)
            {
                tempData.modelPosition = weaponOrigin.modelPosition - ((weaponOrigin.modelRotation.Right *
                                    weaponOrigin.turretArray[weaponOrigin.currentPylon - 1].X)
                                    - (weaponOrigin.modelRotation.Up * weaponOrigin.turretArray[weaponOrigin.currentPylon - 1].Y)
                                    - (weaponOrigin.modelRotation.Forward * weaponOrigin.turretArray[weaponOrigin.currentPylon - 1].Z));
                weaponOrigin.currentPylon++;
            }
            //if (weaponOrigin.currentPylon < weaponOrigin.turretArray.Length) weaponOrigin.currentPylon++;
            else
                weaponOrigin.currentPylon = 1;
            Vector3 vecToTarget = targetObject.modelPosition - weaponOrigin.modelPosition;
                tempData.modelRotation = weaponOrigin.modelRotation;
            tempData.Direction = originDirection;
            tempData.Velocity = weaponOrigin.Velocity;
            //Calculate path
            tempData.calcInitalPath(originDirection);
            tempData.currentTargetObject = targetObject;
            tempData.missileOrigin = weaponOrigin.modelPosition;
            if (weaponTypes.isProjectile[(int)weaponOrigin.currentWeaponIndex])
            {
                trailEmitter = new ParticleEmitter(projectileTrailParticles,
                                               200, weaponOrigin.modelPosition, weaponOrigin.Velocity);
                tempData.trailEmitter = trailEmitter;
                weaponOrigin.cMissileCount -= 1;
            }
            if (weaponOrigin.cMissileCount >0)
                    missileList.Add(tempData);
            //isMissileHit = true;
        }

        public bool CheckForCollision(GameTime gameTime, List<NPCManager> shipList, List<WeaponsManager> missileBSList, 
                                       ref List<WeaponsManager> missileList, ref ExplosionClass ourExplosion)
        {
            foreach (NPCManager ship in shipList)
            {
                foreach (WeaponsManager missile in missileBSList)
                {
                    if (ship.modelBoundingSphere.Contains(missile.modelBoundingSphere) == ContainmentType.Contains 
                        && missile.distanceFromPlayer > 200)
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

        public bool CheckForCollision(GameTime gameTime, PlayerManager player, List<WeaponsManager> missileBSList,
                               ref List<WeaponsManager> missileList, ref ExplosionClass ourExplosion)
        {
                foreach (WeaponsManager missile in missileBSList)
                {
                    //if (player.modelBoundingSphere.Intersects(missile.modelBoundingSphere))
                    if (player.modelBoundingSphere.Contains(missile.modelBoundingSphere) == ContainmentType.Contains
                        && missile.distanceFromPlayer > 200)
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

        private void DrawText()
        {
            Vector2 fontPos = new Vector2(0, 0);
            StringBuilder buffer = new StringBuilder();
            //buffer.AppendFormat("Screen Width: {0}\n", GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width);
            //buffer.AppendFormat("Screen Height: {0}\n", GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height);
            //buffer.AppendFormat("Camera X: {0}\n", ourCamera.campos.X);
            //buffer.AppendFormat("Camera Y: {0}\n", ourCamera.campos.Y);
            ///buffer.AppendFormat("Camera Z: {0}\n", ourCamera.campos.Z);
            //buffer.AppendFormat("Camera Offset X: {0}\n", ourCamera.cameraOffset.X);
            //buffer.AppendFormat("Camera Offset Y: {0}\n", ourCamera.cameraOffset.Y);
            //buffer.AppendFormat("Camera Offset Z: {0}\n", ourCamera.cameraOffset.Z);
            //buffer.AppendFormat("playerShip X: {0}\n", playerShip.modelPosition.X);
            //buffer.AppendFormat("playerShip Y: {0}\n", playerShip.modelPosition.Y);
            //buffer.AppendFormat("playerShip Z: {0}\n", playerShip.modelPosition.Z);
            //buffer.AppendFormat("playerShip Rotation Forward: {0}\n", playerShip.Direction);
            //buffer.AppendFormat("playerShip Rotation Left: {0}\n", playerShip.modelRotation.Left);
            //buffer.AppendFormat("playerShip Rotation Up: {0}\n", playerShip.modelRotation.Up);
            //buffer.AppendFormat("playerShip Rotation Down: {0}\n", playerShip.modelRotation.Down);
            //if (currentAutoTarget != null)
            //{
            //    buffer.AppendFormat("Distance: {0}\n", currentAutoTarget.distanceFromPlayer);
              //  buffer.AppendFormat("Current Enemy  X: {0}\n", currentAutoTarget.modelPosition.X);
               // buffer.AppendFormat("Current Enemy  Y: {0}\n", currentAutoTarget.modelPosition.Y);
               // buffer.AppendFormat("Current Enemy  Z: {0}\n", currentAutoTarget.modelPosition.Z);
               // buffer.AppendFormat("Current Enemy  Screen X: {0}\n", currentAutoTarget.screenCords.X);
               // buffer.AppendFormat("Current Enemy  Screen Y: {0}\n", currentAutoTarget.screenCords.Y);
               // buffer.AppendFormat("Current Enemy  Screen Z: {0}\n", currentAutoTarget.screenCords.Z);
               // buffer.AppendFormat("Distance: {0}\n", currentAutoTarget.distanceFromPlayer);
               // buffer.AppendFormat("Description:" + currentAutoTarget.objectDesc + "\n");
            //}

            //foreach (WeaponsManager missile in missileList)
           // {
                //buffer.AppendFormat("Distance from Target: {0}\n", missile.distanceFromTarget);
                //buffer.AppendFormat("Missile X: {0}\n", missile.modelPosition.X);
                //buffer.AppendFormat("Missile Y: {0}\n", missile.modelPosition.Y);
                //buffer.AppendFormat("Missile Z: {0}\n", missile.modelPosition.Z);
                //buffer.AppendFormat("Missile Target X: {0}\n", missile.targetModel.modelPosition.X);
                //buffer.AppendFormat("Missile Target Y: {0}\n", missile.targetModel.modelPosition.Y);
                //buffer.AppendFormat("Missile Target Z: {0}\n", missile.targetModel.modelPosition.Z);
                //buffer.AppendFormat("Missile Direction X: {0}\n", missile.Direction.X);
                //buffer.AppendFormat("Missile Direction Y: {0}\n", missile.Direction.Y);
                //buffer.AppendFormat("Missile Direction Z: {0}\n", missile.Direction.Z);
            //};
            //buffer.AppendFormat("Active Missile Count: {0}\n", missileList.Count);
            //buffer.AppendFormat("Current Auto Target Index: {0}\n", currentAutoTargetIndex);
            //buffer.AppendFormat("Number of Visable targets: {0}\n", HUDVisableTargets.Length);
            //buffer.AppendFormat("Number of Enemies: {0}\n", npcList.Count);
            //buffer.AppendFormat("Missile Hit:"+ isMissileHit+"/n");
            //buffer.AppendFormat("Number of Explosions: {0}\n", expList.Count);
            //buffer.AppendLine("Press ESCAPE to exit");
            //spriteBatch.End();
            //spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Deferred, SaveStateMode.SaveState);
            //spriteBatch.DrawString(spriteFont, buffer.ToString(), fontPos, Color.White);
            //spriteBatch.End();
            //spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Deferred, SaveStateMode.SaveState);
        }



    }

   
        
}
