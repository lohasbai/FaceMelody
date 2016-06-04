using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace FaceMelody
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// 启动主窗口时注释后两行，启动DEBUG窗口时注释前两行
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Application_Startup(object sender, StartupEventArgs e)
        {

            Application currApp = Application.Current;
            currApp.StartupUri = new Uri("GUI/MainWorkshop.xaml", UriKind.RelativeOrAbsolute);

            //System.Windows.Forms.Application.Run(new DebugForm());
            //Shutdown();
        }
    }
}
