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
    public class EditModeComponent : Microsoft.Xna.Framework.DrawableGameComponent
    {
        private Grid grid;
        private Line3D fLine;
        BoundingSphere directionSphere;
        Color sphereColor;
        bool ischangingDirection = false;
        MouseState mouseOld;
        SpriteBatch spriteBatch;
        string[] NameList;
        int selected = 2;
        
        public EditModeComponent(Game game)
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
            grid = new Grid(100, 350, Game.GraphicsDevice, Game);
            fLine = new Line3D(Game.GraphicsDevice);
            sphereColor = Color.Blue;
            base.Initialize();
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public void Update(GameTime gameTime, Ray currentMouseRay, Vector3 mouse3dVector, 
                                    ref List<newShipStruct> objectList,bool isClicked, bool isRDepressed, 
                                    ref NPCManager npcManager,Camera ourCamera)
        {
            // TODO: Add your update code here
            bool checkResult = false;
            bool isDirectionSphere = false;

            MouseState mouseCurrent; 
            mouseCurrent = Mouse.GetState();

            isDirectionSphere = checkIsSelected(currentMouseRay, mouse3dVector, directionSphere);
           
            if (isClicked && !isDirectionSphere)
            {
                foreach (newShipStruct ourShip in objectList)
                {
                    checkResult = checkIsSelected(currentMouseRay, mouse3dVector, ourShip.modelBoundingSphere);
                    if (checkResult && isClicked)
                    {
                        ourShip.isSelected = true;
                        directionSphere = new BoundingSphere(ourShip.modelPosition + ourShip.Direction * 100, 10);
                    }
                    else
                    {
                        ourShip.isSelected = false;
                    }
                }
            }

            if (isRDepressed && !isDirectionSphere && !ischangingDirection)
            {
                foreach (newShipStruct ourShip in objectList)
                {
                    if (ourShip.isSelected)
                    {
                        ourShip.modelPosition = new Vector3(mouse3dVector.X, 0, mouse3dVector.Z);
                        npcManager.editModeUpdate(gameTime,ourShip,ourCamera);
                        directionSphere.Center = ourShip.modelPosition + ourShip.Direction * 100;
                    }
                }
            }
            else if ((isRDepressed && isDirectionSphere) || (isRDepressed && ischangingDirection))
            {
                sphereColor = Color.Red;
                ischangingDirection = true;
                mouseCurrent = Mouse.GetState();
                //if (mouseCurrent != originalMouseState)
                // {
                foreach (newShipStruct ourShip in objectList)
                {
                    if (ourShip.isSelected)
                    {
                        ourShip.vecToTarget = mouse3dVector;
                        directionSphere.Center = ourShip.modelPosition + ourShip.Direction * 100;
                        npcManager.editModeUpdate(gameTime,ourShip,ourCamera);
                        mouseOld = mouseCurrent;
                    }
                }
            }
            else
            {
                sphereColor = Color.Blue;
                ischangingDirection = false;
            }

            base.Update(gameTime);
        }

        public bool checkIsSelected(Ray currentMouseRay, Vector3 mouse3dVector, 
                        BoundingSphere sphere)
        {
            MouseState currentState = Mouse.GetState();
            //Get Mouse Ray
            Ray ray = currentMouseRay;
            BoundingSphere mouseSphere = new BoundingSphere(mouse3dVector, 10);

            //Set the K value (Tutorial 1) to high number
            float intersectionK = float.MaxValue;

            //Loop through all characters in list stored as game variable
            ///foreach (NPCManager npcobject in objectList)
           // {
                //BoundingSphere sphere = thisObject.modelBoundingSphere;

                if (sphere.Intersects(mouseSphere))
                {
                    //if the ray intersects the transformed sphere, grab the K value
                    //float intersectionValue = ray.Intersects(sphere).Value;

                    //if k value is less (object was intersected closer than previous)
                    //if (intersectionValue < intersectionK)
                    // {
                    return true;
                    //intersectionK = intersectionValue;
                    // }
                }
           // }
                return false;
        }

        public newShipStruct spawnNPC(NPCManager modelManager,Vector3 mouse3dVector,ref List<shipData> shipDefList,
                                    GameTime gameTime,Camera ourCamera,string shipName)
        {
            newShipStruct tempData = new newShipStruct();
            tempData.objectFileName = shipDefList[selected].FileName;
            tempData.objectAlias = shipName;
            tempData.shipModel = modelManager.LoadModel(shipDefList[selected].FileName);
            tempData.objectAgility = shipDefList[selected].Agility;
            tempData.objectMass = shipDefList[selected].Mass;
            tempData.objectThrust = shipDefList[selected].Thrust;
            tempData.objectType = shipDefList[selected].Type;
            tempData.team = shipDefList[selected].BelongsTo;
            tempData.radius = shipDefList[selected].SphereRadius;
            tempData.objectClass = shipDefList[selected].ShipClass;
            tempData.modelPosition = mouse3dVector;
            tempData.modelRotation = Matrix.Identity * Matrix.CreateRotationY(MathHelper.ToRadians(90));
            tempData.viewMatrix = Matrix.Identity;
            tempData.Direction = Vector3.Forward;
            tempData.vecToTarget = Vector3.Forward;
            tempData.currentDisposition = disposition.patrol;
            tempData.currentTarget = null;
            tempData.Up = Vector3.Up;
            tempData.modelBoundingSphere = new BoundingSphere(mouse3dVector, shipDefList[selected].SphereRadius);
            tempData.modelFrustum = new BoundingFrustum(Matrix.Identity);
            tempData.shipThruster = new Athruster();
            tempData.shipThruster.LoadContent(Game, spriteBatch);
            tempData.weaponArray = shipDefList[selected].AvailableWeapons;
            tempData.currentWeapon = tempData.weaponArray[0];
            tempData.EvadeDist = shipDefList[selected].EvadeDist;
            tempData.projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(25.0f), 4.0f / 3.0f, .5f, 500f);
            modelManager.editModeUpdate(gameTime, tempData,ourCamera);
            return tempData;
        }

        public void Draw(GameTime gameTime, ref List<newShipStruct> shipList,Camera ourCamera)
        {
            grid.drawLines();
            foreach (newShipStruct enemy in shipList)
               {
                   fLine.Draw(enemy.modelPosition + enemy.Direction * 25,
                          enemy.modelPosition + enemy.Direction * 100,
                          Color.Orange, ourCamera.viewMatrix, ourCamera.projectionMatrix);
                   BoundingSphere directionSphere = new BoundingSphere(enemy.modelPosition + enemy.Direction * 100, 5);
                   //BoundingFrustumRenderer.Render(enemy.modelFrustum, GraphicsDevice, ourCamera.viewMatrix, ourCamera.projectionMatrix,Color.Yellow);
                   BoundingSphereRenderer.Render3dCircle(enemy.modelBoundingSphere.Center, enemy.modelBoundingSphere.Radius, GraphicsDevice, ourCamera.viewMatrix, ourCamera.projectionMatrix, Color.White);
                   if (enemy.isSelected)
                        BoundingSphereRenderer.Render(directionSphere, GraphicsDevice, ourCamera.viewMatrix, ourCamera.projectionMatrix, sphereColor);
                    
              }

            base.Draw(gameTime);
        }
    }
}