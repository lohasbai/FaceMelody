function MyVideoReader( filepath )
%VIDEOREADER ��ȡ��Ƶ��������Ƶ����Ƶ���
%   INPUT:
%   filepath - �ļ�·�����ļ���
    [audio_,rate_] = audioread(filepath);
    audiowrite('tmp_wavinfo_no_sync.wav',audio_,rate_);
end

