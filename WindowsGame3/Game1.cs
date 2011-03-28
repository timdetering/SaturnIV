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


namespace WindowsGame3
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        SpriteFont spriteFont;
        GraphicsDeviceManager graphics;

        public GraphicsDevice device;
        KeyboardState oldkeyboardState;
        MouseState mouseStateCurrent, mouseStatePrevious, originalMouseState;
        bool isRclicked, isLclicked, isRdown, isLdown;
        Vector3 farpoint, nearpoint = new Vector3(0,0,0);
        SpriteBatch spriteBatch;
        HelperClass helperClass;

        Texture2D HUD;
        Texture2D HUD_Target;
        Texture2D HUDAutoTargetIcon;
        Texture2D HUDTargetLockIcon1, HUDTargetLockIcon2;
        Texture2D HUDMissileTrackIcon;
        Vector2 messagePos1 = new Vector2(0,0);
        Vector2 messagePos2 = new Vector2(0,0);
        String HUDMessage;

        public Vector3[] plyonOffset;
        int currentAutoTargetIndex;
        NPCManager currentAutoTarget;
        NPCManager selectedObject;
        Athruster playerThruster1;

        Ray currentMouseRay;
        RadarClass radar;

        public PlayerManager playerShip;
        HealthBarClass playerShipHealthBar;
        EditModeComponent editModeClass;
        //public NPCManager npcList;
        public List<NPCManager> npcList = new List<NPCManager>();
        public List<WeaponsManager> missileList = new List<WeaponsManager>();
        public List<saveObject> saveList = new List<saveObject>();
        public List<shipData> shipDefList = new List<shipData>();
       // public shipData shipDefList;
        ExplosionClass ourExplosion;
        public PlanetManager planetManager;
        public Effect effect;

        //Vars for auto target effect
        public Vector2 _currentPos = new Vector2(0, 0);
        public float _currentScale;
        public float _RotationAngle, _RotationAngle2;

        SkySphere skySphere;
        RenderStarfield starField;
        BasicEffect basicEffect;
        VertexDeclaration vertexDeclaration;
        public float gameSpeed = 5.0f;

        public Matrix worldMatrix;
        public Matrix viewMatrix;
        public Matrix projectionMatrix;
        public PlayerManager thisPlayer;
        public Camera ourCamera;
        bool isEditMode = false;

        int screenX, screenY, screenCenterX, screenCenterY;
        ParticleSystem projectileTrailParticles;
        
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

            // TODO: Add your initialization logic here
            spriteBatch = new SpriteBatch(GraphicsDevice);
            playerShip = new PlayerManager(this);
            playerThruster1 = new Athruster();
            planetManager = new PlanetManager(this);
            ourExplosion = new ExplosionClass();
            skySphere = new SkySphere(this);
            npcList = new List<NPCManager>();
            currentAutoTarget = new NPCManager(this);
            starField = new RenderStarfield(this);
            InitializeStarFieldEffect();
            ourExplosion.initExplosionClass(this);
            playerShipHealthBar = new HealthBarClass(this);
            radar = new RadarClass(Content, "textures//redDotSmall", "textures//yellowDotSmall", "textures//blackDotLarge");
            projectileTrailParticles = new ProjectileTrailParticleSystem(this, Content);
            editModeClass = new EditModeComponent(this);
            Gui = new guiClass();
            Gui.initalize(this);
            Mouse.SetPosition(graphics.GraphicsDevice.Viewport.Width / 2, graphics.GraphicsDevice.Viewport.Height / 2);
            originalMouseState = Mouse.GetState();

            // Add Components
            Components.Add(projectileTrailParticles);
            Components.Add(playerShipHealthBar);
            Components.Add(editModeClass);

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
            messagePos2 = new Vector2(screenCenterX - (graphics.PreferredBackBufferWidth / 4), screenCenterY 
                                      + (graphics.PreferredBackBufferHeight / 3));
