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
        SpriteFont spriteFont;
        GraphicsDeviceManager graphics;
        Random rand;
        public GraphicsDevice device;
        KeyboardState oldkeyboardState;
        MouseState mouseStateCurrent, mouseStatePrevious, originalMouseState;
        bool isRclicked, isLclicked, isRdown, isLdown;
        Vector3 farpoint, nearpoint = new Vector3(0,0,0);
        SpriteBatch spriteBatch;
        HelperClass helperClass;
        double lastWeaponFireTime;
        double lastKeyPressTime;
        gameServer gServer;
        gameClient gClient;
        Texture2D HUD;
        Texture2D HUD_Target;
        Texture2D HUDAutoTargetIcon;
        Texture2D HUDTargetLockIcon1, HUDTargetLockIcon2;
        Texture2D HUDMissileTrackIcon;
        Vector2 messagePos1 = new Vector2(0,0);
        Vector2 systemMessagePos = new Vector2(30,20);
        String HUDMessage;

        public Vector3[] plyonOffset;

        Ray currentMouseRay;
        RadarClass radar;

        public newShipStruct playerShip;
        EditModeComponent editModeClass;
        public List<newShipStruct> activeShipList = new List<newShipStruct>();
        public NPCManager npcManager;
        public PlayerManager playerManager;
        public ModelManager modelManager;
        public WeaponsManager weaponsManager;
        //public List<WeaponsManager> missileList = new List<WeaponsManager>();
        public List<saveObject> saveList = new List<saveObject>();
        public List<shipData> shipDefList = new List<shipData>();
        public List<weaponData> weaponDefList = new List<weaponData>();
        public randomNames rNameList = new randomNames();
        public string tmpShipName;
        ExplosionClass ourExplosion;
        public PlanetManager planetManager;
        public Effect effect;

        //Vars for auto target effect
        public Vector2 _currentPos = new Vector2(0, 0);
        public float _currentScale;
        public float _RotationAngle, _RotationAngle2;

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
        int screenX, screenY, screenCenterX, screenCenterY;
        ParticleSystem projectileTrailParticles, sparkParticles;
        ParticleEmitter sparkEmitter;
        public string chatMessage;
        Vector3 isFacing;
        Vector3 isRight;
        
        guiClass Gui;

        // The explosions effect works by firing projectiles up into the
        // air, so we need to keep track of all the active projectiles.
        List<Projectile> projectiles = new List<Projectile>();

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            helperClass = new HelperClass();
            Content.RootDirectory = "Content";
            ConfigureGraphicsManager();
            ourCamera = new Camera(screenCenterX,screenCenterY);
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            ourCamera.ResetCamera();
            this.IsMouseVisible = true;
            rand = new Random();
            // TODO: Add your initialization logic here
            spriteBatch = new SpriteBatch(GraphicsDevice);
            playerShip = new newShipStruct();
            ourExplosion = new ExplosionClass();
            skySphere = new SkySphere(this);
            activeShipList = new List<newShipStruct>();
            //Initialize Manager Classes
            modelManager = new ModelManager(this);
            modelManager.Initialize();
            playerManager = new PlayerManager(this);
            playerManager.Initialize();
            npcManager = new NPCManager(this);
            npcManager.Initialize();
            weaponsManager = new WeaponsManager(this);
            weaponsManager.Initialize();
            //Initalize Starfield
            starField = new RenderStarfield(this);
            InitializeStarFieldEffect();
            firingArc = new renderTriangle();
            editModeClass = new EditModeComponent(this);
            ourExplosion.initExplosionClass(this);
            radar = new RadarClass(Content, "textures//redDotSmall", "textures//yellowDotSmall", "textures//blackDotLarge");
            projectileTrailParticles = new ProjectileTrailParticleSystem(this, Content);
            sparkParticles = new SparkParticleSystem(this, Content);

            Gui = new guiClass();

            // Add Components
            Components.Add(projectileTrailParticles);
            Components.Add(sparkParticles);
            //Components.Add(playerShipHealthBar);
            Components.Add(editModeClass);
            Mouse.SetPosition(graphics.GraphicsDevice.Viewport.Width / 2, graphics.GraphicsDevice.Viewport.Height / 2);
            originalMouseState = Mouse.GetState();

            //Network Stuff
            gServer = new gameServer();
            gClient = new gameClient();

            base.Initialize();
        }

        public void ConfigureGraphicsManager()
        {
#if XBOX360
            graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
            graphics.IsFullScreen = true;
            screenX = graphics.PreferredBackBufferWidth;
            screenY = graphics.PreferredBackBufferHeight;
#else
            graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
            screenX = 1280; // graphics.PreferredBackBufferWidth;
            screenY = 720; //graphics.PreferredBackBufferHeight;
            screenCenterX = graphics.PreferredBackBufferWidth / 2;
            screenCenterY = graphics.PreferredBackBufferHeight/2;
            graphics.IsFullScreen = false;
            messagePos1 = new Vector2(screenCenterX - (graphics.PreferredBackBufferWidth / 4), screenCenterY 
                                      - (graphics.PreferredBackBufferHeight / 3));
           
#endif
        }

        protected override void LoadContent()
        {
            device = graphics.GraphicsDevice;
            //effect = Content.Load<Effect>("effects");
            loadShipData();
            Gui.initalize(this, ref shipDefList);
            initPlayer();
            spriteFont = this.Content.Load<SpriteFont>("DemoFont");
            HUD = this.Content.Load<Texture2D>("hud");
            HUD_Target = this.Content.Load<Texture2D>("textures//hud_target_new");
            HUDAutoTargetIcon = this.Content.Load<Texture2D>("textures/target_icon");
            HUDTargetLockIcon1 = this.Content.Load<Texture2D>("textures/targetlock1");
            HUDTargetLockIcon2 = this.Content.Load<Texture2D>("textures/targetlock2");
            HUDMissileTrackIcon = this.Content.Load<Texture2D>("textures/missiletrack");
            skySphere.LoadSkySphere(this);
            starField.LoadStarFieldAssets(this);
        }

        private void initPlayer()
        {
            int shipType = 1;
            playerShip.objectFileName = shipDefList[shipType].FileName;
            playerShip.radius = shipDefList[shipType].SphereRadius;
            playerShip.shipModel = modelManager.LoadModel(shipDefList[shipType].FileName);
            playerShip.objectAgility = shipDefList[shipType].Agility;
            playerShip.objectMass = shipDefList[shipType].Mass;
            playerShip.objectThrust = shipDefList[shipType].Thrust;
            playerShip.modelPosition = Vector3.Zero;
            playerShip.modelRotation = Matrix.Identity * Matrix.CreateRotationY(MathHelper.ToRadians(90));
            playerShip.Direction = Vector3.Forward;
            playerShip.Up = Vector3.Up;
            playerShip.modelFrustum = new BoundingFrustum(Matrix.Identity);
            playerShip.modelBoundingSphere = new BoundingSphere(playerShip.modelPosition, playerShip.radius);
            playerShip.shipThruster = new Athruster();
            playerShip.shipThruster.LoadContent(this, spriteBatch);
            playerShip.weaponArray = shipDefList[shipType].AvailableWeapons;
            playerShip.currentWeapon = playerShip.weaponArray[0];
            playerShip.projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(25.0f), 4.0f / 3.0f, .5f, 500f);
        }

        private void loadShipData()
        {
            XmlReaderSettings xmlSettings = new XmlReaderSettings();
            XmlReader xmlReader = XmlReader.Create("shipdefs.xml");

            shipDefList = IntermediateSerializer.Deserialize<List<shipData>>(xmlReader,null);

            xmlReader = XmlReader.Create("weapondefs.xml");
            weaponDefList = IntermediateSerializer.Deserialize<List<weaponData>>(xmlReader, null);
            xmlReader = XmlReader.Create("listofnames.xml");
            rNameList = IntermediateSerializer.Deserialize<randomNames>(xmlReader, null);
        }

        private void serializeClass()
        {
            // Create the data to save
            saveObject saveMe;
            //weaponTypes exportWeaponDefs;
            shipData exportShipDefs = new shipData();
           // exportWeaponDefs = new weaponTypes();
            randomNames nameList = new randomNames();

            XmlWriterSettings xmlSettings = new XmlWriterSettings();
            xmlSettings.Indent = true;

            foreach (newShipStruct ship in activeShipList)
            {
                saveMe = new saveObject();
                saveMe.shipPosition = ship.modelPosition;
                saveMe.shipDirection = ship.vecToTarget;
                saveMe.shipName = ship.objectAlias;
                saveMe.shipType = ship.objectType;
                saveList.Add(saveMe);
            }

            using (XmlWriter xmlWriter = XmlWriter.Create("scene.xml", xmlSettings))
            {
              IntermediateSerializer.Serialize(xmlWriter, saveList, null);
            }
            
           // using (XmlWriter xmlWriter = XmlWriter.Create("listofnames.xml", xmlSettings))
          //  {
          //      IntermediateSerializer.Serialize(xmlWriter, nameList, null);
          //  }
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            processInput(gameTime);

            if (!isEditMode)
                updateObjects(gameTime);
            else
            {
                editModeClass.Update(gameTime, currentMouseRay, mouse3dVector, ref activeShipList, isLclicked, isLdown,
                    ref npcManager, ourCamera);
                Gui.update(mouseStateCurrent, mouseStatePrevious);
            }
                ourCamera.Update(playerShip.worldMatrix);

            if (weaponsManager.activeWeaponList.Count > 0)
            {
                helperClass.CheckForCollision(gameTime, ref activeShipList, ref weaponsManager.activeWeaponList, ref ourExplosion);
                helperClass.CheckForCollision(gameTime, playerShip, ref weaponsManager.activeWeaponList, ref ourExplosion);
            }

            if (isServer)
                gServer.update();
            if (isClient)
                gClient.Update(null);

            base.Update(gameTime);
        }

        protected void updateObjects(GameTime gameTime)
        {
            playerManager.updateShipMovement(gameTime,gameSpeed,Keyboard.GetState(),playerShip,ourCamera);
            if (sparkEmitter != null)
                sparkEmitter.Update(gameTime, playerShip.modelPosition * 10);
            for (int i = 0; i < activeShipList.Count; i++)
            {
                for (int j = 0; j < activeShipList.Count; j++)
                {
                    if (activeShipList[j] != activeShipList[i])
                        npcManager.performAI(gameTime, ref weaponsManager, projectileTrailParticles, ref weaponDefList, activeShipList[i], activeShipList[j],j);
                }
                npcManager.updateShipMovement(gameTime, gameSpeed, activeShipList[i], ourCamera,false);
               // activeShipList[i].modelBB = HelperClass.updateBB(activeShipList[i].modelBB.Min, activeShipList[i].modelBB.Max, activeShipList[i].modelPosition);
                
            }
            weaponsManager.Update(gameTime, gameSpeed);
        }

        protected void processInput(GameTime gameTime)
        {
            double currentTime = gameTime.TotalGameTime.TotalMilliseconds;
            KeyboardState keyboardState = Keyboard.GetState();
            mouseStateCurrent = Mouse.GetState();
            // Slow Wayyyyy Down the Key Stroke Input!!!!!!!!!! //

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
            }
            else
            {
                isLclicked = false;
                isLdown = false;
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

            if (isRclicked && isEditMode)
            {
                int rLen = rNameList.capitalShipNames.Count;
                int i = rand.Next(0, rLen);
                tmpShipName = rNameList.capitalShipNames[i];
                rNameList.capitalShipNames.Remove(tmpShipName);
                activeShipList.Add(editModeClass.spawnNPC(npcManager, mouse3dVector, ref shipDefList, gameTime, ourCamera, tmpShipName, Gui.thisItem));
            }

            if (currentTime - lastKeyPressTime > 100)
            {
                if (keyboardState.IsKeyDown(Keys.E) && !isEditMode && !isChat)
                {
                    isEditMode = true;
                    ourCamera.offsetDistance = new Vector3(0, 2000, 100);
                }
                else if (keyboardState.IsKeyDown(Keys.E) && isEditMode && !isChat)
                    isEditMode = false;

                // Chat Mode Handler //
                if (keyboardState.IsKeyDown(Keys.Tab) && !oldkeyboardState.IsKeyDown(Keys.Tab))
                {
                    if (isChat)
                        isChat = false;
                    else
                        isChat = true;
                }
                else if (isChat)
                    chatMessage += helperClass.UpdateInput();

                if (isChat && keyboardState.IsKeyDown(Keys.Enter) && !oldkeyboardState.IsKeyDown(Keys.Enter))
                {
                    if (isClient)
                        gClient.SendChat(chatMessage);
                    if (isServer)
                        gServer.SendChat(chatMessage);
                    chatMessage = "";
                    systemMessagePos.Y += 10;
                }
                // Edit mode save Handler
                if (keyboardState.IsKeyDown(Keys.F10) && isEditMode)
                    serializeClass();

                // Turn on/off Server/Client Mode
                if (keyboardState.IsKeyDown(Keys.F1) && !isServer)
                {
                    isServer = true;
                    isClient = false;
                    gServer.initializeServer();
                }
                else if (keyboardState.IsKeyDown(Keys.F2) && !isServer && !isClient)
                {
                    isServer = false;
                    isClient = true;
                    gClient.initializeNetwork();
                }
                lastKeyPressTime = currentTime;
            }

            if (keyboardState.IsKeyDown(Keys.Q) && !oldkeyboardState.IsKeyDown(Keys.Q) && !isDebug)
                isDebug = true;
            else if (keyboardState.IsKeyDown(Keys.Q) && !oldkeyboardState.IsKeyDown(Keys.Q) && isDebug)
                isDebug = false;
                
            if (keyboardState.IsKeyDown(Keys.R) && (currentTime - lastWeaponFireTime > weaponDefList[(int)playerShip.currentWeapon.weaponType].regenTime) && !isChat)
            {
                weaponsManager.fireWeapon(new newShipStruct(), playerShip, projectileTrailParticles, ref weaponDefList, playerShip.pylonIndex);
                playerShip.pylonIndex++;
                if (playerShip.pylonIndex > playerShip.currentWeapon.ModulePositionOnShip.GetLength(0) - 1)
                    playerShip.pylonIndex = 0;
                lastWeaponFireTime = currentTime;
            }

            
                mouseStatePrevious = mouseStateCurrent;
                oldkeyboardState = keyboardState;
}

        private void buildvisableTargetList()
        {
        //    foreach (NPCManager enemy in activeShipList)
        //    {
        //        if (enemy.screenCords.Z < 1 && enemy.distanceFromPlayer < 15000)
        ////            enemy.isVisable = true;
        //        else
        //            enemy.isVisable = false;
        //    }
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

            if (isEditMode) editModeClass.Draw(gameTime,ref activeShipList,ourCamera);
            modelManager.DrawModel(ourCamera,playerShip.shipModel,playerShip.worldMatrix);
            //modelManager.DrawWithCustomEffect(playerShip.shipModel, playerShip.worldMatrix, ourCamera.viewMatrix, ourCamera.projectionMatrix, Vector3.Zero);
           // BoundingFrustumRenderer.Render(playerShip.modelFrustum, device, ourCamera.viewMatrix, ourCamera.projectionMatrix, Color.White);
            //playerManager.DrawFiringArc(device, playerShip, ourCamera);
          if (playerShip.ThrusterEngaged)
               playerShip.shipThruster.draw(ourCamera.viewMatrix, ourCamera.projectionMatrix);
                //firingArc.Render(device, ourCamera.viewMatrix, ourCamera.projectionMatrix, Color.White,
                //            playerShip.currentWeapon.ModulePositionOnShip[playerShip.pylonIndex] + playerShip.Direction * 10, 
                //            playerShip.currentWeapon.ModulePositionOnShip[playerShip.pylonIndex] + playerShip.Direction * 100
                //            + playerShip.right * 25, playerShip.currentWeapon.ModulePositionOnShip[playerShip.pylonIndex] + playerShip.Direction * 100
                //            + playerShip.right * -25);
            foreach (newShipStruct npcship in activeShipList)
            {
                modelManager.DrawModel(ourCamera, npcship.shipModel, npcship.worldMatrix);
                npcship.shipThruster.draw(ourCamera.viewMatrix, ourCamera.projectionMatrix);
                //BoundingFrustumRenderer.Render(npcship.modelFrustum, device, ourCamera.viewMatrix, ourCamera.projectionMatrix, Color.White);
               // BoundingBoxRenderer.Render(npcship.modelBB, device, ourCamera.viewMatrix, ourCamera.projectionMatrix, Color.White);
                if (isDebug)
                {
                    foreach (BoundingFrustum bf in npcship.moduleFrustum)
                    BoundingFrustumRenderer.Render(bf, device, ourCamera.viewMatrix, ourCamera.projectionMatrix, Color.White);
                    isRight = npcship.modelRotation.Right;
                    for (int i = 0; i < npcship.weaponArray.Count(); i++)
                    {
                        for (int j = 0; j < npcship.weaponArray[i].ModulePositionOnShip.Count(); j++)
                        {
                            switch ((int)npcship.currentWeapon.ModulePositionOnShip[j].W)
                            {
                            case 0:
                                isFacing = npcship.Direction;
                                isRight = npcship.right;
                                break;
                            case 1:
                                isFacing = -npcship.Direction;
                                isRight = -npcship.right;
                                break;
                            case 2:
                                isFacing = npcship.right;
                                isRight = npcship.modelRotation.Right;
                                break;
                            case 3:
                                isFacing = -npcship.right;
                                isRight = npcship.modelRotation.Left;
                                break;
                            }
                            Vector3 tVec3 = new Vector3(npcship.modelPosition.X + npcship.weaponArray[i].ModulePositionOnShip[j].X,
                                                         npcship.modelPosition.Y + npcship.weaponArray[i].ModulePositionOnShip[j].Y,
                                                         npcship.modelPosition.Z + npcship.weaponArray[i].ModulePositionOnShip[j].Z);
                            //firingArc.Render(device, ourCamera.viewMatrix, ourCamera.projectionMatrix, Color.White,
                            //    tVec3 + isFacing,
                            //    tVec3 + isFacing * 300 + isRight * npcship.weaponArray[i].FiringEnvelopeAngle * 5,
                           //     tVec3 + isFacing * 300 - isRight * npcship.weaponArray[i].FiringEnvelopeAngle * 5);
                       }
                   }
               }
                
            }
            foreach (weaponStruct theList in weaponsManager.activeWeaponList)
            {
                if (theList.isProjectile)
                    modelManager.DrawModel(ourCamera, theList.shipModel, theList.worldMatrix);
                else
                    weaponsManager.DrawLaser(device, ourCamera.viewMatrix, ourCamera.projectionMatrix, theList.objectColor,theList);
            }

             ourExplosion.DrawExp(gameTime, ourCamera, GraphicsDevice);
            if (ourExplosion.expList.Count > 5)
                ourExplosion.expList = new List<VertexExplosion[]>();
            //spriteBatch.End();
            //spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Deferred, SaveStateMode.SaveState);
            DrawHUD(gameTime);
            helperClass.DrawFPS(gameTime, device, spriteBatch, spriteFont);
            DrawHUDTargets();
            spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Deferred, SaveStateMode.SaveState);
            //radar.Draw(spriteBatch, (float)System.Math.Atan2(playerShip.Direction.Z, playerShip.Direction.X), playerShip.modelPosition, ref activeShipList);
            if (isEditMode) Gui.drawGUI(spriteBatch,spriteFont);
            spriteBatch.End();
            // Pass camera matrices through to the particle system components.
            projectileTrailParticles.SetCamera(ourCamera.viewMatrix, ourCamera.projectionMatrix);
            //projectileTrailParticles.Draw(gameTime);

            sparkParticles.SetCamera(ourCamera.viewMatrix, ourCamera.projectionMatrix);
            sparkParticles.Draw(gameTime);
            
            base.Draw(gameTime);
        }

        private void DrawAutoTarget(GameTime gameTime)
        {
            // The time since Update was called last.
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
            //modelX = playerShip.currentTargetObject.screenCords.X;
            //modelY = playerShip.currentTargetObject.screenCords.Y;
            spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Deferred, SaveStateMode.SaveState);
            _RotationAngle += elapsed * 5.0f;
            float circle = MathHelper.Pi * 2;
            _RotationAngle = _RotationAngle % circle;
            _RotationAngle2 = -_RotationAngle;
            Vector2 spriteCenter = new Vector2(64,64);
            spriteBatch.Draw(HUDTargetLockIcon1, new Vector2(_currentPos.X, _currentPos.Y), 
                null, Color.White, _RotationAngle, spriteCenter, _currentScale, SpriteEffects.None, 0);
            spriteBatch.Draw(HUDTargetLockIcon2, new Vector2(_currentPos.X, _currentPos.Y),
                null, Color.White, _RotationAngle2, spriteCenter, _currentScale, SpriteEffects.None, 0);
          //  playerShipHealthBar.DrawHbar(gameTime, spriteBatch, Color.Green, (int)modelX, (int)modelY-10,
          //                              100, 10, (int)playerShip.currentTargetObject.objectArmorLvl);

            spriteBatch.End();
        }

        private void DrawHUDTargets()
        {
            StringBuilder messageBuffer = new StringBuilder();
            spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Deferred, SaveStateMode.SaveState);
            Vector2 fontPos = new Vector2(0,0);
            int i=0;
            foreach (newShipStruct enemy in activeShipList)
            {
                    StringBuilder buffer = new StringBuilder();
                    fontPos = new Vector2(enemy.screenCords.X, enemy.screenCords.Y);
                    buffer.AppendFormat("\n" + i);
                    if (enemy.currentTarget != null)
                    {
                        buffer.AppendFormat("[Target]" + activeShipList[enemy.currentTargetIndex].objectAlias + "-");
                        buffer.AppendFormat("Waypoint {0} ",enemy.vecToTarget);
                        buffer.AppendFormat("\n[TArget Index]{0} ", enemy.currentTargetIndex);
                        buffer.AppendFormat("\n[Attack Angle]{0} ", enemy.angleOfAttack);
                    }
                    buffer.AppendFormat("Waypoint {0} ", enemy.vecToTarget);
                    buffer.AppendFormat("[Target Level]" + enemy.currentTargetLevel + "-");
                    buffer.AppendFormat("[State]" + enemy.currentDisposition + "-");
                    buffer.AppendFormat("[Engage]" + enemy.isEngaging + "-");
                    buffer.AppendFormat("[Evade]" + enemy.isEvading + "-");
                    spriteBatch.DrawString(spriteFont, buffer.ToString(), fontPos, Color.Yellow);
                    i++;
            }
            spriteBatch.DrawString(spriteFont, messageBuffer.ToString(), new Vector2(0,0), Color.White);
            spriteBatch.End();
        }

        private void DrawHUD(GameTime gameTime)
        {
            StringBuilder messageBuffer = new StringBuilder();

            spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Deferred, SaveStateMode.SaveState);
            //spriteBatch.Draw(HUD, new Vecto    r2(0,0), Color.White);
            //spriteBatch.Draw(HUD_Target, new Vector2(GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width/2-96,
            //                            GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height/2-96), Color.White);
            //playerShipHealthBar.DrawHbar(gameTime, spriteBatch,Color.Green, screenCenterX-(screenX/3),screenCenterY-(screenY/3)-50,
            //                            500, 20, (int)playerShip.objectArmorLvl);
            spriteBatch.DrawString(spriteFont, messageBuffer.ToString(), new Vector2(0,0), Color.White);
            messageBuffer = new StringBuilder();
            //playerShipHealthBar.DrawHbar(gameTime, spriteBatch, Color.Blue, 0,0, 500, 20, (int)playerShip.objectArmorLvl);
            spriteBatch.DrawString(spriteFont, messageBuffer.ToString(), new Vector2(screenCenterX -
                                    (screenX / 3), screenCenterY - (screenY / 3)-25), Color.White);
            messageBuffer = new StringBuilder();
            messageBuffer.AppendFormat("\nCurrent Menu Item {0} ", Gui.thisItem);
            spriteBatch.DrawString(spriteFont, messageBuffer.ToString(), new Vector2(screenCenterX +
                                    (screenX / 6) - 150, screenCenterY + (screenY / 3)), Color.White);

            messageBuffer = new StringBuilder();
            //System.GC.GetTotalMemory(true);
       //     if (selectedObject !=null)
       //     messageBuffer.AppendFormat("Zoom {0}",ourCamera.zoomFactor + "\n");
      //      messageBuffer.AppendFormat("CameraOffset Y {0}", ourCamera.cameraOffset2.Y + "\n");
      //      messageBuffer.AppendFormat("CameraOffset X {0}", ourCamera.cameraOffset2.X + "\n");
           // messageBuffer.AppendFormat("Bounding Sphere Radius {0}", playerShip.radius + "\n");
           // Net Diags
            spriteBatch.DrawString(spriteFont, messageBuffer.ToString(), messagePos1, Color.White);
            messageBuffer = new StringBuilder();
            messageBuffer.AppendFormat("\nClients Connected to Server: {0}", gServer.clientsConnected);
            messageBuffer.AppendFormat("\nServer Mode " + isServer);
            messageBuffer.AppendFormat("\nClient Mode " + isClient);
            messageBuffer.AppendFormat("\nChat " + isChat);
            messageBuffer.AppendFormat("\n" + gServer.fromClient);
            spriteBatch.DrawString(spriteFont, messageBuffer.ToString(), systemMessagePos, Color.Blue);
            spriteBatch.End();
        }

        public void setMouseState()
        {
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

                    //Once it has move pass y=0, stop and record the X
                    // and Y position of the ray, return new Vector3

                    return new Vector3(xPos, 0, zPos);
                }
                else
                    return new Vector3(0, 0, 0);
                //throw new Exception("No intersection!");
            }
        }

    }
}
