function MyVideoReader( filepath )
%VIDEOREADER 读取视频并分离视频与音频轨道
%   INPUT:
%   filepath - 文件路径与文件名
    [audio_,rate_] = audioread(filepath);
    audiowrite('tmp_wavinfo_no_sync.wav',audio_,rate_);
end

