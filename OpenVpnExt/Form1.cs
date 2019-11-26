using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using System.Xml;

namespace OpenVpnExt
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        /// <summary>
        /// 加载配置文件按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog folder = new System.Windows.Forms.FolderBrowserDialog();
            if (folder.ShowDialog() == DialogResult.OK)
            {
                this.textBox1.Text = folder.SelectedPath;
            }
        }
        /// <summary>
        /// 输出文件位置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog folder = new System.Windows.Forms.FolderBrowserDialog();
            if (folder.ShowDialog() == DialogResult.OK)
            {
                this.textBox2.Text = folder.SelectedPath;
            }
        }
        /// <summary>
        /// 执行
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            Thread Thd_Send = new Thread(new ThreadStart(exec));//新建一个线程以发送数据
            Thd_Send.Start();
        }
        /// <summary>
        /// 退出
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void exec()
        {
            try
            {
                richappendtext1(textBox1.Text.Trim());
                richappendtext2(textBox2.Text.Trim());
                DirectoryInfo root = new DirectoryInfo(textBox1.Text.Trim());
                FileInfo[] files = root.GetFiles();
                if (files.Length > 0)
                {
                    foreach (FileInfo fileInfo in files)
                    {
                        if (fileInfo.Extension.Contains(".ovpn") && (fileInfo.Name.Contains("tcp") || fileInfo.Name.Contains("udp")))
                        {
                            richappendtext1("执行文件名:" + fileInfo.Name);

                            string str = fileInfo.Name.Split('_')[0];
                            richappendtext1("获取的ip地址:" + str);
                            string adrrer = GetstringIpAddress(str);
                            richappendtext1("解析的位置:"+adrrer);
                            string ipname = "";
                            if (!string.IsNullOrEmpty(adrrer))
                            {
                                ipname = adrrer.Split(' ')[0].Substring(1, adrrer.Split(' ')[0].Length - 1);
                            }

                            richappendtext1("ip地址所在位置:" + ipname);

                            var ping = new System.Net.NetworkInformation.Ping();

                            var result = ping.Send(str);

                            if (result.Status != System.Net.NetworkInformation.IPStatus.Success)
                            {
                                richappendtext1("执行失败:");
                                richappendtext1("---------返回的状态:" + result.Status);
                                richappendtext1("---------答复的主机地址:" + result.Address);
                                richappendtext1("---------往返时间:" + result.RoundtripTime);
                            }
                            else
                            {
                                richappendtext1("执行成功:");
                                richappendtext1("---------返回的状态:" + result.Status);
                                richappendtext1("---------答复的主机地址:" + result.Address);
                                richappendtext1("---------往返时间:" + result.RoundtripTime);
                                richappendtext1("---------生存时间:" + result.Options.Ttl);
                                richappendtext1("---------缓存区大小:" + result.Buffer.Length);

                                richappendtext2("文件名:" + fileInfo.Name + " 执行成功");
    
                                richappendtext2("ip地址所在位置:" + str+" "+ ipname);
                                //移动文件
                                CopyDirectory(fileInfo.FullName, textBox2.Text.Trim()+ "\\"+ fileInfo.Name, "(" + ipname + ")"+fileInfo.Name);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                richappendtext1(ex.Message);
            }
        }
        /// <summary>
        /// 复制文件
        /// </summary>
        /// <param name="sourceDirPath"></param>
        /// <param name="SaveDirPath"></param>
        private  void CopyDirectory(string file, string SaveDirPath,string newfilename)
        {
            try
            {
                //如果指定的存储路径不存在，则创建该存储路径
                if (!Directory.Exists(SaveDirPath))
                {
                    //创建
                    Directory.CreateDirectory(SaveDirPath);
                }
                //string pFilePath = SaveDirPath + "\\" + Path.GetFileName(file);
                //if (!File.Exists(pFilePath))
                //{
                //    File.Copy(file, pFilePath, true);
                //}

                string pFilePath = SaveDirPath + "\\" + newfilename;

                FileInfo fi = new FileInfo(file); //xx/xx/aa.rar
                fi.MoveTo(pFilePath); //xx/xx/xx.rar
            }
            catch (Exception ex)
            {
                richappendtext1("复制文件失败:"+ex.Message);
            }
        }

        /// <summary>
        /// 获取ip对应的中文地址
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        private string GetstringIpAddress(string ip)
        {
            string url = "http://ipaddr.cz88.net/data.php?ip=" + ip;
            //得到网页源码
            string html = GetHtml(url);
            richappendtext1("获取ip页面源码:"+html);
            if (html.Contains("ShowIPAddr("))
            {
                html = html.Substring(11, html.Length - 11);
                html = html.Substring(0, html.Length-2);
                html= html.Split(',')[1];
            }

            return html;
        }
        /// <summary>
        /// 获取HTML源码信息(Porschev)
        /// </summary>
        /// <param name="url">获取地址</param>
        /// <returns>HTML源码</returns>
        public string GetHtml(string url)
        {
            string str = "";
            try
            {
                Uri uri = new Uri(url);
                WebRequest wr = WebRequest.Create(uri);
                Stream s = wr.GetResponse().GetResponseStream();
                StreamReader sr = new StreamReader(s, Encoding.Default);
                str = sr.ReadToEnd();
            }
            catch (Exception e)
            {
                richappendtext1("获取ip位置页面源码错误:"+e.Message);
            }
            return str;
        }

        private void richappendtext1(string str)
        {
            this.Invoke(new Action(() =>
            {

                //这边开始就是委托的内容
                richTextBox1.AppendText(str + "\r\n");
                richTextBox1.ScrollToCaret();
            }));
        }
        private void richappendtext2(string str)
        {
            this.Invoke(new Action(() => {

                //这边开始就是委托的内容
                richTextBox2.AppendText(str + "\r\n");
                richTextBox2.ScrollToCaret();
            }));
        }
    }
}
