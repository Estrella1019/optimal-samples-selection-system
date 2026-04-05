using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Printing;

namespace frontend_csharp
{
    public partial class Form2 : Form
    {
        private readonly string resultsPath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory, "results");

        public Form2()
        {
            InitializeComponent();
            LoadSavedRuns();
        }

        // ── Load file list ────────────────────────────────────────────────
        private void LoadSavedRuns()
        {
            listBoxRuns.Items.Clear();

            if (!Directory.Exists(resultsPath))
            {
                Directory.CreateDirectory(resultsPath);
                return;
            }

            var files = Directory.GetFiles(resultsPath, "*.txt")
                                 .OrderByDescending(f => File.GetLastWriteTime(f))
                                 .ToArray();

            foreach (var file in files)
            {
                string fname = Path.GetFileNameWithoutExtension(file);
                // Friendly display: m-n-k-j-s-runId-count  →  m=45 n=9 k=6 j=4 s=4  Run#1  (12 groups)
                string display = FriendlyName(fname) ?? fname;
                listBoxRuns.Items.Add(new FileEntry { FileName = Path.GetFileName(file), Display = display });
            }

            listBoxRuns.DisplayMember = "Display";
        }

        /// <summary>
        /// Converts "45-9-6-4-4-1-12" → "m=45  n=9  k=6  j=4  s=4   Run #1   (12 groups)"
        /// </summary>
        private string FriendlyName(string stem)
        {
            var parts = stem.Split('-');
            if (parts.Length >= 7 &&
                int.TryParse(parts[0], out int m) &&
                int.TryParse(parts[1], out int n) &&
                int.TryParse(parts[2], out int k) &&
                int.TryParse(parts[3], out int j) &&
                int.TryParse(parts[4], out int s) &&
                int.TryParse(parts[5], out int run) &&
                int.TryParse(parts[6], out int count))
            {
                return $"m={m}  n={n}  k={k}  j={j}  s={s}   Run #{run}   ({count} groups)";
            }
            return null;
        }

        // ── Button handlers ───────────────────────────────────────────────
        private void buttonDisplay_Click(object sender, EventArgs e)
        {
            var entry = listBoxRuns.SelectedItem as FileEntry;
            if (entry == null)
            {
                MessageBox.Show("Please select a result file to display.",
                                "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string filePath = Path.Combine(resultsPath, entry.FileName);
            try
            {
                textBoxContent.Text = File.ReadAllText(filePath);
            }
            catch (Exception ex)
            {
                textBoxContent.Text = $"Error reading file: {ex.Message}";
            }
        }

        private void buttonDelete_Click(object sender, EventArgs e)
        {
            var entry = listBoxRuns.SelectedItem as FileEntry;
            if (entry == null)
            {
                MessageBox.Show("Please select a result file to delete.",
                                "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var confirm = MessageBox.Show(
                $"Are you sure you want to delete:\n{entry.FileName}?",
                "Confirm Delete",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (confirm != DialogResult.Yes) return;

            string filePath = Path.Combine(resultsPath, entry.FileName);
            try
            {
                File.Delete(filePath);
                listBoxRuns.Items.Remove(entry);
                textBoxContent.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting file: {ex.Message}",
                                "Delete Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void buttonRefresh_Click(object sender, EventArgs e)
        {
            textBoxContent.Clear();
            LoadSavedRuns();
        }

        private void buttonBack_Click(object sender, EventArgs e) => this.Close();

        private void buttonPrint_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBoxContent.Text))
            {
                MessageBox.Show("There is no content to print.", "Print",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var printDocument = new PrintDocument();
            string textToPrint = textBoxContent.Text;

            printDocument.PrintPage += (s, ev) =>
            {
                ev.Graphics.DrawString(
                    textToPrint,
                    new Font("Consolas", 9),
                    Brushes.Black,
                    ev.MarginBounds,
                    StringFormat.GenericTypographic);
            };

            using (var pd = new PrintDialog { Document = printDocument })
            {
                if (pd.ShowDialog() == DialogResult.OK)
                {
                    try { printDocument.Print(); }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Print failed: {ex.Message}",
                                        "Print Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        // ── Helper class ─────────────────────────────────────────────────
        private class FileEntry
        {
            public string FileName { get; set; }
            public string Display  { get; set; }
            public override string ToString() => Display;
        }
    }
}
