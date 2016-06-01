using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Media;
using System.IO;

namespace FaceMelody.SystemCore
{
    class AudioTools
    {
        #region CONST

        const int _chunkID = 1179011410;
        const int _riffType = 1163280727;
        const int _fmtID = 544501094;
        const int _fmtSize = 16;//限制文件大小60分钟！
        const int _dataID = 1635017060;
        const Int16 _fmtCode = 1;
        const Int16 _fmtBlockAlign = 4;

        #endregion

        #region BASE_STRUCT
        /// <summary>
        /// 基本声音结构体，包含声音数组长度，左右声道数组内容
        /// </summary>
        public struct BaseAudio
        {
            /// <summary>
            /// 左声道数据（单声道时为主声道数据）
            /// </summary>
            public List<float> LVoice;
            /// <summary>
            /// 右声道数据（单声道时为null）
            /// </summary>
            public List<float> RVoice;
            /// <summary>
            /// 采样率
            /// </summary>
            public int SampleRate;

            /// <summary>
            /// 比特深度，勿改
            /// </summary>
            public const int BitDepth = 16;

            /// <summary>
            /// 清空本段声音
            /// </summary>
            public void clear()
            {
                LVoice = null;
                RVoice = null;
            }
            /// <summary>
            /// 完全复制
            /// </summary>
            /// <param name="dst">待复制到的目标</param>
            public void copy_to(BaseAudio dst)
            {
                dst.clear();
                dst.LVoice = new List<float>(this.LVoice.ToArray());
                if (this.RVoice != null)
                    dst.RVoice = new List<float>(this.RVoice.ToArray());
                dst.SampleRate = this.SampleRate;
                return;
            }
        }
        #endregion

        #region PUBLIC_FUNCTION

        /// <summary>
        /// <para>读取一个wav格式音频，返回数组流</para>
        /// <para>读取失败则length为0</para>
        /// <para>单声道则RVoice为null</para>
        /// </summary>
        /// <param name="file">包含文件名的完整路径</param>
        /// <returns></returns>
        public BaseAudio audio_reader(string file)
        {
            BaseAudio ret = new BaseAudio();
            float[] L,R;
            int sample_rate = 0;
            if (readWav(file, out L, out R, ref sample_rate))
            {
                ret.SampleRate = sample_rate;
                ret.LVoice = L.ToList<float>();
                if (R != null)
                    ret.RVoice = R.ToList<float>();
                else
                    ret.RVoice = null;
            }
            return ret;
        }

