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
        public double fullTimeLength;
        public double videoHeight;
        public double waveHeight;
        public double waveWidth;

        public string[] materialList;
        public int materialNum;

        public double videoLength;

        public double curX;

        public bool isSelect;
        public bool isArea;
        public bool isEffect;
        public bool isDelete;

        public bool isPlay;

        public bool isDrawing;
        public double startX;
        public double endX;


        public MainWorkshop()
        {
            InitializeComponent();

            fullTimeLength = 15*8.7*1000;
            videoHeight = 15;
            waveHeight = 10;
            waveWidth = 100.0/15.0/10.0;

            isPlay = false;
            videoLength = 0;

            materialList = new string[15];
            materialNum = 0;

            curX = 0;
            DrawSelectBar();

            isSelect = false;
            isArea = false;
            isEffect = false;
            isDelete = false;

            isDrawing = false;

        }

        #region Menu

        private void OpenFile_Menu_Click(object sender, RoutedEventArgs e)
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
                }
            }
        }

        private void AddMaterial_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog();

            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string filePath = openFileDialog.FileName;
                if (filePath != "" || filePath != null)
                {
                    AddMaterial(filePath);
                }
            }
        }
        #endregion

        #region Material
        private void AddMaterial(string filePath)
        {
            Material_StackPanel.Height += 50;

            string fileName = filePath.Substring(filePath.LastIndexOf("\\") + 1, (filePath.LastIndexOf(".") -
                filePath.LastIndexOf("\\") - 1));

            Label List_Label = new Label
            {
                Name = "_"+materialNum.ToString()+"_Material",
                Content = fileName,
                FontSize = 10,
                VerticalContentAlignment = VerticalAlignment.Center,
                Background = new SolidColorBrush(Color.FromArgb(255, 34, 33, 37)),
                Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255)),
                Height = 50,
                Width = 192,
            };

            materialList[materialNum] = filePath;
            materialNum++;

            List_Label.MouseDown += new MouseButtonEventHandler(ChooseMaterial_Click);
            Material_StackPanel.Children.Add(List_Label);
        }

        private void ChooseMaterial_Click(object sender, RoutedEventArgs e)
        {
            Label List_Label = sender as Label;
            string sourceName = List_Label.Name;

            string path = materialList[Convert.ToInt16(sourceName.Substring(1, 1))];


            AudioTools audioTool = new AudioTools();
            AudioTools.BaseAudio baseAudio = audioTool.audio_reader(path);

            DrawWave(baseAudio.LVoice, baseAudio.SampleRate, 1);
            DrawWave(baseAudio.LVoice, baseAudio.SampleRate, 3);

        }
        #endregion

        #region Display

        private void VideoPlayer_MediaOpened(object sender, RoutedEventArgs e)
        {
            videoLength = VideoPlayer.NaturalDuration.TimeSpan.TotalMilliseconds;
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
                Play_Image.Source = new BitmapImage(new Uri(@".\Icon\Pause_Icon.png", UriKind.Relative));
            }
            else
            {
                isPlay = false;
                this.VideoPlayer.Pause();
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
            if (!isDelete)
            {
                isDelete = true;
                Delete_Image.Source = new BitmapImage(new Uri(@".\Icon\DeleteOn_Icon.png", UriKind.Relative));
            }
            else
            {
                isDelete = false;
                Delete_Image.Source = new BitmapImage(new Uri(@".\Icon\DeleteOff_Icon.png", UriKind.Relative));
            }
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
                double curTime = curX / 870 * fullTimeLength;
                if (curTime > videoLength - 100)
                {
                    curTime = videoLength - 100;
                }
                this.VideoPlayer.Position = TimeSpan.FromMilliseconds(curTime);
                this.VideoPlayer.Play();
                Thread.Sleep(50);
                this.VideoPlayer.Pause();
                isPlay = false;
                Play_Image.Source = new BitmapImage(new Uri(@".\Icon\Play_Icon.png", UriKind.Relative));
            }
            if (isArea)
            {
                //Get the position of start point of rectangle
                Point startPoint = e.GetPosition(this.Timeline_Area_Canvas);
                isDrawing = true;
                startX = startPoint.X;
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
            //Delete previous preSelectBar
            Rectangle preChoiceArea = Timeline_Area_Canvas.FindName("choiceArea_Rectangle") as Rectangle;
            if (preChoiceArea != null)
            {
                Timeline_Area_Canvas.Children.Remove(preChoiceArea);
                Timeline_Area_Canvas.UnregisterName("choiceArea_Rectangle");
            }

            //Draw new video area
            double width = endX - startX;
            if (width < 0)
            {
                width = -width;
            }

            Rectangle choiceArea = new Rectangle();
            choiceArea.Width = width;
            choiceArea.Height = 110;
            if (endX - startX > 0)
            {
                choiceArea.SetValue(Canvas.LeftProperty, startX);
            }
            else
            {
                choiceArea.SetValue(Canvas.LeftProperty, endX);
            }
            choiceArea.SetValue(Canvas.BottomProperty, 5.0);
            BrushConverter brushConverter = new BrushConverter();
            Brush myBrush = (Brush)brushConverter.ConvertFromString("#4027AE60");
            choiceArea.Fill = myBrush;
            Timeline_Area_Canvas.Children.Insert(0, choiceArea);
            Timeline_Area_Canvas.RegisterName("choiceArea_Rectangle", choiceArea);
        }

        private void DrawVideoArea()
        {
            //Delete previous preSelectBar
            Rectangle preVideoArea = Timeline_Area_Canvas.FindName("videoArea_Rectangle") as Rectangle;
            if (preVideoArea != null)
            {
                Timeline_Area_Canvas.Children.Remove(preVideoArea);
                Timeline_Area_Canvas.UnregisterName("videoArea_Rectangle");
            }

            //Draw new video area
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



    }
}
