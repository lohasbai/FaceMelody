using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using Microsoft.ProjectOxford.Emotion.Contract;

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
            /// 每隔3秒的表情采样
            /// <para>请注意：采样帧内允许有多个面部</para>
            /// </summary>
            public List<Emotion[]> emotion_per_3_sec;
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

        #region VAR
        public delegate void OnProcessCall(object sender, ProcessReportEventArgs e);
        public event OnProcessCall callback;
        #endregion

        #region PUBLIC_FUNCTION

        /// <summary>
        /// 构造函数，传入回调函数
        /// </summary>
        /// <code>
        /// public void print(object sender, ProcessReportEventArgs e)
        /// {
        ///     textBox1.Text += e.just_done + "\r\n";
        /// }
        /// </code>
        /// </example>
        /// <param name="back_function">回调函数，具体写法可参见本函数注释</param>
        public VideoTools(OnProcessCall back_function)
        {
            callback += back_function;
        }

        //public int debug(int c = 0)
        //{
        //    ProcessReportEventArgs prea = new ProcessReportEventArgs();
        //    prea.just_done = "abc";
        //    callback(this, prea);
        //    return 0;
        //}

        /// <summary>
        /// 读取一个视频（推荐avi）
        /// <para>若读取失败则返回值里的file为null</para>
        /// </summary>
        /// <param name="file">包含文件名的完整路径</param>
        /// <returns></returns>
        public async Task<BaseVideo> video_reader(string file)
        {
            BaseVideo ret = new BaseVideo();
            ret.clear();
            try
            {
                if (!File.Exists(file))
                    throw new Exception("找不到视频文件");
                if (!File.Exists("MyVideoReader.exe"))
                    throw new Exception("找到不到程序MyVideoReader.exe，请确保该可执行程序在主程序目录下");

                fast_callback(0, "寻找相关文件", "分离视频与音频");

                Process matlab_pro = new Process();
                ProcessStartInfo start_info = new ProcessStartInfo("MyVideoReader.exe", file);
                matlab_pro.StartInfo = start_info;
                await Task.Run(() =>
                {
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
                });
                fast_callback(0.1, "分离视频与音频", "读取音频");

                //读取分离的音频
                string wav_file = "tmp_wavinfo_no_sync.wav";
                if (!File.Exists(wav_file))
                    throw new Exception("生成的WAV文件未找到！请确保未手动删除！");
                AudioTools at = new AudioTools();
                ret.audio = at.audio_reader(wav_file);
                File.Delete(wav_file);

                fast_callback(0.15, "读取音频", "读取视频采样");

                //读取分离的帧
                string video_sample_path = "frame_tmp_no_sync";
                string video_sample_file_tail = "_no_sync.jpg";
                if(!Directory.Exists(video_sample_path))
                    throw new Exception("找不到临时采样文件夹");
                DirectoryInfo info = new DirectoryInfo(video_sample_path);
                int sample_num = info.GetFiles().Length;

                EmotionTools et = new EmotionTools();
                ret.emotion_per_3_sec = new List<Emotion[]>();
                for (int i = 0; i < sample_num; i++)
                {
                    fast_callback((1 - 0.15) / sample_num * i + 0.15,
                        ((i == 0) ? ("读取视频采样") : ("处理第" + i.ToString() + "个采样")),
                        "处理第" + (i + 1).ToString() + "个采样");
                    Emotion[] tmp_emotions = await
                        et.get_emotion_from_image_file(video_sample_path +"/"+ i.ToString() + video_sample_file_tail);
                    ret.emotion_per_3_sec.Add(tmp_emotions);
                    await Task.Delay(3500);
                }

                fast_callback(1, "处理第" + (sample_num - 1).ToString() + "个采样", "返回处理结果，删除临时文件");

                if (Directory.Exists(video_sample_path))
                    Directory.Delete(video_sample_path, true);
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

        #region PRIVATE_FUNCTION

        private void fast_callback(double per,string done,string todo)
        {
            ProcessReportEventArgs new_args = new ProcessReportEventArgs();
            new_args.percent = per;
            new_args.just_done = done;
            new_args.to_do = todo;
            callback(this, new_args);
        }

        #endregion
    }

    public class ProcessReportEventArgs : System.EventArgs
    {
        /// <summary>
        /// 进度，0-1
        /// </summary>
        public double percent;
        /// <summary>
        /// 刚刚做完的事情
        /// </summary>
        public string just_done;
        /// <summary>
        /// 即将要做的事情
        /// </summary>
        public string to_do;
    }
}
