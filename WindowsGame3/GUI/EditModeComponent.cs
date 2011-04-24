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
        public bool ischangingDirection = false;
        public bool isDragging = false;
        MouseState mouseOld;
        SpriteBatch spriteBatch;
        string[] NameList;
        public static bool selected = false;
        MouseState prevMouseState;
        public Rectangle selectionRect;

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
            grid = new Grid(100, 250, Game.GraphicsDevice, Game);
            fLine = new Line3D(Game.GraphicsDevice);
            sphereColor = Color.Blue;
            base.Initialize();
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public void Update(GameTime gameTime, Ray currentMouseRay, Vector3 mouse3dVector, 
                                    ref List<newShipStruct> objectList,bool isLClicked, bool isLDepressed, 
                                    ref NPCManager npcManager,Camera ourCamera)
        {
            // TODO: Add your update code here
            bool checkResult = false;
            bool isDirectionSphere = false;

            MouseState mouseCurrent; 
            mouseCurrent = Mouse.GetState();

            isDirectionSphere = checkIsSelected(currentMouseRay, mouse3dVector, directionSphere);
            if (isDirectionSphere)
                    isDragging = false;
           
            if (isLClicked && !isDirectionSphere)
            {
                isDragging = false;
                foreach (newShipStruct ourShip in objectList)
                {
                    checkResult = checkIsSelected(currentMouseRay, mouse3dVector, ourShip.modelBoundingSphere);
                    if (checkResult && isLClicked)
                    {
                        ourShip.isSelected = true;
                        selected = true;
                        directionSphere = new BoundingSphere(ourShip.modelPosition + ourShip.Direction * ourShip.radius * 1.5f, ourShip.radius/3);
                    }
                    else
                        ourShip.isSelected = false;
                }
            }

            if (isLDepressed && !isDirectionSphere && !ischangingDirection)
            {
                foreach (newShipStruct ourShip in objectList)
                {
                    if (ourShip.isSelected)
                    {
                        isDragging = true;
                        ourShip.modelPosition = new Vector3(mouse3dVector.X, 0, mouse3dVector.Z);
                        npcManager.updateShipMovement(gameTime, 5.0f, ourShip, ourCamera, true);
                        directionSphere.Center = ourShip.modelPosition + ourShip.Direction * ourShip.radius * 1.5f;
                    }
                }
            }
            else if ((isLDepressed && isDirectionSphere && !isDragging) || (isLDepressed && ischangingDirection && !isDragging))
            {
                isDragging = false;
                sphereColor = Color.Red;
                ischangingDirection = true;
                mouseCurrent = Mouse.GetState();
                foreach (newShipStruct ourShip in objectList)
                {
                    if (ourShip.isSelected)
                    {
                        ourShip.targetPosition = mouse3dVector;
                        directionSphere.Center = ourShip.modelPosition + ourShip.Direction * ourShip.radius * 1.5f;
                        npcManager.updateShipMovement(gameTime, 5.0f, ourShip, ourCamera, true);
                        mouseOld = mouseCurrent;
                    }
                }
            }
            else
            {
                sphereColor = Color.Blue;
                ischangingDirection = false;
            }
            //if (isLDepressed && !isDragging && !isDirectionSphere && !ischangingDirection)
                selectRectangle();

            base.Update(gameTime);
        }

        public bool checkIsSelected(Ray currentMouseRay, Vector3 mouse3dVector, 
                        BoundingSphere sphere)
        {
            MouseState currentState = Mouse.GetState();
            BoundingSphere mouseSphere = new BoundingSphere(mouse3dVector, 7.5f);

            if (sphere.Intersects(mouseSphere)) return true;

                return false;
        }

        public Rectangle selectRectangle()
        {
            MouseState mouseState = Mouse.GetState();

             // Creating and ajdusting the selection box
            if (mouseState.LeftButton == ButtonState.Pressed)
            {
                if (prevMouseState.LeftButton == ButtonState.Released)
                {
                    selectionRect = new Rectangle((int)mouseState.X, (int)mouseState.Y, 0, 0);
                }
                else
                {
                    // Mouse left button is being held down ( dragging )

                    // Calculate new width,height
                    selectionRect.Width = mouseState.X - selectionRect.X;
                    selectionRect.Height = mouseState.Y - selectionRect.Y;

                    // We want the selection box to be relative to the viewport we're using
                  //  selectionRect.Width -= rightViewport.X;
                   // selectionRect.Height -= rightViewport.Y;
                }
            }
            else if (prevMouseState.LeftButton == ButtonState.Released)
            {   
                // Mouse was released a frame ago
                selectionRect = Rectangle.Empty;
            }

            prevMouseState = mouseState;
            return selectionRect;
        }


        public newShipStruct spawnNPC(NPCManager modelManager,Vector3 mouse3dVector,ref List<shipData> shipDefList,
                                    GameTime gameTime,Camera ourCamera,string shipName,int shipIndex, int team)
        {
            newShipStruct tempData = new newShipStruct();
            tempData.objectFileName = shipDefList[shipIndex].FileName;
            tempData.objectAlias = shipName;
            tempData.shipModel = modelManager.LoadModel(shipDefList[shipIndex].FileName);
            tempData.objectAgility = shipDefList[shipIndex].Agility;
            tempData.objectMass = shipDefList[shipIndex].Mass;
            tempData.objectThrust = shipDefList[shipIndex].Thrust;
            tempData.objectType = shipDefList[shipIndex].Type;
            tempData.team = team;
            tempData.objectClass = shipDefList[shipIndex].ShipClass;
            tempData.modelPosition = mouse3dVector;
            tempData.modelRotation = Matrix.Identity;// *Matrix.CreateRotationY(MathHelper.ToRadians(-90));
            tempData.Direction = Vector3.Forward;
            tempData.targetPosition = tempData.modelPosition + tempData.Direction * 10000;
            tempData.wayPointPosition = tempData.targetPosition;
            tempData.currentDisposition = disposition.patrol;
            tempData.currentTarget = null;
            tempData.Up = Vector3.Up;
            tempData.modelBoundingSphere = new BoundingSphere(mouse3dVector, shipDefList[shipIndex].SphereRadius);
            tempData.modelBB = HelperClass.ComputeBoundingBox(tempData.shipModel,tempData.modelPosition);
            tempData.modelLen = tempData.modelBB.Max.X - tempData.modelBB.Min.X;
            tempData.modelWidth = tempData.modelBB.Max.Z - tempData.modelBB.Min.Z;
            tempData.radius = tempData.modelLen;
            tempData.modelFrustum = new BoundingFrustum(Matrix.Identity);
            tempData.portFrustum = new BoundingFrustum(Matrix.Identity);
            tempData.starboardFrustum = new BoundingFrustum(Matrix.Identity);
            tempData.shipThruster = new Athruster();
            tempData.shipThruster.LoadContent(Game, spriteBatch);
            tempData.weaponArray = shipDefList[shipIndex].AvailableWeapons;
            tempData.currentWeapon = tempData.weaponArray[0];
            tempData.EvadeDist = shipDefList[shipIndex].EvadeDist;
            tempData.TargetPrefs = shipDefList[shipIndex].TargetPrefs;
            tempData.ChasePrefs = shipDefList[shipIndex].Chase;
            tempData.projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(25.0f), 4.0f / 3.0f, .5f, 500f);

            //Build Bounding Frustrum for all Weapon Modules on ship
            tempData.moduleFrustum = new List<BoundingFrustum>();
             int moduleCount = 0;
             foreach (WeaponModule thisWeapon in tempData.weaponArray)
                 for (int i = 0; i < thisWeapon.ModulePositionOnShip.Count(); i++)
                 {
                     tempData.moduleFrustum.Add(new BoundingFrustum(Matrix.Identity));
                     moduleCount++;
                 }

            modelManager.updateShipMovement(gameTime, 5.0f,tempData,ourCamera,true);
            return tempData;
        }

        List<newShipStruct> RectangleSelect(List<newShipStruct> objectsList, Viewport viewport, Matrix projection, Matrix view, Rectangle selectionRect)
        {
            // Create a new list to return it
            List<newShipStruct> selectedObj = new List<newShipStruct>();
            foreach (newShipStruct o in objectsList)
            {
                // Getting the 2D position of the object
                Vector3 screenPos = viewport.Project(o.modelPosition, projection, view, Matrix.Identity);

                if (selectionRect.Contains((int)screenPos.X, (int)screenPos.Y))
                {
                    // Add object to selected objects list
                    selectedObj.Add(o);
                }
            }
            return selectedObj;
        }


        public void Draw(GameTime gameTime, ref List<newShipStruct> shipList,Camera ourCamera)
        {
            grid.drawLines();
            foreach (newShipStruct enemy in shipList)
               {
                   fLine.Draw(enemy.modelPosition + enemy.Direction * enemy.radius/2,
                          enemy.modelPosition + enemy.Direction * enemy.radius * 1,
                          Color.Orange, ourCamera.viewMatrix, ourCamera.projectionMatrix);
                   BoundingSphere directionSphere = new BoundingSphere(enemy.modelPosition + enemy.Direction * enemy.radius * 1.5f, enemy.radius /3);
                   BoundingSphereRenderer.Render3dCircle(enemy.modelBoundingSphere.Center, enemy.modelBoundingSphere.Radius, 
                                                        GraphicsDevice, ourCamera.viewMatrix, ourCamera.projectionMatrix, Color.White);
                   if (enemy.isSelected)
                       BoundingSphereRenderer.Render(directionSphere, GraphicsDevice, ourCamera.viewMatrix, ourCamera.projectionMatrix, Color.Yellow);
                    
              }

            base.Draw(gameTime);
        }
    }
}