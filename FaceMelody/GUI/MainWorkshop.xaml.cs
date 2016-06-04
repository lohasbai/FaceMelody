using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;
using System.IO;

using FaceMelody.SystemCore;

namespace FaceMelody
{
    /// <summary>
    /// MainWorkshop.xaml 的交互逻辑
    /// </summary>
    public partial class MainWorkshop : Window
    {
        private TimeLineCore timeLine;
        MediaPlayer soundPlayer;

        public double fullTimeLength;
        public double videoHeight;
        public double waveHeight;
        public double waveWidth;

        public string[] materialNameList;
        public int materialNum;
        public int[] trackIndexList;
        public int curAddTrack;
        public int curEditTrack;

        public double videoLength;
        public double maxVideoHeight;
        public double maxVideoWidth;
        public double originVideoHeight;
        public double originVideoWidth;
        public double actualVideoHeight;
        public double actualVideoWidth;
        public System.Timers.Timer drawTimer;

        public double curX;

        public bool isSelect;
        public bool isArea;
        public bool isEffect;
        public bool isDelete;

        public bool isPlay;
        public bool isVideoRead;

        public bool isDrawing;
        public double startX;
        public double endX;
        public double startY;

        #region System
        public MainWorkshop()
        {
            InitializeComponent();

            timeLine = new TimeLineCore(ProcessReport);

            fullTimeLength = 15*8.7*1000;
            videoHeight = 15;
            waveHeight = 10;
            waveWidth = 100.0/15.0/10.0;

            isPlay = false;
            isVideoRead = false;
            videoLength = 0;
            maxVideoHeight = 353;
            maxVideoWidth = 576;
            originVideoHeight = 0;
            originVideoWidth = 0;
            actualVideoHeight = 0;
            actualVideoWidth = 0;

            materialNameList = new string[15];
            materialNum = 0;
            trackIndexList = new int[3] { 0, 0, 0 };
            curAddTrack = 0;
            curEditTrack = 0;

            curX = 0;
            DrawSelectBar();

            isSelect = false;
            isArea = false;
            isEffect = false;
            isDelete = false;

            isDrawing = false;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            timeLine.clear_all();
        }
        #endregion

        #region Menu

        private async void LoadVideo_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog();

            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string filePath = openFileDialog.FileName;

