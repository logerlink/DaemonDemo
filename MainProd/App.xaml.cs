using Common;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace MainProd
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// 当前（守护）程序的双开标识
        /// </summary>
        private static Mutex mutex;
        protected override void OnStartup(StartupEventArgs e)
        {
            #region 避免双开
            //避免双开
            mutex = new Mutex(true, "MUTEX_DAEMON");
            if (mutex.WaitOne(0, false))
            {
                RunMonitorTimer();
                base.OnStartup(e);
            }
            else
            {
                MessageBox.Show($"无法双开守护程序"); 
                this.Shutdown();    //关闭此次打开的程序
            }
            #endregion
        }

        #region 检测主程序状态并重启
        /// <summary>
        /// 主进程互斥量
        /// </summary>
        private static Mutex mutex_main;

        /// <summary>
        /// 是否正在记录日志
        /// </summary>
        private bool isLoging { get; set; } = false;
        /// <summary>
        /// 是否记录日志完成
        /// </summary>
        private bool isLoged { get; set; } = false;

        /// <summary>
        /// 打开监视定时器
        /// </summary>
        public void RunMonitorTimer()
        {
            System.Timers.Timer timer = new System.Timers.Timer();
            timer.Elapsed += timer_Elapsed;
            timer.Interval = 2000;          //守护进程启动后2秒  每隔2秒检测一次主进程是否存在,若不存在则启动主进程
            timer.Start();
        }

        /// <summary>
        /// 定时任务检测守护进程是否存在   并记录闪退日志
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (mutex_main == null)
            {
                mutex_main = new Mutex(true, "MUTEX_MAIN");
            }
            if (mutex_main.WaitOne(0, false))
            {
                try
                {
                    //必须释放mutex，否则将导致mutex被占用，主程序不能允许
                    mutex_main.Dispose();
                    mutex_main = null;
                    if (!isLoging)  //避免多次执行
                    {
                        isLoging = true;
                        //"Application"应用程序, "Security"安全, "System"系统    这个记录很快的
                        //这里有一个很严重的bug就是  你每次通过守护进程启动主程序时，他都会去记录有关MainPro的错误日志，因为我们没有一个标识表明这次启动的原因是由于闪退重启还是其他原因（关不掉演示会重复记录错误日志）。
                        //不过我们可以通过时间筛选，如在只获取5或3分钟内的最后两条有关于MainPro的错误日志
                        int time = 0;
                        var eventLog = new EventLog("Application");
                        var logInfo = "";
                        for (int i = eventLog.Entries.Count - 1; i >= 0; i--)
                        {
                            var entry = eventLog.Entries[i];
                            if (time > 1) break;
                            if ((entry.EntryType == EventLogEntryType.Error || entry.EntryType == EventLogEntryType.FailureAudit) && entry.Message.Contains("MainPro.exe"))
                            {
                                //只记录近2分钟内的错误日志     减少重复录入相同的错误日志
                                if (entry.TimeGenerated.AddMinutes(3) > DateTime.Now)
                                {
                                    var info = $"在{entry.TimeGenerated}，程序闪退！！！！！{entry.Message}";
                                    logInfo += info + "\r\n";
                                }
                                time++;
                            }
                        }
                        if (!isLoged)   ////避免多次执行
                        {
                            isLoged = true;
                            if(!string.IsNullOrWhiteSpace(logInfo)) LogHelper.WriteLog(logInfo);
                            RunProcess();
                        }
                        isLoged = false;
                        isLoging = false;
                    }
                }
                catch (Exception ex)
                {
                    isLoged = false;
                    isLoging = false;
                    LogHelper.WriteLog($"【{DateTime.Now}】主程序崩溃记录异常失败！" + ex.ToString());
                }
            }
        }

        /// <summary>
        /// 打开主程序    MainPro
        /// </summary>
        public void RunProcess()
        {
            Process m_Process = new Process();
            var fileName = Process.GetCurrentProcess().MainModule.FileName;     ////当前程序exe完整路径
            m_Process.StartInfo.FileName = fileName.Replace("MainProd.exe", "MainPro.exe");
            m_Process.Start();
        }

        
        #endregion
    }
}
