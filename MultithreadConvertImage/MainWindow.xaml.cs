using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace WpfApp1
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        static string[] postfix = { "jpg", "png", "bmp" };
        static StringBuilder sb = new StringBuilder();
        static double curProgress;
        static double maxProgress;
        static Stopwatch sw = new Stopwatch();
        public MainWindow()
        {
            InitializeComponent();
            //ConsoleManager.Show();//打开控制台窗口
            qualitySilder.Value = 8;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                pathText.Text = dialog.FileName;
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            string path;
            int quality;
            int maxLine;
            if (!CheckInit(out path, out quality, out maxLine))
            {
                return;
            }

            string[] pathList = GetAllFilePathList(path);
            sb.Clear();
           
            curProgress = 0;
            maxProgress = pathList.Length;

            sw.Restart();
          
                ParallelOptions po = new ParallelOptions();
                po.MaxDegreeOfParallelism = pathList.Length/ 2;
                var loopResult = Parallel.For(0, pathList.Length, i =>
                {
                    string sFile = pathList[i];
                    string dFile = sFile + ".jpg";
                    string errMsg = "";
                    if (!HandlerForImg.GetPicThumbnail(sFile, dFile, ref errMsg, quality, maxLine))
                    {
                        Console.WriteLine(errMsg);
                        sb.AppendLine(sFile);
                    }
                    Dispatcher.BeginInvoke(new InvokeDelegate(InvokeMethod), DispatcherPriority.ApplicationIdle);
                    curProgress++;
                });
                sw.Stop();
                Console.WriteLine(loopResult.IsCompleted + "finish  " + sw.Elapsed);
                WriteToFile.WriteInToFile(sb.ToString());
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            textQuality.Text = ((int)(e.NewValue * 10)).ToString();
        }
         
        bool checkFileName(string fileName)
        {
            for (int i = 0; i < postfix.Length; i++)
            {
                if (fileName.EndsWith(postfix[i]))
                {
                    return true;
                }
            }
            return false;
        }
        private delegate void InvokeDelegate();
        private void InvokeMethod()
        {
            double p = curProgress / maxProgress * 100.0;
            progress.Value = p;
            textProgress.Content = string.Format("当前进度：{0:F}%", p);
        }

        private void btnTask_Click(object sender, RoutedEventArgs e)
        {
            string path;
            int quality;
            int maxLine;
            if(!CheckInit(out path,out quality,out maxLine))
            {
                return;
            }

            string[] pathList = GetAllFilePathList(path);
            sb.Clear();
            curProgress = 0;
            maxProgress = pathList.Length;

            Task[] taskList = new Task[pathList.Length];
            sw.Restart();
            btnTask.IsEnabled = false;
            for (int i=0;i<taskList.Length;i++)
            {
                int index = i;
                taskList[i] = Task.Run(() =>
                {
                    string sFile = pathList[index];
                    string dFile = sFile + ".jpg";
                    string errMsg = "";
                    //Console.WriteLine("开始处理："+ sFile);
                    if (!HandlerForImg.GetPicThumbnail(sFile, dFile, ref errMsg, quality, maxLine))
                    {
                        sb.AppendLine(sFile);
                        sb.AppendLine(errMsg);
                    }
                    Dispatcher.BeginInvoke(new InvokeDelegate(InvokeMethod), DispatcherPriority.ApplicationIdle);
                    curProgress++;
                    //Console.WriteLine("处理完毕：" + sFile);
                });
            }

            Task.Run(() =>
            {
                Task.WaitAll(taskList);
                Dispatcher.BeginInvoke(new InvokeDelegate(InvokeMethod), DispatcherPriority.ApplicationIdle);
                sw.Stop();
                string str;
                if (sb.Length > 0)
                {
                    WriteToFile.WriteInToFile(sb.ToString());
                    sb.Clear();
                    str = string.Format("完成！！耗时{0}。\n失败{1}次，见log.txt。", sw.Elapsed.ToString(),sb.Length/2);
                }
                else
                {
                    str = string.Format("完成！！耗时{0}：。", sw.Elapsed.ToString());
                }
                MessageBox.Show(str);
                btnTask.IsEnabled = true;
            });
        }

        private bool CheckInit(out string path,out int quality,out int maxLine)
        {
            if (string.IsNullOrWhiteSpace(pathText.Text))
            {
                MessageBox.Show("请选择路径！！");
                path = null;
                quality = 0;
                maxLine = 0;
                return false;
            }

            if (!int.TryParse(textMaxLine.Text, out maxLine))
            {
                MessageBox.Show("请输入正确的数字！！");
                path = null;
                quality = 0;
                return false;
            }

            path = pathText.Text;
            quality = Convert.ToInt32(textQuality.Text);
            return true;
        }

        private string[] GetAllFilePathList(string path)
        {
            List<string> ls = new List<string>();
            DirectoryInfo dir = new DirectoryInfo(path);
            FileInfo[] files = dir.GetFiles("*", SearchOption.AllDirectories);
            for (int i = 0; i < files.Length; i++)
            {
                if (checkFileName(files[i].Name))
                {
                    ls.Add(files[i].FullName);
                }
            }

            return ls.ToArray();
        }

        private void BtnInvoke_Click(object sender, RoutedEventArgs e)
        {
            string path;
            int quality;
            int maxLine;
            if (!CheckInit(out path, out quality, out maxLine))
            {
                return;
            }

            string[] pathList = GetAllFilePathList(path);
            sb.Clear();
            curProgress = 0;
            maxProgress = pathList.Length;

            Action[] actionList = new Action[pathList.Length];
            Console.WriteLine("start");
            sw.Restart();

            for(int i=0;i<actionList.Length;i++)
            {
                int index = i;
                actionList[i] = () =>
                {
                    string sFile = pathList[index];
                    string dFile = sFile + ".jpg";
                    string errMsg = "";
                    //Console.WriteLine("开始处理：" + sFile);
                    if (!HandlerForImg.GetPicThumbnail(sFile, dFile, ref errMsg, quality, maxLine))
                    {
                        Console.WriteLine(errMsg);
                        sb.AppendLine(sFile);
                    }
                    Dispatcher.BeginInvoke(new InvokeDelegate(InvokeMethod), DispatcherPriority.ApplicationIdle);
                    curProgress++;
                    //Console.WriteLine("处理完毕：" + sFile);
                };
            }

            Task.Run(() =>
            {
                Parallel.Invoke(actionList);
                Console.WriteLine("finish   " + sw.Elapsed);
            });
        }
    }
}
