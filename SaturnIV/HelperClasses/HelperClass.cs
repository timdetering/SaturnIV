using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    class HelperClass
    {
        KeyboardState oldKeyboardState;
        KeyboardState currentKeyboardState;
        string textString;
        private float _FPS = 0f, _TotalTime = 0f, _DisplayFPS = 0f;
        Random random = new Random();

        /// <summary>
        /// Returns a number between two values.
        /// </summary>
        /// <param name="min">Lower bound value</param>
        /// <param name="max">Upper bound value</param>
        /// <returns>Random number between bounds.</returns>
        public static float RandomBetween(double min, double max)
        {
            Random random = new Random();
            return (float)(min + (float)random.NextDouble() * (max - min));
        }

        public static Vector3 RandomPosition(float minBoxPos, float maxBoxPos)
        {
            return new Vector3(
                     RandomBetween(minBoxPos, maxBoxPos),
                     0.0f,
                     RandomBetween(minBoxPos, maxBoxPos));
        }
        /// <summary>  
        /// Choose a random point on the sphere with radius 1 centered at (0, 0, 0).  
        /// </summary>  
        /// <returns>Random vector with length 1</returns>  
        public static Vector3 RandomSpherePoint()
        {
            Vector3 v;
            Random random = new Random();
            // Pick a random vector that has non-zero length.  
            // It's almost certain that the first one picked will be non-zero,  
            // but double check it to avoid division by 0 in Normalize.  
            do
            {
                v.X = (float)random.NextDouble() - 500000.5f;
                v.Y = 0.0f;
                v.Z = (float)random.NextDouble() - 500000.5f;
            } while (v.LengthSquared() == 0);

            // Normalize the vector so that its length is 1.  
            // This snaps it to the surface of the sphere.  
            v.Normalize();

            return v;
        } 

        public static Vector3 RandomDirection()
        {
            Random random = new Random();

            Vector3 direction = new Vector3(
                    RandomBetween(-1.0f, 1.0f),
                     0,
                    RandomBetween(-1.0f, 1.0f));
            direction.Normalize();

            return direction;
        }

        //CheckForCollision give to object lists
        public bool CheckForCollision(GameTime gameTime, ref List<newShipStruct> thisShipList, ref List<weaponStruct> missileList, ref ExplosionClass ourExplosion,ref gameServer gServer)
        {
            for (int j = 0; j < thisShipList.Count; j++)
            {
                for (int i = 0; i < missileList.Count; i++)
                {
                    if ((thisShipList[j].portFrustum.Intersects(missileList[i].modelBoundingSphere) || thisShipList[j].starboardFrustum.Intersects(missileList[i].modelBoundingSphere))
                        && missileList[i].missileOrigin != thisShipList[j])
                    {
                        thisShipList[j].shieldLvl -= thisShipList[j].shieldFactor * missileList[i].damageFactor;
                        if (thisShipList[j].shieldLvl < 0)
                            thisShipList[j].hullLvl -= thisShipList[j].shieldFactor * missileList[i].damageFactor;
                        Vector3 currentExpLocation = missileList[i].modelPosition;
                        missileList.Remove(missileList[i]);
                        ourExplosion.CreateExplosionVertices((float)gameTime.TotalGameTime.TotalMilliseconds,
                                                        currentExpLocation, (float)random.NextDouble());
                        if (thisShipList[j].hullLvl < 1)
                        {
                            for (int y=0; y< 25;y++)
                                ourExplosion.CreateExplosionVertices((float)gameTime.TotalGameTime.TotalMilliseconds,
                                                           currentExpLocation, (float)random.Next(1,3));
                            MessageClass.messageLog.Add(thisShipList[j].objectAlias + " is destroyed");
                            thisShipList.Remove(thisShipList[j]);
                            gServer.removeObject(j);
                         }                       
                        return true;
                    }
                }
            }
            return false;
        }

      //CheckForCollision given a single object and list of objects
        public bool CheckForCollisionMech(GameTime gameTime, newShipStruct thisShip, ref List<newShipStruct> objectList, ref ExplosionClass ourExplosion)
        {
            for (int i = 0; i < objectList.Count; i++)
            {
                
                    if (thisShip.portFrustum.Intersects(objectList[i].modelBoundingSphere) || thisShip.starboardFrustum.Intersects(objectList[i].modelBoundingSphere))
                    {
                        Ray myRay = new Ray(objectList[i].modelPosition + new Vector3(0, objectList[i].modelWidth,0), Vector3.Down);
                        float vertDif = thisShip.modelPosition.Y - objectList[i].modelPosition.Y;
                        thisShip.modelPosition.Y = vertDif;
                        return true;
                    }
            }
            return false;
        }

        public void DrawFPS(GameTime gameTime, GraphicsDevice device, SpriteBatch spriteBatch, SpriteFont spriteFont)
        {
            // Calculate the Frames Per Second
            float ElapsedTime = (float)gameTime.ElapsedRealTime.TotalSeconds;
            _TotalTime += ElapsedTime;

            if (_TotalTime >= 1)
            {
                _DisplayFPS = _FPS;
                _FPS = 0;
                _TotalTime = 0;
            }
            _FPS += 1;

            // Format the string appropriately
            string FpsText = _DisplayFPS.ToString() + " FPS";
            Vector2 FPSPos = new Vector2((device.Viewport.Width - spriteFont.MeasureString(FpsText).X) - 15, 10);
            spriteBatch.Begin();
            spriteBatch.DrawString(spriteFont, FpsText, FPSPos, Color.White);
            spriteBatch.End();
        }

        public string UpdateInput()
        {
            oldKeyboardState = currentKeyboardState;
            currentKeyboardState = Keyboard.GetState();
            textString = "";
            Keys[] pressedKeys;
            
            pressedKeys = currentKeyboardState.GetPressedKeys();
            //if (pressedKeys.Count() > 0)
            //    textString = pressedKeys[0].ToString();
            foreach (Keys key in pressedKeys)
            {
                if (oldKeyboardState.IsKeyUp(key))
                {
                    if (key == Keys.Back && textString.Length>1) // overflows
                        textString = textString.Remove(textString.Length - 1, 1);
                    else
                        if (key == Keys.Space)
                            textString = textString.Insert(textString.Length, " ");
                        else
                            if (key == Keys.Enter)
                                textString = textString.Insert(textString.Length, "\n");
                            else
                            {

                                textString += HelperClass.ConvertKeyToChar(key, false).ToString();
                            }

                }
            }
                return textString;
        }

        public static BoundingBox updateBB(Vector3 min,Vector3 max,Vector3 position)
        {
            Vector3 min1 = Vector3.Transform(min, Matrix.CreateTranslation(position));
            Vector3 max1 = Vector3.Transform(max, Matrix.CreateTranslation(position));
            return new BoundingBox(min1, max1);
        }

        public static BoundingBox ComputeBoundingBox(Model _model,Vector3 position)
        {
            Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);

            Matrix[] bones = new Matrix[_model.Bones.Count];
            _model.CopyAbsoluteBoneTransformsTo(bones);

            List<Vector3> vertices = new List<Vector3>();

            foreach (ModelMesh mesh in _model.Meshes)
            {
                //get the transform of the current mesh
                Matrix transform = bones[mesh.ParentBone.Index];// *worldMatrix;

                foreach (ModelMeshPart part in mesh.MeshParts)
                {
                    //get the current mesh info
                    int stride = part.VertexStride;
                    int numVertices = part.NumVertices;
                    byte[] verticesData = new byte[stride * numVertices];

                    mesh.VertexBuffer.GetData(verticesData);

                    for (int i = 0; i < verticesData.Length; i += stride)
                    {
                        float x = BitConverter.ToSingle(verticesData, i);
                        float y = BitConverter.ToSingle(verticesData, i + sizeof(float));
                        float z = BitConverter.ToSingle(verticesData, i + 2 * sizeof(float));

                        Vector3 vector = new Vector3(x, y, z);
                        //apply transform to the current point
                        vector = Vector3.Transform(vector, transform);

                        vertices.Add(vector);

                        if (vector.X < min.X) min.X = vector.X;
                        if (vector.Y < min.Y) min.Y = vector.Y;
                        if (vector.Z < min.Z) min.Z = vector.Z;
                        if (vector.X > max.X) max.X = vector.X;
                        if (vector.Y > max.Y) max.Y = vector.Y;
                        if (vector.Z > max.Z) max.Z = vector.Z;
                    }
                }
            }

            //_VerticesCount = vertices.Count;
            //_Vertices = vertices.ToArray();
            min = Vector3.Transform(min, Matrix.CreateTranslation(position));
           max = Vector3.Transform(max, Matrix.CreateTranslation(position));
            return new BoundingBox(min, max);
        }
        /// <summary> 
        /// Convert a key to it's respective character or escape sequence. 
        /// </summary> 
        /// <param name="key">The key to convert.</param> 
        /// <param name="shift">Is the shift key pressed or caps lock down.</param> 
        /// <returns>The char for the key that was pressed or string.Empty if it doesn't have a char representation.</returns> 
        public static string ConvertKeyToChar(Keys key, bool shift)
        {
            switch (key)
            {
                case Keys.Space: return " ";

                // Escape Sequences 
                case Keys.Enter: return "\n";                         // Create a new line 
                case Keys.Tab: return "\t";                           // Tab to the right 

                // D-Numerics (strip above the alphabet) 
                case Keys.D0: return shift ? ")" : "0";
                case Keys.D1: return shift ? "!" : "1";
                case Keys.D2: return shift ? "@" : "2";
                case Keys.D3: return shift ? "#" : "3";
                case Keys.D4: return shift ? "$" : "4";
                case Keys.D5: return shift ? "%" : "5";
                case Keys.D6: return shift ? "^" : "6";
                case Keys.D7: return shift ? "&" : "7";
                case Keys.D8: return shift ? "*" : "8";
                case Keys.D9: return shift ? "(" : "9";

                // Numpad 
                case Keys.NumPad0: return "0";
                case Keys.NumPad1: return "1";
                case Keys.NumPad2: return "2";
                case Keys.NumPad3: return "3";
                case Keys.NumPad4: return "4";
                case Keys.NumPad5: return "5";
                case Keys.NumPad6: return "6";
                case Keys.NumPad7: return "7";
                case Keys.NumPad8: return "8";
                case Keys.NumPad9: return "9";
                case Keys.Add: return "+";
                case Keys.Subtract: return "-";
                case Keys.Multiply: return "*";
                case Keys.Divide: return "/";
                case Keys.Decimal: return ".";

                // Alphabet 
                case Keys.A: return shift ? "A" : "a";
                case Keys.B: return shift ? "B" : "b";
                case Keys.C: return shift ? "C" : "c";
                case Keys.D: return shift ? "D" : "d";
                case Keys.E: return shift ? "E" : "e";
                case Keys.F: return shift ? "F" : "f";
                case Keys.G: return shift ? "G" : "g";
                case Keys.H: return shift ? "H" : "h";
                case Keys.I: return shift ? "I" : "i";
                case Keys.J: return shift ? "J" : "j";
                case Keys.K: return shift ? "K" : "k";
                case Keys.L: return shift ? "L" : "l";
                case Keys.M: return shift ? "M" : "m";
                case Keys.N: return shift ? "N" : "n";
                case Keys.O: return shift ? "O" : "o";
                case Keys.P: return shift ? "P" : "p";
                case Keys.Q: return shift ? "Q" : "q";
                case Keys.R: return shift ? "R" : "r";
                case Keys.S: return shift ? "S" : "s";
                case Keys.T: return shift ? "T" : "t";
                case Keys.U: return shift ? "U" : "u";
                case Keys.V: return shift ? "V" : "v";
                case Keys.W: return shift ? "W" : "w";
                case Keys.X: return shift ? "X" : "x";
                case Keys.Y: return shift ? "Y" : "y";
                case Keys.Z: return shift ? "Z" : "z";

                // Oem 
                case Keys.OemOpenBrackets: return shift ? "{" : "[";
                case Keys.OemCloseBrackets: return shift ? "}" : "]";
                case Keys.OemComma: return shift ? "<" : ",";
                case Keys.OemPeriod: return shift ? ">" : ".";
                case Keys.OemMinus: return shift ? "_" : "-";
                case Keys.OemPlus: return shift ? "+" : "=";
                case Keys.OemQuestion: return shift ? "?" : "/";
                case Keys.OemSemicolon: return shift ? ":" : ";";
                case Keys.OemQuotes: return shift ? "\"" : "'";
                case Keys.OemPipe: return shift ? "|" : "\\";
                case Keys.OemTilde: return shift ? "~" : "`";
            }

            return string.Empty;
        } 
    }      
}
