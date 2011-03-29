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


		public class Athruster
		{
        
            Effect effect;
        float rotx = 0;
        float roty = MathHelper.ToRadians(90);


        Vector3 demo_position = Vector3.Zero;
        Vector3 demo_direction;
        Vector3 demo_scaling;

        float demo_heat = 1;
        float xscale, yscale, zscale;
        float allscale = 1;

			#region thruster variables
			public Model model;
			public Texture3D Noise;

			public EffectTechnique technique;
			public EffectParameter shader_matrices;
			public EffectParameter thrust_color;
			public EffectParameter effect_tick;
			public EffectParameter effect_noise;

			public Color[] colors = new Color[2];
			public Vector4[] v4colors = new Vector4[2];

			public float heat = 5; // controls the length of the thrust
			public float tick = 10; // controls the rate of thrust

			public Matrix world_matrix;
			public Matrix inverse_scale_transpose;
			public Matrix scale;
			public Matrix[] matrices_combined = new Matrix[3];

			public Vector3 Up;
			public Vector3 Right;
			public Vector3 dir_to_camera;
			//public float allscale = 1;
			#endregion

            public void LoadContent(Game game, SpriteBatch spriteBatch)
            {
                spriteBatch = new SpriteBatch(game.GraphicsDevice);

                effect = game.Content.Load<Effect>("Thrusters/Thrust");

                this.load_and_assign_effects(game.Content, "Thrusters/ThrustCylinderB", "Thrusters/NoiseVolume", effect);
            }

			public void load_and_assign_effects(ContentManager content, string filename, string noisefilename, Effect effect)
			{
				model = content.Load<Model>(filename);
				Noise = content.Load<Texture3D>(noisefilename);

				model.Meshes[0].MeshParts[0].Effect = effect;

				technique = effect.Techniques["thrust_technique"];
				shader_matrices = effect.Parameters["world_matrices"];
				thrust_color = effect.Parameters["thrust_color"];
				effect_tick = effect.Parameters["ticks"];
				effect_noise = effect.Parameters["noise_texture"];

				world_matrix = Matrix.Identity;

			}

			public void update(Vector3 Put_me_at,
												 Vector3 Point_me_at,
												 Vector3 Scale_factors,
												 float thrustsize,
												 float thrustspeed,
												 Color color_at_exhaust,
												 Color color_at_end,
												 Vector3 camera_position)
			{

				heat = MathHelper.Clamp(thrustsize, 0, 1);

				// rate of thrust
				tick += thrustspeed;

				// color[0] is the first color coming out of the thruster, 1 is the second.
				colors[0] = color_at_end;
				colors[1] = color_at_exhaust;
				//====================================================


				// these manipulations are necessary to get its edges to fade correctly
				// it keeps the "right" vector on a plane that goes through the camera

				world_matrix = Matrix.Identity;

				world_matrix.Forward = Point_me_at;

				#region calculate direction to camera
				dir_to_camera.X = Put_me_at.X - camera_position.X;
				dir_to_camera.Y = Put_me_at.Y - camera_position.Y;
				dir_to_camera.Z = Put_me_at.Z - camera_position.Z;
				
				#endregion

			
				Vector3.Cross(ref Point_me_at, ref dir_to_camera, out Up); // calculate UP

				Up.Normalize();

				Vector3.Cross(ref Point_me_at, ref Up, out Right); // Calculate Right
			
				world_matrix.Right = Right;
				world_matrix.Up = Up;

				Matrix.CreateScale(ref Scale_factors, out scale);  // Create a scale matrix,if you have scale
		
				Matrix.Multiply(ref world_matrix, ref scale, out world_matrix); // scale the world matrix
				
				inverse_scale_transpose = Matrix.Transpose(Matrix.Invert(world_matrix)); 
				// the inverst transpose above is to re-adjust the normals in the shader
				
				world_matrix.Translation = Put_me_at;


				//====================================================

			}

			#region use these two functions if you are using the code to draw many many many thrusters (see game draw)

			public void set_technique(Effect effect)
			{
				effect.CurrentTechnique = technique;
			}

			public void prepare_effect(ref Matrix view, ref Matrix project, Effect effect)
			{
				matrices_combined[0] = world_matrix;
				matrices_combined[1] = world_matrix * view * project;
				matrices_combined[2] = inverse_scale_transpose;

				shader_matrices.SetValue(matrices_combined);

				v4colors[0] = colors[0].ToVector4();
				v4colors[1] = colors[1].ToVector4();
				v4colors[0].W = heat;

				thrust_color.SetValue(v4colors);
				effect_tick.SetValue(tick);
				effect_noise.SetValue(Noise);

				effect.CommitChanges();

			}

			#endregion

			#region use this if you just have a few thrusters
			public void draw(Matrix view, Matrix project)
			{
				matrices_combined[0] = world_matrix;
                matrices_combined[1] = world_matrix * view * project;
				matrices_combined[2] = inverse_scale_transpose;

				effect.CurrentTechnique = technique;
				shader_matrices.SetValue(matrices_combined);

				v4colors[0] = colors[0].ToVector4();
				v4colors[1] = colors[1].ToVector4();
				v4colors[0].W = heat;

				thrust_color.SetValue(v4colors);
				effect_tick.SetValue(tick);
				effect_noise.SetValue(Noise);

				model.Meshes[0].Draw();

			}
			#endregion
		}
    }