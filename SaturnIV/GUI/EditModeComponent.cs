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
        public static bool isSelecting = false;
        MouseState mouseOld;
        public static bool selected = false;
        MouseState prevMouseState;
        MouseState mouseCurrent;
        KeyboardState keyboardState;
        Vector3 mousePosOld;
        static bool isStuffSelected = false;
        static Vector3 offset;
        Vector3 mousePos;
        public Rectangle selectionRect;
        public BoundingBox selectionBB;
        float lineFactor = 2000.00f;
        List<newShipStruct> selectedGroup = new List<newShipStruct>();
        ModelManager modelManager;
        Texture2D selectRecTex;

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

            selectRecTex = Game.Content.Load<Texture2D>("textures/SelectionBox");
            isSelecting = false;
            fLine = new Line3D(Game.GraphicsDevice);
            selectionBB = new BoundingBox();
            base.Initialize();
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public void Update(GameTime gameTime, Ray currentMouseRay, Vector3 mouse3dVector, 
                                    ref List<newShipStruct> objectList,bool isLClicked, bool isRClicked,bool isLDepressed, 
                                    ref NPCManager npcManager,CameraNew ourCamera, ref Viewport viewport)
        {
            // TODO: Add your update code here
            bool checkResult = false;
            bool isDirectionSphere = false;            
            mouseCurrent = Mouse.GetState();
            keyboardState = Keyboard.GetState();
            Vector3 myMouse3dVector = mouse3dVector;
             foreach (newShipStruct ourShip in objectList)                     
             {
                 checkResult = checkIsSelected(myMouse3dVector, ourShip.modelBoundingSphere);
                 if (checkResult) break;
             }
            if (isLDepressed && keyboardState.IsKeyDown(Keys.LeftShift))
            {
              isGroupSelect = true;
              selectRectangle(mouseCurrent, myMouse3dVector);
            } 
            //else
            if (!isLDepressed && isGroupSelect)
            {
                selectionRect = Rectangle.Empty; 
                isGroupSelect = false;
            }

            if (isLDepressed && isStuffSelected && selectionRect == Rectangle.Empty)
            {
               foreach (newShipStruct ourShip in objectList)
               {
                   if (ourShip.isSelected)
                   {
                         isDragging = true;
                         Vector3 newModelPosition = new Vector3(myMouse3dVector.X, 0, myMouse3dVector.Z);
                         ourShip.modelPosition = newModelPosition + ourShip.editModeOffset;
                         npcManager.updateShipMovement(gameTime, 50.0f, ourShip, ourCamera, true);
                         directionSphere.Center = ourShip.modelPosition + ourShip.Direction * lineFactor;
                   }
               }
            }

            if ((isLDepressed && isDirectionSphere && !isDragging)
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

            if (keyboardState.IsKeyDown(Keys.Escape) || (isRClicked && isStuffSelected))
            {
                selectionRect = Rectangle.Empty;
                isGroupSelect = false;
                isSelecting = false;
                isStuffSelected = false;
                unSelectAll(ref objectList);
            }
           if (isGroupSelect)
                RectangleSelect(objectList, viewport, ourCamera.projectionMatrix, ourCamera.viewMatrix, selectionRect);
            mousePosOld = mousePos;
            prevMouseState = mouseCurrent;
            base.Update(gameTime);
        }

        public void Draw(GameTime gameTime, ref List<newShipStruct> shipList,CameraNew ourCamera
            ,SpriteBatch spriteBatch)
        {
            //spriteBatch.Begin();
            // We need to fix the selection rectangle in case one of its dimensions is negative
            BoundingSphere groupBS = new BoundingSphere();
            Rectangle r = new Rectangle(selectionRect.X, selectionRect.Y, selectionRect.Width, selectionRect.Height);
            if (r.Width < 0)
            {
                r.Width = -r.Width;
                r.X -= r.Width;
            }
            if (r.Height < 0)
            {
                r.Height = -r.Height;
                r.Y -= r.Height;
            }
            spriteBatch.Draw(selectRecTex, r, Color.White);
            //spriteBatch.End();
                    
            foreach (newShipStruct enemy in shipList)
            {
                if (enemy.team == 0)
                {
                    if (enemy.objectClass == ClassesEnum.Crusier)
                    {
                        Matrix pMatrix = Matrix.CreateWorld(enemy.modelPosition, Vector3.Forward, Vector3.Up);
                        pMatrix *= Matrix.CreateScale(planisphereScale);
                        modelManager.DrawModel(ourCamera, blueLargePlane, pMatrix, Color.Blue, false);
                    }
                    else if (enemy.objectClass == ClassesEnum.Frigate)
                    {
                        Matrix pMatrix = Matrix.CreateWorld(enemy.modelPosition, Vector3.Forward, Vector3.Up);
                        pMatrix *= Matrix.CreateScale(planisphereScale);
                        modelManager.DrawModel(ourCamera, blueMediumPlane, pMatrix, Color.Blue, false);
                    }
                    else if (enemy.objectClass == ClassesEnum.Fighter)
                    {
                        Matrix pMatrix = Matrix.CreateWorld(enemy.modelPosition, Vector3.Forward, Vector3.Up);
                        pMatrix *= Matrix.CreateScale(planisphereScale);
                        modelManager.DrawModel(ourCamera, blueSmallPlane, pMatrix, Color.Blue, false);
                    }
                    else if (enemy.objectClass == ClassesEnum.Bomber)
                    {
                        Matrix pMatrix = Matrix.CreateWorld(enemy.modelPosition, Vector3.Forward, Vector3.Up);
                        pMatrix *= Matrix.CreateScale(planisphereScale);
                        modelManager.DrawModel(ourCamera, blueMediumPlane, pMatrix, Color.Blue, false);
                    }
                }
                else
                {
                    if (enemy.objectClass == ClassesEnum.Crusier)
                    {
                        Matrix pMatrix = Matrix.CreateWorld(enemy.modelPosition, Vector3.Forward, Vector3.Up);
                        pMatrix *= Matrix.CreateScale(planisphereScale);
                        modelManager.DrawModel(ourCamera, redLargePlane, pMatrix, Color.Blue, false);
                    }
                    else if (enemy.objectClass == ClassesEnum.Frigate)
                    {
                        Matrix pMatrix = Matrix.CreateWorld(enemy.modelPosition, Vector3.Forward, Vector3.Up);
                        pMatrix *= Matrix.CreateScale(planisphereScale);
                        modelManager.DrawModel(ourCamera, redMediumPlane, pMatrix, Color.Blue, false);
                    }
                    else if (enemy.objectClass == ClassesEnum.Fighter)
                    {
                        Matrix pMatrix = Matrix.CreateWorld(enemy.modelPosition, Vector3.Forward, Vector3.Up);
                        pMatrix *= Matrix.CreateScale(planisphereScale);
                        modelManager.DrawModel(ourCamera, redSmallPlane, pMatrix, Color.Blue, false);
                    }
                    else if (enemy.objectClass == ClassesEnum.Bomber)
                    {
                        Matrix pMatrix = Matrix.CreateWorld(enemy.modelPosition, Vector3.Forward, Vector3.Up);
                        pMatrix *= Matrix.CreateScale(planisphereScale);
                        modelManager.DrawModel(ourCamera, redMediumPlane, pMatrix, Color.Blue, false);
                    }
                }
                 fLine.Draw(enemy.modelPosition + enemy.Direction * enemy.radius,
                         enemy.modelPosition + enemy.Direction * 900,
                          Color.Orange, ourCamera.viewMatrix, ourCamera.projectionMatrix);
                
                if (enemy.isSelected)
                   {
                       groupBS = BoundingSphere.CreateMerged(groupBS, enemy.modelBoundingSphere);                      
                       BoundingSphereRenderer.Render(enemy.modelBoundingSphere, GraphicsDevice, ourCamera.viewMatrix, 
                           ourCamera.projectionMatrix, Color.Yellow);
                   }
              }
            Matrix pMatrix1 = Matrix.CreateWorld(Game1.playerShip.modelPosition, Vector3.Forward, Vector3.Up);
            pMatrix1 *= Matrix.CreateScale(planisphereScale);
            modelManager.DrawModel(ourCamera, redSmallPlane, pMatrix1, Color.Yellow, false);
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

        public static newShipStruct spawnNPC(Vector3 mouse3dVector, ref List<shipData> shipDefList,
                                    string shipName, int shipIndex, int team)
        {
            newShipStruct tempData = new newShipStruct();
            tempData.objectIndex = shipIndex;
            tempData.objectFileName = shipDefList[shipIndex].FileName;
            tempData.objectAlias = shipName;
            //tempData.shipModel = modelManager.LoadModel(shipDefList[shipIndex].FileName);
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
            tempData.modelRotation = Matrix.Identity;           
            tempData.Direction = Vector3.Forward;
            tempData.Direction = HelperClass.RandomDirection();
            //tempData.targetPosition = tempData.modelPosition + (tempData.Direction * 100000f);
            //tempData.targetPosition.Y = new Random().Next(-2000, 2000);
            //tempData.wayPointPosition = tempData.targetPosition;
            //tempData.wayPointPosition.Y = new Random().Next(-2000, 2000);
            if (team > 0)
                tempData.currentDisposition = disposition.patrol;
            else
                tempData.currentDisposition = disposition.idle;
            tempData.currentTarget = null;
            if (tempData.objectClass == ClassesEnum.Crusier)
                tempData.canEngageMultipleTargets = true;
            tempData.Up = Vector3.Up;
            tempData.modelBB = HelperClass.ComputeBoundingBox(Game1.modelDictionary[tempData.objectFileName], tempData.modelPosition);
            tempData.modelLen = tempData.modelBB.Max.X - tempData.modelBB.Min.X;
            tempData.modelWidth = tempData.modelBB.Max.Z - tempData.modelBB.Min.Z;
            tempData.modelFrustum = new BoundingFrustum(Matrix.Identity);
            tempData.portFrustum = new BoundingFrustum(Matrix.Identity);
            tempData.starboardFrustum = new BoundingFrustum(Matrix.Identity);
            tempData.radius = tempData.modelLen;
            tempData.modelBoundingSphere = new BoundingSphere(mouse3dVector, tempData.radius / 2);
            tempData.shipThruster = new Athruster();
           // tempData.shipThruster.LoadContent(Game, spriteBatch);
            tempData.weaponArray = shipDefList[shipIndex].AvailableWeapons;
            tempData.currentWeapon = tempData.weaponArray[0];
            tempData.EvadeDist = shipDefList[shipIndex].EvadeDist;
            tempData.TargetPrefs = shipDefList[shipIndex].TargetPrefs;
            tempData.ChasePrefs = shipDefList[shipIndex].ChasePrefs;
            tempData.maxDetectRange = shipDefList[shipIndex].maxDetectRange;
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

            //modelManager.updateShipMovement(gameTime, 5.0f, tempData, ourCamera, true);
            return tempData;
        }
        public Rectangle selectRectangle(MouseState mouseState, Vector3 mouse3d)
        {
            if (selectionRect == Rectangle.Empty)
                selectionRect = new Rectangle((int)mouseState.X, (int)mouseState.Y, 0, 0);
            else if (selectionRect != Rectangle.Empty)
            {
                selectionRect.Width = mouseState.X - selectionRect.X;
                selectionRect.Height = mouseState.Y - selectionRect.Y;
            } //else
            return selectionRect;
        }

        public static void unSelectAll(ref List<newShipStruct> objectList)
        {
            foreach (newShipStruct o in objectList)
                o.isSelected = false;
        }

        public static void RectangleSelect(List<newShipStruct> objectsList, Viewport viewport, Matrix projection, Matrix view, Rectangle selectionRect)
        {
            int i = 0;
            offset = Vector3.Zero;
            foreach (newShipStruct o in objectsList)
            {
                Vector3 screenPos = viewport.Project(o.modelPosition, projection, view, Matrix.Identity);
                if (selectionRect.Contains((int)screenPos.X, (int)screenPos.Y))
                {
                    o.isSelected = true;
                    isSelecting = true;
                    offset += o.modelPosition;
                    isStuffSelected = true;
                    i++;
                }
            }
            if (i>0)
                offset /= i;
            foreach (newShipStruct o in objectsList)
                o.editModeOffset = o.modelPosition - offset;
        }

    }
}