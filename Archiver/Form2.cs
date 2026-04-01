using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Archiver
{
    public partial class Form2 : Form
    {
        private string selectedFilePath;

        public Form2(string selectedFilePath)
        {
            InitializeComponent();
            this.selectedFilePath = selectedFilePath;

            addFormData(selectedFilePath);
        }

        private void setFileData(string selectedFilePath)
        {
            try
            {
                FileInfo fileInfo = new FileInfo(selectedFilePath);
                label2.Text = fileInfo.Name;
                long size = fileInfo.Length;
                label6.Text = FormatFileSize(size);
                label4.Text = selectedFilePath;

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void addFormData(string selectedPath)
        {
            if (Directory.Exists(selectedFilePath))
            {
                SetFolderData(selectedFilePath);
            }
            else if (File.Exists(selectedFilePath))
            {
                setFileData(selectedFilePath);
            }
            else
            {
                MessageBox.Show($"Путь не существует.\nПуть: {selectedFilePath}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private string FormatFileSize(long bytes)
        {
            string[] suffixes = { "байт", "KB", "MB", "GB", "TB" };
            int counter = 0;
            double fileSize = bytes;

            while (fileSize >= 1024 && counter < suffixes.Length - 1)
            {
                fileSize /= 1024;
                counter++;
            }

            return $"{fileSize:0.##} {suffixes[counter]}";
        }

        private void Form2_Load(object sender, EventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {
            Form1 newForm1 = new Form1();
            newForm1.Show();
            this.Hide();
        }

        private void SetFolderData(string selectedFolderPath)
        {
            try
            {
                DirectoryInfo dirInfo = new DirectoryInfo(selectedFolderPath);

                label2.Text = dirInfo.Name;
                label4.Text = selectedFolderPath;

                FileInfo[] files = dirInfo.GetFiles("*", SearchOption.AllDirectories);
                DirectoryInfo[] directories = dirInfo.GetDirectories("*", SearchOption.AllDirectories);

                int fileCount = files.Length;
                int folderCount = directories.Length;

                long totalSize = 0;
                foreach (FileInfo file in files)
                {
                    try
                    {
                        totalSize += file.Length;
                    }
                    catch (UnauthorizedAccessException)
                    {
                        Console.WriteLine($"Нет доступа к файлу {file.Name}");
                    }
                }

                label6.Text = FormatFileSize(totalSize);
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("Нет доступа к некоторым файлам или папкам", "Предупреждение",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                SetBasicFolderInfo(selectedFolderPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SetBasicFolderInfo(string folderPath)
        {
            try
            {
                DirectoryInfo dirInfo = new DirectoryInfo(folderPath);
                label2.Text = dirInfo.Name;
                label4.Text = folderPath;
                label6.Text = "Информация ограничена";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                if (!File.Exists(selectedFilePath) || !selectedFilePath.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show("Выбранный файл не является архивом ZIP", "Ошибка",
                                  MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                using (FolderBrowserDialog folderDialog = new FolderBrowserDialog())
                {
                    folderDialog.Description = "Выберите папку для распаковки архива";
                    folderDialog.ShowNewFolderButton = true;

                    folderDialog.SelectedPath = Path.GetDirectoryName(selectedFilePath);

                    if (folderDialog.ShowDialog() == DialogResult.OK)
                    {
                        string extractPath = folderDialog.SelectedPath;

                        ProgressForm progressForm = new ProgressForm();
                        progressForm.Text = "Распаковка архива";
                        progressForm.Show();
                        Application.DoEvents();

                        try
                        {
                            using (var archive = System.IO.Compression.ZipFile.OpenRead(selectedFilePath))
                            {
                                int totalEntries = archive.Entries.Count;
                                int processed = 0;

                                foreach (var entry in archive.Entries)
                                {
                                    processed++;
                                    int progress = (processed * 100) / totalEntries;

                                    progressForm.UpdateProgress(progress,
                                        $"Распаковка: {entry.FullName}");

                                    string destinationPath = Path.Combine(extractPath, entry.FullName);

                                    if (entry.FullName.EndsWith("/") || entry.FullName.EndsWith("\\"))
                                    {
                                        Directory.CreateDirectory(destinationPath);
                                    }
                                    else
                                    {
                                        string parentDir = Path.GetDirectoryName(destinationPath);
                                        if (!string.IsNullOrEmpty(parentDir) && !Directory.Exists(parentDir))
                                        {
                                            Directory.CreateDirectory(parentDir);
                                        }

                                        entry.ExtractToFile(destinationPath, true);
                                    }
                                }
                            }

                            progressForm.UpdateProgress(100, "Распаковка завершена!");

                            MessageBox.Show($"Архив успешно распакован в:\n{extractPath}", "Успех",
                                          MessageBoxButtons.OK, MessageBoxIcon.Information);

                            this.selectedFilePath = extractPath;
                            addFormData(selectedFilePath);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Ошибка при распаковке архива: {ex.Message}", "Ошибка",
                                          MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        finally
                        {
                            progressForm.Close();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при распаковке архива: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            CreateArchive(true);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            CreateArchive(false);
        }

        private void CreateArchive(bool compress)
        {
            try
            {
                using (SaveFileDialog saveDialog = new SaveFileDialog())
                {
                    string defaultName = Path.GetFileNameWithoutExtension(selectedFilePath);
                    if (Directory.Exists(selectedFilePath))
                    {
                        defaultName = new DirectoryInfo(selectedFilePath).Name;
                    }

                    saveDialog.Filter = "ZIP архивы (*.zip)|*.zip";
                    saveDialog.FileName = $"{defaultName}.zip";
                    saveDialog.DefaultExt = "zip";
                    saveDialog.AddExtension = true;

                    if (Directory.Exists(selectedFilePath))
                    {
                        saveDialog.InitialDirectory = Path.GetDirectoryName(selectedFilePath);
                    }
                    else if (File.Exists(selectedFilePath))
                    {
                        saveDialog.InitialDirectory = Path.GetDirectoryName(selectedFilePath);
                    }

                    if (saveDialog.ShowDialog() == DialogResult.OK)
                    {
                        string archivePath = saveDialog.FileName;

                        if (File.Exists(archivePath))
                        {
                            DialogResult result = MessageBox.Show(
                                $"Файл '{Path.GetFileName(archivePath)}' уже существует.\n" +
                                "Заменить его?",
                                "Подтверждение замены",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Question);

                            if (result == DialogResult.No)
                            {
                                return;
                            }
                        }

                        ProgressForm progressForm = new ProgressForm();
                        progressForm.Text = compress ? "Архивация с сжатием" : "Архивация без сжатия";
                        progressForm.Show();
                        Application.DoEvents();

                        try
                        {
                            using (FileStream zipToOpen = new FileStream(archivePath, FileMode.Create))
                            {
                                using (var archive = new System.IO.Compression.ZipArchive(zipToOpen,
                                       System.IO.Compression.ZipArchiveMode.Create))
                                {
                                    List<string> filesToArchive = new List<string>();

                                    if (Directory.Exists(selectedFilePath))
                                    {
                                        filesToArchive.AddRange(Directory.GetFiles(selectedFilePath, "*",
                                            SearchOption.AllDirectories));
                                    }
                                    else if (File.Exists(selectedFilePath))
                                    {
                                        filesToArchive.Add(selectedFilePath);
                                    }

                                    if (filesToArchive.Count == 0)
                                    {
                                        MessageBox.Show("Нет элементов для архивации", "Предупреждение",
                                                      MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                        return;
                                    }

                                    int totalFiles = filesToArchive.Count;
                                    int processed = 0;

                                    foreach (string file in filesToArchive)
                                    {
                                        processed++;
                                        int progress = (processed * 100) / totalFiles;

                                        string relativePath;
                                        if (Directory.Exists(selectedFilePath))
                                        {
                                            relativePath = GetRelativePath(file, selectedFilePath);
                                        }
                                        else
                                        {
                                            relativePath = Path.GetFileName(file);
                                        }

                                        progressForm.UpdateProgress(progress,
                                            $"Архивация: {relativePath}");

                                        var compressionLevel = compress ?
                                            System.IO.Compression.CompressionLevel.Optimal :
                                            System.IO.Compression.CompressionLevel.NoCompression;

                                        var entry = archive.CreateEntry(relativePath, compressionLevel);

                                        using (var fileStream = File.OpenRead(file))
                                        using (var entryStream = entry.Open())
                                        {
                                            fileStream.CopyTo(entryStream);
                                        }
                                    }

                                    progressForm.UpdateProgress(100, "Архивация завершена!");

                                    string compressionType = compress ? "со сжатием" : "без сжатия";
                                    MessageBox.Show($"Архив успешно создан {compressionType}!\nПуть: {archivePath}", "Успех",
                                                  MessageBoxButtons.OK, MessageBoxIcon.Information);

                                    this.selectedFilePath = archivePath;
                                    addFormData(selectedFilePath);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Ошибка при создании архива: {ex.Message}", "Ошибка",
                                          MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        finally
                        {
                            progressForm.Close();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при создании архива: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public string IncrementNumberInParentheses(string input)
        {
            if (input.EndsWith(")"))
            {
                int lastOpenParen = input.LastIndexOf('(');
                if (lastOpenParen != -1 && lastOpenParen < input.Length - 2)
                {
                    string numberPart = input.Substring(lastOpenParen + 1, input.Length - lastOpenParen - 2);

                    if (int.TryParse(numberPart, out int number))
                    {
                        number++;

                        return input.Substring(0, lastOpenParen + 1) + number.ToString() + ")";
                    }
                }
            }

            return input + "(1)";
        }

        private string GetRelativePath(string fullPath, string basePath)
        {
            Uri fullUri = new Uri(fullPath);
            Uri baseUri = new Uri(basePath + Path.DirectorySeparatorChar);

            Uri relativeUri = baseUri.MakeRelativeUri(fullUri);
            string relativePath = Uri.UnescapeDataString(relativeUri.ToString());

            relativePath = relativePath.Replace('/', Path.DirectorySeparatorChar);

            return relativePath;
        }
    }
}