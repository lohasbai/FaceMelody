function MyVideoReader( filepath )
%MYVIDEOREADER 读取视频并分离视频与音频轨道
%   INPUT:
%   filepath - 文件路径与文件名

    %part1 - 分离音频
    [audio_,rate_] = audioread(filepath);
    audiowrite('tmp_wavinfo_no_sync.wav',audio_,rate_);
    
    %part2 - 读取视频
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

