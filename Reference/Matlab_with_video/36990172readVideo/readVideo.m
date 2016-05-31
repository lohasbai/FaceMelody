xyloObj = VideoReader('test.avi');%读取音频

nFrames = xyloObj.NumberOfFrames;   %帧总数，此为1801

count=1;    %记录截取下来的帧数
letter='a'; %标记号，为使后续读取顺序正常，在阿拉伯数字前加上字母
for k = 1 : 50: nFrames
    mov(k).cdata =  read(xyloObj,k);    %图片颜色数据
    strtemp = strcat('images/',letter+count/10);    
    strtemp = strcat(strtemp, int2str(count));
    strtemp = strcat(strtemp,'.jpg');
    count=count+1;
    imwrite(mov(k).cdata,strtemp);%保存为名为strtemp的jpg图片
end