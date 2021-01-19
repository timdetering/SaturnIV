#region File Description
//-----------------------------------------------------------------------------
// ExplosionParticleSystem.cs
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
    /// Custom particle system for creating the fiery part of the explosions.
    /// </summary>
    class ExplosionParticleSystem : ParticleSystem
    {
        public ExplosionParticleSystem(Game game, ContentManager content)
            : base(game, content)
        { }


        protected override void InitializeSettings(ParticleSettings settings)
        {
            settings.TextureName = "textures//explosion";

            settings.MaxParticles = 25000;

            settings.Duration = TimeSpan.FromSeconds(4);
            settings.DurationRandomness = 0;

            settings.MinHorizontalVelocity = 20;
            settings.MaxHorizontalVelocity = 30;

            settings.MinVerticalVelocity = -20;
            settings.MaxVerticalVelocity = 20;

            settings.EndVelocity = 0;

            settings.MinColor = Color.DarkGray;
            settings.MaxColor = Color.Gray;

            settings.MinRotateSpeed = -1;
            settings.MaxRotateSpeed = 1;

            settings.MinStartSize = 11100;
            settings.MaxStartSize = 11100;

            settings.MinEndSize = 12500;
            settings.MaxEndSize = 12600;

            // Use additive blending.
            settings.SourceBlend = Blend.SourceAlpha;
            settings.DestinationBlend = Blend.One;
        }
    }
}
