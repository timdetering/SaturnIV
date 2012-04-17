using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TomShane.Neoforce.Controls;
using EventArgs = TomShane.Neoforce.Controls.EventArgs;

namespace SaturnIV
{
    class NFClass
    {
        ListBox buildListBox;
        public Window commandWindow;
        public void LoadCommandWindow(Manager manager)
        {
            manager.LayoutDirectory = "Content/Layouts";
            commandWindow = (Window)Layout.Load(manager, "CommandWindow.xml");
            commandWindow.Visible = false;
            commandWindow.Resizable = false;
            manager.Add(commandWindow);
        }

        public void constructionWindow(ref List<shipData> shipList, Manager manager)
        {
            buildListBox = new ListBox(manager);
            buildListBox.Width = 300;
            buildListBox.Height = 200;            
            buildListBox.Top = 450;
            buildListBox.Left = 100;
            buildListBox.Text = "Profile";            
            //buildListBox.ItemIndexChanged += NeoListBox_IndexChanged;
            foreach (shipData tShip in shipList)
                buildListBox.Items.Add(tShip.ShipClass);
            Button buildListCreateBtn = new Button(manager);
            buildListCreateBtn.Text = "Build";
            buildListCreateBtn.Top = 650;
            buildListCreateBtn.Left = 100;
            //buildListBox.Click += NeoButton_Create_OnClick;
            buildListCreateBtn.Init();

            //buildListCreateBtn.Parent = buildListBox;
            manager.Add(buildListBox);
            manager.Add(buildListCreateBtn);
            
        }
    }
}
