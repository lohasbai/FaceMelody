function MyVideoWriter(video_filepath,audio_filepath,output_filepath)
%MYVIDEOWRITER ��ȡ��Ƶ����Ƶ���ϳ�Ϊ�µ���Ƶ
%   INPUT:
%   video_filepath - ��Ƶ�ļ�·�����ļ���
%   audio_filepath - ��Ƶ�ļ�·�����ļ���
%   output_filepath - ����ļ�·�����ļ���

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

