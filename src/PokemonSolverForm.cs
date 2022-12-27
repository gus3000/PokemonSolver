using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using BenchmarkDotNet.Running;
using BizHawk.Client.Common;
using BizHawk.Client.EmuHawk;
using BizHawk.Common;
using Microsoft.Extensions.Primitives;
using PokemonSolver.Algoritm;
using PokemonSolver.Form;
using PokemonSolver.Gpu;
using PokemonSolver.Image;
using PokemonSolver.Image.Colors;
using PokemonSolver.Image.Map;
using PokemonSolver.Image.Tileset;
using PokemonSolver.Interaction;
using PokemonSolver.MapData;
using PokemonSolver.Memory;

namespace PokemonSolver
{
    [ExternalTool("PokemonSolver")]
    public sealed class PokemonSolverForm : ToolFormBase, IExternalToolForm
    {
        protected override string WindowTitleStatic => "Pokemon Viewer";

        public ApiContainer? _maybeAPIContainer { get; private set; }

        private ApiContainer APIs => _maybeAPIContainer!;

        public GameData? GameData { get; private set; }
        public Engine? Engine { get; private set; }
        public CombatEngine? CombatEngine { get; private set; }
        public PatternSearch? PatternSearch { get; private set; }


        // Form fields
        private PositionControl _startPositionControl;
        private PositionControl _endPositionControl;
        private MapControl _mapControl;

        // private DateTimePicker _dateTimePicker;
        // private DataGridView MapView;
        public Map? CurrentMap { get; set; }


        public PokemonSolverForm()
        {
            InitializeComponent();
            Log.EnableDomain("NetworkDebug");
            Log.EnableDomain("Debug");

            // MapView.Click += (sender, args) => OnMapClick(args as MouseEventArgs); //TODO put back
        }

        public override void Restart()
        {
            PatternSearch = new PatternSearch(APIs.Memory);
            Engine = new Engine(APIs.Memory, APIs.Joypad);
            OverworldEngine.Initialize(APIs.Memory);
            OverworldEngine.GetInstance().ComputeMaps();

            _startPositionControl.InitializeFields();
            _endPositionControl.InitializeFields();

            UpdateData();
            UpdateFields();

            // CombatEngine = new CombatEngine(APIs.Memory);

            // APIs.Gui.WithSurface(DisplaySurfaceID.EmuCore, delegate { Utils.Log("Euuh bonjour"); });
            // APIs.Gui.SetDefaultForegroundColor(Color.Coral);
            // APIs.Gui.SetDefaultBackgroundColor(Color.Black);
            CurrentMap = null;
        }

        protected override void UpdateBefore()
        {
            //TODO dequeue input from engine
        }

        protected override void UpdateAfter()
        {
            UpdateData();
            UpdateFields();
        }


        protected override async void OnClosing(CancelEventArgs e)
        {
            Utils.Log("Closing...");
        }

        private void UpdateData()
        {
            // APIs.Memory.UseMemoryDomain(Domain.ROM.ToString());
            GameData = new GameData(APIs.Memory);
            _mapControl.Update();
        }

        private void UpdateFields()
        {
            _startPositionControl.UpdateFieldsFromGameData();
            _endPositionControl.UpdateFieldsFromGameData();
            _mapControl.Tick();
        }


        private string ReadData()
        {
            return GameData.Serialize();
        }

        private void ComputeClick()
        {
            SetVerbose(true);
            // BenchmarkRunner.Run<AStar>();
            Compute();
            SetVerbose(false);
        }

