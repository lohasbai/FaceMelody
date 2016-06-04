function MyVideoReader( filepath )
%MYVIDEOREADER ��ȡ��Ƶ��������Ƶ����Ƶ���
%   INPUT:
%   filepath - �ļ�·�����ļ���

    %part1 - ������Ƶ
    [audio_,rate_] = audioread(filepath);
    audiowrite('tmp_wavinfo_no_sync.wav',audio_,rate_);
    
    %part2 - ��ȡ��Ƶ
    obj = VideoReader(filepath);
    secs = obj.Duration;
    secs = floor(secs);
    sample = floor(secs / 3);
    if ~exist('frame_tmp_no_sync')
        mkdir('frame_tmp_no_sync');
    end
    for i = 1:sample
        sample_tick = floor(obj.FrameRate * (i-1)*3+2);
        img = read(obj,sample_tick);
        imwrite(img,['frame_tmp_no_sync/',num2str(i-1),'_no_sync.jpg']);
    end
end

