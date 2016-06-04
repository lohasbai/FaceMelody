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
        /// 混合音轨的音频
        /// <para>请注意：每次尝试获得此值将会立即渲染混合音轨！</para>
        /// <para>若无音轨渲染，返回空音频</para>
        /// </summary>
        public AudioTools.BaseAudio mix_audio_track
        {
            get
            {
                long max_mili_sec = max_audio_track_sec();
                if (max_mili_sec == -1)
                    return new AudioTools.BaseAudio();
                return audio_tools.audio_mixer(new List<AudioTools.BaseAudio>(audio_track), 0, (int)max_mili_sec);
            }
        }
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
        /// 是否全部音轨均为空
        /// </summary>
        public bool is_all_audio_track_empty
        {
            get
            {
                for (int i = 0; i < MAX_AUDIO_TRACK; i++)
                    if (!audio_track[i].is_empty)
                        return false;
                return true;
            }
        }
        /// <summary>
        /// 是否视频轨道为空
        /// </summary>
        public bool is_video_track_empty
        {
            get
            {
                return video_track.is_empty;
            }
        }

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
            save_refresh();

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
        public async Task<bool> load_to_video_track(string file, bool skip_emotion = false)
        {
            try
            {
                if (!File.Exists(file))
                    throw new Exception("文件不存在");
                video_track = await video_tools.video_reader(file,skip_emotion);
                video_track.audio.copy_to(ref audio_track[0]);
                save_refresh();
            }
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show(e.Message);
                return false;
            }
            return true;
        }
        /// <summary>
        /// 清空某个轨道
        /// </summary>
        /// <param name="track_num">-1:视频轨道
        /// <para>0~(MAX_AUDIO_TRACK-1):音频轨道</para>
        /// </param>
        public void clear_track(int track_num)
        {
            if (track_num >= MAX_AUDIO_TRACK || track_num < -1)
                return;
            else if (track_num == -1)
                video_track.clear();
            else
                audio_track[track_num].clear();
            save_refresh();
            return;
        }

        public async Task<bool> save_all_track_to_file(string output_file)
        {
            bool ret = true;
            try
            {
                bool all_track_empty = true;
                if (!is_video_track_empty)
                    all_track_empty = false;
                else
                {
                    all_track_empty = is_all_audio_track_empty;
                }
                if (all_track_empty)
                    throw new Exception("所有轨道均为空，无法合成最终文件");
                if (is_video_track_empty)
                {
                    if (Path.GetExtension(output_file) != ".wav")
                        throw new Exception("在视频轨道为空时仅能输出wav文件");
                    ret = audio_tools.audio_writer(mix_audio_track, output_file);
                }
                else if (is_all_audio_track_empty)
                {
                    if (Path.GetExtension(output_file) != ".mp4")
                        throw new Exception("仅允许输出mp4文件");
                    ret = await video_tools.video_writer(video_track.file, "", output_file);
                }
                else
                {
                    if (Path.GetExtension(output_file) != ".mp4")
                        throw new Exception("仅允许输出mp4文件");
                    ret = await video_tools.video_writer(
                        video_track.file, audio_tmp_file_path + "/" + audio_mix_tmp_file_name, output_file);
                }
            }
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show(e.Message);
                return false;
            }
            return ret;
        }
        /// <summary>
        /// 音轨操作主入口，返回操作是否成功
        /// <para>处理完毕后会自动保存音轨至临时文件，请勿占用</para>
        /// </summary>
        /// <param name="function">
        /// 功能字符串，当前可选：
        /// <para>gradient: 渐变（需要附加两个var - 线性两点系数）</para>
        /// <para>cut：裁剪（不需要附加参数）</para>
        /// <para>insert：插入（end参数无效，需要一个BaseAudio - 将该Audio插入start点）</para>
        /// <para>upend：倒放（不需要附加参数）</para>
        /// <para>echo：回声（需要附加一个var - 回声强度占比，应小于1）</para>
        /// <para>exchange：交换左右声道（不需要附加参数）</para>
        /// </param>
        /// <param name="track_num">音轨序号</param>
        /// <param name="start">开始操作时间点</param>
        /// <param name="end">结束操作时间点</param>
        /// <param name="var_append">参数列表</param>
        /// <param name="src_append">附加音源列表</param>
        /// <returns></returns>
        public bool audio_function_center
            (string function, int track_num, int start, int end, List<double> var_append = null, List<AudioTools.BaseAudio> src_append = null)
        {
            try
            {
                if (track_num >= MAX_AUDIO_TRACK)
                    throw new Exception("待处理的轨道编号超过了最大轨道数");
                if (audio_track[track_num].is_empty)
                    throw new Exception("试图对空音轨进行操作");
                switch (function)
                {
                    case "gradient":
                        {
                            if (var_append == null || var_append.Count < 2)
                                throw new Exception("使用渐变方法时输入了无效的参数");
                            audio_track[track_num] = audio_tools.audio_gradient(
                                audio_track[track_num], start, end, var_append[0], var_append[1]);
                            break;
                        }
                    case "cut":
                        {
                            audio_track[track_num] = audio_tools.audio_cutter(
                                audio_track[track_num], start, end);
                            break;
                        }
                    case "insert":
                        {
                            if (src_append == null || src_append.Count < 1 || src_append[0].is_empty)
                                throw new Exception("使用插入方法时输入了无效的参数");
                            audio_track[track_num] = audio_tools.audio_inserter(
                                audio_track[track_num], src_append[0], start);
                            break;
                        }
                    case "upend":
                        {
                            audio_track[track_num] = audio_tools.audio_upending(
                                audio_track[track_num], start, end);
                            break;
                        }
                    case "echo":
                        {
                            if (var_append == null || var_append.Count < 1 || var_append[0] >= 1 || var_append[0] <= 0)
                                throw new Exception("使用回声方法时输入了无效的参数");
                            audio_track[track_num] = audio_tools.audio_echoing(
                                audio_track[track_num], start, end, var_append[0]);
                            break;
                        }
                    case "exchange":
                        {
                            if (audio_track[track_num].RVoice == null)
                                throw new Exception("使用交换声道方法时传入了没有右声道的音频");
                            audio_track[track_num] = audio_tools.audio_exchanging(
                                audio_track[track_num], start, end);
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
                clear_tmp_file();
                for (int i = 0; i < MAX_AUDIO_TRACK; i++)
                {
                    audio_track[i].clear();
                }
                video_track.clear();
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

        private bool clear_tmp_file()
        {
            try
            {
                for (int i = 0; i < MAX_AUDIO_TRACK; i++)
                {
                    if (File.Exists(audio_tmp_file_path + "/" + audio_tmp_file_name[i]))
                        File.Delete(audio_tmp_file_path + "/" + audio_tmp_file_name[i]);
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
        private bool save_refresh()
        {
            try
            {
                clear_tmp_file();
                if (!Directory.Exists(audio_tmp_file_path))
                {
                    Directory.CreateDirectory(audio_tmp_file_path);
                }
                FileStream fs = File.Create(audio_tmp_file_path + "/" + "请勿操作此文件夹内文件！");
                fs.Close();
                bool all_empty = true;
                for (int i = 0; i < MAX_AUDIO_TRACK; i++)
                {
                    if (audio_track[i].SampleRate == 0)
                        continue;
                    if (File.Exists(audio_tmp_file_path + "/" + audio_tmp_file_name[i]))
                        File.Delete(audio_tmp_file_path + "/" + audio_tmp_file_name[i]);
                    audio_tools.audio_writer(audio_track[i], audio_tmp_file_name[i], audio_tmp_file_path);
                    all_empty = false;
                }
                if (!all_empty)
                {
                    audio_tools.audio_writer(mix_audio_track, audio_mix_tmp_file_name, audio_tmp_file_path);
                }
            }
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show(e.Message);
                return false;
            }
            return true;
        }
        private long max_audio_track_sec()
        {
            long ret = -1;
            for (int i = 0; i < MAX_AUDIO_TRACK; i++)
            {
                if (!audio_track[i].is_empty)
                    ret = Math.Max(ret, audio_track[i].max_milisec);
            }
            return ret;
        }
        #endregion

    }
}
