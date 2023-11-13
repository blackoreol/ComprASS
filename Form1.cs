using ComprASS.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ComprASS
{
    public partial class Form1 : Form
    {
        private string outLenght;
        private string formattedDur;
        private string debugDur;
        private string inputFilePath;
        private string outputFilePath;
        private readonly List<Process> activeProcesses = new List<Process>();
        private double speed = 1;
        public Form1()
        {
            InitializeComponent();
        }
        private void Speed2toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            speed = 2.0;
        }
        private void Speed4toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            speed = 4.0;
        }
        private void Speed10toolStripMenuItem4_Click(object sender, EventArgs e)
        {
            speed = 10.0;
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
                activeProcesses.Add(process);
                await Task.Run(() => process.Start());
                await Task.Run(() => process.WaitForExit());
                activeProcesses.Remove(process);
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
                activeProcesses.Add(process);
                await Task.Run(() => process.Start());
                string outputDuration = await Task.Run(() => process.StandardOutput.ReadToEnd());
                await Task.Run(() => process.WaitForExit());
                activeProcesses.Remove(process);
                debugDur = outputDuration;
                if (double.TryParse(outputDuration, out double duration))
                {
                    return duration;   
                }

                return 0.0;
                
            }
        }

        private async void LETSGOToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(inputFilePath) || string.IsNullOrEmpty(outputFilePath))
            {
                MessageBox.Show("Пожалуйста, выберите входной и выходной файлы.");
                return;
            }
            label1.Visible = false;
            Label2.Visible = false;
            label3.Visible = false;
            label4.Visible = false;
            tooltipStatus.Visible = false;
            flowLayoutPanel1.Visible = false;
            trackBar1.Visible = false;
            pictureBox1.Image = Resources.billy_herrington_flex;
            string ffmpegPath = Path.Combine(Application.StartupPath, "ffmpeg", "ffmpeg.exe");
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
            pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            tooltipStatus.Text = "Успешно";
            tooltipStatus.BackColor = System.Drawing.Color.Green;
            tooltipStatus.Visible = true;
            int dotIndex = debugDur.IndexOf('.');
            if (dotIndex != -1)
            {
                int endIndex = dotIndex + 3; // Индекс, следующий за вторым знаком после точки

                // Проверяем, что endIndex не превышает длину строки
                if (endIndex < debugDur.Length)
                {
                    formattedDur = debugDur.Substring(0, endIndex);
                    flowLayoutPanel1.Visible = true;
                }
            }
            double length = new System.IO.FileInfo(outputFilePath).Length/1000; // считаем в киллобайтах
            if (length < 1000)
                outLenght = length.ToString()+" кб";
            else
            {
                length /= 1000;
                outLenght = length.ToString() + " мб";
            }
            label3.Visible = true;
            label4.Visible = true;
            label3.Text = ("Длина на выходе: " + formattedDur + " сек");
            label4.Text = ("Вес на выходе: " + outLenght);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Проверяем, есть ли активные процессы ffmpeg
            if (activeProcesses.Count > 0)
            {
                // Завершаем все активные процессы ffmpeg
                foreach (var process in activeProcesses)
                {
                    if (!process.HasExited)
                    {
                        process.Kill(); // Принудительно завершаем процесс
                        GC.Collect();
                    }
                }
            }
        }

        private void Speed2ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            trackBar1.Visible = true;
            label1.Visible = true;
            Label2.Visible = true;

        }

        private void TrackBar1_ValueChanged(object sender, EventArgs e)
        {
            speed = trackBar1.Value;
            Label2.Text = speed.ToString();
        }
    }
}
