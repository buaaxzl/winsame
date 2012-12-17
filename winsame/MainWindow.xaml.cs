using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Windows.Forms;

namespace winsame
{
    
    #region mainForm
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// 当前的确认列表
        /// </summary>
        public string anss;
        /// <summary>
        /// 当前的文件路径
        /// </summary>
        public string path;
        public MainWindow()
        {
            InitializeComponent();
            anss = "";
            path = Consts.STARTPATH;
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.Description = Consts.DIALOGDESCRIPTION;
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                anss = "";
                string foldPath = dialog.SelectedPath;
                List<AnsData> ans = FileOpr.GetAllFiles(foldPath);
                int cnt = ans.Count;
                listBox1.Items.Clear();
                for (int i = 0; i < cnt; ++i)
                    for (int j = i + 1; j < cnt; ++j)
                        if (ans[i].user != ans[j].user && ans[i].calsim(ans[j]) > Consts.TEXTTHRESHOLD) listBox1.Items.Add(ans[i].fullfilename + "\t" + ans[j].fullfilename);
                path = foldPath;
            }
        }

        private void listBox1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                string[] seleted = ((string)e.AddedItems[0]).Split('\t');
                StreamReader sr = new StreamReader(seleted[0], System.Text.Encoding.Default);
                textBox1.Text = sr.ReadToEnd();
                sr.Close();
                sr = new StreamReader(seleted[1], System.Text.Encoding.Default);
                textBox2.Text = sr.ReadToEnd();
                sr.Close();
            }
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            anss = anss + (string)listBox1.SelectedItem + "\n";
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            
        }

        private void button3_Click(object sender, RoutedEventArgs e)
        {
            if (path == "") return;
            FileStream fs;
            try
            {
                fs = new FileStream(path + "result.txt", FileMode.CreateNew);
            }
            catch (Exception ee)
            {
                fs = new FileStream(path + "result.txt", FileMode.Append);
                throw ee.InnerException;
            }
            byte[] data = new UTF8Encoding().GetBytes(anss);
            fs.Write(data, 0, data.Length);
            fs.Flush();
            fs.Close();
        }

        private void button4_Click(object sender, RoutedEventArgs e)
        {
            HelpWindow helpWindow = new HelpWindow();
            helpWindow.Show();
        }
    }
    #endregion
    #region consts
    /// <summary>
    /// 常量列表
    /// </summary>
    public class Consts
    {
        /// <summary>
        /// 默认路径
        /// </summary>
        public static string STARTPATH = @"F:\";
        /// <summary>
        /// 默认打开窗口的提示信息
        /// </summary>
        public static string DIALOGDESCRIPTION = "请选择文件路径";
        /// <summary>
        /// 文件名的关键词列表
        /// </summary>
        public static List<string> KEYWORDS = new List<string>()
        {
            "BCPC", "Accepted"
        };
        /// <summary>
        /// 内存相似度阈值
        /// </summary>
        public static double MEMTHRESHOLD = 0.99;
        /// <summary>
        /// 运行时间相似度阈值
        /// </summary>
        public static double TIMETHRESHOLD = 0.99;
        /// <summary>
        /// 文本相似度阈值
        /// </summary>
        public static double TEXTTHRESHOLD = 0.8;
    }
    #endregion
    #region StringFunction
    /// <summary>
    /// 字符串处理类
    /// </summary>
    public static class StringFunctions
    {
        /// <summary>
        /// 将形如[][][]的文件名切割成字符串数组
        /// </summary>
        /// <param name="input">输入的完整文件名</param>
        /// <returns></returns>
        public static string[] mysplit(string input)
        {
            string[] now = new string[7];
            int pos = 0;
            for (int i = 0; i < 7; ++i)
            {
                pos++;
                while (input[pos] != ']') now[i] = now[i] + input[pos++];
                pos++;
            }
            return now;
        }
        /// <summary>
        /// 将形如"xxx-yyy"的字符串分割成两个字符串
        /// </summary>
        /// <param name="input">输入的待分割字符串</param>
        /// <returns></returns>
        public static string[] splitagain(string input)
        {
            return input.Split('-');
        }
        /// <summary>
        /// 字符串转int
        /// </summary>
        /// <param name="time">输入的字符串</param>
        /// <returns></returns>
        public static int stringtoint(string time)
        {
            int pos = 0, ans = 0;
            while (Char.IsNumber(time[pos]))
            {
                ans = ans * 10 + time[pos++];
            }
            return ans;
        }
        /// <summary>
        /// 删除字符串空白字符
        /// </summary>
        /// <param name="text">输入的待删除文本</param>
        /// <returns>删除后的结果</returns>
        public static string deleteEmpty(string text)
        {
            string temp = text;
            for (char c = (char)0; c < (char)256; ++c)
                if (Char.IsWhiteSpace(c)) temp = temp.Replace(c + "", "");
            return temp;
        }
        /// <summary>
        /// 计算两个文本的相似度
        /// 使用编辑距离算法
        /// </summary>
        /// <param name="text1">第一个文本</param>
        /// <param name="text2">第二个文本</param>
        /// <returns>计算后的比例，在[0..1]范围内</returns>
        public static double calstringsim(string text1, string text2)
        {
            int len1 = text1.Length;
            int len2 = text2.Length;
            int[,] dp = new int[len1, len2];
            for (int i = 0; i < len1; ++i)
                for (int j = 0; j < len2; ++j)
                {
                    dp[i, j] = len1 + len2;
                    if (i == 0 && j == 0)
                    {
                        dp[i, j] = 0;
                    }
                    else
                    {
                        if (i > 0) dp[i, j] = Math.Min(dp[i - 1, j] + 1, dp[i, j]);
                        if (j > 0) dp[i, j] = Math.Min(dp[i, j - 1] + 1, dp[i, j]);
                        if (i > 0 && j > 0)
                        {
                            if (text1[i] != text2[j]) dp[i, j] = Math.Min(dp[i - 1, j - 1] + 1, dp[i, j]);
                            else dp[i, j] = Math.Min(dp[i - 1, j - 1], dp[i, j]);
                        }
                    }
                }
            double dif = dp[len1 - 1, len2 - 1];
            dif /= (double)Math.Max(len1, len2);
            return 1 - dif;
        }
    }
    #endregion
    #region FileOpr
    /// <summary>
    /// 文件操作类
    /// </summary>
    public class FileOpr
    {
        /// <summary>
        /// 自定义文件遍历方法，根据筛选器进行筛选，并生成自定义数据类型列表
        /// </summary>
        /// <param name="folder">文件夹位置</param>
        /// <returns>遍历得到的文件列表</returns>
        public static List<AnsData> GetAllFiles(string folder)
        {
            DirectoryInfo myfolder = new DirectoryInfo(folder);
            FileInfo[] files = myfolder.GetFiles();
            List<AnsData> ans = new List<AnsData>();
            foreach (FileInfo file in files)
            {
                AnsData now = new AnsData(file.Name, file.Length, file.FullName);
                bool flag = true;
                foreach (string key in Consts.KEYWORDS)
                {
                    if (!now.fullfilename.Contains(key))
                    {
                        flag = false;
                        break;
                    }
                }
                if (flag) ans.Add(now);
            }
            return ans;
        }
    }
    #endregion
    #region AnsData
    /// <summary>
    /// 用户数据类，保存通过分析文件名得出的各个信息，以及不同数据之间的相似度计算
    /// 专门针对acm.buaa.edu.cn的提交日志进行分析，文件名格式如下[举例说明]
    /// [][70472][2012-12-15 12.07.36][buaa_gg][Wrong Answer-0.00][3MS-976KB][c++]
    /// 如文件名较为简单可自行分析
    /// </summary>
    public class AnsData : IComparable
    {
        /// <summary>
        /// 数据源，一般为空白或ip
        /// </summary>
        public string source;
        /// <summary>
        /// 提交日期
        /// </summary>
        public string date;
        /// <summary>
        /// 提交用户名
        /// </summary>
        public string user;
        /// <summary>
        /// 测试结果，一般为Accepted,Wrong Answer等
        /// </summary>
        public string result;
        /// <summary>
        /// 程序的运行时间
        /// </summary>
        public int time;
        /// <summary>
        /// 程序运行时占用的内存大小
        /// </summary>
        public int memory;
        /// <summary>
        /// 程序使用的语言，一般为C++,C,Java或Python
        /// </summary>
        public string language;
        /// <summary>
        /// 完整的文件名
        /// </summary>
        public string fullfilename;
        /// <summary>
        /// 代码文件的原始大小
        /// </summary>
        public long size;
        /// <summary>
        /// 代码总长度
        /// </summary>
        public string text;
        /// <summary>
        /// 自定义类型实例化
        /// </summary>
        /// <param name="filename">文件名</param>
        /// <param name="filesize">文件大小</param>
        /// <param name="fullname">带路径的文件名，用以提取各种信息</param>
        public AnsData(string filename, long filesize, string fullname)
        {
            string[] para = StringFunctions.mysplit(filename);
            source = para[0];
            date = para[2];
            user = para[3];
            result = StringFunctions.splitagain(para[4])[0];
            time = StringFunctions.stringtoint(StringFunctions.splitagain(para[5])[0]);
            memory = StringFunctions.stringtoint(StringFunctions.splitagain(para[5])[1]);
            language = para[6];
            fullfilename = fullname;
            size = filesize;
            StreamReader sr = new StreamReader(fullfilename, System.Text.Encoding.Default);
            text = StringFunctions.deleteEmpty(sr.ReadToEnd());
            sr.Close();
        }
        /// <summary>
        /// 
        /// </summary>
        public AnsData()
        {
        }
        /// <summary>
        /// 输出数据信息
        /// </summary>
        public void print()
        {
            Console.WriteLine(fullfilename + "\t" + memory + "\t" + size);
        }
        /// <summary>
        /// 自定义比较函数，用以排序等工作
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public int CompareTo(object obj)
        {
            int res = 0;
            try
            {
                AnsData ansobj = (AnsData)obj;
                if (this.memory > ansobj.memory) res = 1;
                else if (this.memory < ansobj.memory) res = -1;
                else
                {
                    if (this.size > ansobj.size) res = 1;
                    else if (this.size < ansobj.size) res = -1;
                }
            }
            catch (Exception e)
            {
                throw new Exception("compare error", e.InnerException);
            }
            return res;
        }
        /// <summary>
        /// 相似度计算方法，代码核心，调参数的理想之地
        /// </summary>
        /// <param name="other">另一个自定义类型</param>
        /// <returns>计算得到的相似度，在[0..1]范围内</returns>
        public double calsim(AnsData other)
        {
            if (language != other.language) return 0;
            double memsim = (double)Math.Min(other.memory, memory) / (double)Math.Max(other.memory, memory);
            double timesim = (double)Math.Min(other.time, time) / (double)Math.Max(other.time, time);
            if (memsim < Consts.MEMTHRESHOLD || timesim < Consts.TIMETHRESHOLD) return memsim * timesim * 0.5;
            double textsim = StringFunctions.calstringsim(other.text, text);
            return textsim * memsim * timesim;
        }
    }
    #endregion
}
