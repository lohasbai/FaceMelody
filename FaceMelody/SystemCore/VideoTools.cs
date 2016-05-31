using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;


namespace FaceMelody.SystemCore
{
    class VideoTools
    {
        #region BASE_STRUCT
        /// <summary>
        /// 基本视频类，包含一个视频文件名和一个音频
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
                Process matlab = new Process();
                ret.file = file;
            }
            catch(Exception)
            {
                ret.clear();
            }
            return ret;
        }
        #endregion
    }
}
