using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Accord.Audio;
using Accord.Audio.Formats;
using Accord.DirectSound;
using Accord.Audio.Filters;
using Recorder.Recorder;
using Recorder.MFCC;

using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
using AForge.Math.Metrics;

namespace Recorder
{
    public partial class Form1 : Form
    {
        private AudioSignal signal = null;
        Sequence seq = null;

        private string path;

        private Encoder encoder;
        private Decoder decoder;

        private bool isRecorded;
        private bool isSaved;
        public Form1()
        {
            InitializeComponent();
            Name_box.ReadOnly = true;
            chart.SimpleMode = true;
            chart.AddWaveform("wave", Color.Green, 1, false);
            width_label.Visible = false;
            width_box.Visible = false;
            function_box.SelectedIndex = 0;
            updateButtons();
        }

        private void btnRecord_Click(object sender, EventArgs e)
        {
            isRecorded = true;
            this.encoder = new Encoder(source_NewFrame, source_AudioSourceError);
            this.encoder.Start();
            updateButtons();
        }

        /// <summary>
        ///   Plays the recorded audio stream.
        /// </summary>
        /// 
        private void btnPlay_Click(object sender, EventArgs e)
        {
            InitializeDecoder();
            // Configure the track bar so the cursor
            // can show the proper current position
            if (trackBar1.Value < this.decoder.frames)
                this.decoder.Seek(trackBar1.Value);
            trackBar1.Maximum = this.decoder.samples;
            this.decoder.Start();
            updateButtons();
        }

        private void InitializeDecoder()
        {
            if (isRecorded)
            {
                // First, we rewind the stream
                this.encoder.stream.Seek(0, SeekOrigin.Begin);
                this.decoder = new Decoder(this.encoder.stream, this.Handle, output_AudioOutputError, output_FramePlayingStarted, output_NewFrameRequested, output_PlayingFinished);
            }
            else
            {
                this.decoder = new Decoder(this.path, this.Handle, output_AudioOutputError, output_FramePlayingStarted, output_NewFrameRequested, output_PlayingFinished);
            }
        }

        /// <summary>
        ///   Stops recording or playing a stream.
        /// </summary>
        /// 
       
        /// <summary>
        ///   This callback will be called when there is some error with the audio 
        ///   source. It can be used to route exceptions so they don't compromise 
        ///   the audio processing pipeline.
        /// </summary>
        /// 
        private void source_AudioSourceError(object sender, AudioSourceErrorEventArgs e)
        {
            throw new Exception(e.Description);
        }

        /// <summary>
        ///   This method will be called whenever there is a new input audio frame 
        ///   to be processed. This would be the case for samples arriving at the 
        ///   computer's microphone
        /// </summary>
        /// 
        private void source_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            this.encoder.addNewFrame(eventArgs.Signal);
            updateWaveform(this.encoder.current, eventArgs.Signal.Length);
        }


        /// <summary>
        ///   This event will be triggered as soon as the audio starts playing in the 
        ///   computer speakers. It can be used to update the UI and to notify that soon
        ///   we will be requesting additional frames.
        /// </summary>
        /// 
        private void output_FramePlayingStarted(object sender, PlayFrameEventArgs e)
        {
            updateTrackbar(e.FrameIndex);

            if (e.FrameIndex + e.Count < this.decoder.frames)
            {
                int previous = this.decoder.Position;
                decoder.Seek(e.FrameIndex);

                Signal s = this.decoder.Decode(e.Count);
                decoder.Seek(previous);

                updateWaveform(s.ToFloat(), s.Length);
            }
        }

        /// <summary>
        ///   This event will be triggered when the output device finishes
        ///   playing the audio stream. Again we can use it to update the UI.
        /// </summary>
        /// 
        private void output_PlayingFinished(object sender, EventArgs e)
        {
            updateButtons();
            updateWaveform(new float[BaseRecorder.FRAME_SIZE], BaseRecorder.FRAME_SIZE);
        }

        /// <summary>
        ///   This event is triggered when the sound card needs more samples to be
        ///   played. When this happens, we have to feed it additional frames so it
        ///   can continue playing.
        /// </summary>
        /// 
        private void output_NewFrameRequested(object sender, NewFrameRequestedEventArgs e)
        {
            this.decoder.FillNewFrame(e);
        }


        void output_AudioOutputError(object sender, AudioOutputErrorEventArgs e)
        {
            throw new Exception(e.Description);
        }

