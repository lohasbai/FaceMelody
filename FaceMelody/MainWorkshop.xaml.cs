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

namespace FaceMelody
{
    /// <summary>
    /// MainWorkshop.xaml 的交互逻辑
    /// </summary>
    public partial class MainWorkshop : Window
    {
        public string[] materialList;
        public int materialNum;

        public bool isDrawing;
        public bool isAreaChose;
        public double startX;
        public double endX;


        public MainWorkshop()
        {
            InitializeComponent();

            materialList = new string[15];
            materialNum = 0;
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
                Width = 130,
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
            MessageBox.Show(path);
        }


        private void Timeline_Canvas_Click(object sender, MouseButtonEventArgs e)
        {
            Timeline_Canvas.Children.Clear();
            Point startPoint = e.GetPosition(this.Timeline_Canvas);
            isDrawing = true;
            startX = startPoint.X;
            endX = startX;
        }

        private void Timeline_Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDrawing)
            {
                Point endPoint = e.GetPosition(this.Timeline_Canvas);
                endX = endPoint.X;
                double width = endX - startX;
                if (width < 0)
                {
                    width = -width;
                }

                Rectangle choiceArea = new Rectangle();
                //choiceArea.Name = "choiceArea_Rectangle";
                choiceArea.Width = width;
                choiceArea.Height = 130;
                if (endX - startX > 0)
                {
                    choiceArea.SetValue(Canvas.LeftProperty, startX);
                }
                else
                {
                    choiceArea.SetValue(Canvas.LeftProperty, endX);
                }
                SolidColorBrush myBrush = new SolidColorBrush(Colors.Green);
                choiceArea.Fill = myBrush;
                Timeline_Canvas.Children.Insert(0, choiceArea);
            }
            
        }

        private void Timeline_Canvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            isDrawing = false;
            MessageBox.Show(startX.ToString() + "," +  endX.ToString());
        }
    }
}
