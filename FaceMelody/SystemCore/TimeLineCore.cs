using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace FaceMelody.SystemCore
{
    class TimeLineCore
    {
        #region VAR
        /// <summary>
        /// 最大音轨数
        /// </summary>
        public const int MAX_AUDIO_TRACK = 3;

        /// <summary>
        /// 音轨内音频数组
        /// </summary>
        public AudioTools.BaseAudio[] audio_track = new AudioTools.BaseAudio[MAX_AUDIO_TRACK];
        /// <summary>
        /// 音频临时文件的文件名
        /// </summary>
        public string[] audio_tmp_file_name = new string[MAX_AUDIO_TRACK];
        /// <summary>
        /// 音频临时文件存放的文件夹名
        /// </summary>
        public const string audio_tmp_file_path = "_audio_tmp_no_sync";
        /// <summary>
        /// 混合后的渲染音频
        /// </summary>
        public const string audio_mix_tmp_file_name = "_audio_tmp_mix_no_sync.wav"; 

        /// <summary>
        /// 视频轨上的视频
        /// <para>请注意：视频中的音频不会在导出时被渲染</para>
        /// <para>（除非音频被加在音轨中）</para>
        /// </summary>
        public VideoTools.BaseVideo video_track;


        private AudioTools audio_tools = new AudioTools();
        private VideoTools video_tools;
        #endregion

        #region PUBLIC_FUNCTION
        /// <summary>
        /// 默认构造函数，会创建临时文件夹
        /// <para>请确保退出时调用clear清除文件夹</para>
        /// </summary>
        /// <param name="callback">回调函数，编写方法详见VideoTools的构造函数的注释</param>
        public TimeLineCore(VideoTools.OnProcessCall callback)
        {
            for (int i = 0; i < MAX_AUDIO_TRACK; i++)
            {
                audio_tmp_file_name[i] = "_audio_tmp_" + i.ToString() + "_no_sync.wav";
            }
            if (!Directory.Exists(audio_tmp_file_path))
            {
                Directory.CreateDirectory(audio_tmp_file_path);
            }
            FileStream fs = File.Create(audio_tmp_file_path + "/" + "请勿操作此文件夹内文件！");
            fs.Close();

            video_tools = new VideoTools(callback);
        }
        /// <summary>
        /// 将音频读取至音轨
        /// </summary>
        /// <param name="file">包含路径的音频文件名</param>
        /// <param name="track_num">音轨序号</param>
        /// <returns></returns>
        public bool load_to_audio_track(string file, int track_num)
        {
            try
            {
                if (track_num >= MAX_AUDIO_TRACK)
                    throw new Exception("待处理的轨道编号超过了最大轨道数");
                if (!File.Exists(file))
                    throw new Exception("文件不存在");
                audio_track[track_num] = audio_tools.audio_reader(file);
                if (!save_refresh())
                    throw new Exception("临时文件保存失败，请确认文件未被占用！");
            }
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show(e.Message);
                return false;
            }
            return true;
        }
        /// <summary>
        /// 将视频读取至
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public async Task<bool> load_to_video_track(string file)
        {
            try
            {
                if (!File.Exists(file))
                    throw new Exception("文件不存在");
                video_track = await video_tools.video_reader(file);
            }
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show(e.Message);
                return false;
            }
            return true;
        }
        /// <summary>
        /// 音轨操作主入口，返回操作是否成功
        /// <para>处理完毕后会自动保存音轨至临时文件，请勿占用</para>
        /// </summary>
        /// <param name="function">
        /// 功能字符串，当前可选：
        /// <para>gradient: 渐变</para>
        /// </param>
        /// <param name="track_num">音轨序号</param>
        /// <param name="start">开始操作时间点</param>
        /// <param name="end">结束操作时间点</param>
        /// <param name="var_list">参数列表，若无参数输入null即可</param>
        /// <returns></returns>
        public bool audio_function_center(string function, int track_num, int start, int end, List<double> var_list)
        {
            try
            {
                if (track_num >= MAX_AUDIO_TRACK)
                    throw new Exception("待处理的轨道编号超过了最大轨道数");
                switch (function)
                {
                    case "gradient":
                        {
                            if (var_list == null || var_list.Count < 2)
                                throw new Exception("使用渐变方法时输入了无效的参数");
                            audio_track[track_num] = audio_tools.audio_gradient(
                                audio_track[track_num], start, end, var_list[0], var_list[1]);
                            break;
                        }
                    default:
                        {
                            throw new Exception("功能输入错误");
                        }
                }
                if (!save_refresh())
                    throw new Exception("临时文件保存失败，请确认文件未被占用！");
            }
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show(e.Message);
                return false;
            }
            return true;
        }
        /// <summary>
        /// 清除所有内容，请确保程序关闭前被调用
        /// </summary>
        /// <returns></returns>
        public bool clear_all()
        {
            try
            {
                for (int i = 0; i < MAX_AUDIO_TRACK; i++)
                {
                    if (File.Exists(audio_tmp_file_path + "/" + audio_tmp_file_name[i]))
                        File.Delete(audio_tmp_file_path + "/" + audio_tmp_file_name[i]);
                    audio_track[i].clear();
                }
                if (File.Exists(audio_tmp_file_path + "/" + audio_mix_tmp_file_name))
                    File.Delete(audio_tmp_file_path + "/" + audio_mix_tmp_file_name);
                if (Directory.Exists(audio_tmp_file_path))
                    Directory.Delete(audio_tmp_file_path, true);
            }
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show(e.Message);
                return false;
            }
            return true;
        }
        #endregion

        #region PRIVATE_FUNCTION
        private bool save_refresh()
        {
            try
            {
                bool all_empty = true;
                for (int i = 0; i < MAX_AUDIO_TRACK; i++)
                {
                    if(audio_track[i].SampleRate == 0)
                        continue;
                    if (File.Exists(audio_tmp_file_path + "/" + audio_tmp_file_name[i]))
                        File.Delete(audio_tmp_file_path + "/" + audio_tmp_file_name[i]);
                    audio_tools.audio_writer(audio_track[i], audio_tmp_file_name[i], audio_tmp_file_path);
                    all_empty = false;

                    //[TODO]渲染混合文件代码
                }
            }
            catch(Exception e)
            {
                System.Windows.Forms.MessageBox.Show(e.Message);
                return false;
            }
            return true;
        }
        #endregion

    }
}
