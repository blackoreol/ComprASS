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

            string ffmpegCommand = "ffmpeg";

            // Ускорение видео в 4 раза
            string accelerateArguments = $"-i \"{inputFilePath}\" -vf \"setpts=0.2*PTS\" -an \"{outputFilePath}\" -y";

            ProcessStartInfo accelerateStartInfo = new ProcessStartInfo
            {
                FileName = ffmpegCommand,
                Arguments = accelerateArguments,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (Process accelerateProcess = new Process { StartInfo = accelerateStartInfo })
            {
                accelerateProcess.Start();
                accelerateProcess.WaitForExit();
            }

            // Получение длительности ускоренного видео
            string ffprobeCommand = "ffprobe";
            string ffprobeArguments = $"-v error -select_streams v:0 -show_entries stream=duration -of default=noprint_wrappers=1:nokey=1 \"{outputFilePath}\"";

            ProcessStartInfo ffprobeStartInfo = new ProcessStartInfo
            {
                FileName = ffprobeCommand,
                Arguments = ffprobeArguments,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (Process ffprobeProcess = new Process { StartInfo = ffprobeStartInfo })
            {
                ffprobeProcess.Start();
                string outputDuration = ffprobeProcess.StandardOutput.ReadToEnd();
                ffprobeProcess.WaitForExit();

                if (double.TryParse(outputDuration, out double duration))
                {
                    // Обрезка последних 3/4 видео
                    string trimArguments = $"-i \"{outputFilePath}\" -t {duration * 0.2} -c:v copy -c:a copy \"{outputFilePath}\" -y";

                    ProcessStartInfo trimStartInfo = new ProcessStartInfo
                    {
                        FileName = ffmpegCommand,
                        Arguments = trimArguments,
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    using (Process trimProcess = new Process { StartInfo = trimStartInfo })
                    {
                        trimProcess.Start();
                        trimProcess.WaitForExit();
                    }
                }
            }

            MessageBox.Show("Видео успешно ускорено и обрезано.");
        }
    }
}