#endif
        }

        protected override void LoadContent()
        {
            device = graphics.GraphicsDevice;
            //effect = Content.Load<Effect>("effects");
            loadShipData();
            planetManager.generatSpaceObjects(1);
            
            playerShip.Initialize((int)shipTypes.Ships.banshee);
            playerShip.initModelPosition(Vector3.Zero);
            playerShip.currentWeaponIndex = playerShip.primaryWeaponIndex;
            //generateEnemies();
            //playerShip.currentTargetObject = npcList[0];
            spriteFont = this.Content.Load<SpriteFont>("DemoFont");
            HUD = this.Content.Load<Texture2D>("hud");
            HUD_Target = this.Content.Load<Texture2D>("textures//hud_target_new");
            HUDAutoTargetIcon = this.Content.Load<Texture2D>("textures/target_icon");
            HUDTargetLockIcon1 = this.Content.Load<Texture2D>("textures/targetlock1");
            HUDTargetLockIcon2 = this.Content.Load<Texture2D>("textures/targetlock2");
            HUDMissileTrackIcon = this.Content.Load<Texture2D>("textures/missiletrack");
            //ourCamera.SetUpCamera();
            skySphere.LoadSkySphere(this);
            starField.LoadStarFieldAssets(this);
            playerThruster1.LoadContent(this, spriteBatch);

            vertexDeclaration = new VertexDeclaration(
            graphics.GraphicsDevice,
            VertexPositionNormalTexture.VertexElements);
        }

        private void loadShipData()
        {
            XmlReaderSettings xmlSettings = new XmlReaderSettings();



            XmlReader xmlReader = XmlReader.Create("shipdefs.xml");
                shipDefList = IntermediateSerializer.Deserialize<List<shipData>>(xmlReader,null);

        }

        private void serializeClass()
        {
            // Create the data to save
            saveObject saveMe;
            shipData exportMe;
            shipTypes exportShipDefs;
            shipClasses exportShipClasses;
            weaponTypes exportWeaponDefs;
            shipData exportShipDefs2;
            exportShipDefs2 = new shipData();
            exportShipDefs = new shipTypes();
            exportShipClasses = new shipClasses();
            exportWeaponDefs = new weaponTypes();
            // Open the file, creating it if necessary
           // FileStream stream = File.Open("assets.xml", FileMode.OpenOrCreate);
           // XmlSerializer serializer = new XmlSerializer(typeof(saveObject));
            XmlWriterSettings xmlSettings = new XmlWriterSettings();
            xmlSettings.Indent = true;

            foreach (NPCManager ship in npcList)
            {
                exportMe = new shipData();
                saveMe = new saveObject();
                saveMe.shipPosition = ship.modelPosition;
                saveMe.shipDirection = ship.vecToTarget;
                saveMe.shipName = ship.objectName;
                saveMe.shipType = ship.objectDesc;
                saveList.Add(saveMe);

      //  exportMe.shipFileName = "Models//SF-14A";
     //   exportMe.shipDesc = "SF-14A";
    //    exportMe.shipShieldLvl = 100;
    //    exportMe.shipShieldRegenTime = 100;
   //     exportMe.shipMass = 2;
  //      exportMe.shipThrust = 4;
  //      exportMe.shipSphereRadius = 17;
 //      exportMe.shipAgility = 4;
 //       exportMe.shipClass = "Fighter";
 //       exportMe.shipWeapons2 = new weaponTypes.MissileType[] {weaponTypes.MissileType.AC10,weaponTypes.MissileType.KM100};
        //exportList.Add(exportMe);

                // Convert the object to XML data and put it in the stream
               // serializer.Serialize(stream, saveMe);
            }

            using (XmlWriter xmlWriter = XmlWriter.Create("scene.xml", xmlSettings))
            {
                IntermediateSerializer.Serialize(xmlWriter, saveList, null);
            }

            //   // Close the file
            //    stream.Close();

                //using (XmlWriter xmlWriter = XmlWriter.Create("shipdefsTest.xml", xmlSettings))
               // {
              //      IntermediateSerializer.Serialize(xmlWriter, exportList, null);
             //   }
                using (XmlWriter xmlWriter = XmlWriter.Create("classdefs.xml", xmlSettings))
                {
                    IntermediateSerializer.Serialize(xmlWriter, exportShipClasses, null);
                }
                using (XmlWriter xmlWriter = XmlWriter.Create("weapondefs.xml", xmlSettings))
                {
                    IntermediateSerializer.Serialize(xmlWriter, exportWeaponDefs, null);
                }
                using (XmlWriter xmlWriter = XmlWriter.Create("shipDef2.xml", xmlSettings))
                {
                    IntermediateSerializer.Serialize(xmlWriter, exportShipDefs2, null);
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

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            processInput(gameTime);
            Gui.update(mouseStateCurrent.X, mouseStateCurrent.Y);
            if (!isEditMode)
                updateObjects(gameTime);
            else
                editModeClass.Update(gameTime, currentMouseRay, mouse3dVector, ref npcList, isLclicked,isLdown);                                 

            ourCamera.Update(playerShip.worldMatrix);

            buildvisableTargetList();
            playerThruster1.update(playerShip.modelPosition + (playerShip.modelRotation.Forward)
                                        - (playerShip.modelRotation.Up) + (playerShip.modelRotation.Right * -20), 
                                        playerShip.Direction, new Vector3(6,6,6), 40.0f, 10.0f,
                                        Color.White, Color.Blue, ourCamera.position);
            playerThruster1.heat = 3.0f;

            helperClass.CheckForCollision(gameTime, npcList, missileList, ref missileList, ref ourExplosion);
            helperClass.CheckForCollision(gameTime, playerShip, missileList, ref missileList, ref ourExplosion);
            
           
           // BoundingFrustum viewFrustum = new BoundingFrustum();
           // planetManager.Update(gameTime);
            base.Update(gameTime);
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

        

        protected void updateObjects(GameTime gameTime)
        {
            playerShip.updateShipMovement(gameTime, ourCamera, gameSpeed, Keyboard.GetState(), new Vector3(nearpoint.X,0,nearpoint.Y));
            //playerShip.updateModelBoundingSphere(ourCamera);

            foreach (NPCManager enemy in npcList)
            {
                enemy.updateShipMovement(gameTime, ourCamera, gameSpeed);
                //enemy.updateModelBoundingSphere(ourCamera);
            }

            for (int i = 0; i < missileList.Count; i++)
                {
                missileList[i].updateMissileMovement(gameTime, gameSpeed, ourCamera);
                //missileList[i].updateModelBoundingSphere(ourCamera, 1.0f);

                if (Vector3.Distance(missileList[i].modelPosition, missileList[i].missileOrigin) > missileList[i].weaponRange)
                    missileList.Remove(missileList[i]);
            }
        }

        protected void processInput(GameTime gameTime)
        {
            KeyboardState keyboardState = Keyboard.GetState();
            mouseStateCurrent = Mouse.GetState();

            // Get the game pad state.
            GamePadState gamePadState = GamePad.GetState(PlayerIndex.One);

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back ==
                ButtonState.Pressed)
                this.Exit();

            if (keyboardState.IsKeyDown(Keys.E) && !isEditMode)
            {
                isEditMode = true;
                ourCamera.offsetDistance = new Vector3(0, 2000, 100);
           }
            else if (keyboardState.IsKeyDown(Keys.E) && isEditMode)
                isEditMode = false;

          if (keyboardState.IsKeyDown(Keys.S) && isEditMode)
                serializeClass();

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
                editModeClass.spawnNPC(gameTime, ourCamera,gameSpeed, mouse3dVector, ref npcList);

            if (keyboardState.IsKeyDown(Keys.D1))
                playerShip.radius = playerShip.radius + 0.5f;
            if (keyboardState.IsKeyDown(Keys.D2))
                playerShip.thrustAmount = 0.50f;
            if (keyboardState.IsKeyDown(Keys.D3))
                playerShip.thrustAmount = 0.75f;
            if (keyboardState.IsKeyDown(Keys.D4))
                playerShip.thrustAmount = 1.0f;


            if (oldkeyboardState.IsKeyDown(Keys.Q) && keyboardState.IsKeyUp(Keys.Q))
            {
                if (playerShip.currentWeaponIndex == playerShip.primaryWeaponIndex)
                    playerShip.currentWeaponIndex = weaponTypes.MissileType.KM200;
                else
                    playerShip.currentWeaponIndex = playerShip.primaryWeaponIndex;
            }

            if (keyboardState.IsKeyDown(Keys.R))
            {
                playerShip.Initialize((int)shipTypes.Ships.procyon);
                playerShip.initModelPosition(playerShip.modelPosition);
                playerShip.currentWeaponIndex = playerShip.primaryWeaponIndex;
            }
                mouseStatePrevious = mouseStateCurrent;
}

        private void buildvisableTargetList()
        {
            foreach (NPCManager enemy in npcList)
            {
                if (enemy.screenCords.Z < 1 && enemy.distanceFromPlayer < 15000)
                    enemy.isVisable = true;
                else
                    enemy.isVisable = false;
            }
        }

        private void InitializeStarFieldEffect()
        {
            vertexDeclaration = new VertexDeclaration(GraphicsDevice, VertexPositionColor.VertexElements);
            basicEffect = new BasicEffect(GraphicsDevice, null);
            basicEffect.VertexColorEnabled = true;
            Matrix wvp = (Matrix.CreateScale(5.0f) * Matrix.CreateFromQuaternion(Quaternion.Identity) *
                Matrix.CreateTranslation(Vector3.Zero)) * ourCamera.viewMatrix * ourCamera.projectionMatrix;
            worldMatrix = Matrix.CreateTranslation(GraphicsDevice.Viewport.Width / 4f - 150,
                GraphicsDevice.Viewport.Height / 4f - 50, 0);
            basicEffect.World = worldMatrix;
            basicEffect.View = ourCamera.viewMatrix;
            basicEffect.Projection = ourCamera.projectionMatrix;

            //ourCamera.AspectRatio = (float)graphics.GraphicsDevice.Viewport.Width /
            //    graphics.GraphicsDevice.Viewport.Height;

        }

        protected override void Draw(GameTime gameTime)
        {
            float time = (float)gameTime.TotalGameTime.TotalMilliseconds / 100.0f;
            graphics.GraphicsDevice.Clear(Color.Black);
            skySphere.DrawSkySphere(this, ourCamera);
            starField.DrawStars(this, ourCamera);
            if (isEditMode) editModeClass.Draw(gameTime,ref npcList,ourCamera);
            planetManager.DrawPlanets(gameTime, ourCamera.viewMatrix, ourCamera.projectionMatrix);
            spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Deferred, SaveStateMode.SaveState);

            playerShip.DrawModel(ourCamera);
               foreach (NPCManager enemy in npcList)
                   enemy.DrawModel(ourCamera);

            foreach (WeaponsManager missile in missileList)
            {
                if (missile.isProjectile)
                {
                    missile.DrawLaser(device, ourCamera.viewMatrix, ourCamera.projectionMatrix);
                    BoundingSphereRenderer.Render(missile.modelBoundingSphere, GraphicsDevice, ourCamera.viewMatrix, ourCamera.projectionMatrix, Color.White);
                    // missile.DrawModel(ourCamera);
                }
            }
            
            //lanetManager.DrawPlanets(gameTime, ourCamera.viewMatrix, ourCamera.projectionMatrix);
            ourExplosion.DrawExp(gameTime, ourCamera, GraphicsDevice);
            if (ourExplosion.expList.Count > 10)
                ourExplosion.expList = new List<VertexExplosion[]>();
            playerThruster1.draw(ourCamera.viewMatrix, ourCamera.projectionMatrix);
            spriteBatch.End();
            //spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Deferred, SaveStateMode.SaveState);
            DrawHUD(gameTime);
            helperClass.DrawFPS(gameTime, device, spriteBatch, spriteFont);
            DrawHUDTargets();
            spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Deferred, SaveStateMode.SaveState);
            //radar.Draw(spriteBatch, (float)System.Math.Atan2(playerShip.Direction.Z, playerShip.Direction.X), playerShip.modelPosition, ref npcList);
            Gui.drawGUI(spriteBatch,spriteFont);
            spriteBatch.End();
            //if (playerShip.currentTargetObject.screenCords.Z < 1)
             //   DrawAutoTarget(gameTime);
            // Pass camera matrices through to the particle system components.
            projectileTrailParticles.SetCamera(ourCamera.viewMatrix, ourCamera.projectionMatrix);
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
            foreach (NPCManager enemy in npcList)
            {
                if (enemy.isVisable == true)
                {
                    StringBuilder buffer = new StringBuilder();
                    Vector2 fontPos = new Vector2(enemy.screenCords.X, enemy.screenCords.Y);

                    spriteBatch.Draw(HUDAutoTargetIcon, new Vector2(enemy.screenCords.X - 24,
                                            enemy.screenCords.Y - 24), null, Color.White, 0, Vector2.Zero,
                                            1.0f, SpriteEffects.None, 0);
                    buffer.AppendFormat("{0}", enemy.distanceFromPlayer);
                    buffer.AppendFormat("\n" + enemy.npcDisposition);
                    messageBuffer.AppendFormat(" X {0}", enemy.modelPosition.X + "\n");
                    messageBuffer.AppendFormat(" Y {0}", enemy.modelPosition.Y + "\n");
                    messageBuffer.AppendFormat(" Z {0}", enemy.modelPosition.Z + "\n");
                    spriteBatch.DrawString(spriteFont, buffer.ToString(), fontPos, Color.Yellow);
                }

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
            messageBuffer.AppendFormat("Hull Integrity");
            spriteBatch.DrawString(spriteFont, messageBuffer.ToString(), new Vector2(0,0), Color.White);
            messageBuffer = new StringBuilder();
            messageBuffer.AppendFormat("Shields");
            //playerShipHealthBar.DrawHbar(gameTime, spriteBatch, Color.Blue, 0,0, 500, 20, (int)playerShip.objectArmorLvl);
            spriteBatch.DrawString(spriteFont, messageBuffer.ToString(), new Vector2(screenCenterX -
                                    (screenX / 3), screenCenterY - (screenY / 3)-25), Color.White);
            messageBuffer = new StringBuilder();
            messageBuffer.AppendFormat("Player Position: {0}", playerShip.modelPosition + "\n");
            //messageBuffer.AppendFormat("\nCurrent Target\n" + playerShip.currentTargetObject.objectDesc);
            messageBuffer.AppendFormat("\nEdit Mode\n" + isEditMode);
            spriteBatch.DrawString(spriteFont, messageBuffer.ToString(), new Vector2(screenCenterX +
                                    (screenX / 6) - 150, screenCenterY + (screenY / 3)), Color.White);

            messageBuffer = new StringBuilder();
            if (selectedObject !=null)
       //     messageBuffer.AppendFormat("Zoom {0}",ourCamera.zoomFactor + "\n");
      //      messageBuffer.AppendFormat("CameraOffset Y {0}", ourCamera.cameraOffset2.Y + "\n");
      //      messageBuffer.AppendFormat("CameraOffset X {0}", ourCamera.cameraOffset2.X + "\n");
            messageBuffer.AppendFormat("Bounding Sphere Radius {0}", playerShip.radius + "\n");
            messageBuffer.AppendFormat("\nRight Click " + isLclicked + "\n");
            messageBuffer.AppendFormat("\nRight Down " + isLdown + "\n");
            messageBuffer.AppendFormat(" Direction X {0}", playerShip.Direction.X + "\n");
            messageBuffer.AppendFormat(" Direction Y {0}", playerShip.Direction.Y + "\n");
            messageBuffer.AppendFormat(" Direction Z {0}", playerShip.Direction.Z + "\n");

            spriteBatch.DrawString(spriteFont, messageBuffer.ToString(), new Vector2(screenCenterX +
                                    (screenX / 6) - 150, screenCenterY - (screenY / 3)), Color.White);
            spriteBatch.End();

        }

    }
}
