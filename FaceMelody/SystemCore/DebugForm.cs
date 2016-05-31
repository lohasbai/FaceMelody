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
        public DebugForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //SystemCore.AudioTools a = new SystemCore.AudioTools();
            VideoTools vt = new VideoTools();
            VideoTools.BaseVideo bv = vt.video_reader("_no_sync_test_ver_1.avi");
            textBox1.Text = bv.audio.SampleRate.ToString();
        }
    }
}
