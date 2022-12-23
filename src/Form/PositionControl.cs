using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using PokemonSolver.Algoritm;
using PokemonSolver.Interaction;
using PokemonSolver.MapData;
using PokemonSolver.Memory;

namespace PokemonSolver.Form
{
    public class PositionControl : UserControl
    {
        private GroupBox _group;
        private ComboBox _mapIndex;
        private ComboBox _mapBank;
        private Label _xLabel;
        private Label _yLabel;
        private Label _mapIndexLabel;
        private Label _mapBankLabel;

        private ListBox _directionField;

        private NumericUpDown _xField;
        private NumericUpDown _yField;

        private CheckBox useCharacterAsStartPosition;

        public PositionControl(string name, int x, int y)
        {
            _group = new GroupBox();
            _mapIndex = new ComboBox();
            _mapBank = new ComboBox();
            _xLabel = new Label();
            _yLabel = new Label();
            _mapIndexLabel = new Label();
            _mapBankLabel = new Label();
            _directionField = new ListBox();
            _xField = new NumericUpDown();
            _yField = new NumericUpDown();
            useCharacterAsStartPosition = new CheckBox();

            _group.Text = name;
            Location = new Point(x, y);
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            Size = new Size(500, 100);

            _group.Location = new Point(0, 0);
            _group.Size = Size;

            // 
            // startMapBankLabel
            // 
            _mapBankLabel.Location = new Point(6, 22);
            _mapBankLabel.Name = "_mapBankLabel";
            _mapBankLabel.Size = new Size(37, 16);
            _mapBankLabel.TabIndex = 11;
            _mapBankLabel.Text = "bank :";
            _mapBankLabel.TextAlign = ContentAlignment.MiddleRight;

            // 
            // startMapBank
            // 
            _mapBank.FormattingEnabled = true;
            _mapBank.Location = new Point(_mapBankLabel.Bounds.Right + FormUtils.DefaultMargin, _mapBankLabel.Location.Y);
            _mapBank.MaxDropDownItems = 30;
            _mapBank.Name = "_mapBank";
            _mapBank.Size = new Size(180, 21);
            _mapBank.TabIndex = 10;
            // _startMapBank.SelectedIndexChanged += (sender, args) => InitMapIndexField(_startMapBank, _startMapIndex);

            // 
            // startMapIndexLabel
            // 
            _mapIndexLabel.Location = new Point(6, 44);
            _mapIndexLabel.Name = "_mapIndexLabel";
            _mapIndexLabel.Size = new Size(37, 16);
            _mapIndexLabel.TabIndex = 13;
            _mapIndexLabel.Text = "n° :";
            _mapIndexLabel.TextAlign = _mapBankLabel.TextAlign;

            // 
            // startMapIndex
            // 
            _mapIndex.FormattingEnabled = true;
            _mapIndex.Location = new Point(_mapIndexLabel.Bounds.Right + FormUtils.DefaultMargin, _mapIndexLabel.Location.Y);
            _mapIndex.Name = "_mapIndex";
            _mapIndex.Size = new Size(180, 21);
            _mapIndex.TabIndex = 12;

            // 
            // startXLabel
            // 
            _xLabel.Location = new Point(_mapBank.Bounds.Right + FormUtils.DefaultMargin, _mapBankLabel.Location.Y);
            // startXLabel.Location = new Point(0,0);
            _xLabel.Name = "_xLabel";
            _xLabel.Size = new Size(15, 16);
            _xLabel.TabIndex = 1;
            _xLabel.Text = "x:";
            _xLabel.TextAlign = ContentAlignment.MiddleRight;

            // 
            // startX
            // 
            _xField.Location = new Point(_xLabel.Bounds.Right + FormUtils.DefaultMargin, _xLabel.Location.Y);
            _xField.Name = "_xField";
            _xField.Size = new Size(39, 20);
            _xField.TabIndex = 0;
            _xField.Value = 3;
            _xField.Minimum = 0;
            _xField.Maximum = 1000;

            // 
            // startYLabel
            // 
            _yLabel.Location = new Point(_xLabel.Location.X, _xLabel.Bounds.Bottom + FormUtils.DefaultMargin);
            _yLabel.Name = "_yLabel";
            _yLabel.Size = new Size(15, 16);
            _yLabel.TabIndex = 3;
            _yLabel.Text = "y:";
            _yLabel.TextAlign = _xLabel.TextAlign;

            // 
            // startY
            // 
            _yField.Location = new Point(_yLabel.Bounds.Right + FormUtils.DefaultMargin, _yLabel.Location.Y);
            _yField.Name = "_yField";
            _yField.Size = new Size(39, 20);
            _yField.TabIndex = 2;
            _yField.Value = 3;
            _yField.Minimum = 0;
            _yField.Maximum = 1000;

            // 
            // startDirection
            // 
            _directionField.FormattingEnabled = true;
            _directionField.Items.AddRange(new object[] { "Down", "Up", "Left", "Right" });
            _directionField.Location = new Point(_xField.Bounds.Right + FormUtils.DefaultMargin, 17);
            _directionField.Name = "_directionField";
            _directionField.Size = new Size(120, 56);
            _directionField.TabIndex = 5;
            _directionField.SelectedIndex = (int)Direction.Down; //TODO move to field init ?

            useCharacterAsStartPosition.Checked = false;
            useCharacterAsStartPosition.Location = new Point(6, Size.Height - 20);
            useCharacterAsStartPosition.Name = "useCharacterAsStartPosition";
            useCharacterAsStartPosition.Size = new Size(149, 18);
            useCharacterAsStartPosition.TabIndex = 5;
            useCharacterAsStartPosition.Text = "use character position";
            useCharacterAsStartPosition.UseVisualStyleBackColor = true;
            useCharacterAsStartPosition.CheckedChanged += useCharacterPosition_CheckedChanged;

            Controls.AddRange(new Control[]
            {
                _mapIndex,
                _mapBank,
                _xLabel,
                _yLabel,
                _mapIndexLabel,
                _mapBankLabel,
                _directionField,
                _xField,
                _yField,
                useCharacterAsStartPosition,
                _group,
            });
        }

