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
        Vector2 messagePos1 = new Vector2(0,0);
        public Vector2 systemMessagePos = new Vector2(10,100);
        public StringBuilder messageBuffer = new StringBuilder();
        public static List<string> messageLog = new List<string>();
        public Vector3[] plyonOffset;
        Line3D fLine;
        public Rectangle selectionRect;
        public List<Texture2D> objectThumbs = new List<Texture2D>();
        Ray currentMouseRay;
        RadarClass radar;
        HealthBarClass bar;
        Color shipColor;
        EditModeComponent editModeClass;
        public List<newShipStruct> activeShipList = new List<newShipStruct>();
        public List<squadClass> squadList = new List<squadClass>();
        squadClass thisSquad;
        public NPCManager npcManager;
        public PlayerManager playerManager;
        public ModelManager modelManager;
        public WeaponsManager weaponsManager;
        public static List<shipData> shipDefList = new List<shipData>();
        public List<weaponData> weaponDefList = new List<weaponData>();
        public SaveClass saveClass;
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
        public float gameSpeed = 5.0f;

        public renderTriangle firingArc;
        public Camera ourCamera;
        bool isEditMode = false;
        bool isServer = false;
        bool isClient = false;
        bool isChat = false;
        bool isDebug = false;
        bool isInvalidArea = false;
        newShipStruct potentialTarget;
        int screenX, screenY, screenCenterX, screenCenterY;
        ParticleSystem projectileTrailParticles;
        public string chatMessage;
        Vector3 isRight;
        guiClass Gui;
        

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            ConfigureGraphicsManager();
            Content.RootDirectory = "Content";
            //var models = Content.LoadContent<Model>("Models");
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            helperClass = new HelperClass();
            ourCamera = new Camera(screenCenterX, screenCenterY);
            ourCamera.ResetCamera();
            cameraTargetVec3 = new Vector3(0, 10000, 0);
            rand = new Random();
////////////TODO: Add your initialization logic here
            spriteBatch = new SpriteBatch(GraphicsDevice);
            saveClass = new SaveClass();
            messageClass = new MessageClass();
            messagePos1 = new Vector2(screenCenterX - (graphics.PreferredBackBufferWidth / 4), screenCenterY
                          - (graphics.PreferredBackBufferHeight / 3));
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
            firingArc = new renderTriangle();
            editModeClass = new EditModeComponent(this);
            editModeClass.Initialize(modelManager);
            ourExplosion = new ExplosionClass();
            ourExplosion.initExplosionClass(this);
            radar = new RadarClass(Content, "textures//redDotSmall", "textures//yellowDotSmall", "textures//blackDotLarge");
            Gui = new guiClass();
            fLine = new Line3D(GraphicsDevice);          
            skySphere = new SkySphere(this);
            planetManager = new PlanetManager(this);
            planetManager.Initialize();
            
