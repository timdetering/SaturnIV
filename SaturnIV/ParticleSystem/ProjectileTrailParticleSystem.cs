#region File Description
//-----------------------------------------------------------------------------
// ProjectileTrailParticleSystem.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace SaturnIV
{
    /// <summary>
    /// Custom particle system for leaving smoke trails behind the rocket projectiles.
    /// </summary>
    class ProjectileTrailParticleSystem : ParticleSystem
    {
        public static Color color;
        public ProjectileTrailParticleSystem(Game game, ContentManager content)
            : base(game, content)
        {
        }


        public void initColor(Color color, ParticleSettings settings)
        {
            settings.MaxColor = color;
            settings.MinColor = color;
        }
        
        protected override void InitializeSettings(ParticleSettings settings)
        {            
            settings.TextureName = "textures//smoke";

            settings.MaxParticles = 30000;

            settings.Duration = TimeSpan.FromSeconds(1.5);

            settings.DurationRandomness = 0;

            settings.EmitterVelocitySensitivity = 0.2f;

            settings.MinHorizontalVelocity = 0;
            settings.MaxHorizontalVelocity = 1;

            settings.MinVerticalVelocity = -2;
            settings.MaxVerticalVelocity = 2;

            settings.MinColor = Color.White;
            settings.MaxColor = Color.White;

            settings.MinRotateSpeed = 0;
            settings.MaxRotateSpeed = 0;

            settings.MinStartSize = 550;
            settings.MaxStartSize = 950;

            settings.MinEndSize = 1050;
            settings.MaxEndSize = 1150;
        }
    }
}
