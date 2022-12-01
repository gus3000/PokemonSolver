using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using BizHawk.Client.Common;
using BizHawk.Client.EmuHawk;
using BizHawk.Common;
using PokemonSolver;
using PokemonSolver.Algoritm;
using PokemonSolver.Image;
using PokemonSolver.Image.Colors;
using PokemonSolver.Interaction;
using PokemonSolver.MapData;
using PokemonSolver.Memory;
using PokemonSolver.Memory.Global;
using PokemonSolver.PokemonData;

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
        public OverworldEngine? OverworldEngine { get; private set; }
        public PatternSearch? PatternSearch { get; private set; }


        private Label labelInfo;

        // private DateTimePicker _dateTimePicker;
        // private DataGridView MapView;
        public Map? CurrentMap { get; set; }
        private PictureBox MapView;

        // private System.Windows.Controls currentMap;
        private StringBuilder currentStatus;
        private List<KeyValuePair<Position, Color>> customColoredPositions;

        public PokemonSolverForm()
        {
            currentStatus = new StringBuilder();
            InitializeComponent();
            startDirection.SelectedIndex = (int)Direction.Down;
            endDirection.SelectedIndex = (int)Direction.Down;
            SuspendLayout();
            Controls.Add(labelInfo = new Label { AutoSize = true, MaximumSize = new Size(480, 0) });
            Controls.Add(MapView = new PictureBox());
            // Controls.Add(MapView = new DataGridView());
            // Controls.Add(_dateTimePicker = new DateTimePicker());
            // Controls.Add(currentMap);
            ResumeLayout();

            Log.EnableDomain("NetworkDebug");
            Log.EnableDomain("Debug");

            MapView.Click += (sender, args) => OnMapClick(args as MouseEventArgs);
            customColoredPositions = new List<KeyValuePair<Position, Color>>();
        }

        public override void Restart()
        {
            UpdateData();
            PatternSearch = new PatternSearch(APIs.Memory);
            Engine = new Engine(APIs.Memory, APIs.Joypad);
            // CombatEngine = new CombatEngine(APIs.Memory);
            OverworldEngine = new OverworldEngine(APIs.Memory);
            APIs.Gui.WithSurface(DisplaySurfaceID.EmuCore, delegate { Utils.Log("Euuh bonjour"); });
            APIs.Gui.SetDefaultForegroundColor(Color.Coral);
            APIs.Gui.SetDefaultBackgroundColor(Color.Black);
            OverworldEngine.ComputeMaps();
            InitFields();
            CurrentMap = null;
            // labelInfo.Text = "C'ay la mayrde";
            // APIs.Gui.ClearGraphics(DisplaySurfaceID.EmuCore);
        }

        private void InitFields()
        {
            startMapBank.Items.Clear();
            startMapIndex.Items.Clear();
            endMapBank.Items.Clear();
            endMapIndex.Items.Clear();

            var mapBanks = OverworldEngine.Banks.Select((_, index) => index.ToString()).ToArray();
            startMapBank.Items.AddRange(mapBanks);
            endMapBank.Items.AddRange(mapBanks);
            
            InitMapIndexField(startMapBank, startMapIndex);
            InitMapIndexField(endMapBank, endMapIndex);
        }
        private void InitMapIndexField(ComboBox mapBank, ComboBox mapIndex)
        {
            mapIndex.Items.Clear();
            var mapBankIndex = mapBank.SelectedIndex;
            Utils.Log($"selected map bank index : {mapBankIndex}");
            IList<Map> maps;
            if (mapBankIndex == -1)
            {
                maps = OverworldEngine.Maps;
            }
            else
            {
                maps = OverworldEngine.Banks[mapBankIndex];
            }
            mapIndex.Items.AddRange(maps.Select((map) => map.Name).ToArray());
        }

        protected override void UpdateBefore()
        {
            //TODO dequeue input from engine
        }

        protected override void UpdateAfter()
        {
            currentStatus.Clear();
            UpdateData();
        }


        protected override async void OnClosing(CancelEventArgs e)
        {
            Utils.Log("Closing...");
        }

        protected void OnMapClick(MouseEventArgs? mouseEvent)
        {
            if (mouseEvent == null)
                throw new Exception("clicked on map but somehow it's not a mouseEvent");

            var cellSize = MapView.Width / OverworldEngine.GetCurrentMap().MapData.Width;
            var x = mouseEvent.X / cellSize;
            var y = mouseEvent.Y / cellSize;
            Utils.Log($"{mouseEvent.X},{mouseEvent.Y} to map in (0,0,{MapView.Width},{MapView.Height}) -> ({x},{y})");

            endX.Value = x;
            endY.Value = y;
            ComputeClick(); //TODO replace by Compute
        }

        private System.Drawing.Image? getBitmapFromMap(Map? map)
        {
            if (map == null)
            {
                // Utils.Log("no map => no map display");
                return null;
            }

            Utils.Log($"loading map in picture : {map.Name}", true);
            return new MapPreviewImage(map, customColoredPositions).Image;
        }

        private void UpdateData()
        {
            // APIs.Memory.UseMemoryDomain(Domain.ROM.ToString());
            GameData = new GameData(APIs.Memory);
            // currentStatus.Append(GameData.Team[0].Nickname);
            UpdateMap();
            if (useCharacterAsStartPosition.Checked)
            {
                UpdateFieldsFromCharPosition();
            }
        }

        private void UpdateFieldsFromCharPosition()
        {
            //FIXME bug going North from Mauville
            var characterPosition = OverworldEngine?.GetCurrentPosition();
            // Utils.Log(characterPosition?.ToString());
            if (characterPosition == null) return;
            
            startMapBank.SelectedIndex = (int)characterPosition.MapBank;
            startMapIndex.SelectedIndex = (int)characterPosition.MapIndex;
            
            startX.Value = characterPosition.X;
            startY.Value = characterPosition.Y;
            // Utils.Log($"Direction : {startDirection.SelectedIndex} -> {GetSelectIndexFromDirection(characterPosition.Direction)} ({characterPosition.Direction})");
            startDirection.SelectedIndex = GetSelectIndexFromDirection(characterPosition.Direction);
        }

        private void UpdateMap()
        {
            if (OverworldEngine == null)
                return;
            var newMap = OverworldEngine.GetCurrentMap();
            if (newMap != CurrentMap)
            {
                CurrentMap = newMap;
                ResetMap();
            }

            DrawMap();
        }

        private void ResetMap()
        {
            Utils.Log("Reset Map");
            customColoredPositions = new List<KeyValuePair<Position, Color>>();

            DrawMap();
            MapView.SizeMode = PictureBoxSizeMode.Normal;
            MapView.Left = 0;
            MapView.Top = this.goal.Bottom;
            MapView.Size = MapView.GetPreferredSize(ClientSize);

            ClientSize = new Size(ClientSize.Width, MapView.Bottom + 5);
        }

        private void DrawMap()
        {
            MapView.Image = getBitmapFromMap(CurrentMap);
        }

        private string ReadData()
        {
            return GameData.Serialize();
        }

        private void ComputeClick()
        {
            SetVerbose(true);
            Compute();
            SetVerbose(false);
        }

        private void ComputeClick(object sender, EventArgs e)
        {
            ComputeClick();
        }

        private void Compute(bool debug = false)
        {
            InitFields();
            return;
            
            if (OverworldEngine == null)
            {
                Utils.Error("wtf overworldEngine is null");
                return;
            }

            var map = OverworldEngine.GetCurrentMap();
            if (map == null)
            {
                Utils.Error("map is null");
                return;
            }

            // Position characterPosition = OverworldEngine.GetCurrentPosition();
            // Utils.Log($"char position : {characterPosition}");
            var startPosition = GetStartPosition();
            var endPosition = GetEndPosition();

            Utils.Log($"Going from ({startPosition}) to ({endPosition})");


            var astar = new AStar(map.MapData);
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var result = debug ? astar.ResolveBenchmark(startPosition, endPosition) : astar.Resolve(startPosition, endPosition);
            stopwatch.Stop();
            Utils.Log($"({stopwatch.ElapsedMilliseconds} ms) Result (length {result?.Depth()}):\n {result?.Debug()}", true);
            customColoredPositions = new List<KeyValuePair<Position, Color>>();
            result?.Ancestors(true).ForEach(node => { customColoredPositions.Add(new KeyValuePair<Position, Color>(node.State, CustomColors.Path)); });
        }

        private Position GetStartPosition()
        {
            return new Position(
                (uint)startMapBank.SelectedIndex,
                (uint)startMapIndex.SelectedIndex,
                (uint)startX.Value,
                (uint)startY.Value,
                GetDirectionFromSelectIndex(startDirection.SelectedIndex),
                Altitude.Any
            ); //TODO set altitude
        }

        private Position GetEndPosition()
        {
            return new Position(
                (uint)endMapBank.SelectedIndex,
                (uint)endMapIndex.SelectedIndex,
                (uint)endX.Value,
                (uint)endY.Value,
                GetDirectionFromSelectIndex(endDirection.SelectedIndex),
                Altitude.Any
            ); //TODO set altitude
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
            System.Windows.Forms.Label startXLabel;
            System.Windows.Forms.Label startYLabel;
            System.Windows.Forms.Label endYLabel;
            System.Windows.Forms.Label endXLabel;
            System.Windows.Forms.Label endMapBankLabel;
            System.Windows.Forms.Label endMapIndexLabel;
            System.Windows.Forms.Label startMapIndexLabel;
            System.Windows.Forms.Label startMapBankLabel;
            this.start = new System.Windows.Forms.GroupBox();
            this.useCharacterAsStartPosition = new System.Windows.Forms.CheckBox();
            this.startMapIndex = new System.Windows.Forms.ComboBox();
            this.startDirection = new System.Windows.Forms.ListBox();
            this.startMapBank = new System.Windows.Forms.ComboBox();
            this.startY = new System.Windows.Forms.NumericUpDown();
            this.startX = new System.Windows.Forms.NumericUpDown();
            this.endMapBank = new System.Windows.Forms.ComboBox();
            this.goal = new System.Windows.Forms.GroupBox();
            this.endMapIndex = new System.Windows.Forms.ComboBox();
            this.endDirection = new System.Windows.Forms.ListBox();
            this.endY = new System.Windows.Forms.NumericUpDown();
            this.endX = new System.Windows.Forms.NumericUpDown();
            this.travel = new System.Windows.Forms.Button();
            this.computeMap = new System.Windows.Forms.Button();
            startXLabel = new System.Windows.Forms.Label();
            startYLabel = new System.Windows.Forms.Label();
            endYLabel = new System.Windows.Forms.Label();
            endXLabel = new System.Windows.Forms.Label();
            endMapBankLabel = new System.Windows.Forms.Label();
            endMapIndexLabel = new System.Windows.Forms.Label();
            startMapIndexLabel = new System.Windows.Forms.Label();
            startMapBankLabel = new System.Windows.Forms.Label();
            this.start.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.startY)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.startX)).BeginInit();
            this.goal.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.endY)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.endX)).BeginInit();
            this.SuspendLayout();
            // 
            // startXLabel
            // 
            startXLabel.Location = new System.Drawing.Point(161, 31);
            startXLabel.Name = "startXLabel";
            startXLabel.Size = new System.Drawing.Size(15, 16);
            startXLabel.TabIndex = 1;
            startXLabel.Text = "x:";
            // 
            // startYLabel
            // 
            startYLabel.Location = new System.Drawing.Point(161, 53);
            startYLabel.Name = "startYLabel";
            startYLabel.Size = new System.Drawing.Size(15, 16);
            startYLabel.TabIndex = 3;
            startYLabel.Text = "y:";
            // 
            // endYLabel
            // 
            endYLabel.Location = new System.Drawing.Point(161, 55);
            endYLabel.Name = "endYLabel";
            endYLabel.Size = new System.Drawing.Size(15, 16);
            endYLabel.TabIndex = 3;
            endYLabel.Text = "y:";
            // 
            // endXLabel
            // 
            endXLabel.Location = new System.Drawing.Point(161, 31);
            endXLabel.Name = "endXLabel";
            endXLabel.Size = new System.Drawing.Size(15, 16);
            endXLabel.TabIndex = 1;
            endXLabel.Text = "x:";
            // 
            // endMapBankLabel
            // 
            endMapBankLabel.Location = new System.Drawing.Point(6, 33);
            endMapBankLabel.Name = "endMapBankLabel";
            endMapBankLabel.Size = new System.Drawing.Size(37, 16);
            endMapBankLabel.TabIndex = 7;
            endMapBankLabel.Text = "bank :";
            endMapBankLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // endMapIndexLabel
            // 
            endMapIndexLabel.Location = new System.Drawing.Point(6, 55);
            endMapIndexLabel.Name = "endMapIndexLabel";
            endMapIndexLabel.Size = new System.Drawing.Size(37, 16);
            endMapIndexLabel.TabIndex = 9;
            endMapIndexLabel.Text = "n° :";
            endMapIndexLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // startMapIndexLabel
            // 
            startMapIndexLabel.Location = new System.Drawing.Point(6, 44);
            startMapIndexLabel.Name = "startMapIndexLabel";
            startMapIndexLabel.Size = new System.Drawing.Size(37, 16);
            startMapIndexLabel.TabIndex = 13;
            startMapIndexLabel.Text = "n° :";
            startMapIndexLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // startMapBankLabel
            // 
            startMapBankLabel.Location = new System.Drawing.Point(6, 22);
            startMapBankLabel.Name = "startMapBankLabel";
            startMapBankLabel.Size = new System.Drawing.Size(37, 16);
            startMapBankLabel.TabIndex = 11;
            startMapBankLabel.Text = "bank :";
            startMapBankLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // start
            // 
            this.start.Controls.Add(startMapIndexLabel);
            this.start.Controls.Add(this.useCharacterAsStartPosition);
            this.start.Controls.Add(this.startMapIndex);
            this.start.Controls.Add(this.startDirection);
            this.start.Controls.Add(startMapBankLabel);
            this.start.Controls.Add(startYLabel);
            this.start.Controls.Add(this.startMapBank);
            this.start.Controls.Add(this.startY);
            this.start.Controls.Add(startXLabel);
            this.start.Controls.Add(this.startX);
            this.start.Location = new System.Drawing.Point(93, 12);
            this.start.Name = "start";
            this.start.Size = new System.Drawing.Size(363, 81);
            this.start.TabIndex = 1;
            this.start.TabStop = false;
            this.start.Text = "start";
            // 
            // useCharacterAsStartPosition
            // 
            this.useCharacterAsStartPosition.Checked = true;
            this.useCharacterAsStartPosition.CheckState = System.Windows.Forms.CheckState.Checked;
            this.useCharacterAsStartPosition.Location = new System.Drawing.Point(6, 61);
            this.useCharacterAsStartPosition.Name = "useCharacterAsStartPosition";
            this.useCharacterAsStartPosition.Size = new System.Drawing.Size(149, 18);
            this.useCharacterAsStartPosition.TabIndex = 5;
            this.useCharacterAsStartPosition.Text = "use character position";
            this.useCharacterAsStartPosition.UseVisualStyleBackColor = true;
            this.useCharacterAsStartPosition.CheckedChanged += new System.EventHandler(this.useCharacterAsStartPosition_CheckedChanged);
            // 
            // startMapIndex
            // 
            this.startMapIndex.Enabled = false;
            this.startMapIndex.FormattingEnabled = true;
            this.startMapIndex.Location = new System.Drawing.Point(43, 39);
            this.startMapIndex.Name = "startMapIndex";
            this.startMapIndex.Size = new System.Drawing.Size(112, 21);
            this.startMapIndex.TabIndex = 12;
            // 
            // startDirection
            // 
            this.startDirection.Enabled = false;
            this.startDirection.FormattingEnabled = true;
            this.startDirection.Items.AddRange(new object[] { "Down", "Up", "Left", "Right" });
            this.startDirection.Location = new System.Drawing.Point(237, 17);
            this.startDirection.Name = "startDirection";
            this.startDirection.Size = new System.Drawing.Size(120, 56);
            this.startDirection.TabIndex = 5;
            // 
            // startMapBank
            // 
            this.startMapBank.Enabled = false;
            this.startMapBank.FormattingEnabled = true;
            this.startMapBank.Location = new System.Drawing.Point(43, 17);
            this.startMapBank.MaxDropDownItems = 30;
            this.startMapBank.Name = "startMapBank";
            this.startMapBank.Size = new System.Drawing.Size(112, 21);
            this.startMapBank.TabIndex = 10;
            this.startMapBank.SelectedIndexChanged += new System.EventHandler((sender, args) => InitMapIndexField(startMapBank, startMapIndex));
            // 
            // startY
            // 
            this.startY.Enabled = false;
            this.startY.Location = new System.Drawing.Point(182, 53);
            this.startY.Name = "startY";
            this.startY.Size = new System.Drawing.Size(39, 20);
            this.startY.TabIndex = 2;
            this.startY.Value = new decimal(new int[] { 5, 0, 0, 0 });
            // 
            // startX
            // 
            this.startX.Enabled = false;
            this.startX.Location = new System.Drawing.Point(182, 27);
            this.startX.Name = "startX";
            this.startX.Size = new System.Drawing.Size(39, 20);
            this.startX.TabIndex = 0;
            this.startX.Value = new decimal(new int[] { 5, 0, 0, 0 });
            // 
            // endMapBank
            // 
            this.endMapBank.FormattingEnabled = true;
            this.endMapBank.Location = new System.Drawing.Point(43, 28);
            this.endMapBank.MaxDropDownItems = 30;
            this.endMapBank.Name = "endMapBank";
            this.endMapBank.Size = new System.Drawing.Size(112, 21);
            this.endMapBank.TabIndex = 6;
            this.endMapBank.SelectedIndexChanged += new System.EventHandler((sender, args) => InitMapIndexField(endMapBank, endMapIndex));

            // 
            // goal
            // 
            this.goal.Controls.Add(endMapIndexLabel);
            this.goal.Controls.Add(this.endMapIndex);
            this.goal.Controls.Add(endMapBankLabel);
            this.goal.Controls.Add(this.endMapBank);
            this.goal.Controls.Add(this.endDirection);
            this.goal.Controls.Add(endYLabel);
            this.goal.Controls.Add(this.endY);
            this.goal.Controls.Add(endXLabel);
            this.goal.Controls.Add(this.endX);
            this.goal.Location = new System.Drawing.Point(93, 99);
            this.goal.Name = "goal";
            this.goal.Size = new System.Drawing.Size(363, 80);
            this.goal.TabIndex = 4;
            this.goal.TabStop = false;
            this.goal.Text = "goal";
            // 
            // endMapIndex
            // 
            this.endMapIndex.FormattingEnabled = true;
            this.endMapIndex.Location = new System.Drawing.Point(43, 50);
            this.endMapIndex.Name = "endMapIndex";
            this.endMapIndex.Size = new System.Drawing.Size(112, 21);
            this.endMapIndex.TabIndex = 8;
            // 
            // endDirection
            // 
            this.endDirection.FormattingEnabled = true;
            this.endDirection.Items.AddRange(new object[] { "Down", "Up", "Left", "Right" });
            this.endDirection.Location = new System.Drawing.Point(237, 17);
            this.endDirection.Name = "endDirection";
            this.endDirection.Size = new System.Drawing.Size(120, 56);
            this.endDirection.TabIndex = 6;
            // 
            // endY
            // 
            this.endY.Location = new System.Drawing.Point(182, 53);
            this.endY.Name = "endY";
            this.endY.Size = new System.Drawing.Size(39, 20);
            this.endY.TabIndex = 2;
            this.endY.Value = new decimal(new int[] { 9, 0, 0, 0 });
            // 
            // endX
            // 
            this.endX.Location = new System.Drawing.Point(182, 27);
            this.endX.Name = "endX";
            this.endX.Size = new System.Drawing.Size(39, 20);
            this.endX.TabIndex = 0;
            this.endX.Value = new decimal(new int[] { 6, 0, 0, 0 });
            // 
            // travel
            // 
            this.travel.Location = new System.Drawing.Point(12, 53);
            this.travel.Name = "travel";
            this.travel.Size = new System.Drawing.Size(75, 38);
            this.travel.TabIndex = 0;
            this.travel.Text = "Travel (Debug)";
            this.travel.UseVisualStyleBackColor = true;
            this.travel.Click += new System.EventHandler(this.ComputeClick);
            // 
            // computeMap
            // 
            this.computeMap.Location = new System.Drawing.Point(12, 12);
            this.computeMap.Name = "computeMap";
            this.computeMap.Size = new System.Drawing.Size(75, 35);
            this.computeMap.TabIndex = 5;
            this.computeMap.Text = "Compute Maps";
            this.computeMap.UseVisualStyleBackColor = true;
            this.computeMap.Click += new System.EventHandler(this.ForceComputeMaps);
            // 
            // PokemonSolverForm
            // 
            this.ClientSize = new System.Drawing.Size(565, 346);
            this.Controls.Add(this.computeMap);
            this.Controls.Add(this.goal);
            this.Controls.Add(this.start);
            this.Controls.Add(this.travel);
            this.Name = "PokemonSolverForm";
            this.start.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.startY)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.startX)).EndInit();
            this.goal.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.endY)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.endX)).EndInit();
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.ComboBox startMapIndex;
        private System.Windows.Forms.ComboBox startMapBank;
        private System.Windows.Forms.ComboBox endMapBank;

        private System.Windows.Forms.ComboBox endMapIndex;

        private void ForceComputeMaps(object sender, EventArgs e)
        {
            SetVerbose(true);
            OverworldEngine.ComputeMaps(true);
            SetVerbose(false);
        }

        private System.Windows.Forms.Button computeMap;

        private System.Windows.Forms.GroupBox start;

        private System.Windows.Forms.GroupBox goal;

        private System.Windows.Forms.CheckBox useCharacterAsStartPosition;

        private System.Windows.Forms.ListBox startDirection;

        private System.Windows.Forms.ListBox endDirection;

        private System.Windows.Forms.NumericUpDown startX;
        private System.Windows.Forms.NumericUpDown startY;
        private System.Windows.Forms.NumericUpDown endY;
        private System.Windows.Forms.NumericUpDown endX;

        private System.Windows.Forms.Button travel;

        private void useCharacterAsStartPosition_CheckedChanged(object sender, EventArgs e)
        {
            var check = (CheckBox)sender;
            foreach (var c in new Control[] { startMapBank, startMapIndex, startX, startY, startDirection })
            {
                c.Enabled = !check.Checked;
            }
        }

        private static Direction GetDirectionFromSelectIndex(int index)
        {
            return (Direction)(index + 1);
        }

        private static int GetSelectIndexFromDirection(Direction dir)
        {
            return (int)dir - 1;
        }
    }
}