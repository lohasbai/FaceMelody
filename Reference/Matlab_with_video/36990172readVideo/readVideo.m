xyloObj = VideoReader('test.avi');%��ȡ��Ƶ

nFrames = xyloObj.NumberOfFrames;   %֡��������Ϊ1801

count=1;    %��¼��ȡ������֡��
letter='a'; %��Ǻţ�Ϊʹ������ȡ˳���������ڰ���������ǰ������ĸ
for k = 1 : 50: nFrames
    mov(k).cdata =  read(xyloObj,k);    %ͼƬ��ɫ����
    strtemp = strcat('images/',letter+count/10);    
    strtemp = strcat(strtemp, int2str(count));
    strtemp = strcat(strtemp,'.jpg');
    count=count+1;
    imwrite(mov(k).cdata,strtemp);%����Ϊ��Ϊstrtemp��jpgͼƬ
end