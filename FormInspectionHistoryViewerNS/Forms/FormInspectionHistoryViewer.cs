using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace FormInspectionHistoryViewerNS
{
    public partial class FormInspectionHistoryViewer : Form
    {
        private ListBox lstFiles;
        private TextBox txtSearch;
        private Label lblSearch;
        private DataGridView dgvPreview;
        private Button btnOpen;
        private Button btnRefresh;
        private Button btnOpenZipFolder;



        public FormInspectionHistoryViewer()
        {
            InitializeComponent();
            LoadFileList();
        }

 
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();

            this.lstFiles = new ListBox();
            this.txtSearch = new TextBox();
            this.lblSearch = new Label();
            this.dgvPreview = new DataGridView();
            this.btnOpen = new Button();
            this.btnRefresh = new Button();
            this.btnOpenZipFolder = new Button();

            this.SuspendLayout();

            // lblSearch
            this.lblSearch.AutoSize = true;
            this.lblSearch.Location = new System.Drawing.Point(20, 20);
            this.lblSearch.Text = "Search:";

            // txtSearch
            this.txtSearch.Location = new System.Drawing.Point(80, 18);
            this.txtSearch.Size = new System.Drawing.Size(200, 22);

            // lstFiles
            this.lstFiles.Location = new System.Drawing.Point(20, 60);
            this.lstFiles.Size = new System.Drawing.Size(260, 480);

            // dgvPreview
            this.dgvPreview.Location = new System.Drawing.Point(300, 60);
            this.dgvPreview.Size = new System.Drawing.Size(900, 480);
            this.dgvPreview.ReadOnly = true;
            this.dgvPreview.AllowUserToAddRows = false;
            this.dgvPreview.AllowUserToDeleteRows = false;

            // btnOpen
            this.btnOpen.Location = new System.Drawing.Point(20, 550);
            this.btnOpen.Size = new System.Drawing.Size(120, 30);
            this.btnOpen.Text = "Open File";

            // btnRefresh
            this.btnRefresh.Location = new System.Drawing.Point(160, 550);
            this.btnRefresh.Size = new System.Drawing.Size(120, 30);
            this.btnRefresh.Text = "Refresh";

            // btnOpenZipFolder
            this.btnOpenZipFolder.Location = new System.Drawing.Point(300, 550);
            this.btnOpenZipFolder.Size = new System.Drawing.Size(120, 30);
            this.btnOpenZipFolder.Text = "Open Archive";

            // FormInspectionHistoryViewer
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1200, 600);
            this.Controls.Add(this.lblSearch);
            this.Controls.Add(this.txtSearch);
            this.Controls.Add(this.lstFiles);
            this.Controls.Add(this.dgvPreview);
            this.Controls.Add(this.btnOpen);
            this.Controls.Add(this.btnRefresh);
            this.Controls.Add(this.btnOpenZipFolder);
            this.Name = "FormInspectionHistoryViewer";
            this.Text = "טבלת הסטוריה של כל הבדיקות";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.RightToLeftLayout = true;

            this.ResumeLayout(false);
            this.PerformLayout();
        }


        private List<string> _allFiles = new List<string>();

        private string GetSharedFolder()
        {

            string folder = GetSetting("SharedData", "InspectionLogs");
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            folder = @Path.GetFullPath(Path.Combine(baseDir, folder));

            return folder;
        }

        private void LoadFileList()
        {
            string folder = GetSharedFolder();

            Directory.CreateDirectory(folder);

            _allFiles = Directory.GetFiles(folder, "*.csv")
                                 .OrderByDescending(f => f)
                                 .ToList();

            lstFiles.Items.Clear();
            lstFiles.Items.AddRange(_allFiles.Select(Path.GetFileName).ToArray());
        }

        private void FilterFiles()
        {
            string term = (txtSearch.Text ?? string.Empty).Trim().ToLowerInvariant();

            var filtered = _allFiles.Where(f => Path.GetFileName(f).ToLower().Contains(term)).ToList();

            lstFiles.Items.Clear();
            lstFiles.Items.AddRange(filtered.Select(Path.GetFileName).ToArray()!);
        }

        private void LoadCsvPreview()
        {
            try
            {
                if (lstFiles.SelectedItem == null)
                    return;

                string folder = GetSetting("SharedData", "InspectionLogs");
                string? fileName = lstFiles.SelectedItem as string;
                if (string.IsNullOrEmpty(fileName))
                    return;

                string filePath = Path.Combine(folder, fileName);

                dgvPreview.Columns.Clear();
                dgvPreview.Rows.Clear();

                var lines = File.ReadAllLines(filePath, Encoding.UTF8);
                if (lines.Length == 0) return;

                var headers = lines[0].Split(',');
                foreach (var h in headers)
                    dgvPreview.Columns.Add(h, h);

                for (int i = 1; i < lines.Length; i++)
                    dgvPreview.Rows.Add(lines[i].Split(','));
            }
            catch (Exception e)
            {
                MessageBox.Show($"{e.Message}");
            }
        }


        private void OpenSelectedFile()
        {
            try
            {

                if (lstFiles.SelectedItem == null)
                    return;

                string? selectedFile = lstFiles.SelectedItem as string;
                if (string.IsNullOrEmpty(selectedFile))
                    return;

                string folder = GetSharedFolder();
                string filePath = Path.Combine(folder, selectedFile);
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                folder = @Path.GetFullPath(Path.Combine(baseDir, folder));



                var startInfo = new ProcessStartInfo
                {
                    FileName = selectedFile,
                    WorkingDirectory = folder,
                    UseShellExecute = true // Crucial for non-executable files in .NET Core+
                };


                using Process? process = Process.Start(startInfo);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while opening the selected file: {ex.Message}");
            }
        }

        private void OpenZipFolder()
        {
            string zipFolder = GetSharedFolder();
            Directory.CreateDirectory(zipFolder);
            using var _ = Process.Start(zipFolder);
        }

        private string GetSetting(string key, string defaultValue)
        {

            return ConfigurationManager.AppSettings[key] ?? defaultValue;
        }
    }
}
