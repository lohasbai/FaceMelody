function MyVideoWriter(video_filepath,audio_filepath,output_filepath)
%MYVIDEOWRITER 读取视频与音频并合成为新的视频
%   INPUT:
%   video_filepath - 视频文件路径与文件名
%   audio_filepath - 音频文件路径与文件名
%   output_filepath - 输出文件路径与文件名

    myobj = VideoWriter(output_filepath,'MPEG-4');
    obj = VideoReader(video_filepath);
    [audio_,rate_] = audioread(audio_filepath);
    open(myobj);
    
    for k = 1:obj.NumberOfFrames
       img = read(obj,k);
       writeVideo(myobj,img);
    end

    close(myobj);

end

