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
using PokemonSolver.Interaction;
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
        private DataGridView MapView;
        private StringBuilder currentStatus;

        public PokemonSolverForm()
        {
            ClientSize = new Size(480, 800);

            currentStatus = new StringBuilder();
            SuspendLayout();
            Controls.Add(labelInfo = new Label { AutoSize = true, MaximumSize = new Size(480, 0) });
            Controls.Add(MapView = new DataGridView());
            // Controls.Add(_dateTimePicker = new DateTimePicker());
            ResumeLayout();

            Log.EnableDomain("NetworkDebug");
            Log.EnableDomain("Debug");
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
            // labelInfo.Text = "C'ay la mayrde";
            // APIs.Gui.ClearGraphics(DisplaySurfaceID.EmuCore);
            MapView.Location = new Point(0, 0);
            MapView.Size = new Size(400, 400);
            MapView.Name = "Map View";
            MapView.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            MapView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            MapView.CellBorderStyle = DataGridViewCellBorderStyle.Single;
            MapView.GridColor = Color.Black;
            MapView.ColumnHeadersVisible = false;
            MapView.RowHeadersVisible = false;
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

            // Log.Error("NetworkDebug",String.Join("\n",(object[])GameData.Team[0].Moves));
            // LogMessage(GameData.debug);

            // Log.Note("NetworkDebug", String.Join("\n",APIs.Memory.GetMemoryDomainList()));
            // Log.Note("NetworkDebug", String.Join("\n",APIs.Memory.GetCurrentMemoryDomain()));

            // labelInfo.Text = GameData.debug;

            // LogMessage($"[{String.Join(", ", PatternSearch.searchForPokemonString("DOUBLESLAP"))}]");
            // for (int i = 0; i < 355; i++)
            // {
            // const int moveNameLength = 13;

            // Utils.Log(
            // Utils.GetStringFromByteArray(
            // APIs.Memory.ReadByteRange(
            // RomAddress.EmeraldMoveNames + i * moveNameLength,
            // moveNameLength,
            // MemoryDomain.ROM
            // ),
            // true
            // )
            // );
            // }
            // Utils.Log(CombatEngine?.GetSelectedMove().ToString());

            // for (uint i = 0; i < 4; i++)
            // {
            //     Utils.Log($"Select {i}");
            //     Utils.Log(
            //         String.Join("\n", CombatEngine.SelectMove(i))
            //     );
            // }
            // Engine.HandleNextInput();
            // APIs.Gui.DrawRectangle(50,50,100,200,Color.Aqua,Color.Chocolate,DisplaySurfaceID.EmuCore);
            // APIs.Gui.DrawRectangle(50,50,100,200,Color.Aqua,Color.Chocolate,DisplaySurfaceID.Client);
            // APIs.Gui.AddMessage($"Drawing info : {APIs.Gui.GetPadding()}");
            // var result = PatternSearch.searchForByteArray(new byte[] { 1,0xc,0xc,0xc,4,4,4,4,1 });
            // Utils.Log($"[{string.Join(",", result)}]");
            OverworldEngine.ComputeMaps();
            FormatMap(OverworldEngine.Maps[0]);
        }

        public void FormatMap(Map map)
        {
            const int cellSize = 22;

            var mapData = map.MapData;
            var tiles = mapData.Tiles;
            
            MapView.Rows.Clear();
            MapView.ColumnCount = mapData.Width;
            MapView.Size = new Size(mapData.Width * cellSize, mapData.Height * cellSize);
            this.Size = new Size(MapView.Size.Width + cellSize, MapView.Size.Height + cellSize + 50);
            for (int i = 0; i < mapData.Height; i++)
            {
                var row = new List<string>();
                for (int j = 0; j < mapData.Width; j++)
                {
                    row.Add(tiles[i*mapData.Width + j].MovementPermission.ToString("X"));
                }
                MapView.Rows.Add(row.ToArray());
                
                // MapView.Rows.Add(row);
            }
        }

        private void UpdateData()
        {
            // APIs.Memory.UseMemoryDomain(Domain.ROM.ToString());
            GameData = new GameData(APIs.Memory);
            // currentStatus.Append("coucou");
            // currentStatus.Append(GameData.Team[0].Nickname);
        }

        private void LogMessage(string msg)
        {
            Log.Note("Debug", msg);
        }

        private string ReadData()
        {
            return GameData.Serialize();
        }
    }
}