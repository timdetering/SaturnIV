using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace WindowsGame3
{
    public class StarRender : DrawableGameComponent
    {
        protected struct VertexParticle
        {
            public Vector3 Position;
            public Color Color;
            public Vector4 Data;
            
            public VertexParticle(Vector3 position, Color color)
            {
                Position = position;
                Color = color;
                Data = Vector4.Zero;                
            }
            public static readonly VertexElement[] VertexElements = new VertexElement[] {
                new VertexElement(0, 0, VertexElementFormat.Vector3, VertexElementMethod.Default, VertexElementUsage.Position, 0),
                new VertexElement(0, sizeof(float)*3, VertexElementFormat.Color, VertexElementMethod.Default, VertexElementUsage.Color, 0),
                new VertexElement(0, sizeof(float)*7, VertexElementFormat.Vector4, VertexElementMethod.Default, VertexElementUsage.TextureCoordinate, 0),                
            };

            public static int SizeInBytes
            {
                get
                {
                    return sizeof(float) * 13;
                }
            }

            public static bool operator !=(VertexParticle left, VertexParticle right)
            {
                return left.GetHashCode() != right.GetHashCode();
            }
            public static bool operator ==(VertexParticle left, VertexParticle right)
            {
                return left.GetHashCode() == right.GetHashCode();
            }
            public override bool Equals(object obj)
            {
                return this == (VertexParticle)obj;
            }
            public override int GetHashCode()
            {
                return Position.GetHashCode() | Color.GetHashCode() | Data.GetHashCode();
            }
            public override string ToString()
            {
                return Position.ToString();
            }
        }
        VertexDeclaration m_vDec;

        VertexParticle[] m_sprites;        
        string textureAsset;
        Texture2D myTexture;

     
        public int ParticleCount
        {
            get { return m_sprites.Length; }
            set 
            { 
                partCount = value;
                RefreshParticles();
            }
        }
       
        public Vector3 myPosition;
        public Vector3 myScale;
        public Quaternion myRotation;

        private Random m_rand;
        float myPointSize = 32f;

        Effect effect;

        Camera myCamera;

        int partCount;
        int offsetRandomness;
        ContentManager content;
        GraphicsDeviceManager myDeviceManager;
        GraphicsDevice myDevice;

        public StarRender(Game game, GraphicsDeviceManager graphiceDeviceManager, string texture, int particleCount, int offsetRandomness, Camera ourCamera)
            : base(game)
        {
            content = new ContentManager(game.Services);
            myDeviceManager = graphiceDeviceManager;
            
            myPosition = Vector3.Zero;
            myScale = Vector3.One;
            myRotation = new Quaternion(0, 0, 0, 1);
            myCamera = ourCamera;

            Rotate( Vector3.Left, MathHelper.PiOver2);

            partCount = particleCount;
            this.offsetRandomness = offsetRandomness;
            textureAsset = texture;            

        }

        public void Rotate(Vector3 axis, float angle)
        {
            axis = Vector3.Transform(axis, Matrix.CreateFromQuaternion(myRotation));
            myRotation = Quaternion.Normalize(Quaternion.CreateFromAxisAngle(axis, angle) * myRotation);

        }
        public void Translate(Vector3 distance)
        {
            myPosition += Vector3.Transform(distance, Matrix.CreateFromQuaternion(myRotation));
        }

        public void Revolve(Vector3 target, Vector3 axis, float angle)
        {
            Rotate(axis, angle);
            Vector3 revolveAxis = Vector3.Transform(axis, Matrix.CreateFromQuaternion(myRotation));
            Quaternion rotate = Quaternion.CreateFromAxisAngle(revolveAxis, angle);
            myPosition = Vector3.Transform(target - myPosition, Matrix.CreateFromQuaternion(rotate));
        }

        protected override void UnloadContent()
        {
            content.Unload();
            base.UnloadContent();
        }

        protected override void LoadContent()
        {
            
            myDevice = myDeviceManager.GraphicsDevice;
            myTexture = Game.Content.Load<Texture2D>(textureAsset);
            m_rand = new Random();

            m_vDec = new VertexDeclaration(myDevice, VertexParticle.VertexElements);

            effect = Game.Content.Load<Effect>("ScaledParticle");
            try
            {
                effect.Parameters["particleTexture"].SetValue(myTexture);
            }
            catch { }
            RefreshParticles();
            
            
            base.LoadContent();
        }

        private void RefreshParticles()
        {
            float radius = 5;
            m_sprites = new VertexParticle[partCount];
            for (int i = 0; i < m_sprites.Length; i++)
            {
                m_sprites[i] = new VertexParticle();
                m_sprites[i].Position = new Vector3(0, 0, 0);
                m_sprites[i].Data = new Vector4(m_rand.Next(0, 360), m_rand.Next(0, 360), 0, 0);

               
                m_sprites[i].Color = Color.White;

                radius = (float)(m_rand.NextDouble() * 30.0f) + 8.0f;
               
                

                float angle = m_sprites[i].Data.X;
                float angle2 = m_sprites[i].Data.Y;

    

                float cos = radius * (float)Math.Cos(angle);
                float sin = radius * (float)Math.Sin(angle);
                float cos2 = radius * (float)Math.Cos(angle2);
                float sin2 = (float)Math.Pow(radius, 2) * (float)Math.Sin(angle2);

                m_sprites[i].Position = new Vector3(cos * cos2 + m_rand.Next(-offsetRandomness, offsetRandomness), sin * cos2 + m_rand.Next(-offsetRandomness, offsetRandomness), sin2 + m_rand.Next(-offsetRandomness, offsetRandomness));

            }

        }

        public override void Update(GameTime gameTime)
        {
            //if (Microsoft.Xna.Framework.Input.Keyboard.GetState().IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Space))
            //    RefreshParticles();

            base.Update(gameTime);
        }


        public virtual void Ball()
        {
            float radius = 2;
            
            for (int i = 0; i < m_sprites.Length; i++)
            {
                float angle = m_sprites[i].Data.X;
                float angle2 = m_sprites[i].Data.Y;

                angle += .01f;
                if (angle > 360)
                    angle = 0;
                angle2 += .01f;
                if (angle2 > 360)
                    angle2 = 0;

                float cos = radius * (float)Math.Cos(angle);
                float sin = radius * (float)Math.Sin(angle);
                float cos2 = radius * (float)Math.Cos(angle2);
                float sin2 = (float)Math.Pow(radius, 2) * (float)Math.Sin(angle2);

                m_sprites[i].Position = new Vector3(cos * cos2, sin * cos2, sin2);
                m_sprites[i].Color = new Color(new Vector4(m_sprites[i].Color.ToVector3(), 1f));

                m_sprites[i].Data = new Vector4(angle, angle2, 0, 0);
            }
        }
       
        public override void Draw(GameTime gameTime)
        {
            bool PointSpriteEnable = myDevice.RenderState.PointSpriteEnable;

            float PointSize = myDevice.RenderState.PointSize;

            bool AlphaBlendEnable = myDevice.RenderState.AlphaBlendEnable;
            BlendFunction AlphaBlendOperation = myDevice.RenderState.AlphaBlendOperation;
            Blend SourceBlend = myDevice.RenderState.SourceBlend;
            Blend DestinationBlend = myDevice.RenderState.DestinationBlend;
            bool SeparateAlphaBlendEnabled = myDevice.RenderState.SeparateAlphaBlendEnabled;
            bool AlphaTestEnable = myDevice.RenderState.AlphaTestEnable;
            CompareFunction AlphaFunction = myDevice.RenderState.AlphaFunction;
            int ReferenceAlpha = myDevice.RenderState.ReferenceAlpha;
            bool DepthBufferWriteEnable = myDevice.RenderState.DepthBufferWriteEnable;


            if (myDevice.RenderState.PointSpriteEnable != true)
                myDevice.RenderState.PointSpriteEnable = true;
            if (myDevice.RenderState.AlphaBlendEnable != true)
                myDevice.RenderState.AlphaBlendEnable = true;
            if (myDevice.RenderState.DestinationBlend != Blend.One)
                myDevice.RenderState.DestinationBlend = Blend.One;
            if (myDevice.RenderState.DepthBufferWriteEnable != false)
                myDevice.RenderState.DepthBufferWriteEnable = false;

            myDevice.VertexDeclaration = m_vDec;

            Matrix wvp = (Matrix.CreateScale(myScale) * Matrix.CreateFromQuaternion(myRotation) * Matrix.CreateTranslation(myPosition)) * myCamera.viewMatrix * myCamera.projectionMatrix;

                myPointSize = .15f;
                effect.Parameters["Projection"].SetValue(myCamera.projectionMatrix);
                effect.Parameters["ParticleSize"].SetValue(myPointSize);
                effect.Parameters["ViewportHeight"].SetValue(5000.0f);


                effect.Parameters["WorldViewProj"].SetValue(wvp);

                effect.Begin();
                for (int ps = 0; ps < effect.CurrentTechnique.Passes.Count; ps++)
            {
                effect.CurrentTechnique.Passes[ps].Begin();

                if (m_sprites.Length >= 15000)
                {
                    myDevice.DrawUserPrimitives<VertexParticle>(PrimitiveType.PointList, m_sprites, 0, m_sprites.Length / 3);
                    myDevice.DrawUserPrimitives<VertexParticle>(PrimitiveType.PointList, m_sprites, m_sprites.Length / 3, m_sprites.Length / 3);
                    myDevice.DrawUserPrimitives<VertexParticle>(PrimitiveType.PointList, m_sprites, 2 * m_sprites.Length / 3, m_sprites.Length / 3);
                }
                else
                    myDevice.DrawUserPrimitives<VertexParticle>(PrimitiveType.PointList, m_sprites, 0, m_sprites.Length);

                effect.CurrentTechnique.Passes[ps].End();
            }
                effect.End();

            if (myDevice.RenderState.PointSpriteEnable != PointSpriteEnable)
                myDevice.RenderState.PointSpriteEnable = PointSpriteEnable;
            if (myDevice.RenderState.AlphaBlendEnable != AlphaBlendEnable)
                myDevice.RenderState.AlphaBlendEnable = AlphaBlendEnable;
            if (myDevice.RenderState.DestinationBlend != DestinationBlend)
                myDevice.RenderState.DestinationBlend = DestinationBlend;
            if (myDevice.RenderState.DepthBufferWriteEnable != DepthBufferWriteEnable)
                myDevice.RenderState.DepthBufferWriteEnable = DepthBufferWriteEnable;

            base.Draw(gameTime);
        }        
    }

}
