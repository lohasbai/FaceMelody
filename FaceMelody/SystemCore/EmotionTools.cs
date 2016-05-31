using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.ProjectOxford.Emotion;
using Microsoft.ProjectOxford.Common;
using Microsoft.ProjectOxford.Emotion.Contract;
using Microsoft.ProjectOxford.Common.Contract;
using Newtonsoft.Json;

namespace FaceMelody.SystemCore
{
    class EmotionTools
    {
        #region CONST
        private const string KEY1 = "f8fe23814783416b985453154aeb035c";
        private const string KEY2 = "7a9d04af4f984e1c97600dd7020b3ff3";
        public const float PRECISION = 0.000001f;
        #endregion

        #region STRUCT
        public struct VideoEmotionResultPer100MiliSec
        {
            public List<Scores> scores;
            public List<bool> has_face;
        }
        #endregion

        #region VAR
        private EmotionServiceClient Client = new EmotionServiceClient(KEY1);
        /// <summary>
        /// 上一次调取图片表情读取时的结果
        /// </summary>
        public Emotion[] img_last_emotions = null;
        /// <summary>
        /// 程序是否正在与服务器对接图片表情处理结果
        /// </summary>
        public bool img_processing = false;

        public VideoAggregateRecognitionResult ans_last;
        #endregion

        #region PUBLIC_FUNCTION
        /// <summary>
        /// 异步返回表情信息，当img_processing为false时可以读取
        /// </summary>
        /// <param name="file"></param>
        public async Task<Emotion[]> get_emotion_from_image_file(string file)
        {
            img_processing = true;
            try
            {
                Stream image_stream = File.OpenRead(file);
                img_last_emotions = await Client.RecognizeAsync(image_stream);
            }
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show(e.Message);
            }
            img_processing = false;
            return img_last_emotions;
        }
        /// <summary>
        /// 异步返回视频表情信息
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public async Task<VideoEmotionResultPer100MiliSec> get_emotion_from_video_file(string file)
        {
            VideoEmotionResultPer100MiliSec ret = new VideoEmotionResultPer100MiliSec();
            ret.scores = new List<Scores>();
            ret.has_face = new List<bool>();
            try
            {
                FileStream video_stream = new FileStream(file,FileMode.Open);
                VideoEmotionRecognitionOperation vero = await Client.RecognizeInVideoAsync(video_stream);
                VideoOperationResult voir;
                while (true)
                {
                    voir = await Client.GetOperationResultAsync(vero);
                    if (voir.Status == VideoOperationStatus.Succeeded)
                    {
                        break;
                    }
                    else if (voir.Status == VideoOperationStatus.Failed)
                    {
                        throw new Exception("视频表情识别失败！");
                    }
                    Task.Delay(20000).Wait();
                }
                VideoAggregateRecognitionResult ans_tmp = 
                    ((VideoOperationInfoResult<VideoAggregateRecognitionResult>)voir).ProcessingResult;
                ans_last = ans_tmp;
                //将ans_tmp转为ret的格式
                int ticks_per_100milisec = (int)(ans_tmp.Timescale / 10);
                int max_ticks = 
                    (int)(ans_tmp.Fragments[ans_tmp.Fragments.Length - 1].Start + 
                    ans_tmp.Fragments[ans_tmp.Fragments.Length - 1].Duration);

                int face_amount_in_100milisec = 0;
                for (int i = 0; i < max_ticks; i++)
                {
                    int this_index = i / ticks_per_100milisec;
                    if(i % ticks_per_100milisec == 0)
                    {
                        ret.scores.Add(new Scores());
                        ret.has_face.Add(false);
                        face_amount_in_100milisec = 0;
                    }
                    int fragment_index = find_fragment(ans_tmp, i);
                    if (fragment_index != -1)
                    {
                        if (ans_tmp.Fragments[fragment_index].Events == null)
                            continue;
                        int fragment_inside_index = (int)(i - ans_tmp.Fragments[fragment_index].Start);
                        if (fragment_inside_index >= ans_tmp.Fragments[fragment_index].Events.Length)
                            continue;
                        if (ans_tmp.Fragments[fragment_index].Events[fragment_inside_index].Length == 0)
                            continue;
                        Scores tmp_score = ans_tmp.Fragments[fragment_index].Events[fragment_inside_index][0].WindowMeanScores;
                        if (!is_empty_face(tmp_score))
                        {
                            ret.scores[this_index] = mean_emotion(
                                face_amount_in_100milisec, ret.scores[this_index], tmp_score);
                            ret.has_face[this_index] = true;
                            face_amount_in_100milisec++;
                        }
                    }
                }
                return ret;
            }
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show(e.Message);
            }

            return ret;
        }
        #endregion

        #region PRIVATE_FUNCTION

        private int find_fragment(VideoAggregateRecognitionResult result, int tick)
        {
            int i = 0;
            foreach (var fra in result.Fragments)
            {
                if (tick >= fra.Start && tick < fra.Start + fra.Duration)
                    return i;
                i++;
            }
            return -1;
        }

        private bool is_empty_face(Scores s)
        {
            if (
                Math.Abs(s.Anger) <= PRECISION &&
                Math.Abs(s.Contempt) <= PRECISION &&
                Math.Abs(s.Disgust) <= PRECISION &&
                Math.Abs(s.Fear) <= PRECISION &&
                Math.Abs(s.Happiness) <= PRECISION &&
                Math.Abs(s.Neutral) <= PRECISION &&
                Math.Abs(s.Sadness) <= PRECISION &&
                Math.Abs(s.Surprise) <= PRECISION)
                return true;
            return false;
        }

        private Scores mean_emotion(int already_num, Scores score_mean_now, Scores score_new)
        {
            Scores ret = new Scores();
            ret.Anger = (already_num * score_mean_now.Anger + score_new.Anger) / (already_num + 1);
            ret.Contempt = (already_num * score_mean_now.Contempt + score_new.Contempt) / (already_num + 1);
            ret.Disgust = (already_num * score_mean_now.Disgust + score_new.Disgust) / (already_num + 1);
            ret.Fear = (already_num * score_mean_now.Fear + score_new.Fear) / (already_num + 1);

            ret.Happiness = (already_num * score_mean_now.Happiness + score_new.Happiness) / (already_num + 1);
            ret.Neutral = (already_num * score_mean_now.Neutral + score_new.Neutral) / (already_num + 1);
            ret.Sadness = (already_num * score_mean_now.Sadness + score_new.Sadness) / (already_num + 1);
            ret.Surprise = (already_num * score_mean_now.Surprise + score_new.Surprise) / (already_num + 1);

            return ret;
        }
        #endregion
    }
}
