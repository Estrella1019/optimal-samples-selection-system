using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace frontend_csharp
{
    public partial class Form1 : Form
    {
        // Track last execution's selected samples for display
        private int[] _lastSelected = null;

        public Form1()
        {
            InitializeComponent();
        }

        // ── Event wiring ──────────────────────────────────────────────────────
        private void comboExamples_SelectionChangeCommitted(object sender, EventArgs e)
        {
            var parts = comboExamples.SelectedItem
                        .ToString()
                        .Split(',')
                        .Select(int.Parse)
                        .ToArray();

            textBoxM.Text = parts[0].ToString();
            textBoxN.Text = parts[1].ToString();
            textBoxK.Text = parts[2].ToString();
            textBoxJ.Text = parts[3].ToString();
            textBoxS.Text = parts[4].ToString();
        }

        private void buttonRun_Click(object s, EventArgs e)     => RunOnce(store: false);
        private void buttonStore_Click(object s, EventArgs e)   => RunOnce(store: true);
        private void buttonTest_Click(object s, EventArgs e)    => PopulateDefaults();
        private void buttonClear_Click(object s, EventArgs e)   => ClearAll();
        private void buttonOpen_Click(object s, EventArgs e)    => OpenResultsFolder();
        private void buttonView_Click(object s, EventArgs e)    => ViewLatestResult();
        private void buttonPrint_Click(object s, EventArgs e)   => PrintForm();
        private void buttonNext_Click(object s, EventArgs e)    => OpenForm2();
        private void buttonBack_Click(object s, EventArgs e)    => this.Close();
        private void buttonRunExample_Click(object s, EventArgs e) => RunOnce(store: false);

        // ── Helpers ───────────────────────────────────────────────────────────
        private void PopulateDefaults()
        {
            textBoxM.Text        = "45";
            textBoxN.Text        = "12";
            textBoxK.Text        = "6";
            textBoxJ.Text        = "6";
            textBoxS.Text        = "4";
            textBoxCoverage.Text = "1";
            textBoxManual.Clear();
        }

        private void ClearAll()
        {
            textBoxM.Clear();
            textBoxN.Clear();
            textBoxK.Clear();
            textBoxJ.Clear();
            textBoxS.Clear();
            textBoxManual.Clear();
            textBoxCoverage.Clear();
            textBoxOutput.Clear();
            labelSelected.Text = "Selected Samples: (none)";
            _lastSelected = null;
        }

        private void OpenForm2()
        {
            var f2 = new Form2();
            f2.ShowDialog(this);
        }

        /// <summary>
        /// Validate all inputs. Returns false and shows MessageBox on error.
        /// </summary>
        private bool ValidateInputs(out int m, out int n, out int k, out int j,
                                    out int s, out int minCov, out int[] manual)
        {
            m = n = k = j = s = minCov = 0;
            manual = null;

            // Parse integers
            if (!int.TryParse(textBoxM.Text.Trim(), out m) || m < 45 || m > 54)
            {
                ShowError("m must be an integer between 45 and 54.");
                return false;
            }
            if (!int.TryParse(textBoxN.Text.Trim(), out n) || n < 7 || n > 25)
            {
                ShowError("n must be an integer between 7 and 25.");
                return false;
            }
            if (n > m)
            {
                ShowError("n cannot be greater than m.");
                return false;
            }
            if (!int.TryParse(textBoxK.Text.Trim(), out k) || k < 4 || k > 7)
            {
                ShowError("k must be an integer between 4 and 7.");
                return false;
            }
            if (k > n)
            {
                ShowError("k cannot be greater than n.");
                return false;
            }
            if (!int.TryParse(textBoxJ.Text.Trim(), out j) || j < 4 || j > 7)
            {
                ShowError("j must be an integer between 4 and 7.");
                return false;
            }
            if (j > n)
            {
                ShowError("j cannot be greater than n.");
                return false;
            }
            if (!int.TryParse(textBoxS.Text.Trim(), out s) || s < 3 || s > 7)
            {
                ShowError("s must be an integer between 3 and 7.");
                return false;
            }
            if (s > j)
            {
                ShowError("s cannot be greater than j.");
                return false;
            }
            if (s > k)
            {
                ShowError("s cannot be greater than k.");
                return false;
            }

            // min_coverage (optional, default 1)
            string covText = textBoxCoverage.Text.Trim();
            if (string.IsNullOrEmpty(covText))
                minCov = 1;
            else if (!int.TryParse(covText, out minCov) || minCov < 1)
            {
                ShowError("Min Coverage must be a positive integer.");
                return false;
            }

            // Manual input (optional)
            string manualText = textBoxManual.Text.Trim();
            if (!string.IsNullOrEmpty(manualText))
            {
                try
                {
                    manual = manualText.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                                       .Select(int.Parse)
                                       .ToArray();
                    if (manual.Length != n)
                    {
                        ShowError($"Manual input must contain exactly {n} numbers.");
                        return false;
                    }
                    if (manual.Any(x => x < 1 || x > m))
                    {
                        ShowError($"Manual input values must be between 1 and {m}.");
                        return false;
                    }
                    if (manual.Distinct().Count() != manual.Length)
                    {
                        ShowError("Manual input must not contain duplicate numbers.");
                        return false;
                    }
                }
                catch
                {
                    ShowError("Manual input must be comma-separated integers.");
                    return false;
                }
            }

            return true;
        }

        private void ShowError(string msg)
        {
            MessageBox.Show(msg, "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void RunOnce(bool store)
        {
            if (!ValidateInputs(out int m, out int n, out int k, out int j,
                                out int s, out int minCov, out int[] manual))
                return;

            var root = FindProjectRootContaining("backend_python");

            // Build manual_input string for params.txt
            string manualStr = (manual != null && manual.Length > 0)
                               ? string.Join(",", manual)
                               : "";

            // Write params.txt
            string correctParamPath = Path.Combine(root, "frontend_csharp", "params.txt");
            File.WriteAllLines(correctParamPath, new[]
            {
                $"m={m}",
                $"n={n}",
                $"k={k}",
                $"j={j}",
                $"s={s}",
                $"manual_input={manualStr}",
                $"min_coverage={minCov}"
            });

            textBoxOutput.Clear();
            labelSelected.Text = "Selected Samples: running...";
            Application.DoEvents();

            // Run backend
            RunPython(Path.Combine(root, "backend_python"));

            // Read back the latest result
            string latestContent = GetLatestResultContent(root);
            if (!string.IsNullOrEmpty(latestContent))
            {
                textBoxOutput.Text = latestContent;

                // Parse selected samples line for display
                string samplesLine = latestContent
                    .Split('\n')
                    .FirstOrDefault(l => l.TrimStart().StartsWith("Selected Samples"));
                labelSelected.Text = samplesLine != null
                    ? samplesLine.Trim()
                    : "Selected Samples: (see output)";
            }

            if (store)
            {
                MessageBox.Show("Result stored to database (results folder).",
                                "Store", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void RunPython(string dir)
        {
            string runner = Path.Combine(dir, "runner.py");
            var psi = new ProcessStartInfo
            {
                FileName               = "python",
                Arguments              = $"\"{runner}\"",
                WorkingDirectory       = dir,
                UseShellExecute        = false,
                RedirectStandardOutput = true,
                RedirectStandardError  = true,
                CreateNoWindow         = true
            };

            using (var p = Process.Start(psi))
            {
                var outText = p.StandardOutput.ReadToEnd();
                var errText = p.StandardError.ReadToEnd();
                p.WaitForExit();

                if (!string.IsNullOrWhiteSpace(outText))
                    textBoxOutput.AppendText(outText);
                if (!string.IsNullOrWhiteSpace(errText))
                    textBoxOutput.AppendText(Environment.NewLine + "ERROR: " + errText);
            }
        }

        private string GetLatestResultContent(string projectRoot)
        {
            var resultsDir = Path.Combine(projectRoot, "frontend_csharp", "bin", "Debug", "results");
            if (!Directory.Exists(resultsDir)) return null;

            var latest = new DirectoryInfo(resultsDir)
                .GetFiles("*.txt")
                .OrderByDescending(f => f.LastWriteTime)
                .FirstOrDefault();

            return latest != null ? File.ReadAllText(latest.FullName) : null;
        }

        private void OpenResultsFolder()
        {
            var root = FindProjectRootContaining("backend_python");
            var dir  = Path.Combine(root, "frontend_csharp", "bin", "Debug", "results");
            if (Directory.Exists(dir))
                Process.Start("explorer.exe", dir);
            else
                MessageBox.Show("Results folder not found.", "Open",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void ViewLatestResult()
        {
            var root   = FindProjectRootContaining("backend_python");
            var dir    = Path.Combine(root, "frontend_csharp", "bin", "Debug", "results");
            if (!Directory.Exists(dir)) { MessageBox.Show("No results yet."); return; }

            var latest = new DirectoryInfo(dir)
                .GetFiles("*.txt")
                .OrderByDescending(f => f.LastWriteTime)
                .FirstOrDefault();
            if (latest != null)
                Process.Start("notepad.exe", latest.FullName);
            else
                MessageBox.Show("No result to view.", "View",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void PrintForm()
        {
            using (var bmp = new Bitmap(Width, Height))
            {
                DrawToBitmap(bmp, new Rectangle(0, 0, Width, Height));
                var doc = new System.Drawing.Printing.PrintDocument();
                doc.PrintPage += (s, ev) =>
                {
                    var mb    = ev.MarginBounds;
                    float r   = Math.Min((float)mb.Width / bmp.Width, (float)mb.Height / bmp.Height);
                    ev.Graphics.DrawImage(bmp, mb.X, mb.Y, (int)(bmp.Width * r), (int)(bmp.Height * r));
                };
                using (var pd = new PrintDialog { Document = doc })
                    if (pd.ShowDialog() == DialogResult.OK)
                        doc.Print();
            }
        }

        private string FindProjectRootContaining(string folderName)
        {
            var dir = new DirectoryInfo(Application.StartupPath);
            while (dir != null)
            {
                if (dir.GetDirectories(folderName).Any())
                    return dir.FullName;
                dir = dir.Parent;
            }
            throw new DirectoryNotFoundException($"Could not find folder '{folderName}'");
        }
    }
}
