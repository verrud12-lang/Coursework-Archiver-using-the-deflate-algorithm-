namespace Archiver
{
    public partial class Form1 : Form
    {
        private string selectedFilePath = string.Empty;

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Title = "Выберите файл";
                openFileDialog.Filter = "Все файлы (*.*)|*.*";
                openFileDialog.FilterIndex = 1;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    selectedFilePath = openFileDialog.FileName;
                    MessageBox.Show($"Файл выбран успешно!\nПуть: {selectedFilePath}",
                        "Файл выбран",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    changeForm();
                }
                else
                {
                    return;
                }
            }
        }

        private void changeForm()
        {
            if (!string.IsNullOrEmpty(selectedFilePath))
            {
                Form2 newForm2 = new Form2(selectedFilePath);
                newForm2.Show();
                this.Hide();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog folderDialog = new FolderBrowserDialog())
            {
                folderDialog.Description = "Выберите директорию";
                folderDialog.ShowNewFolderButton = true;

                if (folderDialog.ShowDialog() == DialogResult.OK)
                {
                    selectedFilePath = folderDialog.SelectedPath;
                    MessageBox.Show($"Директория выбрана успешно!\nПуть: {selectedFilePath}",
                        "Директория выбрана",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    changeForm();
                }
                else
                {
                    return;
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}