////////////Mousey Stuff
            this.IsMouseVisible = true;
            Mouse.SetPosition(graphics.GraphicsDevice.Viewport.Width / 2, graphics.GraphicsDevice.Viewport.Height / 2);
            mouseStateCurrent = Mouse.GetState();

            //Network Stuff
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
            loadShipData();
            Gui.initalize(this, ref shipDefList);
            rectTex = this.Content.Load<Texture2D>("textures//SelectionBox");
            dummyTex = this.Content.Load<Texture2D>("textures//dummy");
            shipRec = this.Content.Load<Texture2D>("textures//missiletrack");
            selectRecTex = this.Content.Load<Texture2D>("textures//SelectionBox");
            skySphere.LoadSkySphere(this);
            starField.LoadStarFieldAssets(this);
        }

        private void loadShipData()
        {
            XmlReaderSettings xmlSettings = new XmlReaderSettings();
            XmlReader xmlReader = XmlReader.Create("shipdefs.xml");
            shipDefList = IntermediateSerializer.Deserialize<List<shipData>>(xmlReader,null);
            xmlReader = XmlReader.Create("weapondefs.xml");
            weaponDefList = IntermediateSerializer.Deserialize<List<weaponData>>(xmlReader, null);
            xmlReader = XmlReader.Create("listofnames.xml");
            rNameList = IntermediateSerializer.Deserialize<RandomNames>(xmlReader, null);
            foreach (shipData thisShip in shipDefList)
                objectThumbs.Add(this.Content.Load<Texture2D>(thisShip.ThumbFileName));
            foreach (shipData thisShip in shipDefList)
                 Content.Load<Model>(thisShip.FileName);
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
            cameraTarget = Matrix.CreateWorld(cameraTargetVec3, Vector3.Forward, Vector3.Up);            
            ourCamera.Update(cameraTarget);

            if (!isEditMode)
                updateObjects(gameTime);
            else
            {
                editModeClass.Update(gameTime, currentMouseRay, mouse3dVector, ref activeShipList, isLclicked, isRclicked, isLdown,
                    ref npcManager, ourCamera,ref viewport);
                Gui.update(mouseStateCurrent, mouseStatePrevious);
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
           //playerManager.updateShipMovement(gameTime,gameSpeed,Keyboard.GetState(),playerShip,ourCamera);
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
                            npcManager.performAI(gameTime, ref weaponsManager, ref projectileTrailParticles, ref weaponDefList,
                                                activeShipList[i], activeShipList[j], j, thisSquad);
                    }
                }
                npcManager.updateShipMovement(gameTime, gameSpeed, activeShipList[i], ourCamera,false);
            }
            weaponsManager.Update(gameTime, gameSpeed, ourExplosion);
        }

        protected void processInput(GameTime gameTime)
        {
            double currentTime = gameTime.TotalGameTime.TotalMilliseconds;
            KeyboardState keyboardState = Keyboard.GetState();
            mouseStateCurrent = Mouse.GetState();

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
                        newShipStruct newShip = EditModeComponent.spawnNPC(npcManager, mouse3dVector, ref shipDefList, 
                            tmpShipName, Gui.thisItem, Gui.thisTeam);
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
            // T will form a squad of all selected ships
            if (!isEditMode && isSelected && keyboardState.IsKeyDown(Keys.T) && 
                !oldkeyboardState.IsKeyDown (Keys.T))
            {
                squadClass createSquad = new squadClass();
                createSquad.squadmate = new List<newShipStruct>();
                createSquad.squadOrders = SquadDisposition.tight;
                squadList.Add(createSquad);

                bool isLeader = false;
                foreach (newShipStruct thisShip in activeShipList)
                    if (thisShip.isSelected && thisShip.squadNo < 0)
                    {
                        createSquad.squadmate.Add(thisShip);
                        if (!isLeader)
                        {
                            MessageClass.messageLog.Add(thisShip.objectAlias + " Squad Formed");
                            thisShip.isSquadLeader = true;
                            squadList[squadList.Count-1].leader = thisShip;
                        }
                        thisShip.squadNo = 0;
                        squadList[squadList.Count-1].squadmate.Add(thisShip);
                        isLeader = true;
                    }
            }
            if (!isChat)
            {
                if (keyboardState.IsKeyDown(Keys.A))
                    cameraTargetVec3 += Vector3.Left * mapScrollSpeed;
                if (keyboardState.IsKeyDown(Keys.D))
                    cameraTargetVec3 += Vector3.Right * mapScrollSpeed;
                if (keyboardState.IsKeyDown(Keys.W))
                    cameraTargetVec3 += Vector3.Forward * mapScrollSpeed;
                if (keyboardState.IsKeyDown(Keys.S))
                    cameraTargetVec3 += Vector3.Backward * mapScrollSpeed;
            }

            if (isChat) typeSpeed = 50;
            else
                typeSpeed = 150;

            if (currentTime - lastKeyPressTime > typeSpeed)
            {
                if (keyboardState.IsKeyDown(Keys.Z) && !isTacmap)
                {
                    isTacmap = true;
                    mapScrollSpeed = 1000;
                    cameraTargetVec3.Y = 90000;
                    Camera.zoomFactor = 5.0f;
                    messageClass.sendSystemMsg(spriteFont, spriteBatch, "Tactical Map On", systemMessagePos);
                }
                else
                    if (keyboardState.IsKeyDown(Keys.Z) && isTacmap && !isChat)
                    {
                        isTacmap = false;
                        mapScrollSpeed = 500f;
                        cameraTargetVec3.Y = 8000;
                        messageClass.sendSystemMsg(spriteFont, spriteBatch, "Tactical Map Off", systemMessagePos);
                    } 

                if (keyboardState.IsKeyDown(Keys.E) && !isEditMode && !isChat)
                {
                    isEditMode = true;
                    string msg = "Edit Mode";
                    messageClass.sendSystemMsg(spriteFont, spriteBatch,msg, systemMessagePos);
                    //Camera.zoomFactor = 4.0f;                    
                }
                else if (keyboardState.IsKeyDown(Keys.E) && isEditMode && !isChat)
                {
                    //Camera.zoomFactor = 2.0f;
                    isEditMode = false;
                }
                // Chat Mode Handler //
                if (keyboardState.IsKeyDown(Keys.Tab) )
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
                // Edit mode save Handler
                if (keyboardState.IsKeyDown(Keys.F10) && isEditMode)
                   saveClass.serializeClass(activeShipList);

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

            if (keyboardState.IsKeyDown(Keys.Q) && !oldkeyboardState.IsKeyDown(Keys.Q) && !isDebug && !isChat)
            {
                MessageClass.messageLog.Add("Debug Mode On");
                isDebug = true;
            }
            else if (keyboardState.IsKeyDown(Keys.Q) && !oldkeyboardState.IsKeyDown(Keys.Q) && isDebug && !isChat)
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
            //Draw Skybox and Starfield
            skySphere.DrawSkySphere(this, ourCamera);
            starField.DrawStars(this, ourCamera);
            //planetManager.DrawPlanets(gameTime, ourCamera.viewMatrix, ourCamera.projectionMatrix,ourCamera);
            
            foreach (newShipStruct npcship in activeShipList)
            {                
                modelManager.DrawModel(ourCamera, npcship.shipModel, npcship.worldMatrix,shipColor);
                if (isDebug)
                    debug(npcship);                
            }
                    
            projectileTrailParticles.SetCamera(ourCamera.viewMatrix, ourCamera.projectionMatrix);
            foreach (weaponStruct theList in weaponsManager.activeWeaponList)
            {
                if (theList.isProjectile)
                    modelManager.DrawModel(ourCamera, theList.shipModel, theList.worldMatrix, Color.White);
                else if (theList.objectClass == WeaponClassEnum.Beam)
                {
                    theList.beamQuad.DrawQuad(ourCamera.viewMatrix,
                        ourCamera.projectionMatrix, Matrix.Identity, theList.beamQuad);
                }
                else
                    weaponsManager.DrawLaser(device, ourCamera.viewMatrix, ourCamera.projectionMatrix, theList.objectColor, theList);
            }            
             ourExplosion.DrawExp(gameTime, ourCamera, GraphicsDevice);           
            // Start HUD and other 2d stuff
            //DrawHUD(gameTime);
            helperClass.DrawFPS(gameTime, device, spriteBatch, spriteFont);
            DrawHUDTargets(gameTime);
            spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Deferred, SaveStateMode.SaveState);
            //radar.Draw(spriteBatch, (float)System.Math.Atan2(playerShip.Direction.Z, playerShip.Direction.X), playerShip.modelPosition, ref activeShipList);
            if (isEditMode || isTacmap) editModeClass.Draw(gameTime, ref activeShipList, ourCamera,spriteBatch);
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
            messageClass.sendSystemMsg(spriteFont, spriteBatch,null, systemMessagePos);
            base.Draw(gameTime); messageClass.sendSystemMsg(spriteFont, spriteBatch, null, systemMessagePos);
        }

        private void DrawHUDTargets(GameTime gameTime)
        {
            bool isDone = false;
            StringBuilder messageBuffer = new StringBuilder(); 
            spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Deferred, SaveStateMode.SaveState);
            Vector2 fontPos = new Vector2(0,0);
            foreach (newShipStruct enemy in activeShipList)
            {
                if (enemy.team == 0)
                    shipColor = Color.Blue;
                else if (enemy.team == 1)
                    shipColor = Color.Red;
                if (enemy.isSelected)
                    shipColor = Color.White;
                    StringBuilder buffer = new StringBuilder();
                    fontPos = new Vector2(enemy.screenCords.X, enemy.screenCords.Y - 45);
                    buffer.AppendFormat("[" + enemy.objectAlias + "]");
                    if (isDebug)
                    {
                        if (enemy.currentTarget != null)
                        {
                            buffer.AppendFormat("[" + enemy.currentTarget.objectClass + "]");
                            //buffer.AppendFormat("[" + enemy.distanceFromTarget + "]");
                            // buffer.AppendFormat("[" + enemy.EvadeDist[(int)enemy.currentTarget.objectClass] + "]");
                        }
                        buffer.AppendFormat("[Evade:" + enemy.isEvading + "]");
                        buffer.AppendFormat("[" + enemy.angleOfAttack + "]");
                    }
                   // if (!isEditMode)
                        spriteBatch.DrawString(spriteFontSmall, buffer.ToString(), fontPos, Color.White);
                     if (!isEditMode)
                        spriteBatch.Draw(shipRec, new Vector2(enemy.screenCords.X-16, enemy.screenCords.Y-16), shipColor);
                    if (enemy.isSelected && !isDone)
                    {
                        isDone = true;
                        int starty = 600;
                        int timerIndex = 0;
                        double hbarValue;
                        int hbarwidth = 100;
                        buffer = new StringBuilder();
                        buffer.AppendLine(enemy.objectAlias + "");
                        spriteBatch.DrawString(spriteFont, buffer.ToString(), new Vector2(1100, starty - 38), 
                            Color.White);
                        
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
            }
            spriteBatch.DrawString(spriteFont, messageBuffer.ToString(), new Vector2(0,0), Color.White);
            spriteBatch.End();
        }

        private void DrawHUD(GameTime gameTime)
        {
            StringBuilder messageBuffer = new StringBuilder(); 
            spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Deferred, SaveStateMode.SaveState);
            spriteBatch.DrawString(spriteFont, messageBuffer.ToString(), new Vector2(0,0), Color.White);
            messageBuffer = new StringBuilder();
            messageBuffer = new StringBuilder();
            messageBuffer.AppendFormat("\nCamerea Pos {0} ", cameraTargetVec3);
            messageBuffer.AppendFormat("\nCurrent Menu " + guiClass.currentSelection);
            messageBuffer.AppendFormat("\nisDragging: " + editModeClass.isDragging);
            spriteBatch.DrawString(spriteFont, messageBuffer.ToString(), new Vector2(screenCenterX +
                                    (screenX / 6) - 150, screenCenterY + (screenY / 3)), Color.White);
            messageBuffer = new StringBuilder();
            spriteBatch.DrawString(spriteFont, messageBuffer.ToString(), systemMessagePos, Color.Blue);
            spriteBatch.End();
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
