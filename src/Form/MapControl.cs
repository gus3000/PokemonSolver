using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using PokemonSolver.Algoritm;
using PokemonSolver.Image;
using PokemonSolver.Image.Map;
using PokemonSolver.Interaction;
using PokemonSolver.MapData;
using PokemonSolver.Memory;

namespace PokemonSolver.Form
{
    public class MapControl : UserControl
    {
        private Map? _map;
        private PictureBox MapView;
        private List<KeyValuePair<Position, Color>> customColoredPositions;


        public MapControl(int x, int y)
        {
            MapView = new PictureBox();
            customColoredPositions = new List<KeyValuePair<Position, Color>>();

            Location = new Point(x, y);
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            Controls.Add(MapView);
            Size = new Size(FormUtils.WindowWidth, FormUtils.WindowHeight);
        }
        private System.Drawing.Image? getBitmapFromMap()
        {
            if (_map == null)
            {
                // Utils.Log("no map => no map display");
                return null;
            }

            Utils.Log($"loading map in picture : {_map.Name}", true);
            return new MapPreviewImage(_map, customColoredPositions).Image;
        }
        
        public void Reset()
        {
            Utils.Log("Reset Map");
            // customColoredPositions = new List<KeyValuePair<Position, Color>>();

            MapView.Image = getBitmapFromMap();
            MapView.SizeMode = PictureBoxSizeMode.Normal;
            MapView.Left = 0;
            // MapView.Top = goal.Bottom; //TODO same with custom controls
            // Utils.Log($"Setting map size from {MapView.Size} to {MapView.GetPreferredSize(ClientSize)}");
            MapView.Size = MapView.GetPreferredSize(ClientSize);

            ClientSize = new Size(ClientSize.Width, MapView.Bottom + 5);
        }
        protected void OnMapClick(MouseEventArgs? mouseEvent)
        {
            if (mouseEvent == null)
                throw new Exception("clicked on map but somehow it's not a mouseEvent");

            var cellSize = MapView.Width / OverworldEngine.GetInstance().GetCurrentMap().MapData.Width;
            var x = mouseEvent.X / cellSize;
            var y = mouseEvent.Y / cellSize;
            Utils.Log($"{mouseEvent.X},{mouseEvent.Y} to map in (0,0,{MapView.Width},{MapView.Height}) -> ({x},{y})");

            //TODO call setPosition(x,y) on endPositionField
            // endX.Value = x;
            // endY.Value = y;
            // ComputeClick(); //TODO replace by Compute
        }

        public void ResetCustomColors()
        {
            customColoredPositions.Clear();
        }

        public void AddCustomColor(Position p, Color c)
        {
            customColoredPositions.Add(new KeyValuePair<Position, Color>(p,c));
        }

        public void Tick()
        {
            
            var newMap = OverworldEngine.GetInstance().GetCurrentMap();
            
            if (newMap == _map) return;
            
            _map = newMap;
            Reset();
        }
    }
}