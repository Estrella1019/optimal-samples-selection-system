using System.Windows.Forms;
using System.Drawing;

namespace frontend_csharp
{
    partial class Form2
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.listBoxRuns    = new System.Windows.Forms.ListBox();
            this.textBoxContent = new System.Windows.Forms.TextBox();
            this.buttonDisplay  = new System.Windows.Forms.Button();
            this.buttonDelete   = new System.Windows.Forms.Button();
            this.buttonRefresh  = new System.Windows.Forms.Button();
            this.buttonBack     = new System.Windows.Forms.Button();
            this.buttonPrint    = new System.Windows.Forms.Button();
            this.labelTitle     = new System.Windows.Forms.Label();
            this.labelHint      = new System.Windows.Forms.Label();
            this.SuspendLayout();

            // ── Form ────────────────────────────────────────────────────────
            this.ClientSize  = new System.Drawing.Size(900, 560);
            this.Name        = "Form2";
            this.Text        = "Saved Results Database";
            this.BackColor   = System.Drawing.Color.FromArgb(235, 245, 255);
            this.MinimumSize = new System.Drawing.Size(920, 600);
            this.Font        = new System.Drawing.Font("Segoe UI", 9F);

            // ── Title ────────────────────────────────────────────────────────
            this.labelTitle.Text      = "Saved Results Database";
            this.labelTitle.Font      = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            this.labelTitle.ForeColor = System.Drawing.Color.DarkSlateBlue;
            this.labelTitle.Location  = new System.Drawing.Point(20, 10);
            this.labelTitle.AutoSize  = true;

            // ── Hint label ──────────────────────────────────────────────────
            this.labelHint.Text      = "Format: m=  n=  k=  j=  s=   Run #   (groups)";
            this.labelHint.Font      = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Italic);
            this.labelHint.ForeColor = System.Drawing.Color.Gray;
            this.labelHint.Location  = new System.Drawing.Point(20, 40);
            this.labelHint.AutoSize  = true;

            // ── ListBox ─────────────────────────────────────────────────────
            this.listBoxRuns.FormattingEnabled = true;
            this.listBoxRuns.ItemHeight        = 20;
            this.listBoxRuns.Location          = new System.Drawing.Point(20, 65);
            this.listBoxRuns.Name              = "listBoxRuns";
            this.listBoxRuns.Size              = new System.Drawing.Size(360, 400);
            this.listBoxRuns.Font              = new System.Drawing.Font("Consolas", 9F);
            this.listBoxRuns.Anchor            = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            this.listBoxRuns.DoubleClick      += new System.EventHandler(this.buttonDisplay_Click);

            // ── TextBox (content viewer) ─────────────────────────────────────
            this.textBoxContent.Location    = new System.Drawing.Point(395, 65);
            this.textBoxContent.Multiline   = true;
            this.textBoxContent.Name        = "textBoxContent";
            this.textBoxContent.ScrollBars  = System.Windows.Forms.ScrollBars.Both;
            this.textBoxContent.Size        = new System.Drawing.Size(480, 400);
            this.textBoxContent.Font        = new System.Drawing.Font("Consolas", 9F);
            this.textBoxContent.ReadOnly    = true;
            this.textBoxContent.BackColor   = System.Drawing.Color.White;
            this.textBoxContent.ForeColor   = System.Drawing.Color.DarkBlue;
            this.textBoxContent.Anchor      = AnchorStyles.Top | AnchorStyles.Bottom |
                                               AnchorStyles.Left | AnchorStyles.Right;

            // ── Buttons ─────────────────────────────────────────────────────
            int btnY = 480, btnH = 30, btnW = 100, bs = 8;

            StyleButton(this.buttonDisplay, "Display",  System.Drawing.Color.SteelBlue,
                        new System.Drawing.Point(20, btnY), btnW, btnH);
            this.buttonDisplay.Click += new System.EventHandler(this.buttonDisplay_Click);

            StyleButton(this.buttonDelete, "Delete",   System.Drawing.Color.Crimson,
                        new System.Drawing.Point(20 + (btnW + bs), btnY), btnW, btnH);
            this.buttonDelete.Click += new System.EventHandler(this.buttonDelete_Click);

            StyleButton(this.buttonRefresh, "Refresh", System.Drawing.Color.DarkOrange,
                        new System.Drawing.Point(20 + 2*(btnW + bs), btnY), btnW, btnH);
            this.buttonRefresh.Click += new System.EventHandler(this.buttonRefresh_Click);

            StyleButton(this.buttonPrint, "Print",     System.Drawing.Color.Teal,
                        new System.Drawing.Point(20 + 3*(btnW + bs), btnY), btnW, btnH);
            this.buttonPrint.Click += new System.EventHandler(this.buttonPrint_Click);

            StyleButton(this.buttonBack, "Back",       System.Drawing.Color.DimGray,
                        new System.Drawing.Point(20 + 4*(btnW + bs), btnY), btnW, btnH);
            this.buttonBack.Click += new System.EventHandler(this.buttonBack_Click);

            // ── Add controls ─────────────────────────────────────────────────
            this.Controls.Add(this.labelTitle);
            this.Controls.Add(this.labelHint);
            this.Controls.Add(this.listBoxRuns);
            this.Controls.Add(this.textBoxContent);
            this.Controls.Add(this.buttonDisplay);
            this.Controls.Add(this.buttonDelete);
            this.Controls.Add(this.buttonRefresh);
            this.Controls.Add(this.buttonBack);
            this.Controls.Add(this.buttonPrint);

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void StyleButton(Button btn, string text, System.Drawing.Color color,
                                  System.Drawing.Point loc, int w, int h)
        {
            btn.Text      = text;
            btn.BackColor = color;
            btn.ForeColor = System.Drawing.Color.White;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.Size      = new System.Drawing.Size(w, h);
            btn.Location  = loc;
            btn.Font      = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            btn.Anchor    = AnchorStyles.Bottom | AnchorStyles.Left;
        }

        private System.Windows.Forms.ListBox  listBoxRuns;
        private System.Windows.Forms.TextBox  textBoxContent;
        private System.Windows.Forms.Button   buttonDisplay;
        private System.Windows.Forms.Button   buttonDelete;
        private System.Windows.Forms.Button   buttonRefresh;
        private System.Windows.Forms.Button   buttonBack;
        private System.Windows.Forms.Button   buttonPrint;
        private System.Windows.Forms.Label    labelTitle;
        private System.Windows.Forms.Label    labelHint;
    }
}
