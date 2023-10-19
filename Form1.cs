using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace ComprASS
{
    public partial class Form1 : Form
    {
        private string inputFilePath;
        private string outputFilePath;

        public Form1()
        {
            InitializeComponent();
        }

        private void OpenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Video Files|*.mp4;*.avi;*.mkv|All Files|*.*";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    inputFilePath = openFileDialog.FileName;
                }
            }
        }

        private void SaveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "MP4 Files|*.mp4|All Files|*.*";
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    outputFilePath = saveFileDialog.FileName;
                }
            }
        }

        private void SettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(inputFilePath) || string.IsNullOrEmpty(outputFilePath))
            {
                MessageBox.Show("Пожалуйста, выберите входной и выходной файлы.");
                return;
            }

            // Ускорение видео в 4 раза
            string accelerateArguments = $"-i \"{inputFilePath}\" -vf \"setpts=0.2*PTS\" -an \"{outputFilePath}\" -y";
            RunFFmpegCommand(accelerateArguments);

            // Получение длительности ускоренного видео
            string ffprobeArguments = $"-v error -select_streams v:0 -show_entries stream=duration -of default=noprint_wrappers=1:nokey=1 \"{outputFilePath}\"";
            double duration = GetVideoDuration(ffprobeArguments);

            // Обрезка последних 3/4 видео
            string trimArguments = $"-i \"{outputFilePath}\" -t {duration * 0.25} -c:v copy -c:a copy \"{outputFilePath}\" -y";
            RunFFmpegCommand(trimArguments);

            MessageBox.Show("Видео успешно ускорено и обрезано.");
        }

        private void RunFFmpegCommand(string arguments)
        {
            using (Process process = new Process())
            {
                process.StartInfo.FileName = "ffmpeg";
                process.StartInfo.Arguments = arguments;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.RedirectStandardOutput = true;

                process.Start();
                process.WaitForExit();
            }
        }

        private double GetVideoDuration(string arguments)
        {
            using (Process process = new Process())
            {
                process.StartInfo.FileName = "ffprobe";
                process.StartInfo.Arguments = arguments;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.RedirectStandardOutput = true;

                process.Start();
                string outputDuration = process.StandardOutput.ReadToEnd();
                process.WaitForExit();

                if (double.TryParse(outputDuration, out double duration))
                {
                    return duration;
                }

                return 0.0;
            }
        }
    }
}