        /// <summary>
        /// 将BaseSound写入wav文件
        /// </summary>
        /// <param name="audio">要写入的声音</param>
        /// <param name="file">包含文件名的完整路径</param>
        /// <returns></returns>
        public bool audio_writer(BaseAudio audio, string file)
        {
            try
            {
                if (File.Exists(file))
                    File.Delete(file);
                FileStream fs = new FileStream(file, FileMode.Create);
                BinaryWriter bw = new BinaryWriter(fs);
                bool double_wave = (audio.RVoice != null);
                int bytes = ((double_wave) ? (2) : (1)) * audio.LVoice.Count * BaseAudio.BitDepth / 8;
                //chunk 1
                bw.Write(_chunkID);
                bw.Write(9 * 4 + bytes);
                bw.Write(_riffType);
                //chunk 2
                bw.Write(_fmtID);
                bw.Write(_fmtSize);
                bw.Write(_fmtCode);
                bw.Write((double_wave ? ((short)2) : ((short)1)));
                bw.Write(audio.SampleRate);
                bw.Write(audio.SampleRate * (double_wave ? (2) : (1)) * BaseAudio.BitDepth / 8);
                bw.Write(_fmtBlockAlign);
                bw.Write((short)BaseAudio.BitDepth);
                //chunk 3
                bw.Write(_dataID);
                bw.Write(bytes);

                if (double_wave)
                {
                    for (int i = 0; i < bytes / 2; i++)
                    {
                        Int16 to_save = (Int16)(audio.LVoice[i] * Int16.MaxValue);
                        byte[] s1 = BitConverter.GetBytes(to_save);
                        bw.Write(s1);
                        to_save = (Int16)(audio.RVoice[i] * Int16.MaxValue);
                        byte[] s2 = BitConverter.GetBytes(to_save);
                        bw.Write(s2);
                    }
                }
                else
                {
                    for (int i = 0; i < bytes; i++)
                    {
                        Int16 to_save = (Int16)(audio.LVoice[i] * Int16.MaxValue);
                        byte[] s1 = BitConverter.GetBytes(to_save);
                        bw.Write(s1);
                    }
                }

                fs.Flush();
                bw.Close();
                fs.Close();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 将音频减去一段，若出错将返回原音频
        /// <para>若start小于零则默认从0开始</para>
        /// <para>若end大于最大毫秒数将截取到最后</para>
        /// </summary>
        /// <param name="src">音频源</param>
        /// <param name="start">开始毫秒数</param>
        /// <param name="end">结束毫秒数</param>
        /// <returns>处理后的音频</returns>
        public BaseAudio audio_cutter(BaseAudio src,int start, int end)
        {
            int sample_start, sample_end;
            if (!locate(src, start, end, out sample_start, out sample_end))
                return src;

            if (sample_start == 0 && sample_end == src.LVoice.Count)
                return new BaseAudio();

            BaseAudio ret = new BaseAudio();
            src.copy_to(ret);

            ret.LVoice.RemoveRange(sample_start, sample_end - sample_start);
            if(ret.RVoice != null)
                ret.RVoice.RemoveRange(sample_start, sample_end - sample_start);

            return ret;
        }

        /// <summary>
        /// 处理音频渐强渐弱，若出错将返回原音频
        /// <para>若start小于零则默认从0开始</para>
        /// <para>若end大于最大毫秒数将截取到最后</para>
        /// </summary>
        /// <param name="src">音频源</param>
        /// <param name="start">开始毫秒数</param>
        /// <param name="end">结束毫秒数</param>
        /// <param name="start_vol">始点音量比例</param>
        /// <param name="end_vol">终点音量比例</param>
        /// <returns></returns>
        public BaseAudio audio_gradient(BaseAudio src, int start, int end, double start_vol = 1.00, double end_vol = 1.00)
        {
            int start_tick, end_tick;
            if (!locate(src, start, end, out start_tick, out end_tick))
                return src;

            BaseAudio ret = new BaseAudio();
            src.copy_to(ret);

            for (int i = start_tick; i < end_tick; i++)
            {
                double rate1 = ((double)(i - start_tick)) / (end_tick - start_tick);
                double rate2 = (1 - rate1) * start_vol + rate1 * end_vol;

                ret.LVoice[i] *= (float)(rate2);
                if (ret.RVoice != null)
                    ret.RVoice[i] *= (float)(rate2);
            }

            return ret;
        }
        /// <summary>
        /// 音频倒放，若出错将返回原音频
        /// <para>若start小于零则默认从0开始</para>
        /// <para>若end大于最大毫秒数将截取到最后</para>
        /// </summary>
        /// <param name="src">音频源</param>
        /// <param name="start">开始毫秒数</param>
        /// <param name="end">结束毫秒数</param>
        /// <returns></returns>
        public BaseAudio audio_upending(BaseAudio src, int start, int end)
        {
            int start_tick, end_tick;
            if (!locate(src, start, end, out start_tick, out end_tick))
                return src;

            BaseAudio ret = new BaseAudio();
            src.copy_to(ret);

            for (int i = start_tick; i < end_tick; i++)
            {
                ret.LVoice[i] = src.LVoice[end_tick - (i - start_tick + 1)];
                if(ret.RVoice != null)
                    ret.RVoice[i] = src.RVoice[end_tick - (i - start_tick + 1)];
            }
            return ret;
        }
        #endregion

        #region PRIVATE_FUNCTION

        private bool readWav(string filename, out float[] L, out float[] R, ref int sample_rate)
        {
            L = R = null;
            //float [] left = new float[1];

            //float [] right;
            try
            {
                FileStream fs = File.Open(filename, FileMode.Open);
                BinaryReader reader = new BinaryReader(fs);

                // chunk 0
                int chunkID = reader.ReadInt32();
                int fileSize = reader.ReadInt32();//从下一个变量开始所占的大小，共计9*4+bytes比特
                int riffType = reader.ReadInt32();


                // chunk 1
                int fmtID = reader.ReadInt32();
                int fmtSize = reader.ReadInt32(); // bytes for this chunk
                int fmtCode = reader.ReadInt16();
                int channels = reader.ReadInt16();
                int sampleRate = reader.ReadInt32();
                int byteRate = reader.ReadInt32(); // = sampleRate * channels * bitDepth / 8
                int fmtBlockAlign = reader.ReadInt16();
                int bitDepth = reader.ReadInt16();

                sample_rate = sampleRate;

                if (fmtSize == 18)
                {
                    // Read any extra values
                    int fmtExtraSize = reader.ReadInt16();
                    reader.ReadBytes(fmtExtraSize);
                }

                // chunk 2
                int dataID = reader.ReadInt32();
                int bytes = reader.ReadInt32();

                // DATA!
                byte[] byteArray = reader.ReadBytes(bytes);
                reader.Close();
                fs.Close();
                reader.Dispose();
                fs.Dispose();
                int bytesForSamp = bitDepth / 8;
                int samps = bytes / bytesForSamp;


                float[] asFloat = null;
                switch (bitDepth)
                {
                    case 64:
                        double[] asDouble = new double[samps];
                        Buffer.BlockCopy(byteArray, 0, asDouble, 0, bytes);
                        asFloat = Array.ConvertAll(asDouble, e => (float)e);
                        asDouble = null;
                        break;
                    case 32:
                        asFloat = new float[samps];
                        Buffer.BlockCopy(byteArray, 0, asFloat, 0, bytes);
                        break;
                    case 16:
                        Int16[] asInt16 = new Int16[samps];
                        Buffer.BlockCopy(byteArray, 0, asInt16, 0, bytes);
                        asFloat = Array.ConvertAll(asInt16, e => e / (float)Int16.MaxValue);
                        asInt16 = null;
                        break;
                    default:
                        return false;
                }

                switch (channels)
                {
                    case 1:
                        L = asFloat;
                        R = null;
                        asFloat = null;
                        byteArray = null;
                        return true;
                    case 2:
                        L = new float[samps / 2];
                        R = new float[samps / 2];
                        for (int i = 0, s = 0; i < samps / 2; i++)
                        {
                            L[i] = asFloat[s++];
                            R[i] = asFloat[s++];
                        }
                        asFloat = null;
                        byteArray = null;
                        return true;
                    default:
                        asFloat = null;
                        byteArray = null;
                        return false;
                }
            }
            catch
            {
                //Debug.Log("...Failed to load note: " + filename);
                return false;
                //left = new float[ 1 ]{ 0f };
            }
        }

        private bool locate(BaseAudio src,int start,int end,out int start_tick,out int end_tick)
        {
            start_tick = 0;
            end_tick = 0;
            if (src.LVoice == null || src.LVoice.Count == 0 || src.SampleRate == 0 || start >= end || end <= 0)
                return false;
            int full_milisec = src.LVoice.Count * 1000 / src.SampleRate;
            if (start <= 0)
                start_tick = 0;
            else if (start >= full_milisec)
                return false;
            else
                start_tick = start * src.SampleRate / 1000;
            if (end >= full_milisec)
                end_tick = src.LVoice.Count;
            else
                end_tick = end * src.SampleRate / 1000;
            return true;
        }
        #endregion
    }
}
