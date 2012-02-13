 using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace SaturnIV
{
    class ControlPanelClass
    {
        // Load GUI Assets
        Texture2D bottomPanel;
        Texture2D mediumWindow;
        Texture2D TextInputBox;
        Texture2D dummyTex;
        Vector2 bottomPanelPos = new Vector2(128,800);
        StringBuilder buffer = new StringBuilder();
        KeyboardState oldKeyboardState, currentKeyboardState;
        String textString;
        public static TextBoxActions textBoxActions = new TextBoxActions();
        SpriteBatch spritebatch;
        SpriteFont medFont, smallFont;

        public void LoadPanel(ContentManager Content,SpriteBatch Tspritebatch)
        {
            bottomPanel = Content.Load<Texture2D>("Textures/GUI/cpanel2");
            smallFont = Content.Load<SpriteFont>("Fonts//SmallFont");
            medFont = Content.Load<SpriteFont>("Fonts//MedFont");
            mediumWindow = Content.Load<Texture2D>("Textures/GUI/mWindow");
            TextInputBox = Content.Load<Texture2D>("Textures/GUI/textdialog1");
            dummyTex = Content.Load<Texture2D>("Textures/dummy");
            spritebatch = Tspritebatch;
        }
     public void Draw()
     {
         spritebatch.Begin();
            //spritebatch.Draw(mediumWindow, new Rectangle(20, 512, 275, 384), Color.White);
            spritebatch.Draw(mediumWindow, new Rectangle(25, 30, 275, 256), Color.White);
            //spritebatch.DrawString(smallFont, buffer, new Vector2(148, 825), Color.White);
            spritebatch.End();
    }
        public void drawTextbox(SpriteBatch spritebatch, string text,Vector2 boxPos,List<newShipStruct> itemList)
        {
            bool isDone = false;
            textString += UpdateInput();
            spritebatch.Begin();
            spritebatch.Draw(TextInputBox, new Rectangle((int)boxPos.X - 25, (int)boxPos.Y - 20, 300, 50), Color.White);
            boxPos.Y -= 7;
            spritebatch.DrawString(medFont, text , boxPos, Color.Gray);
            boxPos.X += 70;
            spritebatch.DrawString(medFont, textString, boxPos, Color.White);          
            if (new Rectangle((int)boxPos.X + 150, (int)boxPos.Y - 5, 50, 20).Intersects(new Rectangle(
                Mouse.GetState().X, Mouse.GetState().Y, 2, 2)) && Mouse.GetState().LeftButton == ButtonState.Pressed && !isDone)
            {
                isDone = true;
                Game1.drawTextbox = false;
            }
            spritebatch.End();
            switch (textBoxActions)
            {
                case TextBoxActions.SaveScenario:
                    if (isDone)
                    {
                        
                    }
                    //SerializerClass.exportSaveScenario(thisScenario);                
                    break;
                case TextBoxActions.SaveMap:
                    if (isDone)
                    {                        
                        textBoxActions = TextBoxActions.None;
                    }
                    break;
            }

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
                    if (key == Keys.Back && textString.Length > 1) // overflows
                        textString = textString.Remove(textString.Length - 1, 1);
                    else
                        if (key == Keys.Space)
                            textString = textString.Insert(textString.Length, " ");
                        else
                            if (key == Keys.Enter)
                                textString = textString.Insert(textString.Length, "\n");
                            else
                            {

                                textString += ConvertKeyToChar(key, false).ToString();
                            }

                }
            }
            return textString;
        }

        /// <summary> 
        /// Convert a key to it's respective character or escape sequence. 
        /// </summary> 
        /// <param name="key">The key to convert.</param> 
        /// <param name="shift">Is the shift key pressed or caps lock down.</param> 
        /// <returns>The char for the key that was pressed or string.Empty if it doesn't have a char representation.</returns> 
        public string ConvertKeyToChar(Keys key, bool shift)
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