        /// <summary>
        ///   Updates the audio display in the wave chart
        /// </summary>
        /// 
        private void updateWaveform(float[] samples, int length)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() =>
                {
                    chart.UpdateWaveform("wave", samples, length);
                }));
            }
            else
            {
                if (this.encoder != null) { chart.UpdateWaveform("wave", this.encoder.current, length); }
            }
        }

        /// <summary>
        ///   Updates the current position at the trackbar.
        /// </summary>
        /// 
        private void updateTrackbar(int value)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() =>
                {
                    trackBar1.Value = Math.Max(trackBar1.Minimum, Math.Min(trackBar1.Maximum, value));
                }));
            }
            else
            {
                trackBar1.Value = Math.Max(trackBar1.Minimum, Math.Min(trackBar1.Maximum, value));
            }
        }

        private void updateButtons()
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(updateButtons));
                return;
            }

            if (this.encoder != null && this.encoder.IsRunning())
            {
                
                btnIdentify.Enabled = false;
                btnPlay.Enabled = false;
                btnStop.Enabled = true;
                btnRecord.Enabled = false;
                trackBar1.Enabled = false;
            }
            else if (this.decoder != null && this.decoder.IsRunning())
            {
                
                btnIdentify.Enabled = false;
                btnPlay.Enabled = false;
                btnStop.Enabled = true;
                btnRecord.Enabled = false;
                trackBar1.Enabled = true;
            }
            else
            {
                
                btnIdentify.Enabled = seq != null;
                btnPlay.Enabled = this.path != null || this.encoder != null;//stream != null;
                btnStop.Enabled = false;
                btnRecord.Enabled = true;
                trackBar1.Enabled = this.decoder != null;
                trackBar1.Value = 0;
            }
        }

        private void MainFormFormClosed(object sender, FormClosedEventArgs e)
        {
            Stop();
        }

        private void updateTimer_Tick(object sender, EventArgs e)
        {
            if (this.encoder != null) { lbLength.Text = String.Format("Length: {0:00.00} sec.", this.encoder.duration / 1000.0); }
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog open = new OpenFileDialog();
            if (open.ShowDialog() == DialogResult.OK)
            {
                isRecorded = false;
                path = open.FileName;


                //Open the selected audio file

                signal = AudioOperations.OpenAudioFile(path);
                signal = AudioOperations.RemoveSilence(signal);
                seq = AudioOperations.ExtractFeatures(signal);
                for (int i = 0; i < seq.Frames.Length; i++)
                {
                    for (int j = 0; j < 13; j++)
                    {

                        if (double.IsNaN(seq.Frames[i].Features[j]) || double.IsInfinity(seq.Frames[i].Features[j]))
                            throw new Exception("NaN");
                    }
                }
                updateButtons();
                isSaved = true;
            }
        }

        private void Stop()
        {
            if (this.encoder != null) { this.encoder.Stop(); }
            if (this.decoder != null) { this.decoder.Stop(); }
        } 
        private void button1_Click(object sender, EventArgs e)
        {
            GUI mainForm = new GUI();
            mainForm.Show();
            this.Hide();
        }
        private MFCCFrame[] ParseTemplate(string templateString)
        {
            var frames = new List<MFCCFrame>();

            // Split the whole string into frame strings using ';'
            string[] frameStrings = templateString.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string frame in frameStrings)
            {
                // Split each frame string into 13 MFCC values using ','
                string[] coef = frame.Split(',');

                var mfcc = new MFCCFrame();
                mfcc.Features = coef.Select(double.Parse).ToArray();  // requires using System.Linq;
                frames.Add(mfcc);
            }

            return frames.ToArray();
        }
        private void btnIdentify_Click(object sender, EventArgs e)
        {
            try
            {
                if (seq == null)
                {
                    MessageBox.Show("Please record or load an audio first.");
                    return;
                }
                if ((function_box.Text == "Pruning(Path)" || function_box.Text == "Pruning(Cost)") && string.IsNullOrWhiteSpace(width_box.Text))
                {
                    MessageBox.Show("Please add the width first.");
                    return;
                }
                var inputFrames = seq.Frames;
                string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string projectRoot = Directory.GetParent(baseDirectory).Parent.Parent.FullName;
                string dbPath = Path.Combine(projectRoot, "GUI", "voice_enrollment_data.mdf");

                string connectionString = $@"Data Source=(localdb)\MSSQLLocalDB;AttachDbFilename={dbPath};Integrated Security=True;Connect Timeout=30;";

                var templates = new Dictionary<string, MFCCFrame[]>();

                using (var conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string sql = "SELECT user_name, template_sequence FROM voice_templates";
                    using (var cmd = new SqlCommand(sql, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string user = reader.GetString(0);
                            string templateString = reader.GetString(1);
                            templates[user] = ParseTemplate(templateString);
                        }
                    }
                }

                string bestMatch = null;
                double minDistance = double.PositiveInfinity;
                double distance;               
                foreach (var kvp in templates)
                {
                    string user = kvp.Key;
                    MFCCFrame[] template = kvp.Value;
                    if(function_box.Text == "DTW")
                    {
                        distance = DTW.MatchingVoices(inputFrames, template);
                    }
                    else if(function_box.Text == "DTW(Time Sync)")
                    {                        
                        // bestMatch = DTW.MatchingVoicesTimeSync(inputFrames, templates);
                        distance = minDistance;
                    }
                    else if (function_box.Text == "Pruning(Cost)")
                    {
                        distance = Prunning.PruningLimitngPathCost(inputFrames, template , Convert.ToInt32(width_box.Text));
                    }
                    else if (function_box.Text == "Pruning(Path)")
                    {
                        distance = Prunning.PruningLimitngSearchPath(inputFrames, template, Convert.ToInt32(width_box.Text));
                    }
                    else
                    {
                        //Beam(Time sync) Code Here 
                        distance = DTW.MatchingVoices(inputFrames, template);
                    }
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        bestMatch = user;
                    }
                }

                Name_box.Text = bestMatch ?? "No match found";
            }
            catch (Exception ex)
            {
                Name_box.Text = "Error";
                MessageBox.Show("Identification failed: " + ex.Message);

            }
        }
        private void btnRecord_Click_1(object sender, EventArgs e)
        {
            isRecorded = true;
            this.encoder = new Encoder(source_NewFrame, source_AudioSourceError);
            this.encoder.Start();
            updateButtons();
        }

        private void btnPlay_Click_1(object sender, EventArgs e)
        {
            InitializeDecoder();
            // Configure the track bar so the cursor
            // can show the proper current position
            if (trackBar1.Value < this.decoder.frames)
                this.decoder.Seek(trackBar1.Value);
            trackBar1.Maximum = this.decoder.samples;
            this.decoder.Start();
            updateButtons();
        }


        private void btnStop_Click_1(object sender, EventArgs e)
        {
            Stop();
            if (encoder?.stream != null)
            {
                encoder.stream.Seek(0, SeekOrigin.Begin);
                path = Path.GetTempFileName() + ".wav";

                using (FileStream fs = new FileStream(path, FileMode.Create))
                {
                    encoder.Save(fs);
                }

                signal = AudioOperations.OpenAudioFile(path);
                signal = AudioOperations.RemoveSilence(signal);
                seq = AudioOperations.ExtractFeatures(signal);
            }
            updateButtons();
            updateWaveform(new float[BaseRecorder.FRAME_SIZE], BaseRecorder.FRAME_SIZE);

        }

        private void loadTrain1ToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string projectRoot = Directory.GetParent(baseDirectory).Parent.Parent.FullName;
            string dbPath = Path.Combine(projectRoot, "GUI", "voice_enrollment_data.mdf");
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.ShowDialog();

            var hobba = TestcaseLoader.LoadTestcase1Testing(fileDialog.FileName);           
            string connectionString = $@"Data Source=(localdb)\MSSQLLocalDB;AttachDbFilename={dbPath};Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
            var templates = new Dictionary<string, MFCCFrame[]>();
            List<string> bestMatches= new List<string>();
            
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string sql = "SELECT user_name, template_sequence FROM voice_templates";
                using (var cmd = new SqlCommand(sql, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string user = reader.GetString(0);
                        string templateString = reader.GetString(1);
                        //if (hobba.Any(h => h.UserName == user))
                        
                        templates[user] = ParseTemplate(templateString);
                        
                    }
                }
            }
            for (int i = 0; i < hobba.Count; i++)
            {
                //var hobba[i] = hobba[i];
                for (int k = 0; k < hobba[i].UserTemplates.Count; k++)
                {
                    //Console.WriteLine("In Function = "+hobba[i].UserTemplates.Count);
                    Console.WriteLine($"Test #{i}-{k} for User {hobba[i].UserName}");

                    seq = AudioOperations.ExtractFeatures(hobba[i].UserTemplates[k]);
                    //if (seq.Frames.Any(f => f.Features.Any(feat => double.IsNaN(feat) || double.IsInfinity(feat))))
                                           
                        double min = double.PositiveInfinity;
                        string bestMatch = "";
                        foreach (var kvp in templates)
                        {
                            string templateUser = kvp.Key;
                            MFCCFrame[] templateSeq = kvp.Value;

                            double distance = DTW.MatchingVoices(seq.Frames, templateSeq);                            
                            if (distance < min)
                            {
                                min = distance;
                                bestMatch = templateUser;
                            }
                        }
                        bestMatches.Add(bestMatch);                       
                        Console.WriteLine($"Test sample {i}-{k} best matched: {bestMatch}");
                    }
                
            }    
            double acc = TestcaseLoader.CheckTestcaseAccuracy(hobba,bestMatches);
            Console.WriteLine("Accuracy = " + acc);
        }

        private void function_box_SelectedIndexChanged(object sender, EventArgs e)
        {
            width_label.Visible = false;
            width_box.Visible = false;
            if(function_box.Text == "Pruning(Path)" || function_box.Text == "Pruning(Cost)")
            {
                width_label.Visible = true;
                width_box.Visible = true;
            }
        }
    }
}
