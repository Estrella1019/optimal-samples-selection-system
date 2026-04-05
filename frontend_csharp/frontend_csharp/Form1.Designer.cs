using System;
using System.Drawing;
using System.Windows.Forms;

namespace frontend_csharp
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        // Labels
        private Label labelTitle, labelM, labelN, labelK, labelJ, labelS;
        private Label labelManual, labelCoverage, labelSelected, labelParams;

        // TextBoxes
        private TextBox textBoxM, textBoxN, textBoxK, textBoxJ, textBoxS;
        private TextBox textBoxManual, textBoxOutput, textBoxCoverage;

        // Buttons
        private Button buttonRun, buttonStore, buttonOpen, buttonTest;
        private Button buttonClear, buttonView, buttonPrint, buttonBack;
        private Button buttonNext, buttonRunExample;

        // ComboBox
        private ComboBox comboExamples;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.SuspendLayout();

            // ── Form ────────────────────────────────────────────────────────
            this.Text        = "Optimal Samples Selection System";
            this.ClientSize  = new Size(860, 680);
            this.BackColor   = Color.FromArgb(240, 248, 255);   // AliceBlue
            this.MinimumSize = new Size(880, 720);
            this.Font        = new Font("Segoe UI", 9F);

            // ── Title ────────────────────────────────────────────────────────
            labelTitle = new Label
            {
                Text      = "Optimal Samples Selection System",
                Font      = new Font("Segoe UI", 15F, FontStyle.Bold),
                ForeColor = Color.DarkSlateBlue,
                Location  = new Point(20, 12),
                AutoSize  = true
            };

            // ── Parameter hint label ─────────────────────────────────────────
            labelParams = new Label
            {
                Text      = "m: 45-54  |  n: 7-25  |  k: 4-7  |  j: 4-7  |  s: 3-7",
                Font      = new Font("Segoe UI", 8F, FontStyle.Italic),
                ForeColor = Color.Gray,
                Location  = new Point(20, 42),
                AutoSize  = true
            };

            // ── Parameter rows ───────────────────────────────────────────────
            int lblX = 20, txtX = 115, rowY = 65, dy = 32;
            int txtW = 80;

            // Row 1: m, n
            labelM   = MakeLabel("m :", new Point(lblX, rowY));
            textBoxM = MakeTextBox(new Point(txtX, rowY - 2), txtW);

            labelN   = MakeLabel("n :", new Point(lblX + 230, rowY));
            textBoxN = MakeTextBox(new Point(txtX + 230, rowY - 2), txtW);

            // Row 2: k, j
            rowY += dy;
            labelK   = MakeLabel("k :", new Point(lblX, rowY));
            textBoxK = MakeTextBox(new Point(txtX, rowY - 2), txtW);

            labelJ   = MakeLabel("j :", new Point(lblX + 230, rowY));
            textBoxJ = MakeTextBox(new Point(txtX + 230, rowY - 2), txtW);

            // Row 3: s, min_coverage
            rowY += dy;
            labelS   = MakeLabel("s :", new Point(lblX, rowY));
            textBoxS = MakeTextBox(new Point(txtX, rowY - 2), txtW);

            labelCoverage   = MakeLabel("Min Coverage:", new Point(lblX + 230, rowY));
            labelCoverage.AutoSize = true;
            textBoxCoverage = MakeTextBox(new Point(txtX + 230, rowY - 2), txtW);
            textBoxCoverage.Text = "1";

            // Row 4: manual input
            rowY += dy;
            labelManual   = MakeLabel("Manual Input:", new Point(lblX, rowY));
            labelManual.AutoSize = true;
            textBoxManual = new TextBox
            {
                Location    = new Point(txtX, rowY - 2),
                Size        = new Size(720, 23),
                BackColor   = Color.White,
                ForeColor   = Color.Gray,
                Text        = "Optional: comma-separated numbers, e.g. 1,5,9,12,...",
                Anchor      = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            // Clear placeholder on focus
            textBoxManual.GotFocus  += (s, ev) => {
                if (textBoxManual.ForeColor == Color.Gray) {
                    textBoxManual.Text = "";
                    textBoxManual.ForeColor = Color.Navy;
                }
            };
            textBoxManual.LostFocus += (s, ev) => {
                if (string.IsNullOrWhiteSpace(textBoxManual.Text)) {
                    textBoxManual.ForeColor = Color.Gray;
                    textBoxManual.Text = "Optional: comma-separated numbers, e.g. 1,5,9,12,...";
                }
            };

            // Row 5: Examples dropdown + Run Example button
            rowY += dy;
            var labelEx = new Label
            {
                Text     = "Quick Example:",
                Location = new Point(lblX, rowY),
                AutoSize = true,
                ForeColor = Color.DimGray
            };
            comboExamples = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location      = new Point(txtX, rowY - 2),
                Size          = new Size(160, 24)
            };
            comboExamples.Items.AddRange(new object[]
            {
                "45,8,6,6,5",
                "45,9,6,5,4",
                "45,9,6,4,4",
                "45,12,6,6,4",
                "50,10,5,3,3"
            });
            comboExamples.SelectedIndex = 0;
            comboExamples.SelectionChangeCommitted += comboExamples_SelectionChangeCommitted;

            buttonRunExample = NewButton("Run Example", Color.MediumOrchid,
                                         new Point(txtX + 170, rowY - 2), buttonRunExample_Click,
                                         width: 110);

            // ── Selected samples display ─────────────────────────────────────
            rowY += dy;
            labelSelected = new Label
            {
                Text      = "Selected Samples: (none yet — press Execute or Run Example)",
                Font      = new Font("Consolas", 9F, FontStyle.Bold),
                ForeColor = Color.DarkGreen,
                Location  = new Point(20, rowY),
                Size      = new Size(820, 22),
                Anchor    = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            // ── Output TextBox ───────────────────────────────────────────────
            rowY += 28;
            textBoxOutput = new TextBox
            {
                Location    = new Point(20, rowY),
                Size        = new Size(820, 330),
                Multiline   = true,
                ScrollBars  = ScrollBars.Both,
                BackColor   = Color.White,
                ForeColor   = Color.DarkBlue,
                Font        = new Font("Consolas", 9F),
                ReadOnly    = true,
                Anchor      = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };

            // ── Button rows ──────────────────────────────────────────────────
            int btnY = textBoxOutput.Bottom + 12;
            int btnW = 100, btnH = 30, bs = 8;

            // Row A: Execute  Store  Open  Test  Clear
            buttonRun   = NewButton("Execute",    Color.Crimson,         new Point(20, btnY),                       buttonRun_Click);
            buttonStore = NewButton("Store",      Color.SeaGreen,        new Point(20 + (btnW + bs),     btnY),     buttonStore_Click);
            buttonOpen  = NewButton("Open Folder",Color.SteelBlue,       new Point(20 + 2*(btnW + bs),   btnY),     buttonOpen_Click, width: 110);
            buttonTest  = NewButton("Test",       Color.DarkOrange,      new Point(20 + 2*(btnW+bs)+120,  btnY),    buttonTest_Click);
            buttonClear = NewButton("Clear",      Color.SlateGray,       new Point(20 + 3*(btnW+bs)+120,  btnY),    buttonClear_Click);

            // Row B: View  Print  Back  Next
            btnY += btnH + bs;
            buttonView  = NewButton("View Result", Color.DarkCyan,       new Point(20, btnY),                      buttonView_Click, width: 110);
            buttonPrint = NewButton("Print",        Color.Teal,           new Point(20 + 120,              btnY),   buttonPrint_Click);
            buttonBack  = NewButton("Back",         Color.DimGray,        new Point(20 + 120 + (btnW+bs),  btnY),   buttonBack_Click);
            buttonNext  = NewButton("Next ▶",       Color.DarkSlateBlue,  new Point(20 + 120 + 2*(btnW+bs),btnY),   buttonNext_Click);

            // ── Add controls ─────────────────────────────────────────────────
            this.Controls.AddRange(new Control[]
            {
                labelTitle, labelParams,
                labelM, textBoxM,
                labelN, textBoxN,
                labelK, textBoxK,
                labelJ, textBoxJ,
                labelS, textBoxS,
                labelCoverage, textBoxCoverage,
                labelManual, textBoxManual,
                labelEx, comboExamples, buttonRunExample,
                labelSelected,
                textBoxOutput,
                buttonRun, buttonStore, buttonOpen, buttonTest, buttonClear,
                buttonView, buttonPrint, buttonBack, buttonNext
            });

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        // ── Helper factories ────────────────────────────────────────────────
        private Label MakeLabel(string text, Point loc)
        {
            return new Label
            {
                Text      = text,
                Location  = loc,
                Size      = new Size(90, 20),
                ForeColor = Color.DarkSlateGray,
                TextAlign = ContentAlignment.MiddleRight
            };
        }

        private TextBox MakeTextBox(Point loc, int width)
        {
            return new TextBox
            {
                Location  = loc,
                Size      = new Size(width, 23),
                BackColor = Color.White,
                ForeColor = Color.Navy
            };
        }

        private Button NewButton(string text, Color backColor, Point location,
                                  EventHandler clickHandler, int width = 100)
        {
            var btn = new Button
            {
                Text      = text,
                BackColor = backColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Size      = new Size(width, 30),
                Location  = location,
                Anchor    = AnchorStyles.Bottom | AnchorStyles.Left,
                Font      = new Font("Segoe UI", 8.5F, FontStyle.Bold)
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.Click += clickHandler;
            return btn;
        }
    }
}
