#region File Description
//-----------------------------------------------------------------------------
// Projectile.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
#endregion

namespace SaturnIV
{
    /// <summary>
    /// This class demonstrates how to combine several different particle systems
    /// to build up a more sophisticated composite effect. It implements a rocket
    /// projectile, which arcs up into the sky using a ParticleEmitter to leave a
    /// steady stream of trail particles behind it. After a while it explodes,
    /// creating a sudden burst of explosion and smoke particles.
    /// </summary>
    public class Projectile
    {
        #region Constants

        const float trailParticlesPerSecond = 200;
        const int numExplosionParticles = 30;
        const int numExplosionSmokeParticles = 50;
        const float projectileLifespan = 1.5f;
        const float sidewaysVelocityRange = 10;
        const float verticalVelocityRange = 15;
        const float gravity = 0f;

        #endregion

        #region Fields

        ParticleEmitter trailEmitter;

        Vector3 velocity;
        float age;

        static Random random = new Random();

        #endregion


        /// <summary>
        /// Constructs a new projectile.
        /// </summary>
        public Projectile(ParticleSystem projectileTrailParticles, Vector3 position,Vector3 velocity)
        {
            // Use the particle emitter helper to output our trail particles.
            trailEmitter = new ParticleEmitter(projectileTrailParticles,
                                               trailParticlesPerSecond, position, velocity);
        }

        /// <summary>
        /// Updates the projectile.
        /// </summary>
        public bool Update(GameTime gameTime, Vector3 position)
        {
            float elapsedTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Simple projectile physics.
            //position += velocity * elapsedTime;
            //velocity.Y -= elapsedTime * gravity;
            age += elapsedTime;

            // Update the particle emitter, which will create our particle trail.
            trailEmitter.Update(gameTime, position);    
            return true;
        }
    }
}
