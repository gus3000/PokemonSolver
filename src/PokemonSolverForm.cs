using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using BizHawk.Client.Common;
using BizHawk.Client.EmuHawk;
using BizHawk.Common;
using PokemonSolver;
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


        private Label labelInfo;
        private StringBuilder currentStatus;

        public PokemonSolverForm()
        {
            ClientSize = new Size(480, 800);
            currentStatus = new StringBuilder();
            SuspendLayout();
            
            Controls.Add(labelInfo = new Label { AutoSize = true, MaximumSize = new Size(480,0)});
            ResumeLayout();
            Log.EnableDomain("NetworkDebug");
        }

        public override void Restart()
        {
            UpdateData();
        }

        protected override void UpdateAfter()
        {
            currentStatus.Clear();
            UpdateData();
            labelInfo.Text = currentStatus.ToString();
        }

        
        protected override async void OnClosing(CancelEventArgs e)
        {

        }

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);
            
            // Log.Error("NetworkDebug",String.Join("\n",(object[])GameData.Team[0].Moves));
            Log.Note("NetworkDebug",  GameData.debug);
            // Log.Note("NetworkDebug", String.Join("\n",APIs.Memory.GetMemoryDomainList()));
            // Log.Note("NetworkDebug", String.Join("\n",APIs.Memory.GetCurrentMemoryDomain()));
            
            // labelInfo.Text = "clicked";
        }

        private void UpdateData()
        {
            // APIs.Memory.UseMemoryDomain(Domain.ROM.ToString());
            GameData = new GameData(APIs.Memory);
            currentStatus.Append("coucou");
            // currentStatus.Append(GameData.Team[0].Nickname);
        }

        private string ReadData()
        {
            return GameData.Serialize();
        }
    }
}