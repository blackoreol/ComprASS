﻿using ComprASS.Properties;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
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
        public int speed;
        private void Speed2toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            speed = 2;
        }

        private void Speed4toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            speed = 4;
        }

        private void Speed10toolStripMenuItem4_Click(object sender, EventArgs e)
        {
            speed = 10;
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
        async Task RunFFmpegCommand(string ffmpegPath, string arguments)
        {
            using (Process process = new Process())
            {
                process.StartInfo.FileName = ffmpegPath;
                process.StartInfo.Arguments = arguments;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.RedirectStandardOutput = true;

                await Task.Run(() => process.Start());
                await Task.Run(() => process.WaitForExit());
            }
        }

        async Task<double> GetVideoDuration(string ffprobePath, string arguments)
        {
            using (Process process = new Process())
            {
                process.StartInfo.FileName = ffprobePath;
                process.StartInfo.Arguments = arguments;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.RedirectStandardOutput = true;

                await Task.Run(() => process.Start());
                string outputDuration = await Task.Run(() => process.StandardOutput.ReadToEnd());
                await Task.Run(() => process.WaitForExit());

                if (double.TryParse(outputDuration, out double duration))
                {
                    return duration;
                }

                return 0.0;
            }
        }

        private async void LETSGOToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.DoEvents();
            if (string.IsNullOrEmpty(inputFilePath) || string.IsNullOrEmpty(outputFilePath))
            {
                MessageBox.Show("Пожалуйста, выберите входной и выходной файлы.");
                return;
            }
            pictureBox1.Image = Resources.billy_herrington_flex;
            // Путь к исполняемому файлу ffmpeg
            string ffmpegPath = Path.Combine(Application.StartupPath, "ffmpeg", "ffmpeg.exe");
            // Путь к исполняемому файлу ffprobe
            string ffprobePath = Path.Combine(Application.StartupPath, "ffmpeg", "ffprobe.exe");
            // Ускорение видео
            string accelerateArguments = $"-i \"{inputFilePath}\" -vf \"setpts=PTS/{speed}\" -an \"{outputFilePath}\" -y";
            await RunFFmpegCommand(ffmpegPath, accelerateArguments);
            // Получение длительности ускоренного видео
            string ffprobeArguments = $"-v error -select_streams v:0 -show_entries stream=duration -of default=noprint_wrappers=1:nokey=1 \"{outputFilePath}\"";
            double duration = await GetVideoDuration(ffprobePath, ffprobeArguments);
            // Обрезка
            string trimArguments = $"-i \"{outputFilePath}\" -t {duration / speed} -c:v copy -c:a copy \"{outputFilePath}\" -y";
            await RunFFmpegCommand(ffmpegPath, trimArguments);
            pictureBox1.Image = Resources.pngwing_com;
            MessageBox.Show("Успех");
        }

    }
}
