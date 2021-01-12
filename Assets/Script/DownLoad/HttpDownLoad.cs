using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;

public class HttpDownLoad
{
    ///
    /// 下载文件方法
    ///
    /// 文件保存路径和文件名
    /// 返回服务器文件名
    ///
    public static bool DownLoadFile(string url, string localfile)
    {
        FileUtils.DelFile(localfile);
        FileUtils.CreateDirectory(localfile);
        bool flag = false;
        //打开上次下载的文件
        long sPosition = 0;
        //实例化流对象
        FileStream fileStream;
        //判断要下载的文件夹是否存在
        if (File.Exists(localfile))
        {
            //打开要下载的文件
            fileStream = File.OpenWrite(localfile);
            //获取已经下载的长度
            sPosition = fileStream.Length;
            fileStream.Seek(sPosition, SeekOrigin.Current);
        }
        else
        {
            //文件不保存创建一个文件
            fileStream = new FileStream(localfile, FileMode.Create);
            sPosition = 0;
        }
        try
        {
            HttpWebRequest myRequest = null;
            if (url.StartsWith("https", StringComparison.OrdinalIgnoreCase))
            {
                ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);
                myRequest = WebRequest.Create(url) as HttpWebRequest;
                myRequest.ProtocolVersion = HttpVersion.Version10;
            }
            else
            {
                myRequest = WebRequest.Create(url) as HttpWebRequest;
            }

            if (sPosition > 0)
                myRequest.AddRange((int)sPosition);             //设置Range值
                                                                //向服务器请求,获得服务器的回应数据流
            Stream myStream = myRequest.GetResponse().GetResponseStream();
            //定义一个字节数据
            byte[] btContent = new byte[1024];
            int intSize = myStream.Read(btContent, 0, btContent.Length);
            while (intSize > 0)
            {
                fileStream.Write(btContent, 0, intSize);
                intSize = myStream.Read(btContent, 0, btContent.Length);
            }
            //关闭流
            fileStream.Close();
            myStream.Close();
            flag = true;        //返回true下载成功
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            fileStream.Close();
            flag = false;       //返回false下载失败
        }
        return flag;
    }


    private static bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
    {
        return true; //总是接受
    }

}
