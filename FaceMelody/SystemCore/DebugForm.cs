using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using FaceMelody.SystemCore;

namespace FaceMelody
{
    public partial class DebugForm : Form
    {
        EmotionTools.VideoEmotionResultPer100MiliSec last;
        EmotionTools et = new EmotionTools();
        public DebugForm()
        {
            InitializeComponent();
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            ////SystemCore.AudioTools a = new SystemCore.AudioTools();
            //VideoTools vt = new VideoTools();
            //VideoTools.BaseVideo bv = vt.video_reader("_no_sync_test_ver_1.avi");
            //textBox1.Text = bv.audio.SampleRate.ToString();

            //EmotionTools et = new EmotionTools();
            //await et.get_emotion_from_image_file("test_2_no_sync.jpg");
            ////while (et.img_processing) ;
            //for (int i = 0; i < et.img_last_emotions.Length; i++)
            //{
            //    textBox1.Text += "Happiness:";
            //    textBox1.Text += et.img_last_emotions[i].Scores.Happiness.ToString();
            //    textBox1.Text += "\r\nFear:";
            //    textBox1.Text += et.img_last_emotions[i].Scores.Fear.ToString();
            //    textBox1.Text += "\r\nSadness:";
            //    textBox1.Text += et.img_last_emotions[i].Scores.Sadness.ToString();
            //    textBox1.Text += "\r\nAnger:";
            //    textBox1.Text += et.img_last_emotions[i].Scores.Anger.ToString();
            //    textBox1.Text += "\r\nContempt:";
            //    textBox1.Text += et.img_last_emotions[i].Scores.Contempt.ToString();
            //    textBox1.Text += "\r\nDisgust:";
            //    textBox1.Text += et.img_last_emotions[i].Scores.Disgust.ToString();
            //    textBox1.Text += "\r\nNeutral:";
            //    textBox1.Text += et.img_last_emotions[i].Scores.Neutral.ToString();
            //    textBox1.Text += "\r\nSurprise:";
            //    textBox1.Text += et.img_last_emotions[i].Scores.Surprise.ToString();
            //}


            
            //EmotionTools.VideoEmotionResultPer100MiliSec video_result = 
            //    await et.get_emotion_from_video_file("_no_sync_test_ver_2.mp4");
            //last = video_result;
            //for (int i = 0; i < video_result.scores.Count; i++)
            //{
            //    if (i % 10 == 0)
            //        textBox1.Text += "\r\n" + (i / 10).ToString() + ":";
            //    if (video_result.has_face[i] == false)
            //        textBox1.Text += "\r\nNONE!";
            //    else
            //        textBox1.Text += "\r\n" + Get_Best_Emotion(video_result.scores[i]);
            //}

        }

        private string Get_Best_Emotion(Microsoft.ProjectOxford.Emotion.Contract.Scores sc)
        {
            float best = sc.Anger;
            string ret = "Anger";
            if (best < sc.Contempt)
            {
                best = sc.Contempt;
                ret = "Contempt";
            }
            if (best < sc.Disgust)
            {
                best = sc.Disgust;
                ret = "Disgust";
            }
            if (best < sc.Fear)
            {
                best = sc.Fear;
                ret = "Fear";
            }

            if (best < sc.Happiness)
            {
                best = sc.Happiness;
                ret = "Happiness";
            }
            if (best < sc.Neutral)
            {
                best = sc.Neutral;
                ret = "Neutral";
            }
            if (best < sc.Sadness)
            {
                best = sc.Sadness;
                ret = "Sadness";
            }
            if (best < sc.Surprise)
            {
                best = sc.Surprise;
                ret = "Surprise";
            }
            return ret;
        }
    }
}