        private void DebugClick(object sender, EventArgs e)
        {
            SetVerbose(true);
            Utils.Log("Debugging :");

            // ComputeClick();
            // var map = OverworldEngine.GetInstance().GetCurrentMap();
            // Utils.Log($"{map.Debug()}");
            // int x = 0, y = -1;
            // for (x = 0; x < map.MapData.Width; x++)
            //     Utils.Log($"map in ({x},{y}) : {map.GetNextMap(OverworldEngine.GetInstance(), x, y)?.Debug()}");

            // var mapData = OverworldEngine.GetInstance().GetCurrentMap()?.MapData;
            // Palette.DebugPalettes();
            // Tileset.DebugTilesets();
            // Utils.Log($"current tileset => global :{mapData?.GlobalTileset}", true);
            // Utils.Log($"     local :{mapData?.LocalTileset}", true);
            // SetVerbose(false);
            var gpu = new GpuHandler();
            // const string level = "000000000000000000000000000000000000000\n" +
            //                      "111111111111111111111111111111111111110\n" +
            //                      "000000000000000000000000000000000000000\n" +
            //                      "011111111111111111111111111111111111111\n" +
            //                      "000000000000000000000000000000000000000\n" +
            //                      "111111111111111111111111111111111111110\n" +
            //                      "000000000000000000000000000000000000000\n" +
            //                      "011111111111111111111111111111111111111\n" +
            //                      "000000000000000000000000000000000000000\n" +
            //                      "111111111111111111111111111111111111110\n" +
            //                      "000000000000000000000000000000000000000\n" +
            //                      "011111111111111111111111111111111111111\n" +
            //                      "000000000000000000000000000000000000000\n" +
            //                      "111111111111111111111111111111111111110\n" +
            //                      "000000000000000000000000000000000000000\n" +
            //                      "011111111111111111111111111111111111111\n" +
            //                      "000000000000000000000000000000000000000\n" +
            //                      "111111111111111111111111111111111111110\n" +
            //                      "000000000000000000000000000000000000000\n" +
            //                      "011111111111111111111111111111111111111\n" +
            //                      "000000000000000000000000000000000000000";
            //
            // string[] levelLines = level.Split('\n');
            // int height = levelLines.Length;
            // int width = levelLines[0].Length;

            // byte[] permissionBytes = new byte[width * height];

            // var data = new int[width * height];
            // var sb = new StringBuilder(width * height);
            // for (var y = 0; y < height; y++)
            // {
                // for (var x = 0; x < width; x++)
                // {
                    // data[y * width + x] = -1;
                    // permissionBytes[y * width + x] = (byte)(levelLines[y][x] - '0');
                    // sb.Append(permissionBytes[y * width + x].ToString());
                // }

                // sb.Append('\n');
            // }
            
            // data[4] = 0;

            // Utils.Log("permission bytes :");
            // Utils.Log(sb.ToString());

            var map = OverworldEngine.GetInstance().GetCurrentMap();
            int height = map.MapData.Height;
            int width = map.MapData.Width;
            var permissionBytes = (from tile in map.MapData.Tiles select tile.MovementPermission).ToArray();
            
            var endPosition = _endPositionControl.GetPosition();
            var startPosition = _startPositionControl.GetPosition();
            var endIndex = startPosition.Y * width + startPosition.X;
            var data = (from i in Enumerable.Range(0,width * height) select -1).ToArray();
            data[endIndex] = 0;
            
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var results = gpu.Execute(data, permissionBytes, width, endPosition.X, endPosition.Y);
            Utils.Log($"goal : {endPosition}");
            stopwatch.Stop();
            var sbResults = new StringBuilder();
            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    sbResults.Append($"{results[y * width + x],3} ");
                }
                sbResults.Append("\n");
            }
            Utils.Log("Results :");
            Utils.Log(sbResults.ToString());


