/* Generated by MyraPad at 12/28/2024 1:43:57 PM */
using Myra;
using Myra.Graphics2D;
using Myra.Graphics2D.TextureAtlases;
using Myra.Graphics2D.UI;
using Myra.Graphics2D.Brushes;
using Myra.Graphics2D.UI.Properties;
using FontStashSharp.RichText;
using AssetManagementBase;

#if STRIDE
using Stride.Core.Mathematics;
#elif PLATFORM_AGNOSTIC
using System.Drawing;
using System.Numerics;
using Color = FontStashSharp.FSColor;
#else
// MonoGame/FNA
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endif

namespace PurplePuffin
{
	partial class MainMenuMyra: Panel
	{
		private void BuildUI()
		{
			var label1 = new Label();
			label1.Text = "Purple Puffin";
			label1.TextColor = ColorStorage.CreateColor(248, 255, 199, 255);
			label1.Top = 20;
			label1.HorizontalAlignment = Myra.Graphics2D.UI.HorizontalAlignment.Center;

			_menuStartNewGame = new MenuItem();
			_menuStartNewGame.Text = "Start New Game";
			_menuStartNewGame.Id = "_menuStartNewGame";

			_menuOptions = new MenuItem();
			_menuOptions.Text = "Options";
			_menuOptions.Id = "_menuOptions";

			_menuQuit = new MenuItem();
			_menuQuit.Text = "Quit";
			_menuQuit.Id = "_menuQuit";

			_mainMenu = new VerticalMenu();
			_mainMenu.HorizontalAlignment = Myra.Graphics2D.UI.HorizontalAlignment.Center;
			_mainMenu.VerticalAlignment = Myra.Graphics2D.UI.VerticalAlignment.Center;
			_mainMenu.LabelColor = ColorStorage.CreateColor(248, 255, 199, 255);
			_mainMenu.SelectionHoverBackground = new SolidBrush("#3F1178C0");
			_mainMenu.SelectionBackground = new SolidBrush("#711ED6C0");
			_mainMenu.Border = new SolidBrush("#00000000");
			_mainMenu.Id = "_mainMenu";
			_mainMenu.Items.Add(_menuStartNewGame);
			_mainMenu.Items.Add(_menuOptions);
			_mainMenu.Items.Add(_menuQuit);

			var label2 = new Label();
			label2.Text = "Version 0.1";
			label2.TextColor = ColorStorage.CreateColor(248, 255, 199, 255);
			label2.Left = -10;
			label2.Top = -10;
			label2.HorizontalAlignment = Myra.Graphics2D.UI.HorizontalAlignment.Right;
			label2.VerticalAlignment = Myra.Graphics2D.UI.VerticalAlignment.Bottom;

			
			Background = new SolidBrush("#711ED6FF");
			Widgets.Add(label1);
			Widgets.Add(_mainMenu);
			Widgets.Add(label2);
		}

		
		public MenuItem _menuStartNewGame;
		public MenuItem _menuOptions;
		public MenuItem _menuQuit;
		public VerticalMenu _mainMenu;
	}
}
