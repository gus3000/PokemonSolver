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
        private PictureBox MapView;
        private Button ComputeButton;

        // private System.Windows.Controls currentMap;
        private StringBuilder currentStatus;
        private List<KeyValuePair<Position,Color>> customColoredPositions;

        public PokemonSolverForm()
        {
            currentStatus = new StringBuilder();
            InitializeComponent();
            startDirection.SelectedIndex = (int)Direction.Down;
            endDirection.SelectedIndex = (int)Direction.Down;
            SuspendLayout();
            Controls.Add(labelInfo = new Label { AutoSize = true, MaximumSize = new Size(480, 0) });
            Controls.Add(MapView = new PictureBox());
            Controls.Add(ComputeButton = new Button
                { AutoSize = true, MaximumSize = new Size(480, 0), Text = "compute" });
            // Controls.Add(MapView = new DataGridView());
            // Controls.Add(_dateTimePicker = new DateTimePicker());
            // Controls.Add(currentMap);
            ResumeLayout();

            Log.EnableDomain("NetworkDebug");
            Log.EnableDomain("Debug");

            MapView.Click += (sender, args) => OnClick(args);
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
            // labelInfo.Text = "C'ay la mayrde";
            // APIs.Gui.ClearGraphics(DisplaySurfaceID.EmuCore);
            // MapView.Location = new Point(0, 0);
            // MapView.Size = new Size(400, 400);
            // MapView.Name = "Map View";
            // MapView.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            // MapView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            // MapView.CellBorderStyle = DataGridViewCellBorderStyle.Single;
            // MapView.GridColor = Color.Black;
            // MapView.ColumnHeadersVisible = false;
            // MapView.RowHeadersVisible = false;
        }

        protected override void UpdateBefore()
        {
            //TODO dequeue input from engine
        }

        protected override void UpdateAfter()
        {
            currentStatus.Clear();
            UpdateData();
            // labelInfo.Text = currentStatus.ToString();
            // APIs.Gui.Text(0,0,"BLAAAAAAAAAAAAAAAAAAAAAAAAAA");
            // APIs.Gui.Text(0,20,"BLAAAAAAAAAAAAAAAAAAAAAAAAAA");
            // APIs.Gui.DrawText(0,20,"BLAAAAAAAAAAAAAAAAAAAAAAAAAA", surfaceID: DisplaySurfaceID.Client);
            // APIs.Gui.DrawText(0,20,"BLAAAAAAAAAAAAAAAAAAAAAAAAAA", surfaceID: DisplaySurfaceID.EmuCore);
            // APIs.Gui.DrawRectangle(50,50,1000,1000, null,Color.Black,DisplaySurfaceID.Client);
            // APIs.Gui.DrawRectangle(50,50,1000,1000, null,Color.Black,DisplaySurfaceID.EmuCore);
            // APIs.Gui.DrawRectangle(50,50,1000,1000, null,Color.Black);
        }


        protected override async void OnClosing(CancelEventArgs e)
        {
        }


        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);

            SetVerbose(true);
            Utils.Log("Click");

            OverworldEngine.ComputeMaps();
            UpdateMap();
            SetVerbose(false);
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
            // if(OverworldEngine.)
        }

        private void UpdateMap()
        {
            if (OverworldEngine == null)
                return;
            MapView.Image = getBitmapFromMap(OverworldEngine.getCurrentMap());
            MapView.SizeMode = PictureBoxSizeMode.Zoom;
            MapView.Size = ClientSize;
            
        }

        private string ReadData()
        {
            return GameData.Serialize();
        }

        private void ComputeClick(object sender, EventArgs e)
        {
            if (OverworldEngine == null)
            {
                Utils.Error("wtf overworldEngine is null");
                return;
            }

            var map = OverworldEngine.getCurrentMap();
            if (map == null)
            {
                Utils.Error("map is null");
                return;
            }
            
            SetVerbose(true);
            Position characterPosition = OverworldEngine.getCurrentPosition();
            Utils.Log($"char position : {characterPosition}");
            var fieldPosition = new Position((uint)startX.Value, (uint)startY.Value, (Direction)startDirection.SelectedIndex);

            var startPosition = useCharacterAsStartPosition.Checked ? characterPosition : fieldPosition;
            var endPosition = new Position((uint)endX.Value, (uint)endY.Value, (Direction)endDirection.SelectedIndex);
            Utils.Log($"Going from ({startPosition}) to ({endPosition})");
            

            var astar = new AStar(map.MapData);
            var result = astar.resolve(startPosition, endPosition);
            Utils.Log($"Result : {result?.Debug()}");
            customColoredPositions = new List<KeyValuePair<Position,Color>>();
            result?.Ancestors().ForEach(node =>
            {
                customColoredPositions.Add( new KeyValuePair<Position, Color>(node.State,CustomColors.Path));
            });

            SetVerbose(false);
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
            System.Windows.Forms.GroupBox start;
            System.Windows.Forms.GroupBox goal;
            this.useCharacterAsStartPosition = new System.Windows.Forms.CheckBox();
            this.startDirection = new System.Windows.Forms.ListBox();
            this.startY = new System.Windows.Forms.NumericUpDown();
            this.startX = new System.Windows.Forms.NumericUpDown();
            this.endDirection = new System.Windows.Forms.ListBox();
            this.endY = new System.Windows.Forms.NumericUpDown();
            this.endX = new System.Windows.Forms.NumericUpDown();
            this.compute = new System.Windows.Forms.Button();
            startXLabel = new System.Windows.Forms.Label();
            startYLabel = new System.Windows.Forms.Label();
            endYLabel = new System.Windows.Forms.Label();
            endXLabel = new System.Windows.Forms.Label();
            start = new System.Windows.Forms.GroupBox();
            goal = new System.Windows.Forms.GroupBox();
            start.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.startY)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.startX)).BeginInit();
            goal.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.endY)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.endX)).BeginInit();
            this.SuspendLayout();
            // 
            // startXLabel
            // 
            startXLabel.Location = new System.Drawing.Point(6, 21);
            startXLabel.Name = "startXLabel";
            startXLabel.Size = new System.Drawing.Size(15, 16);
            startXLabel.TabIndex = 1;
            startXLabel.Text = "x:";
            // 
            // startYLabel
            // 
            startYLabel.Location = new System.Drawing.Point(72, 19);
            startYLabel.Name = "startYLabel";
            startYLabel.Size = new System.Drawing.Size(15, 16);
            startYLabel.TabIndex = 3;
            startYLabel.Text = "y:";
            // 
            // endYLabel
            // 
            endYLabel.Location = new System.Drawing.Point(72, 19);
            endYLabel.Name = "endYLabel";
            endYLabel.Size = new System.Drawing.Size(15, 16);
            endYLabel.TabIndex = 3;
            endYLabel.Text = "y:";
            // 
            // endXLabel
            // 
            endXLabel.Location = new System.Drawing.Point(6, 21);
            endXLabel.Name = "endXLabel";
            endXLabel.Size = new System.Drawing.Size(15, 16);
            endXLabel.TabIndex = 1;
            endXLabel.Text = "x:";
            // 
            // start
            // 
            start.Controls.Add(this.useCharacterAsStartPosition);
            start.Controls.Add(this.startDirection);
            start.Controls.Add(startYLabel);
            start.Controls.Add(this.startY);
            start.Controls.Add(startXLabel);
            start.Controls.Add(this.startX);
            start.Location = new System.Drawing.Point(93, 12);
            start.Name = "start";
            start.Size = new System.Drawing.Size(277, 81);
            start.TabIndex = 1;
            start.TabStop = false;
            start.Text = "start";
            // 
            // useCharacterAsStartPosition
            // 
            this.useCharacterAsStartPosition.Checked = true;
            this.useCharacterAsStartPosition.CheckState = System.Windows.Forms.CheckState.Checked;
            this.useCharacterAsStartPosition.Location = new System.Drawing.Point(28, 43);
            this.useCharacterAsStartPosition.Name = "useCharacterAsStartPosition";
            this.useCharacterAsStartPosition.Size = new System.Drawing.Size(104, 36);
            this.useCharacterAsStartPosition.TabIndex = 5;
            this.useCharacterAsStartPosition.Text = "use character position";
            this.useCharacterAsStartPosition.UseVisualStyleBackColor = true;
            this.useCharacterAsStartPosition.CheckedChanged += new System.EventHandler(this.useCharacterAsStartPosition_CheckedChanged);
            // 
            // startDirection
            // 
            this.startDirection.Enabled = false;
            this.startDirection.FormattingEnabled = true;
            this.startDirection.Items.AddRange(new object[] { "Down", "Up", "Left", "Right" });
            this.startDirection.Location = new System.Drawing.Point(151, 17);
            this.startDirection.Name = "startDirection";
            this.startDirection.Size = new System.Drawing.Size(120, 56);
            this.startDirection.TabIndex = 5;
            // 
            // startY
            // 
            this.startY.Enabled = false;
            this.startY.Location = new System.Drawing.Point(93, 17);
            this.startY.Name = "startY";
            this.startY.Size = new System.Drawing.Size(39, 20);
            this.startY.TabIndex = 2;
            this.startY.Value = new decimal(new int[] { 5, 0, 0, 0 });
            // 
            // startX
            // 
            this.startX.Enabled = false;
            this.startX.Location = new System.Drawing.Point(27, 17);
            this.startX.Name = "startX";
            this.startX.Size = new System.Drawing.Size(39, 20);
            this.startX.TabIndex = 0;
            this.startX.Value = new decimal(new int[] { 5, 0, 0, 0 });
            // 
            // goal
            // 
            goal.Controls.Add(this.endDirection);
            goal.Controls.Add(endYLabel);
            goal.Controls.Add(this.endY);
            goal.Controls.Add(endXLabel);
            goal.Controls.Add(this.endX);
            goal.Location = new System.Drawing.Point(93, 99);
            goal.Name = "goal";
            goal.Size = new System.Drawing.Size(277, 80);
            goal.TabIndex = 4;
            goal.TabStop = false;
            goal.Text = "goal";
            // 
            // endDirection
            // 
            this.endDirection.FormattingEnabled = true;
            this.endDirection.Items.AddRange(new object[] { "Down", "Up", "Left", "Right" });
            this.endDirection.Location = new System.Drawing.Point(151, 17);
            this.endDirection.Name = "endDirection";
            this.endDirection.Size = new System.Drawing.Size(120, 56);
            this.endDirection.TabIndex = 6;
            // 
            // endY
            // 
            this.endY.Location = new System.Drawing.Point(93, 17);
            this.endY.Name = "endY";
            this.endY.Size = new System.Drawing.Size(39, 20);
            this.endY.TabIndex = 2;
            this.endY.Value = new decimal(new int[] { 9, 0, 0, 0 });
            // 
            // endX
            // 
            this.endX.Location = new System.Drawing.Point(27, 17);
            this.endX.Name = "endX";
            this.endX.Size = new System.Drawing.Size(39, 20);
            this.endX.TabIndex = 0;
            this.endX.Value = new decimal(new int[] { 6, 0, 0, 0 });
            // 
            // compute
            // 
            this.compute.Location = new System.Drawing.Point(12, 12);
            this.compute.Name = "compute";
            this.compute.Size = new System.Drawing.Size(75, 23);
            this.compute.TabIndex = 0;
            this.compute.Text = "Compute";
            this.compute.UseVisualStyleBackColor = true;
            this.compute.Click += new System.EventHandler(this.ComputeClick);
            // 
            // PokemonSolverForm
            // 
            this.ClientSize = new System.Drawing.Size(565, 923);
            this.Controls.Add(goal);
            this.Controls.Add(start);
            this.Controls.Add(this.compute);
            this.Name = "PokemonSolverForm";
            start.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.startY)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.startX)).EndInit();
            goal.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.endY)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.endX)).EndInit();
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.CheckBox useCharacterAsStartPosition;

        private System.Windows.Forms.ListBox startDirection;

        private System.Windows.Forms.ListBox endDirection;

        private System.Windows.Forms.NumericUpDown startX;
        private System.Windows.Forms.NumericUpDown startY;
        private System.Windows.Forms.NumericUpDown endY;
        private System.Windows.Forms.NumericUpDown endX;

        private System.Windows.Forms.Button compute;

        private void useCharacterAsStartPosition_CheckedChanged(object sender, EventArgs e)
        {
            var check = (CheckBox)sender;
            foreach (var c in new Control[] { startX, startY, startDirection })
            {
                c.Enabled = !check.Checked;
            }
        }
    }
}