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
        public bool isGroupSelect = false;
        public bool isGroupObjectSelected = false;
        public bool isCameraSet = false;
        MouseState mouseOld;
        SpriteBatch spriteBatch;
        string[] NameList;
        public static bool selected = false;
        MouseState prevMouseState;
        MouseState mouseCurrent;
        KeyboardState keyboardState;
        KeyboardState prevKeyboardState;
        Vector3 mousePosOld;
        Vector3 mousePos;
        public Rectangle selectionRect;
        public BoundingBox selectionBB;
        Vector3 mouse3dStart;
        List<newShipStruct> selectedGroup = new List<newShipStruct>();

        //Define edit mode "Rings"
        BoundingSphere ring1 = new BoundingSphere(Vector3.Zero, 1000);
        BoundingSphere ring2 = new BoundingSphere(Vector3.Zero, 2000);
        BoundingSphere ring3 = new BoundingSphere(Vector3.Zero, 3000);

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
            grid = new Grid(100, 300, Game.GraphicsDevice, Game);
            fLine = new Line3D(Game.GraphicsDevice);
            selectionBB = new BoundingBox();
            base.Initialize();
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public void Update(GameTime gameTime, Ray currentMouseRay, Vector3 mouse3dVector, 
                                    ref List<newShipStruct> objectList,bool isLClicked, bool isLDepressed, 
                                    ref NPCManager npcManager,Camera ourCamera, ref Viewport viewport)
        {
            // TODO: Add your update code here
            bool checkResult = false;
            bool isDirectionSphere = false;
            selected = false;
            mouseCurrent = Mouse.GetState();
            keyboardState = Keyboard.GetState();

            isDirectionSphere = checkIsSelected(currentMouseRay, mouse3dVector, directionSphere);
            if (isDirectionSphere)
                isDragging = false;
            if (isLClicked && !isDirectionSphere && !isGroupSelect)
            {
                isDragging = false;
                foreach (newShipStruct ourShip in objectList)
                {
                    checkResult = checkIsSelected(currentMouseRay, mouse3dVector, ourShip.modelBoundingSphere);
                    if (checkResult && isLClicked)
                    {
                        ourShip.isSelected = true;
                        selected = true;

                        directionSphere = new BoundingSphere(ourShip.modelPosition + ourShip.Direction * ourShip.radius, ourShip.radius / 4);
                    }
                    else
                        ourShip.isSelected = false;
                }
            }
           //if (isLClicked && !isDirectionSphere && !isGroupSelect && !selected)
            //    Game1.cameraTarget = Matrix.CreateWorld(mouse3dVector, Vector3.Forward, Vector3.Up);
            if (isLDepressed && !isDirectionSphere && !isDragging)
            {
         //     isGroupSelect = true;
             // selectRectangle(mouseCurrent, mouse3dVector);
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
                         directionSphere.Center = ourShip.modelPosition + ourShip.Direction * ourShip.radius;
                    }
               }
            }
       //     else if (isLDepressed && !isDirectionSphere && !ischangingDirection && isGroupSelect)
     //       {
     //           
     //       }
            else if ((isLDepressed && isDirectionSphere && !isDragging)
                      || (isLDepressed && ischangingDirection && !isDragging))
            {
                isDragging = false;
                sphereColor = Color.Red;
                ischangingDirection = true;

                foreach (newShipStruct ourShip in objectList)
                {
                    if (ourShip.isSelected)
                    {
                        ourShip.targetPosition = mouse3dVector;
                        directionSphere.Center = ourShip.modelPosition + ourShip.Direction * ourShip.radius;
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

            if (keyboardState.IsKeyDown(Keys.Escape))
            {
                selectionRect = Rectangle.Empty;
                isGroupSelect = false;
            }
           //if (isGroupSelect)
               // RectangleSelect(objectList, viewport, ourCamera.projectionMatrix, ourCamera.viewMatrix, selectionRect);
            mousePosOld = mousePos;
            prevMouseState = mouseCurrent;
            base.Update(gameTime);
        }

        

        public void Draw(GameTime gameTime, ref List<newShipStruct> shipList,Camera ourCamera)
        {
            //grid.drawLines();

          
            foreach (newShipStruct enemy in shipList)
               {
                   fLine.Draw(enemy.modelPosition + enemy.Direction * enemy.radius/2,
                          enemy.modelPosition + enemy.Direction * enemy.radius * 1,
                          Color.Orange, ourCamera.viewMatrix, ourCamera.projectionMatrix);
                   BoundingSphere directionSphere = new BoundingSphere(enemy.modelPosition + enemy.Direction * enemy.radius,enemy.radius/4);
                  // BoundingSphereRenderer.Render3dCircle(enemy.modelBoundingSphere.Center, enemy.modelBoundingSphere.Radius,
                                            //                 GraphicsDevice, ourCamera.viewMatrix, ourCamera.projectionMatrix, Color.White);   
                if (enemy.isSelected)
                   {
                       BoundingSphereRenderer.Render3dCircle(directionSphere.Center,directionSphere.Radius/1.75f, GraphicsDevice, ourCamera.viewMatrix, ourCamera.projectionMatrix, Color.Blue);
                       BoundingSphereRenderer.Render3dCircle(enemy.modelBoundingSphere.Center, enemy.modelBoundingSphere.Radius,
                                                          GraphicsDevice, ourCamera.viewMatrix, ourCamera.projectionMatrix, Color.White);
                   }
                    
              }
           // BoundingSphereRenderer.Render3dCircle(Vector3.Zero, 500, GraphicsDevice, ourCamera.viewMatrix, ourCamera.projectionMatrix, Color.LawnGreen);
            BoundingSphereRenderer.Render3dCircle(Vector3.Zero, 1000, GraphicsDevice, ourCamera.viewMatrix, ourCamera.projectionMatrix, Color.Green);
            BoundingSphereRenderer.Render3dCircle(Vector3.Zero, 2000, GraphicsDevice, ourCamera.viewMatrix, ourCamera.projectionMatrix, Color.LawnGreen);
            BoundingSphereRenderer.Render3dCircle(Vector3.Zero, 4000, GraphicsDevice, ourCamera.viewMatrix, ourCamera.projectionMatrix, Color.Green);
            BoundingSphereRenderer.Render3dCircle(Vector3.Zero, 6000, GraphicsDevice, ourCamera.viewMatrix, ourCamera.projectionMatrix, Color.LawnGreen);
            BoundingSphereRenderer.Render3dCircle(Vector3.Zero, 8000, GraphicsDevice, ourCamera.viewMatrix, ourCamera.projectionMatrix, Color.Green);
            BoundingSphereRenderer.Render3dCircle(Vector3.Zero, 10000, GraphicsDevice, ourCamera.viewMatrix, ourCamera.projectionMatrix, Color.LawnGreen);
            BoundingSphereRenderer.Render3dCircle(Vector3.Zero, 12000, GraphicsDevice, ourCamera.viewMatrix, ourCamera.projectionMatrix, Color.Green);
            BoundingSphereRenderer.Render3dCircle(Vector3.Zero, 14000, GraphicsDevice, ourCamera.viewMatrix, ourCamera.projectionMatrix, Color.LawnGreen);
            base.Draw(gameTime);
        }
        
        public static bool checkIsSelected(Ray currentMouseRay, Vector3 mouse3dVector,
                BoundingSphere sphere)
        {
            MouseState currentState = Mouse.GetState();
            BoundingSphere mouseSphere = new BoundingSphere(mouse3dVector, 7.5f);

            if (sphere.Intersects(mouseSphere)) return true;

            return false;
        }

        public newShipStruct spawnNPC(NPCManager modelManager, Vector3 mouse3dVector, ref List<shipData> shipDefList,
                                   GameTime gameTime, Camera ourCamera, string shipName, int shipIndex, int team)
        {
            newShipStruct tempData = new newShipStruct();
            tempData.objectFileName = shipDefList[shipIndex].FileName;
            tempData.objectAlias = shipName;
            tempData.shipModel = modelManager.LoadModel(shipDefList[shipIndex].FileName);
            tempData.objectAgility = shipDefList[shipIndex].Agility;
            tempData.objectMass = shipDefList[shipIndex].Mass;
            tempData.objectThrust = shipDefList[shipIndex].Thrust;
            tempData.objectType = shipDefList[shipIndex].Type;
            tempData.hullLvl = 100;
            tempData.shieldLvl = 100;
            tempData.shieldFactor = shipDefList[shipIndex].ShieldFactor;
            tempData.team = team;
            tempData.objectClass = shipDefList[shipIndex].ShipClass;
            tempData.modelPosition = mouse3dVector;
            tempData.modelRotation = Matrix.Identity;// *Matrix.CreateRotationY(MathHelper.ToRadians(-90));
            tempData.Direction = Vector3.Forward;
            tempData.targetPosition = tempData.modelPosition + tempData.Direction * 10000;
            tempData.wayPointPosition = tempData.targetPosition;
            if (team == 0)
                tempData.currentDisposition = disposition.idle;
            else
                tempData.currentDisposition = disposition.patrol;
            tempData.currentTarget = null;
            tempData.Up = Vector3.Up;
            tempData.modelBB = HelperClass.ComputeBoundingBox(tempData.shipModel, tempData.modelPosition);
            tempData.modelLen = tempData.modelBB.Max.X - tempData.modelBB.Min.X;
            tempData.modelWidth = tempData.modelBB.Max.Z - tempData.modelBB.Min.Z;
            tempData.modelFrustum = new BoundingFrustum(Matrix.Identity);
            tempData.portFrustum = new BoundingFrustum(Matrix.Identity);
            tempData.starboardFrustum = new BoundingFrustum(Matrix.Identity);
            tempData.radius = tempData.modelLen;
            tempData.modelBoundingSphere = new BoundingSphere(mouse3dVector, tempData.radius / 2);
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

            modelManager.updateShipMovement(gameTime, 5.0f, tempData, ourCamera, true);
            return tempData;
        }
    }
}