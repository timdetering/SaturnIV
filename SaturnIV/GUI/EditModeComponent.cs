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
        Color mouseOverColor;
        Color mouseOverColor2;
        public bool ischangingDirection = false;
        public bool isDragging = false;
        public bool isGroupSelect = false;
        public bool isGroupObjectSelected = false;
        public bool isCameraSet = false;
        MouseState mouseOld;
        SpriteBatch spriteBatch;
        public static bool selected = false;
        MouseState prevMouseState;
        MouseState mouseCurrent;
        KeyboardState keyboardState;
        KeyboardState prevKeyboardState;
        Vector3 mousePosOld;
        Vector3 mousePos;
        public Rectangle selectionRect;
        public BoundingBox selectionBB;
        float lineFactor = 2000.00f;
        List<newShipStruct> selectedGroup = new List<newShipStruct>();
        ModelManager modelManager;

        /// <summary>
        /// Load Tactical Map items
        /// </summary>
        Model redLargePlane;
        Model redMediumPlane;
        Model redSmallPlane;
        Model blueLargePlane;
        Model blueMediumPlane;
        Model blueSmallPlane;


        float planisphereScale = 1.0f;

        public EditModeComponent(Game game)
            : base(game)
        {
            // TODO: Construct any child components here
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public void Initialize(ModelManager ModelManager)
        {
            // TODO: Add your initialization code here
         //   grid = new Grid(20000, 17000, Game.GraphicsDevice, Game);
            modelManager = ModelManager;
            redLargePlane = modelManager.LoadModel("Models/tacmap_items/red_large_plane");
            redMediumPlane = modelManager.LoadModel("Models/tacmap_items/red_medium_plane");
            redSmallPlane = modelManager.LoadModel("Models/tacmap_items/red_small_plane");
            blueLargePlane = modelManager.LoadModel("Models/tacmap_items/blue_large_plane");
            blueMediumPlane = modelManager.LoadModel("Models/tacmap_items/blue_medium_plane");
            blueSmallPlane = modelManager.LoadModel("Models/tacmap_items/blue_small_plane");

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

            isDirectionSphere = checkIsSelected(mouse3dVector, directionSphere);

            if (isDirectionSphere)
            {
                isDragging = false;
                mouseOverColor = Color.Yellow;
            }
            else
                mouseOverColor = Color.White;
            if (isLClicked && !isDirectionSphere && !isGroupSelect)
            {
                isDragging = false;
                foreach (newShipStruct ourShip in objectList)
                {
                    checkResult = checkIsSelected(mouse3dVector, ourShip.modelBoundingSphere);
                    if (checkResult && isLClicked)
                    {
                        ourShip.isSelected = true;
                        selected = true;
                        mouseOverColor2 = Color.Yellow;
                        directionSphere = new BoundingSphere(ourShip.modelPosition + ourShip.Direction * lineFactor, 200);
                    }
                    else
                    {
                        mouseOverColor2 = Color.White;
                        ourShip.isSelected = false;
                    }

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
                         directionSphere.Center = ourShip.modelPosition + ourShip.Direction * lineFactor;
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
                        directionSphere.Center = ourShip.modelPosition + ourShip.Direction * lineFactor;
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
             //egrid.drawPoints(10);                       
            foreach (newShipStruct enemy in shipList)
            {
                if (enemy.team == 0)
                {
                    if (enemy.objectClass == ClassesEnum.Capitalship)
                    {
                        Matrix pMatrix = Matrix.CreateWorld(enemy.modelPosition, Vector3.Forward, Vector3.Up);
                        pMatrix *= Matrix.CreateScale(planisphereScale);
                        modelManager.DrawModel(ourCamera, blueLargePlane, pMatrix, Color.Blue);
                    }
                    else if (enemy.objectClass == ClassesEnum.Crusier)
                    {
                        Matrix pMatrix = Matrix.CreateWorld(enemy.modelPosition, Vector3.Forward, Vector3.Up);
                        pMatrix *= Matrix.CreateScale(planisphereScale);
                        modelManager.DrawModel(ourCamera, blueMediumPlane, pMatrix, Color.Blue);
                    }
                    else if (enemy.objectClass == ClassesEnum.Fighter)
                    {
                        Matrix pMatrix = Matrix.CreateWorld(enemy.modelPosition, Vector3.Forward, Vector3.Up);
                        pMatrix *= Matrix.CreateScale(planisphereScale);
                        modelManager.DrawModel(ourCamera, blueSmallPlane, pMatrix, Color.Blue);
                    }
                }
                else
                {
                    if (enemy.objectClass == ClassesEnum.Capitalship)
                    {
                        Matrix pMatrix = Matrix.CreateWorld(enemy.modelPosition, Vector3.Forward, Vector3.Up);
                        pMatrix *= Matrix.CreateScale(planisphereScale);
                        modelManager.DrawModel(ourCamera, redLargePlane, pMatrix, Color.Blue);
                    }
                    else if (enemy.objectClass == ClassesEnum.Crusier)
                    {
                        Matrix pMatrix = Matrix.CreateWorld(enemy.modelPosition, Vector3.Forward, Vector3.Up);
                        pMatrix *= Matrix.CreateScale(planisphereScale);
                        modelManager.DrawModel(ourCamera, redMediumPlane, pMatrix, Color.Blue);
                    }
                    else if (enemy.objectClass == ClassesEnum.Fighter)
                    {
                        Matrix pMatrix = Matrix.CreateWorld(enemy.modelPosition, Vector3.Forward, Vector3.Up);
                        pMatrix *= Matrix.CreateScale(planisphereScale);
                        modelManager.DrawModel(ourCamera, redSmallPlane, pMatrix, Color.Blue);
                    }
                }
                 //fLine.Draw(enemy.modelPosition + enemy.Direction * enemy.radius,
                     //     enemy.modelPosition + enemy.Direction * 900,
                      //    Color.Orange, ourCamera.viewMatrix, ourCamera.projectionMatrix);
                 // BoundingSphere directionSphere = new BoundingSphere(enemy.modelPosition + enemy.Direction * lineFactor,100);
                //   BoundingSphereRenderer.Render(directionSphere, GraphicsDevice, ourCamera.viewMatrix, ourCamera.projectionMatrix, mouseOverColor);
                   BoundingSphereRenderer.Render(enemy.modelBoundingSphere, GraphicsDevice, ourCamera.viewMatrix, ourCamera.projectionMatrix, Color.White);
                if (enemy.isSelected)
                   {
                       BoundingSphereRenderer.Render(directionSphere, GraphicsDevice, ourCamera.viewMatrix, 
                           ourCamera.projectionMatrix, mouseOverColor);
                       BoundingSphereRenderer.Render(enemy.modelBoundingSphere, GraphicsDevice, ourCamera.viewMatrix, 
                           ourCamera.projectionMatrix, Color.Yellow);
                   }

              }
            base.Draw(gameTime);
        }
        
        public static bool checkIsSelected(Vector3 mouse3dVector,
                BoundingSphere sphere)
        {
            MouseState currentState = Mouse.GetState();
            BoundingSphere mouseSphere = new BoundingSphere(mouse3dVector, 15f);
            if (sphere.Intersects(mouseSphere)) return true;

            return false;
        }

        public static newShipStruct spawnNPC(NPCManager modelManager, Vector3 mouse3dVector, ref List<shipData> shipDefList,
                                   GameTime gameTime, Camera ourCamera, string shipName, int shipIndex, int team)
        {
            newShipStruct tempData = new newShipStruct();
            tempData.objectIndex = shipIndex;
            tempData.objectFileName = shipDefList[shipIndex].FileName;
            tempData.objectAlias = shipName;
            tempData.shipModel = modelManager.LoadModel(shipDefList[shipIndex].FileName);
            tempData.objectAgility = shipDefList[shipIndex].Agility;
            tempData.objectMass = shipDefList[shipIndex].Mass;
            tempData.objectThrust = shipDefList[shipIndex].Thrust;
            tempData.objectType = shipDefList[shipIndex].Type;
            tempData.hullLvl = shipDefList[shipIndex].hullValue;
            tempData.shieldLvl = shipDefList[shipIndex].shieldValue;
            tempData.shieldFactor = shipDefList[shipIndex].ShieldFactor;
            tempData.hullFactor = shipDefList[shipIndex].HullFactor;
            tempData.team = team;
            tempData.objectClass = shipDefList[shipIndex].ShipClass;
            tempData.modelPosition = mouse3dVector;
            tempData.modelRotation = Matrix.Identity;// *Matrix.CreateRotationY(MathHelper.ToRadians(-90));
            tempData.Direction = Vector3.Forward;
            //tempData.targetPosition = tempData.modelPosition + tempData.Direction * 10000;
            tempData.wayPointPosition = tempData.targetPosition;
            if (team == 0)
                tempData.currentDisposition = disposition.patrol;
            else
                tempData.currentDisposition = disposition.patrol;
            tempData.currentTarget = null;
            if (tempData.objectClass == ClassesEnum.Capitalship)
                tempData.currentDisposition = disposition.defensive;
            tempData.Up = Vector3.Up;
            tempData.modelBB = HelperClass.ComputeBoundingBox(tempData.shipModel, tempData.modelPosition);
            tempData.modelLen = tempData.modelBB.Max.X - tempData.modelBB.Min.X;
            tempData.modelWidth = tempData.modelBB.Max.Z - tempData.modelBB.Min.Z;
            tempData.modelFrustum = new BoundingFrustum(Matrix.Identity);
            tempData.portFrustum = new BoundingFrustum(Matrix.Identity);
            tempData.starboardFrustum = new BoundingFrustum(Matrix.Identity);
            tempData.radius = tempData.modelLen;
            tempData.modelBoundingSphere = new BoundingSphere(mouse3dVector, tempData.radius / 2);
            //tempData.shipThruster = new Athruster();
           // tempData.shipThruster.LoadContent(Game, spriteBatch);
            tempData.weaponArray = shipDefList[shipIndex].AvailableWeapons;
            tempData.currentWeapon = tempData.weaponArray[0];
            tempData.EvadeDist = shipDefList[shipIndex].EvadeDist;
            tempData.TargetPrefs = shipDefList[shipIndex].TargetPrefs;
            tempData.ChasePrefs = shipDefList[shipIndex].Chase;
            tempData.squadNo = -1;
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
            tempData.regenTimer = new double[moduleCount];

            modelManager.updateShipMovement(gameTime, 5.0f, tempData, ourCamera, true);
            return tempData;
        }
    }
}