            Utils.Log($"gpu time : {stopwatch.ElapsedMilliseconds}ms");
        }

        private void Compute()
        {
            // InitFields();
            // return;

            var map = OverworldEngine.GetInstance().GetCurrentMap();
            if (map == null)
            {
                Utils.Error("map is null");
                return;
            }

            // Position characterPosition = OverworldEngine.GetCurrentPosition();
            // Utils.Log($"char position : {characterPosition}");

            //TODO same with custom controls
            var startPosition = _startPositionControl.GetPosition();
            var endPosition = _endPositionControl.GetPosition();

            Utils.Log($"Going from ({startPosition}) to ({endPosition})", true);


            // var astar = new AStar();
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            // var result = astar.Resolve(startPosition, endPosition);
            OverworldEngine.GetInstance().Flood(endPosition);
            stopwatch.Stop();
            return; //TODO remove
            // Utils.Log($"({stopwatch.ElapsedMilliseconds} ms) Result (length {result?.Depth()}):\n {result?.Debug()}", true);
            // _mapControl.ResetCustomColors();
            // // result?.Ancestors(true).ForEach(node => { customColoredPositions.Add(new KeyValuePair<Position, Color>(node.State, CustomColors.Path)); });
            // result?.Ancestors(true).ForEach(node => { _mapControl.AddCustomColor(node.State, CustomColors.Path); });
            // _mapControl.Reset();
        }

        private void GenerateImages(object sender, EventArgs e)
        {
            GenerateTilesetImages();
            // GenerateMapImages();
        }

        private void GenerateTilesetImages()
        {
            foreach (var palette in Palette.GetPalettes())
            {
                new PaletteImage(palette).Save();
            }
        }

        private void GenerateMapImages()
        {
            Utils.Log("Generating maps images");
            foreach (var map in OverworldEngine.GetInstance().Maps)
            {
                new MapPermissionImage(map).Save();
            }
        }

        private void SetVerbose(bool verbose)
        {
            if (verbose)
            {
                Log.EnableDomain("Debug-verbose");
            }
            else
            {
                Log.DisableDomain("Debug-verbose");
            }
        }

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            var miscButtons = new GroupBox();
            debugButton = new Button();
            computeMap = new Button();
            generateMapImages = new Button();
            SuspendLayout();

            //
            // miscButtons
            //
            miscButtons.Controls.Add(debugButton);
            miscButtons.Controls.Add(computeMap);
            miscButtons.Controls.Add(generateMapImages);
            miscButtons.Name = "miscButtons";
            miscButtons.Size = new Size(500, 50);
            miscButtons.TabIndex = 1;
            miscButtons.TabStop = false;
            miscButtons.Text = "misc";
            miscButtons.Dock = DockStyle.Top;
            miscButtons.Anchor = (AnchorStyles.Top | AnchorStyles.Left);

            // 
            // debugButton
            // 
            debugButton.Location = new Point(12, 15);
            debugButton.Name = "debugButton";
            // debugButton.Size = new Size(75, 38);
            debugButton.TabIndex = 0;
            debugButton.Text = "Debug";
            debugButton.UseVisualStyleBackColor = true;
            debugButton.Click += DebugClick;
            // 
            // computeMap
            // 
            computeMap.Location = new Point(FormUtils.DefaultMargin + debugButton.Right, 15);
            computeMap.Name = "computeMap";
            // computeMap.Size = new Size(75, 35);
            computeMap.TabIndex = 1;
            computeMap.Text = "Compute Maps";
            computeMap.UseVisualStyleBackColor = true;
            computeMap.Click += (_, _) => ComputeClick();


            //
            // generateMapImages
            //
            generateMapImages.Location = new Point(FormUtils.DefaultMargin + computeMap.Right, 15);
            generateMapImages.Name = "generateMapImages";
            generateMapImages.TabIndex = 0;
            generateMapImages.Text = "Generate Map Images";
            generateMapImages.UseVisualStyleBackColor = true;
            generateMapImages.Click += GenerateImages;

            _startPositionControl = new PositionControl("start", FormUtils.DefaultMargin, miscButtons.Bottom + FormUtils.DefaultMargin);
            _endPositionControl = new PositionControl("goal", FormUtils.DefaultMargin, _startPositionControl.Bottom + FormUtils.DefaultMargin);

            _mapControl = new MapControl(FormUtils.DefaultMargin, _endPositionControl.Bottom + FormUtils.DefaultMargin);

            _startPositionControl.SetUseCharacterPosition(true);

            // 
            // PokemonSolverForm
            // 
            ClientSize = new Size(FormUtils.WindowWidth, FormUtils.WindowHeight);
            Controls.Add(miscButtons);
            Controls.Add(_startPositionControl);
            Controls.Add(_endPositionControl);
            Controls.Add(_mapControl);
            Name = "PokemonSolverForm";
            ResumeLayout(false);
        }

        private void ForceComputeMaps(object sender, EventArgs e)
        {
            SetVerbose(true);
            OverworldEngine.GetInstance().ComputeMaps(true);
            SetVerbose(false);
        }

        private Button computeMap;
        private Button debugButton;
        private Button generateMapImages;
    }
}