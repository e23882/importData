using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Data.SQLite;
using System.Runtime.InteropServices;
using System.IO;
using System.Xml;
using Microsoft.Win32;
using System.Threading;
using System.Data.Common;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace CommodityinfoToSQLLite
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window,INotifyPropertyChanged
    {
        #region Declarations
        List<Data> list = new List<Data>();
        List<string> errorData = new List<string>();
        #endregion Declarations

        #region Member Data
        object obj = new object();
        private string _filePath = string.Empty;
        private string _SQLPath = string.Empty;
        string _connectString = string.Empty;
        private int _totalCount = 0;
        private int _count = 0;
        private string _mode = string.Empty;
        #endregion

        #region Property
        public event PropertyChangedEventHandler PropertyChanged;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="propertyName"></param>
        public void onPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        /// <summary>
        /// 執行模式
        /// </summary>
        public string Mode
        {
            get
            {
                return _mode;
            }
            set
            {
                _mode = value;
                onPropertyChanged("Mode");
            }
        }
        /// <summary>
        /// 商品檔路徑
        /// </summary>
        public string FilePath
        {
            get
            {
                return _filePath;
            }
            set
            {
                _filePath = value;
                onPropertyChanged("FilePath");
            }
        }
        /// <summary>
        /// SQLLite檔案路徑
        /// </summary>
        public string SQLPath
        {
            get
            {
                return _SQLPath;
            }
            set
            {
                _SQLPath = value;
                onPropertyChanged("SQLPath");
            }
        }
        /// <summary>
        /// 資料庫連線字串
        /// </summary>
        public string ConnectionString
        {
            get
            {
                return string.Format(@"Data Source={0};Pooling=true;FailIfMissing=false", SQLPath);
            }
        }
        /// <summary>
        /// 顯示進度
        /// </summary>
        public string Process
        {
            get
            {
                return Count + "/" + TotalCount;
            }
        }
        /// <summary>
        /// 目前匯入資料筆數
        /// </summary>
        public int Count
        {
            get
            {
                return _count;
            }
            set
            {
                _count = value;
                onPropertyChanged("Count");
                onPropertyChanged("Process");
            }
        }
        /// <summary>
        /// 所有資料筆數
        /// </summary>
        public int TotalCount
        {
            get
            {
                return _totalCount;
            }
            set
            {
                _totalCount = value;
                onPropertyChanged("Process");
                onPropertyChanged("TotalCount");
            }
        }
        public class Data
        {
            public string ID { get; set; }
            public string Value { get; set; }
        }
        #endregion

        #region Member Function
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool AllocConsole();

        [DllImport("Kernel32")]
        public static extern void FreeConsole();

        public MainWindow()
        {
            readSetting();
            InitializeComponent();
            tbPath.DataContext = tbSQLPath.DataContext = tbProcess.DataContext= pgProgress.DataContext= this;

            string[] args = Environment.GetCommandLineArgs();
            if (args.Count() > 1)
            {
                string tempMode = args[1].ToUpper();
                if (tempMode.Equals("AUTO"))
                {
                    Mode = "AUTO";
                }
            }
            if (Mode == "AUTO")
            {
                AllocConsole();
                ConsoleColor oriColor = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("目前資料庫位置 : "+FilePath);
                Console.WriteLine("開始匯入資料...");
                Console.ForegroundColor = oriColor;
                readCsv();
                Console.WriteLine("匯入完成");
                this.Close();
            }
        }
     
        /// <summary>
        /// 讀取設定(最後一次關閉程式儲存的儲存商品檔 SQL位置)
        /// </summary>
        public void readSetting()
        {
            if (System.IO.File.Exists("setting.xml"))
            {
                XmlDocument doc = new XmlDocument();
                doc.Load("Setting.xml");

                XmlNodeList NodeLists = doc.SelectNodes("PathSetting/Information");
                foreach (XmlNode OneNode in NodeLists)
                {
                    if (OneNode.Attributes["Type"] != null && OneNode.Attributes["Path"] != null)
                    {
                        if (OneNode.Attributes["Type"].Value.Equals("CommodityInfo"))
                            this.FilePath = OneNode.Attributes["Path"].Value;
                        else if (OneNode.Attributes["Type"].Value.Equals("DataBase"))
                            this.SQLPath = OneNode.Attributes["Path"].Value;
                    }
                }
            }
            else
            {
                XmlDocument doc = new XmlDocument();
                XmlElement Path = doc.CreateElement("PathSetting");
                doc.AppendChild(Path);
                XmlElement info = doc.CreateElement("Information");
                info.SetAttribute("Type", "CommodityInfo");
                info.SetAttribute("Path", System.Environment.CurrentDirectory + @"\CommodityInfo.csv");
                this.FilePath = System.Environment.CurrentDirectory + @"\CommodityInfo.csv";
                Path.AppendChild(info);
                info = doc.CreateElement("Information");
                info.SetAttribute("Type", "DataBase");
                info.SetAttribute("Path", System.Environment.CurrentDirectory + @"\DataBase.db3");
                this.FilePath = System.Environment.CurrentDirectory + @"\DataBase.db3";
                Path.AppendChild(info);
                doc.Save("Setting.xml");
            }
        }
        /// <summary>
        /// 讀商品檔
        /// </summary>
        public void readCsv()
        {
            //暫存字串,讀取商品檔存到後暫存變數方便存取
            string temp = string.Empty;
            //檔案目前所在行數
            int rowCount = 1;
            //重設商品檔總數.目前新增數量
            TotalCount = 0;
            Count = 0;
            list.Clear();

            Action makeBtNotEnable = delegate ()
            {
                this.btImportSQL.IsEnabled = false;
                this.btImportCsv.IsEnabled = false;
                this.btChooseCsv.IsEnabled = false;
                this.btChooseSQL.IsEnabled = false;
            };
            Action makeBtEnable = delegate ()
            {
                this.btImportSQL.IsEnabled = true;
                this.btImportCsv.IsEnabled = true;
                this.btChooseCsv.IsEnabled = true;
                this.btChooseSQL.IsEnabled = true;
            };
            this.Dispatcher.BeginInvoke(makeBtNotEnable);

            try
            {
                if (System.IO.File.Exists(FilePath))
                {
                    System.IO.StreamReader CalTotalCount = new System.IO.StreamReader(FilePath);
                    while ((temp = CalTotalCount.ReadLine()) != null)
                    {
                        if (!string.IsNullOrEmpty(temp))
                            TotalCount++;
                    }
                    System.IO.StreamReader file = new System.IO.StreamReader(FilePath);
                    if (Mode.Equals("AUTO"))
                    {
                        while ((temp = file.ReadLine()) != null)
                        {
                            if (!string.IsNullOrEmpty(temp) && temp.Contains(","))
                                list.Add(new Data { ID = temp.Substring(0, temp.IndexOf(",")).Replace("'", "’"), Value = temp.Substring(temp.IndexOf(","), temp.Length - temp.IndexOf(",")).Replace("'", "’") });
                            else
                                WriteLog(DateTime.Now + " 商品檔第 " + rowCount + "行 資料格式不正確沒有新增 :" + temp);
                        }
                    }
                    else
                    {
                        lock (obj)
                        {
                            while ((temp = file.ReadLine()) != null)
                            {
                                if (!string.IsNullOrEmpty(temp) && temp.Contains(","))
                                    list.Add(new Data { ID = temp.Substring(0, temp.IndexOf(",")).Replace("'", "’"), Value = temp.Substring(temp.IndexOf(","), temp.Length - temp.IndexOf(",")).Replace("'", "’") });
                                else
                                    WriteLog(DateTime.Now + " Row " + rowCount + "資料格式不正確沒有新增 :" + temp);
                            }
                        }
                    }
                    lock (obj)
                    {
                        SQLiteConnection dbConnection = new SQLiteConnection(ConnectionString);
                        DbProviderFactory factory = SQLiteFactory.Instance;
                        using (DbConnection conn = factory.CreateConnection())
                        {
                            conn.ConnectionString = ConnectionString;
                            conn.Open();
                            DbCommand cmd = conn.CreateCommand();
                            cmd.Connection = conn;
                            DbTransaction trans = conn.BeginTransaction();
                            ConcurrentQueue<Data> Queue = new ConcurrentQueue<Data>();

                            foreach (var item in list)
                            {
                                try
                                {
                                    cmd.CommandText = string.Format("Replace Into CommodityInfo Values('{0}','{1}',datetime('now', 'localtime'));", item.ID, item.Value);
                                    cmd.ExecuteNonQuery();
                                    Count++;
                                    Console.WriteLine(Count + "/" + TotalCount);
                                }
                                catch (Exception ie)
                                {
                                    WriteLog(DateTime.Now + "新增失敗 : \n" + cmd.CommandText);
                                }

                            }
                            trans.Commit();
                            if (Mode == "AUTO")
                                Console.WriteLine("done");
                            else
                            {
                                MessageBox.Show("done");
                                this.Dispatcher.BeginInvoke(makeBtEnable);
                            }
                        }
                    }
                }
                else
                {
                    if (Mode.Equals("AUTO"))
                        Console.WriteLine("找不到商品檔。");
                    else
                        MessageBox.Show("找不到商品檔。");
                }
            }
            catch (Exception ie)
            {
                if (Mode.Equals("AUTO"))
                {
                    Console.WriteLine("讀取商品檔發生例外 " + ie.StackTrace);
                    Console.ReadLine();
                }
                else
                    MessageBox.Show("讀取商品檔發生例外 " + ie.StackTrace);
                this.Dispatcher.BeginInvoke(makeBtEnable);
            }
            finally
            {
                this.Dispatcher.BeginInvoke(makeBtEnable);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btChoose_Click(object sender, RoutedEventArgs e)
        {
            bool? result;
            OpenFileDialog openFileDialog;
            Button bt = (sender as Button);
            if (bt == null)
                return;
            if (bt.Name.Equals("btChooseCsv"))//選商品檔
            {
                openFileDialog = new Microsoft.Win32.OpenFileDialog()
                {
                    Filter = "Excel Files (*.csv)|*.csv"
                };
                result = openFileDialog.ShowDialog();
                if (result == true)
                {
                    FilePath = openFileDialog.FileName;
                }
            }
            else if (bt.Name.Equals("btChooseSQL"))//選SQLLITE檔案
            {
                openFileDialog = new Microsoft.Win32.OpenFileDialog()
                {
                    Filter = "Excel Files (*.db3)|*.db3"
                };
                result = openFileDialog.ShowDialog();
                if (result == true)
                    SQLPath = openFileDialog.FileName;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btImportSQL_Click(object sender, RoutedEventArgs e)
        {
            
            Thread th = new Thread(readCsv);
            th.IsBackground = true;
            th.Start();
        }
        /// <summary>
        /// 關視窗事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closed(object sender, EventArgs e)
        {
            XmlDocument doc = new XmlDocument();
            XmlElement Path = doc.CreateElement("PathSetting");
            doc.AppendChild(Path);
            XmlElement info = doc.CreateElement("Information");
            info.SetAttribute("Type", "CommodityInfo");
            info.SetAttribute("Path", FilePath);
            Path.AppendChild(info);
            info = doc.CreateElement("Information");
            info.SetAttribute("Type", "DataBase");
            info.SetAttribute("Path", SQLPath);
            Path.AppendChild(info);
            doc.Save("Setting.xml");
        }
        /// <summary>
        /// 把資料從SQLLite匯入商品檔(舊的資料會被清掉)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btImportCsv_Click(object sender, RoutedEventArgs e)
        {
            SQLiteConnection dbConnection = new SQLiteConnection(ConnectionString);
            dbConnection.Open();
            SQLiteCommand cmd = new SQLiteCommand("Select * from CommodityInfo", dbConnection);
            var csv = new StringBuilder();
            var result = MessageBox.Show("這個動作會將目前的商品檔資料給取代為資料庫中的資料，舊的商品檔資料將不會被保留，你確定要繼續嗎?", "確認", MessageBoxButton.YesNo);
            if (result.ToString().Equals("Yes"))
            {
                try
                {
                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var id = reader[0].ToString().Replace("’","'");
                            var data = reader[1].ToString().Replace("’", "'");
                            var newLine = id + data;
                            csv.AppendLine(newLine);
                        }
                    }
                    File.WriteAllText(FilePath, csv.ToString());
                    MessageBox.Show("done");
                }
                catch (Exception ie)
                {
                    MessageBox.Show("SQLLite匯出資料發生例外 /n"+ie.StackTrace);
                }
            }
        }
        /// <summary>
        /// 寫入log檔
        /// </summary>
        /// <param name="logText"></param>
        public void WriteLog(string logText)
        {
            if (!File.Exists("ImportError.log"))
            {
                FileStream fs = File.Create(System.Environment.CurrentDirectory+ @"\"+ "ImportError.log");
                fs.Close();
            }
            //第二個參數設定為true表示不覆蓋原本的內容，把新內容直接添加進去
            StreamWriter sw = new StreamWriter("ImportError.log", true);
            sw.WriteLine(logText);
            sw.Close();
        }
        #endregion
    }
}
