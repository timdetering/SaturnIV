 using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Intermediate;

namespace SaturnIV
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        public CameraNew ourCamera;
        Vector3 defaultCameraOffset = new Vector3(10, 5000, 0);
        public static GraphicsDeviceManager graphics;
        SpriteFont spriteFont;
        SpriteFont spriteFontSmall;
        Random rand;
        public GraphicsDevice device;
        public Viewport viewport;
        KeyboardState oldkeyboardState;
        public static MouseState mouseStateCurrent, mouseStatePrevious;
        bool isRclicked, isLclicked, isRdown, isLdown,isSelected;
        int thisTeam;
        public static bool isTacmap;
        float mapScrollSpeed = 380f;
        Vector3 farpoint, nearpoint = new Vector3(0,0,0);
        SpriteBatch spriteBatch;
        HelperClass helperClass;
        double lastKeyPressTime;
        gameServer gServer;
        gameClient gClient;
        Texture2D rectTex, shipRec, selectRecTex,dummyTex;
        MessageClass messageClass;
        public Vector2 systemMessagePos = new Vector2(55, 30);
        public StringBuilder messageBuffer = new StringBuilder();
        public static List<string> messageLog = new List<string>();
        public Vector3[] plyonOffset;
        Line3D fLine;
        public Rectangle selectionRect;
        public List<Texture2D> objectThumbs = new List<Texture2D>();
        Ray currentMouseRay;
        HealthBarClass bar;
        Color shipColor;
        EditModeComponent editModeClass;
        public List<newShipStruct> activeShipList = new List<newShipStruct>();
        public List<squadClass> squadList = new List<squadClass>();
        squadClass thisSquad;
        public NPCManager npcManager;
        public PlayerManager playerManager;
        public static newShipStruct playerShip = new newShipStruct();
        public ModelManager modelManager;
        public WeaponsManager weaponsManager;
        public static List<shipData> shipDefList = new List<shipData>();
        public List<weaponData> weaponDefList = new List<weaponData>();
        public SerializerClass serializerClass;
        public RandomNames rNameList = new RandomNames();
        public string tmpShipName;
        ExplosionClass ourExplosion;
        public PlanetManager planetManager;
        public Effect effect;
        public Matrix cameraTarget;
        public Vector3 cameraTargetVec3;
        int typeSpeed = 125;
        SkySphere skySphere;
        RenderStarfield starField;
        VertexDeclaration vertexDeclaration;
        ControlPanelClass cPanel = new ControlPanelClass();
        public float gameSpeed = 5.0f;
        //public Camera ourCamera;
        bool isEditMode = false;
        bool isServer = false;
        bool isClient = false;
        bool isChat = false;
        bool isDebug = false;
        bool isInvalidArea = false;
        public static bool drawTextbox = false;
        newShipStruct potentialTarget;
        int screenX, screenY, screenCenterX, screenCenterY;
        ParticleSystem projectileTrailParticles;
        public string chatMessage;
        Vector3 isRight;
        guiClass Gui;

        // Define Hud Components
        Texture2D centerHUD;
        Texture2D targetTracker;
        public static Dictionary<string, Model> modelDictionary = new Dictionary<string, Model>();
        
        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            ConfigureGraphicsManager();
            Content.RootDirectory = "Content";
            //var models = Content.LoadContent<Model>("Models");
        }

        protected override void Initialize()
        {
            helperClass = new HelperClass();
            //ourCamera = new CameraNew(screenCenterX, screenCenterY);
            //ourCamera.ResetCamera();
            ourCamera = new CameraNew();
            ourCamera.ResetCamera();
            cameraTargetVec3 = new Vector3(0, 10000, 50);
            CameraNew.offsetDistance = new Vector3(-2400, 3200, 250);
            rand = new Random();
////////////TODO: Add your initialization logic here
            spriteBatch = new SpriteBatch(GraphicsDevice);
            serializerClass = new SerializerClass();
            messageClass = new MessageClass();
            bar = new HealthBarClass(this);
            bar.Initialize();
            activeShipList = new List<newShipStruct>();
////////////Initialize Manager Classes
            modelManager = new ModelManager(this);
            modelManager.Initialize();
            playerManager = new PlayerManager(this);
            playerManager.Initialize();
            npcManager = new NPCManager(this);
            npcManager.Initialize();
            weaponsManager = new WeaponsManager(this);
            weaponsManager.Initialize();
////////////Initalize Starfield
            starField = new RenderStarfield(this);
            InitializeStarFieldEffect();
            //firingArc = new renderTriangle();
            editModeClass = new EditModeComponent(this);
            editModeClass.Initialize(modelManager);
            ourExplosion = new ExplosionClass();
            ourExplosion.initExplosionClass(this);            
            Gui = new guiClass();
            fLine = new Line3D(GraphicsDevice);          
            skySphere = new SkySphere(this);
            planetManager = new PlanetManager(this);
            planetManager.Initialize();           
////////////Mousey Stuff
            this.IsMouseVisible = false;
            Mouse.SetPosition(graphics.GraphicsDevice.Viewport.Width / 2, graphics.GraphicsDevice.Viewport.Height / 2);
            //mouseStateCurrent = Mouse.GetState();
////////////Network Stuff
            gServer = new gameServer();
            gClient = new gameClient();
////////////Random Stuff             
            projectileTrailParticles = new ProjectileTrailParticleSystem(this, Content);
////////////Add Components
            Components.Add(projectileTrailParticles);
            Components.Add(editModeClass);
            base.Initialize();
        }

        public void ConfigureGraphicsManager()
        {
            screenX = 1280;
            screenY = 1024;
            graphics.PreferredBackBufferWidth = screenX;
            graphics.PreferredBackBufferHeight = screenY;
            screenCenterX = screenX / 2;
            screenCenterY = screenY /2;
            graphics.IsFullScreen = false;      
        }

        protected override void LoadContent()
        {
            device = graphics.GraphicsDevice;
            viewport = device.Viewport;
            spriteFont = this.Content.Load<SpriteFont>("MedFont");
            spriteFontSmall = this.Content.Load<SpriteFont>("SmallFont");
            loadMetaData();
            Gui.initalize(this, ref shipDefList);
            cPanel.LoadPanel(Content, spriteBatch);
            rectTex = this.Content.Load<Texture2D>("textures//SelectionBox");
            dummyTex = this.Content.Load<Texture2D>("textures//dummy");
            shipRec = this.Content.Load<Texture2D>("textures//missiletrack");
            selectRecTex = this.Content.Load<Texture2D>("textures//SelectionBox");
            centerHUD = this.Content.Load<Texture2D>("textures//HUD/centertarget");
            targetTracker = this.Content.Load<Texture2D>("textures//HUD/target_track");
            skySphere.LoadSkySphere(this);
            starField.LoadStarFieldAssets(this);
            // Init Player ship
            playerShip = EditModeComponent.spawnNPC(Vector3.Zero, ref shipDefList, "player1", 2, 0);
            playerShip.modelRotation *= Matrix.CreateRotationY(MathHelper.ToRadians(90));
        }

        private void loadMetaData()
        {
            /// <summary>
            /// Load Ship/Unit Data from XML            
            /// </summary>
            /// <param name="ShipData">Provide Reference to List to populate</param>
            serializerClass.loadMetaData(ref shipDefList, ref weaponDefList, ref rNameList);
            foreach (shipData thisShip in shipDefList)
            {
                if (!modelDictionary.ContainsKey(thisShip.FileName))
                {
                    modelDictionary.Add(thisShip.FileName, Content.Load<Model>(thisShip.FileName));
                     //messageClass.messageLog.Add("Loading " + thisShip.FileName);
                }
            }

            foreach (weaponData thisWeapon in weaponDefList)
            {
                if (!modelDictionary.ContainsKey(thisWeapon.FileName))
                {
                    modelDictionary.Add(thisWeapon.FileName, Content.Load<Model>(thisWeapon.FileName));
                    //SystemLogClass.messageLog.Add("Loading " + thisWeapon.assetString);
                }
            }
        }
       
        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        protected override void Update(GameTime gameTime)
        {
            processInput(gameTime);
            
            cameraTarget = Matrix.CreateWorld(playerShip.modelPosition, Vector3.Forward, Vector3.Up);
            if (isEditMode)
            {
                ourCamera.ResetCamera();
                CameraNew.offsetDistance = new Vector3(0, 20000, 250);
                CameraNew.currentCameraMode = CameraNew.CameraMode.orbit;
                ourCamera.Update(cameraTarget, isEditMode);
                this.IsMouseVisible = true;
                editModeClass.Update(gameTime, currentMouseRay, mouse3dVector, ref activeShipList, isLclicked, isRclicked, isLdown,
                    ref npcManager, ourCamera, ref viewport);
                Gui.update(mouseStateCurrent, mouseStatePrevious);
            }
            else
            {                
                CameraNew.offsetDistance = new Vector3(0, 20000, 250);
                CameraNew.currentCameraMode = CameraNew.CameraMode.orbit;
                ourCamera.Update(cameraTarget, isEditMode);
                updateObjects(gameTime);
                if (Keyboard.GetState().IsKeyDown(Keys.LeftShift))
                    Mouse.SetPosition(graphics.GraphicsDevice.Viewport.Width / 2, graphics.GraphicsDevice.Viewport.Height / 2);
            }

            if (weaponsManager.activeWeaponList.Count > 0)
                helperClass.CheckForCollision(gameTime, ref activeShipList, ref weaponsManager.activeWeaponList, 
                    ref ourExplosion);            
            
            if (isServer)
                gServer.update(ref activeShipList);
            if (isClient)
                gClient.Update(ref activeShipList, ref npcManager);

            if (selectionRect != Rectangle.Empty)
                RectangleSelect(activeShipList, viewport, ourCamera.projectionMatrix, ourCamera.viewMatrix, 
                    selectionRect);
 
            base.Update(gameTime);
        }

        protected void updateObjects(GameTime gameTime)
        {
            for (int i = 0; i < activeShipList.Count; i++)
            {
                if (!isClient)
                {
                    if (activeShipList[i].squadNo > -1)
                        thisSquad = squadList[activeShipList[i].squadNo];
                    else
                        thisSquad = null;
                    for (int j = 0; j < activeShipList.Count; j++)
                    {
                        if (activeShipList[j] != activeShipList[i])
                        {
                            npcManager.performAI(gameTime, ref weaponsManager, ref projectileTrailParticles, ref weaponDefList,
                                activeShipList[i], activeShipList[j], j, thisSquad);
                            //npcManager.performAI(gameTime, ref weaponsManager, ref projectileTrailParticles, ref weaponDefList,
                            //    activeShipList[i], playerShip, j, thisSquad);
                        }
                    }
                }
                npcManager.updateShipMovement(gameTime, gameSpeed, activeShipList[i], ourCamera,false);
            }
            playerManager.updateShipMovement(gameTime, gameSpeed, Keyboard.GetState(), playerShip, ourCamera);
            weaponsManager.Update(gameTime, gameSpeed, ourExplosion);
        }

        protected void processInput(GameTime gameTime)
        {
            double currentTime = gameTime.TotalGameTime.TotalMilliseconds;
            KeyboardState keyboardState = Keyboard.GetState();
            mouseStateCurrent = Mouse.GetState();
            // Check if I need to do anything triggered by the gui menu in Edit Mode or elsewhere (etc. Load Scenario)....
            if (guiClass.LoadScenario)
                serializerClass.loadScenario(Gui.loadThisScenario ,ref activeShipList, ref shipDefList);

            if (mouseStateCurrent.LeftButton == ButtonState.Pressed &&
                mouseStatePrevious.LeftButton == ButtonState.Released)
            {
                isLclicked = true;
                isLdown = false;
            }
            else if (mouseStateCurrent.LeftButton == ButtonState.Pressed &&
                     mouseStatePrevious.LeftButton == ButtonState.Pressed)
            {
                isLclicked = false;
                isLdown = true;
                if (!isEditMode)
                {
                    isSelected = true;
                    selectionRect = selectRectangle(mouseStateCurrent, mouse3dVector);
                }
            }
            else
            {
                isLclicked = false;
                isLdown = false;
                selectionRect = Rectangle.Empty;
            }

            if (mouseStateCurrent.RightButton == ButtonState.Pressed &&
    mouseStatePrevious.RightButton == ButtonState.Released)
            {
                isRclicked = true;
                isRdown = false;
            }
            else if (mouseStateCurrent.RightButton == ButtonState.Pressed &&
         mouseStatePrevious.RightButton == ButtonState.Pressed)
            {
                isRclicked = false;
                isRdown = true;
            }
            else
            {
                isRclicked = false;
                isRdown = false;
            }

            if (isRclicked && isEditMode && !EditModeComponent.isSelecting)
            {
                isInvalidArea = false;
                potentialTarget = null;
                foreach (newShipStruct thisShip in activeShipList)
                    if (EditModeComponent.checkIsSelected(mouse3dVector, thisShip.modelBoundingSphere))
                    {
                        isInvalidArea = true;
                        potentialTarget = thisShip;
                        break;
                    }
                    if (!isInvalidArea)
                    {
                        int rLen = rNameList.capitalShipNames.Count;
                        int i = rand.Next(0, rLen);
                        tmpShipName = rNameList.capitalShipNames[2];
                        rNameList.capitalShipNames.Remove(tmpShipName);
                        messageClass.sendSystemMsg(spriteFont, spriteBatch, tmpShipName + " Added", systemMessagePos);
                        newShipStruct newShip = EditModeComponent.spawnNPC(mouse3dVector, ref shipDefList, 
                            tmpShipName, Gui.thisShip, Gui.thisFaction);
                        activeShipList.Add(newShip);
                    }
            }

            if (isLclicked && !isEditMode)
            {
               isSelected = false;
               foreach (newShipStruct thisShip in activeShipList)
                   if (EditModeComponent.checkIsSelected(mouse3dVector, thisShip.modelBoundingSphere))
                   {
                           thisShip.isSelected = true;
                           isSelected = true;
                    }
                   else
                       thisShip.isSelected = false;
            }

            if (isRclicked && !isEditMode && isSelected)
            {
                isInvalidArea = false;
                potentialTarget = null;
                foreach (newShipStruct thisShip in activeShipList)
                    if (EditModeComponent.checkIsSelected(mouse3dVector, thisShip.modelBoundingSphere))
                    {
                        isInvalidArea = true;
                        potentialTarget = thisShip;
                        break;
                    }

                foreach (newShipStruct thisShip in activeShipList)
                    if (thisShip.isSelected)
                    {
                            if (potentialTarget != null && potentialTarget.team != thisShip.team)
                                thisShip.currentTarget = potentialTarget;
                                thisShip.currentDisposition = disposition.moving;
                                thisShip.targetPosition = mouse3dVector;
                                thisShip.wayPointPosition = mouse3dVector;
                    }
            }           
            if (isChat) typeSpeed = 50;
            else
                typeSpeed = 150;

            if (currentTime - lastKeyPressTime > typeSpeed)
            {
                playerShip.currentWeaponMod = 0;
                if (keyboardState.IsKeyDown(Keys.Space) && !isEditMode && !isChat)
                    if (currentTime - playerShip.regenTimer[playerShip.currentWeaponMod] > weaponDefList[(int)playerShip.weaponArray[0].weaponType].regenTime)
                    {
                        weaponsManager.fireWeapon(playerShip.currentTarget, playerShip, ref projectileTrailParticles, ref weaponDefList, playerShip.weaponArray[0], playerShip.currentWeaponMod);
                        playerShip.regenTimer[playerShip.currentWeaponMod] = currentTime;
                        playerShip.isEngaging = true;                    
                    }

                if (keyboardState.IsKeyDown(Keys.E) && !isEditMode && !isChat)
                {
                    isEditMode = true;
                    string msg = "Edit Mode";
                    messageClass.sendSystemMsg(spriteFont, spriteBatch,msg, systemMessagePos);
                    CameraNew.zoomFactor = 8.0f;                    
                }
                else if (keyboardState.IsKeyDown(Keys.E) && isEditMode && !isChat)
                    isEditMode = false;

                // Chat Mode Handler //
                if (keyboardState.IsKeyDown(Keys.C) )
                {
                    if (isChat)
                    {
                        isChat = false;
                        string msg = "Chat Off";
                        messageClass.sendSystemMsg(spriteFont, spriteBatch, msg, systemMessagePos);
                    }
                    else
                    {
                        isChat = true;
                        string msg = "Chat On";
                        messageClass.sendSystemMsg(spriteFont, spriteBatch, msg, systemMessagePos);
                    }
                }
                else if (isChat)
                {
                    string pmsg = chatMessage;
                    chatMessage += helperClass.UpdateInput();
                    string msg = chatMessage;
                    if (pmsg != msg)
                        messageClass.sendSystemMsg(spriteFont, spriteBatch, msg, systemMessagePos);
                }

                if (isChat && keyboardState.IsKeyDown(Keys.Enter))// && !oldkeyboardState.IsKeyDown(Keys.Enter))
                {
                    if (isClient)
                        gClient.SendChat(chatMessage);
                    if (isServer)
                        gServer.SendChat(chatMessage);
                    messageClass.sendSystemMsg(spriteFont, spriteBatch, chatMessage, systemMessagePos);
                    chatMessage = "";
                    //systemMessagePos.Y += 10;
                }
 
                // Turn on/off Server/Client Mode
                if (keyboardState.IsKeyDown(Keys.F1) && !isServer)
                {
                    isServer = true;
                    isClient = false;
                    thisTeam = 0;
                    string msg = "Network: Server Mode";
                    messageClass.sendSystemMsg(spriteFont, spriteBatch, msg, systemMessagePos);
                    gServer.initializeServer();
                }
                else if (keyboardState.IsKeyDown(Keys.F2) && !isServer && !isClient)
                {
                    isServer = false;
                    isClient = true;
                    thisTeam = 1;
                    string msg = "Network:Client Mode";
                    messageClass.sendSystemMsg(spriteFont, spriteBatch, msg, systemMessagePos);
                    gClient.initializeNetwork();
                }               
                lastKeyPressTime = currentTime;
            }

            if (keyboardState.IsKeyDown(Keys.F12) && !oldkeyboardState.IsKeyDown(Keys.F12) && !isDebug && !isChat)
            {
                MessageClass.messageLog.Add("Debug Mode On");
                isDebug = true;
            }
            else if (keyboardState.IsKeyDown(Keys.F12) && !oldkeyboardState.IsKeyDown(Keys.F12) && isDebug && !isChat)
            {
                MessageClass.messageLog.Add("Debug Mode Off");
                isDebug = false;
            }
                mouseStatePrevious = mouseStateCurrent;
                oldkeyboardState = keyboardState;
}
         private void InitializeStarFieldEffect()
        {
            vertexDeclaration = new VertexDeclaration(GraphicsDevice, VertexPositionColor.VertexElements);
            Matrix wvp = (Matrix.CreateScale(5.0f) * Matrix.CreateFromQuaternion(Quaternion.Identity) *
                Matrix.CreateTranslation(Vector3.Zero)) * ourCamera.viewMatrix * ourCamera.projectionMatrix;
            Matrix worldMatrix = Matrix.CreateTranslation(GraphicsDevice.Viewport.Width / 4f - 150,
                GraphicsDevice.Viewport.Height / 4f - 50, 0);
        }

        protected override void Draw(GameTime gameTime)
        {
            float time = (float)gameTime.TotalGameTime.TotalMilliseconds / 100.0f;
            graphics.GraphicsDevice.Clear(Color.Black);
            // Draw Skybox and Starfield Elements
            skySphere.DrawSkySphere(this, ourCamera);
            starField.DrawStars(this, ourCamera);
            //planetManager.DrawPlanets(gameTime, ourCamera.viewMatrix, ourCamera.projectionMatrix,ourCamera);
            //cPanel.Draw();
            foreach (newShipStruct npcship in activeShipList)
            {
                modelManager.DrawModel(ourCamera, modelDictionary[npcship.objectFileName], npcship.worldMatrix, shipColor);
                if (isDebug)
                    debug(npcship);                
            }
            modelManager.DrawModel(ourCamera, modelDictionary[playerShip.objectFileName], playerShip.worldMatrix, Color.Blue);
            projectileTrailParticles.SetCamera(ourCamera.viewMatrix, ourCamera.projectionMatrix);
            foreach (weaponStruct theList in weaponsManager.activeWeaponList)
            {
                if (theList.isProjectile)
                    modelManager.DrawModel(ourCamera, modelDictionary[theList.objectFileName], theList.worldMatrix, Color.White);
                else if (theList.objectClass == WeaponClassEnum.Beam)
                {
                    theList.beamQuad.DrawQuad(ourCamera.viewMatrix,
                        ourCamera.projectionMatrix, Matrix.Identity, theList.beamQuad);
                }
                else
                    weaponsManager.DrawLaser(device, ourCamera.viewMatrix, ourCamera.projectionMatrix, theList.objectColor, theList);
            }            
             ourExplosion.DrawExp(gameTime, ourCamera, GraphicsDevice);           
            helperClass.DrawFPS(gameTime, device, spriteBatch, spriteFont);
            DrawHUDTargets(gameTime);
            spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Deferred, SaveStateMode.SaveState);
            //radar.Draw(spriteBatch, (float)System.Math.Atan2(playerShip.Direction.Z, playerShip.Direction.X), playerShip.modelPosition, ref activeShipList);
            if (isEditMode) editModeClass.Draw(gameTime, ref activeShipList, ourCamera,spriteBatch,mouse3dVector);
            if (isEditMode) Gui.drawGUI(spriteBatch,spriteFont);
            spriteBatch.End();
                spriteBatch.Begin();
                // We need to fix the selection rectangle in case one of its dimensions is negative                
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
                spriteBatch.End();
                if (drawTextbox && ControlPanelClass.textBoxActions == TextBoxActions.SaveScenario)
                    cPanel.drawTextbox(spriteBatch, "Scenario: ", new Vector2(screenX / 2 - 50, screenY / 2 - 50),
                        activeShipList, serializerClass);

            messageClass.sendSystemMsg(spriteFont, spriteBatch,null, systemMessagePos);
            base.Draw(gameTime); 
        }

        private void DrawHUDTargets(GameTime gameTime)
        {
            bool isDone = false;
            StringBuilder messageBuffer = new StringBuilder(); 
            spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Deferred, SaveStateMode.SaveState);
            Vector2 fontPos = new Vector2(0,0);
            foreach (newShipStruct enemy in activeShipList)
            {
                StringBuilder buffer = new StringBuilder();
                if (enemy.isSelected && !isDone)
                {
                    isDone = true;
                    int starty = 600;
                    int timerIndex = 0;
                        double hbarValue;
                        int hbarwidth = 100;
                        buffer = new StringBuilder();
                        buffer.AppendLine(enemy.objectAlias + "");
                        spriteBatch.DrawString(spriteFont, buffer.ToString(), new Vector2(1100, starty - 38), Color.White);
                        
                        foreach (WeaponModule thisMod in enemy.weaponArray)
                        {
                            buffer = new StringBuilder();
                            buffer.AppendLine(thisMod.weaponType + "");
                            spriteBatch.DrawString(spriteFont, buffer.ToString(), new Vector2(1100, starty-18), Color.Green);
                            foreach (Vector4 thisWeapon in thisMod.ModulePositionOnShip)
                            {
                                    double currentTime = gameTime.TotalGameTime.TotalMilliseconds - enemy.regenTimer[timerIndex];
                                    if (enemy.regenTimer[timerIndex] == 0)
                                        currentTime = 0;
                                        hbarValue = currentTime / weaponDefList[(int)thisMod.weaponType].regenTime * 100;
                                        timerIndex++;
                                        hbarValue = hbarValue / hbarwidth * 100;
                                        if (hbarValue > hbarwidth) hbarValue = hbarwidth;
                                        bar.DrawHbar(gameTime, spriteBatch, Color.Red, 1100, starty, hbarwidth, 10, hbarwidth);
                                        bar.DrawHbar(gameTime, spriteBatch, Color.LightBlue, 1100, starty, hbarwidth, 10, (int)hbarValue);
                                        starty += 20;
                                }
                            starty += 10;
                        }
                    }
                if (Project(enemy.modelPosition, viewport, ourCamera, Matrix.Identity).Z < 1)
                {
                    switch(enemy.team)
                    {
                        case 0:
                            shipColor = Color.Blue;
                            break;
                        case 1:
                            shipColor = Color.Green;
                            break;
                    }
                    if (enemy.isSelected)
                        shipColor = Color.White;
                    fontPos = new Vector2(enemy.screenCords.X, enemy.screenCords.Y - 45);
                    buffer.AppendFormat("[" + enemy.objectAlias + "]");
                    buffer.AppendFormat("[Evade:" + enemy.isEvading + "]");
                    buffer.AppendFormat("[AOA:" + enemy.angleOfAttack + "]");
                    buffer.AppendFormat("[" + enemy.isBehind + "]");
                    spriteBatch.DrawString(spriteFontSmall, buffer.ToString(), fontPos, shipColor);
                }
            }
            spriteBatch.DrawString(spriteFont, messageBuffer.ToString(), new Vector2(0,0), Color.White);           
            spriteBatch.End();
            DrawHUD(gameTime);
        }

        private void DrawHUD(GameTime gameTime)
        {
            StringBuilder messageBuffer = new StringBuilder(); 
            spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Deferred, SaveStateMode.SaveState);
            spriteBatch.DrawString(spriteFont, messageBuffer.ToString(), new Vector2(0,0), Color.White);
            messageBuffer = new StringBuilder();
            messageBuffer.AppendFormat("\nZoomFactor {0} ", CameraNew.zoomFactor);
            messageBuffer.AppendFormat("\nCamera Mode " + CameraNew.currentCameraMode);
            messageBuffer.AppendFormat("\nMenu CurrentSlection: " + guiClass.currentSelection);
            spriteBatch.DrawString(spriteFont, messageBuffer.ToString(), new Vector2(screenCenterX +
                                    (screenX / 6) - 150, screenCenterY + (screenY / 3)), Color.White);
            messageBuffer = new StringBuilder();
            spriteBatch.DrawString(spriteFont, messageBuffer.ToString(), systemMessagePos, Color.Blue);
            //spriteBatch.Draw(centerHUD, new Vector2(screenCenterX - 149, screenCenterY - 155), Color.Wheat);

            StringBuilder buffer = new StringBuilder();
            buffer.AppendFormat("Ninja76");
            spriteBatch.DrawString(spriteFont, buffer.ToString(), new Vector2(playerShip.screenCords.X - 16, playerShip.screenCords.Y - 16), Color.Gray);
            spriteBatch.End();
        }

        // Rectangle Drag N Drop routines //
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

        public void RectangleSelect(List<newShipStruct> objectsList, Viewport viewport, Matrix projection, Matrix view, Rectangle selectionRect)
        {
            foreach (newShipStruct o in objectsList)
            {
                Vector3 screenPos = viewport.Project(o.modelPosition, projection, view, Matrix.Identity);
                if (selectionRect.Contains((int)screenPos.X, (int)screenPos.Y) && o.team == 0)
                    o.isSelected = true;
                else
                    o.isSelected = false;
            }
        }

        private Vector3 Project(Vector3 point, Viewport wholeViewport, CameraNew camera, Matrix world)
        {
            Vector4 mp = Vector4.Transform(new Vector4(point, 1.0f), Matrix.Invert(world));
            Vector3 pt = wholeViewport.Project(new Vector3(mp.X, mp.Y, mp.Z), ourCamera.projectionMatrix, ourCamera.viewMatrix, world);
            return pt;
        } 

        Vector3 mouse3dVector
        {
            get
            {
                MouseState mouseState = Mouse.GetState();
                int mouseX = mouseState.X;
                int mouseY = mouseState.Y;
                Vector3 nearsource = new Vector3((float)mouseX, (float)mouseY, 0f);
                Vector3 farsource = new Vector3((float)mouseX, (float)mouseY, 1f);

                Matrix world = Matrix.CreateTranslation(0, 0, 0);

                nearpoint = GraphicsDevice.Viewport.Unproject(nearsource,
                 ourCamera.projectionMatrix, ourCamera.viewMatrix, Matrix.Identity);

                farpoint = GraphicsDevice.Viewport.Unproject(farsource,
                    ourCamera.projectionMatrix, ourCamera.viewMatrix, Matrix.Identity);
                // Create a ray from the near clip plane to the far clip plane.
                Vector3 direction = farpoint - nearpoint;
                direction.Normalize();
                currentMouseRay = new Ray(nearpoint, direction);

                //Check if the ray is pointing down towards the ground
                //(aka will it intersect the plane)
                if (currentMouseRay.Direction.Y < 0)
                {
                    float xPos = 0f;
                    float zPos = 0f;

                    //Move the ray lower along its direction vector
                    while (currentMouseRay.Position.Y > 0)
                    {
                        currentMouseRay.Position += currentMouseRay.Direction;
                        xPos = currentMouseRay.Position.X;
                        zPos = currentMouseRay.Position.Z;
                    }
                    return new Vector3(xPos, 0, zPos);
                }
                else
                    return new Vector3(0, 0, 0);
            }
        }

        public void debug (newShipStruct npcship)
        {
             fLine.Draw(npcship.modelPosition, npcship.targetPosition, Color.Blue, ourCamera.viewMatrix, ourCamera.projectionMatrix);
                   foreach (BoundingFrustum bf in npcship.moduleFrustum)                        
                     BoundingFrustumRenderer.Render(bf, device, ourCamera.viewMatrix, ourCamera.projectionMatrix, Color.White);
                   BoundingFrustumRenderer.Render(npcship.portFrustum, device, ourCamera.viewMatrix, ourCamera.projectionMatrix, Color.White);
                    BoundingFrustumRenderer.Render(npcship.starboardFrustum, device, ourCamera.viewMatrix, ourCamera.projectionMatrix, Color.White);

                    isRight = npcship.modelRotation.Right;
        }       
    }

}
