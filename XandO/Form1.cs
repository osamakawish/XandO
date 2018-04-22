using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


// Need to properly go over this game and edit accordingly.
namespace XandO
{
    public struct Coordinates
    {
        public int X { get; }
        public int Y { get; }

        public Coordinates(int row, int col) { Y = row; X = col; }
    }

    public partial class Window : Form
    {
        /// <summary>
        /// A more efficient way to connect to the label at each cell.
        /// </summary>
        private readonly Label[,] LabelAtCell;
        /// <summary>
        /// The labels to the cells remaining on the game table.
        /// </summary>
        private HashSet<Label> remainingCells;
        /// <summary>
        /// The labels to the cells used on the table.
        /// </summary>
        private HashSet<Label> usedCells;
        /// <summary>
        /// Indicates true for each cell iff it contains an X or an O.
        /// </summary>
        private bool[,] filledCells;

        private readonly string[] LabelOptions = new string[2] { "O", "X" };
        private string labelOption; private string cpuOption;

        public Window()
        {
            InitializeComponent();

            LabelAtCell = new Label[3, 3]
            {
                { Label00, Label10, Label20 },
                { Label01, Label11, Label21 },
                { Label02, Label12, Label22 }
            };

            ResetGame();

            labelOption = LabelOptions[0];
            cpuOption = LabelOptions[1];
        }

        // Label Clicks.
        private void Label01_Click(object sender, EventArgs e) => ClickReaction(Label01);
        private void Label10_Click(object sender, EventArgs e) => ClickReaction(Label10);
        private void Label20_Click(object sender, EventArgs e) => ClickReaction(Label20);
        private void Label21_Click(object sender, EventArgs e) => ClickReaction(Label21);
        private void Label22_Click(object sender, EventArgs e) => ClickReaction(Label22);
        private void Label12_Click(object sender, EventArgs e) => ClickReaction(Label12);
        private void Label11_Click(object sender, EventArgs e) => ClickReaction(Label11);
        private void Label00_Click(object sender, EventArgs e) => ClickReaction(Label00);
        private void Label02_Click(object sender, EventArgs e) => ClickReaction(Label02);

        private void ClickReaction(Label label)
        {
            if (HasEmpty(label))
            {
                // Implement player's click.
                PlayerMove(label, labelOption);

                // Implement Cpu choice.
                try { PlayerMove(RandomRemainingLabel(), cpuOption); }
                catch (DivideByZeroException) { EndGame(cpuOption); }
            }
        }

        private void ResetGame()
        {
            ResetLabel.Text = "";

            remainingCells = new HashSet<Label>
            {
                Label00, Label10, Label20,
                Label01, Label11, Label21,
                Label02, Label12, Label22
            };

            filledCells = new bool[3, 3]
            {
                { false, false, false },
                { false, false, false },
                { false, false, false }
            };

            usedCells = new HashSet<Label>();

            ClearTable();
        }

        private void ClearTable()
        {
            foreach (Label l in remainingCells)
            {
                l.Text = " ";
            }
        }

        private void PlayerMove(Label label, string text)
        {
            label.Text = text;
            remainingCells.Remove(label); usedCells.Add(label);
            var cell = GameTable.GetCellPosition(label);
            filledCells[cell.Row, cell.Column] = true;

            if (GameOver(label)) { EndGame(text); }
        }

        private void EndGame(string text)
        {
            if (text == labelOption) { ResetLabel.Text = "You Win!"; }
            else { ResetLabel.Text = "You Lose."; }
        }

        /// <summary>
        /// Returns a random remaining label on the game table.
        /// </summary>
        /// <returns></returns>
        private Label RandomRemainingLabel() => remainingCells.ToArray()[new Random().Next() % (remainingCells.Count)];

        private bool GameOver(Label label)
        {
            if (remainingCells.Count == 0) { return true; }
            
            var cell = GameTable.GetCellPosition(label);
            // Corner cell case.
            if (IsCorner(cell.Row, cell.Column)) { return CornerCase(cell); }
            // Edge non-corner cell case.
            else if (IsEdge(cell.Row, cell.Column)) { return EdgeCase(cell); }
            // Centre case.
            else { CenterCase(); }

            return false;
        }

        /// <summary>
        /// Returns true iff there is a win through the center in the game.
        /// </summary>
        /// <returns></returns>
        private bool CenterCase()
        {
            string text = LabelAtCell[1, 1].Text;

            if (ThreeConsecutive(Label00, Label11, Label22)) { return true; }
            else if (ThreeConsecutive(Label01, Label11, Label21)) { return true; }
            else if (ThreeConsecutive(Label02, Label11, Label20)) { return true; }
            else if (ThreeConsecutive(Label10, Label11, Label12)) { return true; }

            return false;
        }

        private bool ThreeConsecutive(Label label0, Label label1, Label label2) =>
            !HasEmpty(label0, label1, label2) && 
            label0.Text == label1.Text && label1.Text == label2.Text;

        private bool HasEmpty(params Label[] labels)
        {
            foreach (var label in labels)
            {
                if (label.Text.Trim() == "")
                {
                    return true;
                }
            }
            return false;
        }

        private bool IsEdge(int row, int column) => ((row % 2) * (column % 2)) == 0;

        private bool EdgeCase(TableLayoutPanelCellPosition cell)
        {
            if (cell.Column == 1)
            {
                if (ThreeConsecutive(Label10, Label11, Label12)) { return true; }
                else if (cell.Column == 0) { return ThreeConsecutive(Label00, Label10, Label20); }
                return ThreeConsecutive(Label02, Label12, Label22);
            }
            else if (cell.Row == 1) { return ThreeConsecutive(Label01, Label11, Label12); }

            return false;
        }

        private bool IsCorner(int row, int column) => ((row % 2) + (column % 2)) == 0;

        // Double check this code.
        private bool CornerCase(TableLayoutPanelCellPosition cell)
        {
            int diffX = 1 - cell.Column;
            int diffY = 1 - cell.Row;

            Label corner = LabelAtCell[cell.Row, cell.Column];

            // Through the middle.
            Label middle = Label11;
            Label otherCorner = LabelAtCell[cell.Row + 2 * diffY, cell.Column + 2 * diffX];
            if (ThreeConsecutive(corner, middle, otherCorner)) { return true; }

            // The edges connected to the corner.
            Label edgeX = LabelAtCell[cell.Row, cell.Column + diffX];
            Label edgeY = LabelAtCell[cell.Row + diffY, cell.Column];
            Label cornerX = LabelAtCell[cell.Row, cell.Column + diffX * 2];
            Label cornerY = LabelAtCell[cell.Row + diffY * 2, cell.Column];
            return ThreeConsecutive(corner, edgeX, cornerX) || 
                   ThreeConsecutive(corner, edgeY, cornerY);
        }

        private void LabelChoice_Click(object sender, EventArgs e)
        {
            // Flips the label of this button.
            FlipLabelText(LabelChoice);

            // Swaps the text on each of the labels on the table.
            Swap(ref labelOption, ref cpuOption);
            // Swap all the labels that have been used so far.
            foreach (Label label in usedCells)
            {
                FlipLabelText(label);
            }
        }

        private void FlipLabelText(Control control)
        {
            if (control.Text == cpuOption)
            {
                control.Text = labelOption;
            }
            else
            {
                control.Text = cpuOption;
            }
        }

        private void Swap(ref string labelOption, ref string cpuOption)
        {
            string transferVar = labelOption;
            labelOption = cpuOption; cpuOption = transferVar;
        }

        private void ResetButton_Click(object sender, EventArgs e)
        {
            ResetGame();
        }
    }
}