                if (filePath != "" || filePath != null)
                {
                    this.VideoPlayer.Source = new Uri(filePath);
                    this.VideoPlayer.Position = TimeSpan.FromMilliseconds(100);
                    this.VideoPlayer.Play();
                    Thread.Sleep(50);
                    this.VideoPlayer.Pause();

                    isVideoRead = await timeLine.load_to_video_track(filePath);

                    curAddTrack = 1;
                    DrawWave(timeLine.audio_track[curAddTrack - 1].LVoice, 
                        timeLine.audio_track[curAddTrack - 1].SampleRate, curAddTrack);

                    if (isVideoRead)
                    {
                        MessageBox.Show("视频读取成功");
                    }
                    else
                    {
                        MessageBox.Show("视频读取失败");
                    }
                }
            }
        }

        private void LoadVoice_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog();

            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string filePath = openFileDialog.FileName;
                if (filePath != "" || filePath != null)
                {
                    LoadVoice(filePath);
                }
            }
        }

        private async void Export_Click(object sender, RoutedEventArgs e)
        {
            await timeLine.save_all_track_to_file("mixTest.mp4");
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            timeLine.clear_all();
            this.Close();
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("作者：\n" + "    自36  刘  柏 \n" + "    自32  朱天奕");
        }

        #endregion


        #region Voice

        private void LoadVoice(string filePath)
        {
            Material_StackPanel.Height += 50;

            string fileName = filePath.Substring(filePath.LastIndexOf("\\") + 1, (filePath.LastIndexOf(".") -
                filePath.LastIndexOf("\\") - 1));

            Label List_Label = new Label
            {
                Name = "_"+materialNum.ToString()+"_Material",
                Content = fileName,
                FontSize = 12,
                VerticalContentAlignment = VerticalAlignment.Center,
                Background = new SolidColorBrush(Color.FromArgb(100, 34, 33, 37)),
                Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255)),
                BorderBrush = new SolidColorBrush(Color.FromArgb(0, 255, 255, 255)),
                BorderThickness = new Thickness(1),
                Height = 50,
                Width = 192,
            };

            materialNameList[materialNum] = filePath;
            materialNum++;

            List_Label.MouseDown += new MouseButtonEventHandler(ChooseVoice_Click);
            Material_StackPanel.Children.Add(List_Label);
        }

        private void ChooseVoice_Click(object sender, RoutedEventArgs e)
        {
            Label List_Label = sender as Label;
            string sourceName = List_Label.Name;
            int curMaterial = Convert.ToInt16(sourceName.Substring(1, 1));

            string path = materialNameList[curMaterial];

            curAddTrack++;

            if (curAddTrack == 4)
            {
                curAddTrack = 1;
            }

            timeLine.load_to_audio_track(path, curAddTrack-1);
            trackIndexList[curAddTrack-1] = curMaterial;

            DrawWave(timeLine.audio_track[curAddTrack - 1].LVoice, timeLine.audio_track[curAddTrack - 1].SampleRate, curAddTrack);
            //MessageBox.Show(materialNameList[trackIndexList[curAddTrack - 1]]);
        }
        #endregion

        #region Video

        private void VideoPlayer_MediaOpened(object sender, RoutedEventArgs e)
        {
            videoLength = VideoPlayer.NaturalDuration.TimeSpan.TotalMilliseconds;
            originVideoHeight = VideoPlayer.NaturalVideoHeight;
            originVideoWidth = VideoPlayer.NaturalVideoWidth;
            //MessageBox.Show(videoLength.ToString());
            DrawVideoArea();
        }

        private void Display_Grid_MouseEnter(object sender, MouseEventArgs e)
        {
            Play_Image.Visibility = Visibility.Visible;
        }

        private void Display_Grid_MouseLeave(object sender, MouseEventArgs e)
        {
            Play_Image.Visibility = Visibility.Hidden;
        }

        private void Play_Image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!isPlay)
            {
                isPlay = true;
                this.VideoPlayer.Play();

                soundPlayer = new MediaPlayer();
                soundPlayer.Open(new Uri(TimeLineCore.audio_tmp_file_path + "/" + TimeLineCore.audio_mix_tmp_file_name, UriKind.Relative));
                this.soundPlayer.Position = TimeSpan.FromMilliseconds(PositionToTime(curX));
                this.soundPlayer.Play();
                Play_Image.Source = new BitmapImage(new Uri(@".\Icon\Pause_Icon.png", UriKind.Relative));

                if (isVideoRead)
                {
                    //drawTimer = new System.Timers.Timer(3000);
                    //drawTimer.Elapsed += new System.Timers.ElapsedEventHandler(DisplayInformation);
                    //DisplayInformation(6000);
                }
            }
            else
            {
                isPlay = false;
                this.VideoPlayer.Pause();
                this.soundPlayer.Pause();
                this.soundPlayer.Close();
                Play_Image.Source = new BitmapImage(new Uri(@".\Icon\Play_Icon.png", UriKind.Relative));
            }
        }

        private void VideoPlayer_MediaEnded(object sender, RoutedEventArgs e)
        {
            isPlay = false;
            this.VideoPlayer.Position = TimeSpan.FromMilliseconds(100);
            this.VideoPlayer.Play();
            Thread.Sleep(50);
            this.VideoPlayer.Pause();
        }

        private void DisplayRecognitionResult(double curVideoTime)
        {
            int recognitionResultIndex = Convert.ToInt16(Math.Floor((curVideoTime /1000) / 3));
            int recognitionResultNum = Convert.ToInt16(Math.Floor((videoLength / 1000) / 3));
            if (recognitionResultIndex > recognitionResultNum - 1)
            {
                recognitionResultIndex = recognitionResultNum - 1;
            }

            if (timeLine.video_track.emotion_per_3_sec[recognitionResultIndex]!= null)
            {
                int faceNum = 1;
                DrawFaceFrame_Canvas.Children.Clear();
                DisplayRecognitionText_Canvas.Children.Clear();
                for (int i = 0; i < faceNum; i++)
                {
                    // Draw the frame
                    DrawFaceFrame(recognitionResultIndex, i);
                    // Display the text
                    DisplayRecognitionText(recognitionResultIndex, i);
                }
            }
        }

        private void DrawFaceFrame(int recognizeResultIndex, int faceIndex)
        {
            // Get parameters for rectangles
            int originFrameHeight = timeLine.video_track.emotion_per_3_sec[recognizeResultIndex][faceIndex].FaceRectangle.Height;
            int originFrameWidth = timeLine.video_track.emotion_per_3_sec[recognizeResultIndex][faceIndex].FaceRectangle.Width;
            int originFrameLeft = timeLine.video_track.emotion_per_3_sec[recognizeResultIndex][faceIndex].FaceRectangle.Left;
            int originFrameTop = timeLine.video_track.emotion_per_3_sec[recognizeResultIndex][faceIndex].FaceRectangle.Top;

            // Get parameters for positions
            double zoomRatio = 0;
            if (originVideoHeight / originVideoWidth > maxVideoHeight / maxVideoWidth)
            {
                zoomRatio = maxVideoHeight / originVideoHeight;
            }
            else
            {
                zoomRatio = maxVideoWidth / originVideoWidth;
            }
            actualVideoHeight = zoomRatio * originVideoHeight;
            actualVideoWidth = zoomRatio * originVideoWidth;
            double actualFrameHeight = zoomRatio * originFrameHeight;
            double actualFrameWidth = zoomRatio * originFrameWidth;
            double actualFrameLeft = (maxVideoWidth - actualVideoWidth) / 2 + zoomRatio * originFrameLeft;
            double actualFrameTop = (maxVideoHeight - actualVideoHeight) / 2 + zoomRatio * originFrameTop;

            //Draw rectangle
            Rectangle faceFrame = new Rectangle();
            faceFrame.Width = actualFrameHeight;
            faceFrame.Height = actualFrameWidth;
            faceFrame.SetValue(Canvas.LeftProperty, actualFrameLeft);
            faceFrame.SetValue(Canvas.TopProperty, actualFrameTop);
            BrushConverter brushConverter = new BrushConverter();
            Brush myBrush = (Brush)brushConverter.ConvertFromString("#40FFFFFF");
            faceFrame.Fill = myBrush;
            DrawFaceFrame_Canvas.Children.Insert(0, faceFrame);

            //MessageBox.Show("zoomRatio: " + zoomRatio.ToString() +
            //                " height: " + actualFrameHeight.ToString() +
            //                " width: " + actualFrameWidth.ToString() +
            //                " leftOffset: " + actualFrameLeft.ToString() +
            //                " topOffset: " + actualFrameTop.ToString());

        }

        private void DisplayRecognitionText(int recognizeResultIndex, int faceIndex)
        {
            double[] emotionScore = new double[8];
            emotionScore[0] = timeLine.video_track.emotion_per_3_sec[recognizeResultIndex]
                [faceIndex].Scores.Anger;
            emotionScore[1] = timeLine.video_track.emotion_per_3_sec[recognizeResultIndex]
                [faceIndex].Scores.Contempt;
            emotionScore[2] = timeLine.video_track.emotion_per_3_sec[recognizeResultIndex]
                [faceIndex].Scores.Disgust;
            emotionScore[3] = timeLine.video_track.emotion_per_3_sec[recognizeResultIndex]
                [faceIndex].Scores.Fear;
            emotionScore[4] = timeLine.video_track.emotion_per_3_sec[recognizeResultIndex]
                [faceIndex].Scores.Happiness;
            emotionScore[5] = timeLine.video_track.emotion_per_3_sec[recognizeResultIndex]
                [faceIndex].Scores.Neutral;
            emotionScore[6] = timeLine.video_track.emotion_per_3_sec[recognizeResultIndex]
                [faceIndex].Scores.Sadness;
            emotionScore[7] = timeLine.video_track.emotion_per_3_sec[recognizeResultIndex]
                [faceIndex].Scores.Surprise;

            string[] emotionMapping = {"愤怒", "轻蔑", "厌恶", "恐惧", "开心", "平静", "悲伤", "惊讶"};

            // Find the most obvious three emotion
            int[] emotionIndex = {-1, -1, -1};
            for (int i = 0; i < 3; i++)
            {
                int maxIndex = 0;
                for (int j = 0; j < 8; j++)
                {
                    if (emotionScore[j] > emotionScore[maxIndex] && j != emotionIndex[0]
                        && j != emotionIndex[1])
                    {
                        maxIndex = j;
                    }
                }
                emotionIndex[i] = maxIndex;
            }

            // Display the text
            TextBlock TextBlock = new TextBlock
            {
                Text = "第" + (faceIndex+1).ToString() + "张脸的识别结果：\n" + 
                          emotionMapping[emotionIndex[0]] + ": " +
                          Math.Round(100 * emotionScore[emotionIndex[0]], 2).ToString() + "%\n" +
                          emotionMapping[emotionIndex[1]] + ": " +
                          Math.Round(100 * emotionScore[emotionIndex[1]], 2).ToString() + "%\n" +
                          emotionMapping[emotionIndex[2]] + ": " +
                          Math.Round(100 * emotionScore[emotionIndex[2]], 2).ToString() + "%\n" +
                          "建议选用 的音乐",
                FontSize = 12,
                Background = new SolidColorBrush(Color.FromArgb(100, 35, 35, 35)),
                Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255)),
                Height = 100,
                Width = 177,
                LineHeight = 20,
            };
            TextBlock.SetValue(Canvas.TopProperty, Convert.ToDouble(faceIndex * 80 + 10));
            TextBlock.SetValue(Canvas.LeftProperty, Convert.ToDouble(5));

            DisplayRecognitionText_Canvas.Children.Insert(0, TextBlock);

        }

        #endregion

        #region Information

        #endregion

        #region Manipulation
        private void Select_Image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!isSelect)
            {
                isSelect = true;
                Select_Image.Source = new BitmapImage(new Uri(@".\Icon\SelectOn_Icon.png", UriKind.Relative));
                isArea = false;
                Area_Image.Source = new BitmapImage(new Uri(@".\Icon\AreaOff_Icon.png", UriKind.Relative));
            }
            else
            {
                isSelect = false;
                Select_Image.Source = new BitmapImage(new Uri(@".\Icon\SelectOff_Icon.png", UriKind.Relative));
            }
        }

        private void Area_Image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!isArea)
            {
                isArea = true;
                Area_Image.Source = new BitmapImage(new Uri(@".\Icon\AreaOn_Icon.png", UriKind.Relative));
                isSelect = false;
                Select_Image.Source = new BitmapImage(new Uri(@".\Icon\SelectOff_Icon.png", UriKind.Relative));
            }
            else
            {
                isArea = false;
                Area_Image.Source = new BitmapImage(new Uri(@".\Icon\AreaOff_Icon.png", UriKind.Relative));
            }
        }

        private void Effect_Image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!isEffect)
            {
                isEffect = true;
                Effect_Pop.IsOpen = true;
                Effect_Image.Source = new BitmapImage(new Uri(@".\Icon\EffectOn_Icon.png", UriKind.Relative));
            }
            else
            {
                isEffect = false;
                Effect_Pop.IsOpen = false;
                Effect_Image.Source = new BitmapImage(new Uri(@".\Icon\EffectOff_Icon.png", UriKind.Relative));
            }
        }

        private void Delete_Image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Delete_Image.Source = new BitmapImage(new Uri(@".\Icon\DeleteOn_Icon.png", UriKind.Relative));

            timeLine.audio_function_center("cut", curEditTrack-1, PositionToTime(startX), PositionToTime(endX));
            DrawWave(timeLine.audio_track[curEditTrack - 1].LVoice, timeLine.audio_track[curEditTrack - 1].SampleRate, curEditTrack);
            
            // Remove previous preChoiceArea
            Rectangle preChoiceArea = Timeline_Area_Canvas.FindName("choiceArea_Rectangle") as Rectangle;
            if (preChoiceArea != null)
            {
                Timeline_Area_Canvas.Children.Remove(preChoiceArea);
                Timeline_Area_Canvas.UnregisterName("choiceArea_Rectangle");
            }
        }

        private void Delete_Image_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Delete_Image.Source = new BitmapImage(new Uri(@".\Icon\DeleteOff_Icon.png", UriKind.Relative));
        }

        #endregion

        #region Timeline

        private void Timeline_Area_Canvas_Click(object sender, MouseButtonEventArgs e)
        {
            if (isSelect)
            {
                // Get current position of select bar
                Point curPoint = e.GetPosition(this.Timeline_Area_Canvas);
                curX = curPoint.X;
                // Draw the current select bar
                DrawSelectBar();
                // Display the video at current position
                int curTime = PositionToTime(curX);
                if (curTime > (int)videoLength - 100)
                {
                    curTime = (int)videoLength - 100;
                }
                if (curTime < 100)
                {
                    curTime = 100;
                }
                this.VideoPlayer.Position = TimeSpan.FromMilliseconds(curTime - 50);
                this.VideoPlayer.Play();
                Thread.Sleep(50);
                this.VideoPlayer.Pause();
                // Display the video at current position
                this.soundPlayer.Position = TimeSpan.FromMilliseconds(curTime);
                this.soundPlayer.Pause();

                isPlay = false;
                Play_Image.Source = new BitmapImage(new Uri(@".\Icon\Play_Icon.png", UriKind.Relative));
                // Add face frames
                if (isVideoRead)
                {
                    DisplayRecognitionResult(curTime);
                }
            }
            if (isArea)
            {
                //Get the position of start point of rectangle
                Point startPoint = e.GetPosition(this.Timeline_Area_Canvas);
                isDrawing = true;
                startX = startPoint.X;
                startY = startPoint.Y;
                endX = startX;
            }
        }

        private void Timeline_Area_Canvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (isArea)
            {
                //Get the position of end point of rectangle
                Point endPoint = e.GetPosition(this.Timeline_Area_Canvas);
                endX = endPoint.X;
                isDrawing = false;
                //Draw the rectangle of the chioce area
                DrawChoiceArea();
                //MessageBox.Show(startX.ToString() + "," + endX.ToString());
                InsureStartEnd();
            }
        }

        private void DrawSelectBar()
        {
            //Delete previous preSelectBar
            Rectangle preSelectBar = Timeline_Area_Canvas.FindName("selectBar_Rectangle") as Rectangle;
            if (preSelectBar != null)
            {
                Timeline_Area_Canvas.Children.Remove(preSelectBar);
                Timeline_Area_Canvas.UnregisterName("selectBar_Rectangle");
            }

            //Draw new SelectBar
            Rectangle selectBar = new Rectangle();
            selectBar.Width = 2;
            selectBar.Height = 130;
            selectBar.SetValue(Canvas.LeftProperty, curX);
            BrushConverter brushConverter = new BrushConverter();
            Brush myBrush = (Brush)brushConverter.ConvertFromString("#80C0392B");
            selectBar.Fill = myBrush;
            Timeline_Area_Canvas.Children.Insert(0, selectBar);
            Timeline_Area_Canvas.RegisterName("selectBar_Rectangle", selectBar);
        }

        private void DrawChoiceArea()
        {
            // Remove previous preChoiceArea
            Rectangle preChoiceArea = Timeline_Area_Canvas.FindName("choiceArea_Rectangle") as Rectangle;
            if (preChoiceArea != null)
            {
                Timeline_Area_Canvas.Children.Remove(preChoiceArea);
                Timeline_Area_Canvas.UnregisterName("choiceArea_Rectangle");
            }

            // Draw new video area
            double width = endX - startX;
            if (width < 0)
            {
                width = -width;
            }

            double topY;
            if (startY < 62.5)
            {
                topY = 37.5;
                curEditTrack = 1;
            }
            else if (startY >= 62.5 && startY < 92.5)
            {
                topY = 67.5;
                curEditTrack = 2;
            }
            else
            {
                topY = 97.5;
                curEditTrack = 3;
            }

            Rectangle choiceArea = new Rectangle();
            choiceArea.Width = width;
            choiceArea.Height = 20;
            if (endX - startX > 0)
            {
                choiceArea.SetValue(Canvas.LeftProperty, startX);
            }
            else
            {
                choiceArea.SetValue(Canvas.LeftProperty, endX);
            }

            choiceArea.SetValue(Canvas.TopProperty, topY);
            BrushConverter brushConverter = new BrushConverter();
            Brush myBrush = (Brush)brushConverter.ConvertFromString("#4027AE60");
            choiceArea.Fill = myBrush;
            Timeline_Area_Canvas.Children.Insert(0, choiceArea);
            Timeline_Area_Canvas.RegisterName("choiceArea_Rectangle", choiceArea);
        }

        private void DrawVideoArea()
        {
            // Delete previous preSelectBar
            Rectangle preVideoArea = Timeline_Area_Canvas.FindName("videoArea_Rectangle") as Rectangle;
            if (preVideoArea != null)
            {
                Timeline_Area_Canvas.Children.Remove(preVideoArea);
                Timeline_Area_Canvas.UnregisterName("videoArea_Rectangle");
            }

            // Draw new video area
            double width = videoLength/fullTimeLength*870;

            Rectangle videoArea = new Rectangle();
            videoArea.Width = width;
            videoArea.Height = videoHeight;
            videoArea.SetValue(Canvas.LeftProperty, 0.0);
            videoArea.SetValue(Canvas.TopProperty, 10.0);

            BrushConverter brushConverter = new BrushConverter();
            Brush myBrush = (Brush)brushConverter.ConvertFromString("#40F39C12");
            videoArea.Fill = myBrush;
            Timeline_Area_Canvas.Children.Insert(0, videoArea);
            Timeline_Area_Canvas.RegisterName("videoArea_Rectangle", videoArea);
        }

        private void DrawWave(List<float> originVoiceArray, int SampleRate, int trackIndex)
        {
            // Calculate the parameters
            int unitNum = Convert.ToInt16(originVoiceArray.Count / SampleRate * 10);
            int unitArrayNum = Convert.ToInt16(SampleRate / 10);

            // Calaulate the data of array for drawing
            double[] waveHeightArray = new double[unitNum];
            for (int i = 0; i < unitNum; i++)
            {
                double sum = 0;
                for (int j = 0; j < unitArrayNum; j++)
                {
                    sum += originVoiceArray[i * unitArrayNum + j];
                }
                waveHeightArray[i] = sum / unitArrayNum;
            }

            // Remove previous wave
            switch (trackIndex)
            {
                case 1:
                    VoiceTrack1_Canvas.Children.Clear();
                    break;
                case 2:
                    VoiceTrack2_Canvas.Children.Clear();
                    break;
                case 3:
                    VoiceTrack3_Canvas.Children.Clear();
                    break;
            }

            // Draw the wave
            for (int i = 0; i < unitNum; i++)
            {
                Rectangle waveLine = new Rectangle();
                waveLine.Width = waveWidth;

                if (waveHeightArray[i] > 0)
                {
                    waveLine.Height = 1000 * waveHeight * waveHeightArray[i];
                    waveLine.SetValue(Canvas.TopProperty, 12.5 - waveLine.Height);
                }
                else
                {
                    waveLine.Height = -waveHeight * waveHeightArray[i] * 1000;
                    waveLine.SetValue(Canvas.TopProperty, 12.5);
                }
                waveLine.SetValue(Canvas.LeftProperty, Convert.ToDouble(waveWidth * i));
                //MessageBox.Show((1000*waveHeightArray[i]).ToString());

                BrushConverter brushConverter = new BrushConverter();
                Brush myBrush = (Brush)brushConverter.ConvertFromString("#80F39C12");
                waveLine.Fill = myBrush;

                switch (trackIndex)
                {
                    case 1:
                        VoiceTrack1_Canvas.Children.Insert(0, waveLine);
                        break;
                    case 2:
                        VoiceTrack2_Canvas.Children.Insert(0, waveLine);
                        break;
                    case 3:
                        VoiceTrack3_Canvas.Children.Insert(0, waveLine);
                        break;
                }
            }
        }
        #endregion

        #region Effects
        private void Weaken_Label_MouseDown(object sender, MouseButtonEventArgs e)
        {
            List<double> parameter = new List<double>();
            parameter.Add(1.0);
            parameter.Add(0.1);
            timeLine.audio_function_center("gradient", curEditTrack - 1, 100, 10000, parameter);
        }

        private void Strengthen_Label_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void Insert_Label_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void Rewind_Label_MouseDown(object sender, MouseButtonEventArgs e)
        {
            timeLine.audio_function_center("upend", curEditTrack - 1, PositionToTime(startX), PositionToTime(endX));
            DrawWave(timeLine.audio_track[curEditTrack - 1].LVoice, timeLine.audio_track[curEditTrack - 1].SampleRate, curEditTrack);
        }
       
        private void Echo_Label_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }


        private void SoundTrack_Label_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        #endregion

        #region Process
        public void ProcessReport(object sender, ProcessReportEventArgs e)
        {
            // Display the current event
            ToDo_Label.Content = e.to_do;

            // Display the percentage
            int processInt = Convert.ToInt16(e.percent*100);
            Precess_Label.Content = processInt.ToString()+"%";
            if (processInt == 100)
            {
                ToDo_Label.Content = "完毕";
            }

            // Draw the process bar
            double width = e.percent * 250;
            Rectangle preProcessBar = Timeline_Area_Canvas.FindName("process_Rectangle") as Rectangle;
            if (preProcessBar == null)
            {
                Rectangle processBar = new Rectangle();
                processBar.Width = width;
                processBar.Height = 15;
                processBar.SetValue(Canvas.LeftProperty, 0.0);
                processBar.SetValue(Canvas.TopProperty, 0.0);

                BrushConverter brushConverter = new BrushConverter();
                Brush myBrush = (Brush)brushConverter.ConvertFromString("#802980B9");
                processBar.Fill = myBrush;
                Process_Canvas.Children.Insert(0, processBar);
                Process_Canvas.RegisterName("process_Rectangle", processBar);
            }
            else
            {
                Rectangle processBar = Timeline_Area_Canvas.FindName("process_Rectangle") as Rectangle;
                processBar.Width = width;
            }
        }
        #endregion

        #region Assist
        private void InsureStartEnd()
        {
            if (startX > endX)
            {
                double temp = startX;
                startX = endX;
                endX = temp;
            }
        }

        private int PositionToTime(double position)
        {
            int milliTime;
            milliTime = (int)(position / 870 * fullTimeLength);
            return milliTime;
        }

        private double TimeToPostion(int milliTime)
        {
            double position;
            position = 870.0 * (double) milliTime / fullTimeLength;
            return position;
        }

        #endregion



    }
}
