using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;

namespace FaceMelody.SystemCore
{
    class VideoTools
    {
        #region CONST
        /// <summary>
        /// 视频读取超时（毫秒数/10）
        /// <para>分钟 * 60 * 100</para>
        /// </summary>
        const int TIME_OUT = 5 * 60 * 100;
        #endregion

        #region BASE_STRUCT
        /// <summary>
        /// 基本视频结构体，包含一个视频文件名和一个音频
        /// </summary>
        public struct BaseVideo
        {
            /// <summary>
            /// 视频的音频信息
            /// </summary>
            public AudioTools.BaseAudio audio;
            /// <summary>
            /// 视频的文件地址与文件名
            /// </summary>
            public string file;
            /// <summary>
            /// 清空本段视频信息
            /// </summary>
            public void clear()
            {
                audio.clear();
                file = "";
            }
        }
        #endregion

        #region PUBLIC_FUNCTION
        /// <summary>
        /// 读取一个视频（推荐avi）
        /// <para>若读取失败则返回值里的file为null</para>
        /// </summary>
        /// <param name="file">包含文件名的完整路径</param>
        /// <returns></returns>
        public BaseVideo video_reader(string file)
        {
            BaseVideo ret = new BaseVideo();
            ret.clear();
            try
            {
                if (!File.Exists(file))
                    throw new Exception("找不到视频文件");
                if (!File.Exists("MyVideoReader.exe"))
                    throw new Exception("找到不到程序MyVideoReader.exe，请确保该可执行程序在主程序目录下");
                Process matlab_pro = new Process();
                ProcessStartInfo start_info = new ProcessStartInfo("MyVideoReader.exe", file);
                matlab_pro.StartInfo = start_info;
                matlab_pro.Start();
                int i = 0;
                while (true)
                {
                    System.Threading.Thread.Sleep(10);
                    i++;
                    if (i > TIME_OUT)
                    {
                        matlab_pro.Kill();
                        throw new Exception("程序运行超时");
                    }
                    if (matlab_pro.HasExited)
                        break;
                }
                string wav_file = "tmp_wavinfo_no_sync.wav";
                if (!File.Exists(wav_file))
                    throw new Exception("生成的WAV文件未找到！请确保未手动删除！");
                AudioTools at = new AudioTools();
                ret.audio = at.audio_reader(wav_file);
                File.Delete(wav_file);
                ret.file = file;
            }
            catch(Exception e)
            {
                System.Windows.Forms.MessageBox.Show(e.Message);
                ret.clear();
            }
            return ret;
        }
        #endregion
    }
}
