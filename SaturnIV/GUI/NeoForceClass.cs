using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Intermediate;
using TomShane.Neoforce.Controls;

namespace SaturnIV
{
    public class NeoForceClass
    {
        public Manager manager;
        Window window;
        Button button;
        List<string> level1buttons = new List<string>();
        
        public void initGUI(Game game, GraphicsDeviceManager graphics)
        {
            
            manager = new Manager(game, graphics, "Default", false);
            manager.Initialize();
            // Create and setup Window control.
            window = new Window(manager);
            window.Init();
            window.Text = "Build Menu";
            window.Width = 70;
            window.Height = 100;
            window.Center();
            window.Visible = true;
            window.BorderVisible = false;
            window.MaximumHeight = 200;
            window.MinimumHeight = 200;
            //window.MaximumWidth = 200;
            //window.MinimumWidth = 200;
            window.Alpha = 255;
            int h = 20;
            foreach (ClassesEnum lclass in Enum.GetValues(typeof(ClassesEnum)))
            {
                // Create Button control and set the previous window as its parent.
                button = new TomShane.Neoforce.Controls.Button(manager);
                button.Init();
                button.Text = lclass.ToString();
                button.Width = 100;
                button.Height = 25;
                button.Left = 0;
                button.Top = h; h += 25;
                //button.Left = (window.ClientWidth / 2) - (button.Width / 2);
                //button.Top = window.ClientHeight - button.Height - 8;
                button.Anchor = Anchors.Bottom;
                button.Parent = window;
            }
            manager.Add(window);
        }        
    }
}