        public void InitializeFields()
        {
            _mapBank.Items.Clear();
            _mapIndex.Items.Clear();

            var mapBanks = OverworldEngine.GetInstance().Banks.Select((_, index) => index.ToString()).ToArray();

            _mapBank.Items.AddRange(mapBanks);
            InitMapIndexField();
        }

        private void InitMapIndexField()
        {
            _mapIndex.Items.Clear();
            var mapBankIndex = _mapBank.SelectedIndex;
            Utils.Log($"selected map bank index : {mapBankIndex}");
            IList<Map> maps;
            if (mapBankIndex == -1)
            {
                maps = OverworldEngine.GetInstance().Maps;
            }
            else
            {
                maps = OverworldEngine.GetInstance().Banks[mapBankIndex];
            }

            _mapIndex.Items.AddRange(maps.Select(map => $"{map.Name} ({map.Bank},{map.MapIndex})").ToArray());
            _mapBank.SelectedIndex = 0;
            _mapIndex.SelectedIndex = 9;
        }

        public void UpdateFieldsFromGameData()
        {
            if(useCharacterAsStartPosition.Checked)
                UpdateFieldsFromCharPosition();
        }
        
        private void UpdateFieldsFromCharPosition()
        {
            //FIXME bug going North from Mauville
            var characterPosition = OverworldEngine.GetInstance().GetCurrentPosition();
            // Utils.Log(characterPosition?.ToString());
            if (characterPosition == null) return;

            _mapBank.SelectedIndex = characterPosition.MapBank;
            _mapIndex.SelectedIndex = characterPosition.MapIndex;

            if (characterPosition.X < 0 || characterPosition.Y < 0)
            {
                Utils.Error($"wtf : charposition = {characterPosition}");
                return;
            }

            _xField.Value = characterPosition.X;
            _yField.Value = characterPosition.Y;
            // Utils.Log($"Direction : {_directionField.SelectedIndex} -> {FormUtils.GetSelectIndexFromDirection(characterPosition.Direction)} ({characterPosition.Direction})");
            _directionField.SelectedIndex = FormUtils.GetSelectIndexFromDirection(characterPosition.Direction);
        }

        public Position GetPosition()
        {
            return new Position(
                _mapBank.SelectedIndex,
                _mapIndex.SelectedIndex,
                (int)_xField.Value,
                (int)_yField.Value,
                FormUtils.GetDirectionFromSelectIndex(_directionField.SelectedIndex),
                Altitude.Any
            );
        }

        public void SetUseCharacterPosition(bool check)
        {
            useCharacterAsStartPosition.Checked = check;
        }

        public void ToggleUseCharacterPosition()
        {
            useCharacterAsStartPosition.Checked = !useCharacterAsStartPosition.Checked;
        }

        private void useCharacterPosition_CheckedChanged(object sender, EventArgs e)
        {
            var check = (CheckBox)sender;
            foreach (var c in new Control[] { _mapBank, _mapIndex, _xField, _yField, _directionField })
            {
                c.Enabled = !check.Checked;
            }
        }
    }
}