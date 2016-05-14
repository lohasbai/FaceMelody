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
using System.IO;

namespace FaceMelody
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            SystemCore.SoundTools st = new SystemCore.SoundTools();
            SystemCore.SoundTools.BaseSound bs = st.sound_reader("F:\\d多媒体技术及应用\\Program\\FaceMelody\\FaceMelody\\testwav.wav");

            //bs = null;
            //bs.clear();
            bs.LVoice.RemoveRange(bs.LVoice.Count / 4, bs.LVoice.Count - bs.LVoice.Count / 4);
            bs.RVoice.RemoveRange(bs.RVoice.Count / 4, bs.RVoice.Count - bs.RVoice.Count / 4);
            st.sound_writer(bs,"F:\\d多媒体技术及应用\\Program\\FaceMelody\\FaceMelody\\testwavv.wav");
            for (int i = 870; i < 890; i++)
            {
                Debug_Box.AppendText(bs.LVoice[i].ToString() + " ");
            }
            st = null;
        }
    }
}
