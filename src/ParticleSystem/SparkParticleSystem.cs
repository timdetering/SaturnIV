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
    class SparkParticleSystem : ParticleSystem
    {
        public SparkParticleSystem(Game game, ContentManager content)
            : base(game, content)
        { }


        protected override void InitializeSettings(ParticleSettings settings)
        {
            settings.TextureName = "textures//31";

            settings.MaxParticles = 10;

            settings.Duration = TimeSpan.FromSeconds(0.5);

            settings.DurationRandomness = 0.1f;

            settings.EmitterVelocitySensitivity = 1.0f;

            settings.MinHorizontalVelocity = 10;
            settings.MaxHorizontalVelocity = 10;

            settings.MinVerticalVelocity = 10;
            settings.MaxVerticalVelocity = 10;

            //settings.MinColor = new Color(64, 96, 128, 255);
            //settings.MaxColor = new Color(255, 255, 255, 128);
            settings.MinColor = new Color(255, 255, 255, 255);
            settings.MaxColor = new Color(255, 255, 255, 255);

            settings.MinRotateSpeed = 45;
            settings.MaxRotateSpeed = 50;

            settings.MinStartSize = 2;
            settings.MaxStartSize = 15;

            settings.MinEndSize = 2;
            settings.MaxEndSize = 15;
        }
    }
}
