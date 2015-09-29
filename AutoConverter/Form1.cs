using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace AutoConverter
{
    public partial class Form1 : Form
    {
        string _fileIn = "";
        string _fileOut = "";
        public Form1()
        {
            InitializeComponent();
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Enabled = false;
            label2.Text = DateTime.Now.ToString();
            string[] files = Directory.GetFiles(ConfigurationSettings.AppSettings["Input"].ToString().Trim());
            foreach (var item in files)
            {
                _fileIn = item;
                label1.Text = _fileIn;
                _fileOut = ConfigurationSettings.AppSettings["Output"].ToString().Trim() + Path.GetFileNameWithoutExtension(_fileIn);

                //if (!File.Exists(_fileOut+ ConfigurationSettings.AppSettings["Extention"].ToString().Trim()))
                //{
                    Process proc = new Process();
                    proc.StartInfo.FileName = Path.GetDirectoryName(Application.ExecutablePath) + "\\ffmpeg";

                    proc.StartInfo.Arguments = "-i " + "\"" + _fileIn + "\"" + "  -r 25 -b "
                                                + ConfigurationSettings.AppSettings["MaxBitrate"].ToString().Trim() 
                                                + "k -y  " + "\"" + _fileOut
                                                + ConfigurationSettings.AppSettings["Extention"].ToString().Trim() + "\"";

                    proc.StartInfo.RedirectStandardError = true;
                    proc.StartInfo.UseShellExecute = false;
                    proc.StartInfo.CreateNoWindow = true;
                    proc.EnableRaisingEvents = true;
                    proc.Exited += new EventHandler(myProcess_Exited);
                    proc.Start();
                    StreamReader reader = proc.StandardError;
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (richTextBox1.Lines.Length > 5)
                        {
                            richTextBox1.Text = "";
                        }

                        FindDuration(line);
                        richTextBox1.Text += (line) + " \n";
                        richTextBox1.SelectionStart = richTextBox1.Text.Length;
                        richTextBox1.ScrollToCaret();
                        Application.DoEvents();
                    }
                }
                try
                {
                    File.Delete(_fileIn);
                }
                catch
                {

                }
           // }
            timer1.Enabled = true;
        }
        private void myProcess_Exited(object sender, EventArgs e)
        {
        }
        protected void FindDuration(string Str)
        {
            try
            {
                string TimeCode = "";
                if (Str.Contains("Duration:"))
                {
                    TimeCode = Str.Substring(Str.IndexOf("Duration: "), 21).Replace("Duration: ", "").Trim();
                    string[] Times = TimeCode.Split('.')[0].Split(':');
                    double Frames = double.Parse(Times[0].ToString()) * (3600) * (25) +
                        double.Parse(Times[1].ToString()) * (60) * (25) +
                        double.Parse(Times[2].ToString()) * (25);
                    progressBar1.Maximum = int.Parse(Frames.ToString());


                }
                if (Str.Contains("time="))
                {
                    try
                    {
                        string CurTime = "";
                        CurTime = Str.Substring(Str.IndexOf("time="), 16).Replace("time=", "").Trim();
                        string[] CTimes = CurTime.Split('.')[0].Split(':');
                        double CurFrame = double.Parse(CTimes[0].ToString()) * (3600) * (25) +
                            double.Parse(CTimes[1].ToString()) * (60) * (25) +
                            double.Parse(CTimes[2].ToString()) * (25);
                        progressBar1.Value = int.Parse(CurFrame.ToString());
                        label3.Text = ((progressBar1.Value * 100) / progressBar1.Maximum).ToString() + "%";
                        Application.DoEvents();
                    }
                    catch
                    { }

                }
                if (Str.Contains("fps="))
                {
                    string Speed = "";
                    Speed = Str.Substring(Str.IndexOf("fps="), 8).Replace("fps=", "").Trim();
                    label4.Text = "Speed: " + (float.Parse(Speed) / 25).ToString() + " X ";
                    Application.DoEvents();
                }
            }
            catch { }
        }
        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
