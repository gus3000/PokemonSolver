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
using PokemonSolver.Algoritm;
using PokemonSolver.Image;
using PokemonSolver.Image.Colors;
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
            // Controls.Add(labelInfo = new Label { AutoSize = true, MaximumSize = new Size(480, 0) });
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
            Position.SetOverworldEngine(OverworldEngine);
            
            APIs.Gui.WithSurface(DisplaySurfaceID.EmuCore, delegate { Utils.Log("Euuh bonjour"); });
            APIs.Gui.SetDefaultForegroundColor(Color.Coral);
            APIs.Gui.SetDefaultBackgroundColor(Color.Black);
            OverworldEngine.ComputeMaps();
            InitFields();
            CurrentMap = null;
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
            UpdateFieldsFromCharPosition();
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

            mapIndex.Items.AddRange(maps.Select(map => $"{map.Name} ({map.Bank},{map.MapIndex})").ToArray());
            mapBank.SelectedIndex = 0;
            mapIndex.SelectedIndex = 0;
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

            startMapBank.SelectedIndex = characterPosition.MapBank;
            startMapIndex.SelectedIndex = characterPosition.MapIndex;

            if (characterPosition.X < 0 || characterPosition.Y < 0)
            {
                Utils.Error($"wtf : charposition = {characterPosition}");
                return;
            }
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

        }

        private void ResetMap()
        {
            Utils.Log("Reset Map");
            // customColoredPositions = new List<KeyValuePair<Position, Color>>();

            MapView.Image = getBitmapFromMap(CurrentMap);
            MapView.SizeMode = PictureBoxSizeMode.Normal;
            MapView.Left = 0;
            MapView.Top = goal.Bottom;
            MapView.Size = MapView.GetPreferredSize(ClientSize);

            ClientSize = new Size(ClientSize.Width, MapView.Bottom + 5);
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
            Utils.Log("Debugging :");
            // ComputeClick();
            var map = OverworldEngine.GetCurrentMap();
            Utils.Log($"{map.Debug()}");
            int x = 0, y = -1;
            for (x = 0; x < map.MapData.Width; x++)
                Utils.Log($"map in ({x},{y}) : {map.GetNextMap(OverworldEngine, x, y)?.Debug()}");
        }

        private void Compute()
        {
            // InitFields();
            // return;

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

            Utils.Log($"Going from ({startPosition}) to ({endPosition})",true);


            var astar = new AStar(OverworldEngine);
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var result = astar.Resolve(startPosition, endPosition);
            stopwatch.Stop();
            Utils.Log($"({stopwatch.ElapsedMilliseconds} ms) Result (length {result?.Depth()}):\n {result?.Debug()}", true);
            customColoredPositions = new List<KeyValuePair<Position, Color>>();
            result?.Ancestors(true).ForEach(node => { customColoredPositions.Add(new KeyValuePair<Position, Color>(node.State, CustomColors.Path)); });
            ResetMap();
        }

        private Position GetStartPosition()
        {
            return new Position(
                startMapBank.SelectedIndex,
                startMapIndex.SelectedIndex,
                (int)startX.Value,
                (int)startY.Value,
                GetDirectionFromSelectIndex(startDirection.SelectedIndex),
                Altitude.Any
            ); //TODO set altitude
        }

        private Position GetEndPosition()
        {
            return new Position(
                endMapBank.SelectedIndex,
                endMapIndex.SelectedIndex,
                (int)endX.Value,
                (int)endY.Value,
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
            const int margin = 5;
            Label startXLabel;
            Label startYLabel;
            Label endYLabel;
            Label endXLabel;
            Label endMapBankLabel;
            Label endMapIndexLabel;
            Label startMapIndexLabel;
            Label startMapBankLabel;
            var miscButtons = new GroupBox();
            start = new GroupBox();
            goal = new GroupBox();
            useCharacterAsStartPosition = new CheckBox();
            startMapIndex = new ComboBox();
            startDirection = new ListBox();
            startMapBank = new ComboBox();
            startY = new NumericUpDown();
            startX = new NumericUpDown();
            endMapBank = new ComboBox();
            endMapIndex = new ComboBox();
            endDirection = new ListBox();
            endY = new NumericUpDown();
            endX = new NumericUpDown();
            debugButton = new Button();
            computeMap = new Button();
            startXLabel = new Label();
            startYLabel = new Label();
            endYLabel = new Label();
            endXLabel = new Label();
            endMapBankLabel = new Label();
            endMapIndexLabel = new Label();
            startMapIndexLabel = new Label();
            startMapBankLabel = new Label();
            start.SuspendLayout();
            ((ISupportInitialize)(startY)).BeginInit();
            ((ISupportInitialize)(startX)).BeginInit();
            goal.SuspendLayout();
            ((ISupportInitialize)(endY)).BeginInit();
            ((ISupportInitialize)(endX)).BeginInit();
            SuspendLayout();

            //
            // miscButtons
            //
            miscButtons.Controls.Add(debugButton);
            miscButtons.Controls.Add(computeMap);
            miscButtons.Name = "miscButtons";
            miscButtons.Size = new Size(500, 50);
            miscButtons.TabIndex = 1;
            miscButtons.TabStop = false;
            miscButtons.Text = "misc";
            miscButtons.Dock = DockStyle.Top;
            miscButtons.Anchor = (AnchorStyles.Top | AnchorStyles.Left);


            // 
            // start
            // 
            start.Controls.Add(startMapIndexLabel);
            start.Controls.Add(useCharacterAsStartPosition);
            start.Controls.Add(startMapIndex);
            start.Controls.Add(startDirection);
            start.Controls.Add(startMapBankLabel);
            start.Controls.Add(startYLabel);
            start.Controls.Add(startMapBank);
            start.Controls.Add(startY);
            start.Controls.Add(startXLabel);
            start.Controls.Add(startX);
            start.Name = "start";
            start.Size = new Size(500, 100);
            start.TabIndex = miscButtons.TabIndex + 1;
            start.TabStop = false;
            start.Text = "start";
            start.Dock = miscButtons.Dock;
            start.Anchor = miscButtons.Anchor;

            // 
            // goal
            // 
            goal.Controls.Add(endMapIndexLabel);
            goal.Controls.Add(endMapIndex);
            goal.Controls.Add(endMapBankLabel);
            goal.Controls.Add(endMapBank);
            goal.Controls.Add(endDirection);
            goal.Controls.Add(endYLabel);
            goal.Controls.Add(endY);
            goal.Controls.Add(endXLabel);
            goal.Controls.Add(endX);
            goal.Name = "goal";
            goal.Size = start.Size;
            goal.TabIndex = start.TabIndex + 1;
            goal.TabStop = false;
            goal.Text = "goal";
            goal.Dock = miscButtons.Dock;
            goal.Anchor = miscButtons.Anchor;

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
            computeMap.Location = new Point(12 + debugButton.Bounds.Right, 15);
            computeMap.Name = "computeMap";
            // computeMap.Size = new Size(75, 35);
            computeMap.TabIndex = 1;
            computeMap.Text = "Compute Maps";
            computeMap.UseVisualStyleBackColor = true;
            computeMap.Click += (_, _) => ComputeClick();

            // 
            // startMapBankLabel
            // 
            startMapBankLabel.Location = new Point(6, 22);
            startMapBankLabel.Name = "startMapBankLabel";
            startMapBankLabel.Size = new Size(37, 16);
            startMapBankLabel.TabIndex = 11;
            startMapBankLabel.Text = "bank :";
            startMapBankLabel.TextAlign = ContentAlignment.MiddleRight;

            // 
            // startMapBank
            // 
            startMapBank.Enabled = false;
            startMapBank.FormattingEnabled = true;
            startMapBank.Location = new Point(startMapBankLabel.Bounds.Right + margin, startMapBankLabel.Location.Y);
            startMapBank.MaxDropDownItems = 30;
            startMapBank.Name = "startMapBank";
            startMapBank.Size = new Size(180, 21);
            startMapBank.TabIndex = 10;
            startMapBank.SelectedIndexChanged += (sender, args) => InitMapIndexField(startMapBank, startMapIndex);

            // 
            // startMapIndexLabel
            // 
            startMapIndexLabel.Location = new Point(6, 44);
            startMapIndexLabel.Name = "startMapIndexLabel";
            startMapIndexLabel.Size = new Size(37, 16);
            startMapIndexLabel.TabIndex = 13;
            startMapIndexLabel.Text = "n° :";
            startMapIndexLabel.TextAlign = startMapBankLabel.TextAlign;

            // 
            // startMapIndex
            // 
            startMapIndex.Enabled = false;
            startMapIndex.FormattingEnabled = true;
            startMapIndex.Location = new Point(startMapIndexLabel.Bounds.Right + margin, startMapIndexLabel.Location.Y);
            startMapIndex.Name = "startMapIndex";
            startMapIndex.Size = new Size(180, 21);
            startMapIndex.TabIndex = 12;

            // 
            // startXLabel
            // 
            startXLabel.Location = new Point(startMapBank.Bounds.Right + margin, startMapBankLabel.Location.Y);
            // startXLabel.Location = new Point(0,0);
            startXLabel.Name = "startXLabel";
            startXLabel.Size = new Size(15, 16);
            startXLabel.TabIndex = 1;
            startXLabel.Text = "x:";
            startXLabel.TextAlign = ContentAlignment.MiddleRight;
            
            // 
            // startX
            // 
            startX.Enabled = false;
            startX.Location = new Point(startXLabel.Bounds.Right + margin, startXLabel.Location.Y);
            startX.Name = "startX";
            startX.Size = new Size(39, 20);
            startX.TabIndex = 0;
            startX.Value = 5;
            startX.Minimum = 0;
            startX.Maximum = 1000;
            
            // 
            // startYLabel
            // 
            startYLabel.Location = new Point(startXLabel.Location.X, startXLabel.Bounds.Bottom + margin);
            startYLabel.Name = "startYLabel";
            startYLabel.Size = new Size(15, 16);
            startYLabel.TabIndex = 3;
            startYLabel.Text = "y:";
            startYLabel.TextAlign = startXLabel.TextAlign;
            
            // 
            // startY
            // 
            startY.Enabled = false;
            startY.Location = new Point(startYLabel.Bounds.Right + margin, startYLabel.Location.Y);
            startY.Name = "startY";
            startY.Size = new Size(39, 20);
            startY.TabIndex = 2;
            startY.Value = 5;
            startY.Minimum = 0;
            startY.Maximum = 1000;
            
            // 
            // startDirection
            // 
            startDirection.Enabled = false;
            startDirection.FormattingEnabled = true;
            startDirection.Items.AddRange(new object[] { "Down", "Up", "Left", "Right" });
            startDirection.Location = new Point(startX.Bounds.Right + margin, 17);
            startDirection.Name = "startDirection";
            startDirection.Size = new Size(120, 56);
            startDirection.TabIndex = 5;
            
            // 
            // useCharacterAsStartPosition
            // 
            useCharacterAsStartPosition.Checked = true;
            useCharacterAsStartPosition.CheckState = CheckState.Checked;
            useCharacterAsStartPosition.Location = new Point(6, start.Size.Height - 20);
            useCharacterAsStartPosition.Name = "useCharacterAsStartPosition";
            useCharacterAsStartPosition.Size = new Size(149, 18);
            useCharacterAsStartPosition.TabIndex = 5;
            useCharacterAsStartPosition.Text = "use character position";
            useCharacterAsStartPosition.UseVisualStyleBackColor = true;
            useCharacterAsStartPosition.CheckedChanged += useCharacterAsStartPosition_CheckedChanged;
            
            // 
            // endMapBankLabel
            // 
            endMapBankLabel.Location = startMapBankLabel.Location;
            endMapBankLabel.Name = "endMapBankLabel";
            endMapBankLabel.Size = startMapBankLabel.Size;
            endMapBankLabel.TabIndex = 7;
            endMapBankLabel.Text = "bank :";
            endMapBankLabel.TextAlign = startMapBankLabel.TextAlign;

            // 
            // endMapBank
            // 
            endMapBank.FormattingEnabled = true;
            endMapBank.Location = startMapBank.Location;
            endMapBank.MaxDropDownItems = 30;
            endMapBank.Name = "endMapBank";
            endMapBank.Size = startMapBank.Size;
            endMapBank.TabIndex = 6;
            endMapBank.SelectedIndexChanged += (sender, args) => InitMapIndexField(endMapBank, endMapIndex);

            // 
            // endMapIndexLabel
            // 
            endMapIndexLabel.Location = startMapIndexLabel.Location;
            endMapIndexLabel.Name = "endMapIndexLabel";
            endMapIndexLabel.Size = startMapIndexLabel.Size;
            endMapIndexLabel.TabIndex = 9;
            endMapIndexLabel.Text = "n° :";
            endMapIndexLabel.TextAlign = startMapBankLabel.TextAlign;


            // 
            // endMapIndex
            // 
            endMapIndex.FormattingEnabled = true;
            endMapIndex.Location = startMapIndex.Location;
            endMapIndex.Name = "endMapIndex";
            endMapIndex.Size = startMapIndex.Size;
            endMapIndex.TabIndex = 8;

            // 
            // endXLabel
            // 
            endXLabel.Location = startXLabel.Location;
            endXLabel.Name = "endXLabel";
            endXLabel.Size = startXLabel.Size;
            endXLabel.TabIndex = 1;
            endXLabel.Text = "x:";
            
            // 
            // endX
            // 
            endX.Location = startX.Location;
            endX.Name = "endX";
            endX.Size = startX.Size;
            endX.TabIndex = 0;
            endX.Value = 6;
            endX.Minimum = 0;
            endX.Maximum = 1000;
            
            // 
            // endYLabel
            // 
            endYLabel.Location = startYLabel.Location;
            endYLabel.Name = "endYLabel";
            endYLabel.Size = startYLabel.Size;
            endYLabel.TabIndex = 3;
            endYLabel.Text = "y:";
            
            // 
            // endY
            // 
            endY.Location = startY.Location;
            endY.Name = "endY";
            endY.Size = startY.Size;
            endY.TabIndex = 2;
            endY.Value = 9;
            endY.Minimum = 0;
            endY.Maximum = 1000;
            
            
            // 
            // endDirection
            // 
            endDirection.FormattingEnabled = true;
            endDirection.Items.AddRange(new object[] { "Down", "Up", "Left", "Right" });
            endDirection.Location = startDirection.Location;
            endDirection.Name = "endDirection";
            endDirection.Size = startDirection.Size;
            endDirection.TabIndex = 6;

            // 
            // PokemonSolverForm
            // 
            ClientSize = new Size(600, 480);
            Controls.Add(goal);
            Controls.Add(start);
            Controls.Add(miscButtons);
            Name = "PokemonSolverForm";
            start.ResumeLayout(false);
            ((ISupportInitialize)(startY)).EndInit();
            ((ISupportInitialize)(startX)).EndInit();
            goal.ResumeLayout(false);
            ((ISupportInitialize)(endY)).EndInit();
            ((ISupportInitialize)(endX)).EndInit();
            ResumeLayout(false);
        }

        private ComboBox startMapIndex;
        private ComboBox startMapBank;
        private ComboBox endMapBank;

        private ComboBox endMapIndex;

        private void ForceComputeMaps(object sender, EventArgs e)
        {
            SetVerbose(true);
            OverworldEngine.ComputeMaps(true);
            SetVerbose(false);
        }

        private Button computeMap;

        private GroupBox start;

        private GroupBox goal;

        private CheckBox useCharacterAsStartPosition;

        private ListBox startDirection;

        private ListBox endDirection;

        private NumericUpDown startX;
        private NumericUpDown startY;
        private NumericUpDown endY;
        private NumericUpDown endX;

        private Button debugButton;

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