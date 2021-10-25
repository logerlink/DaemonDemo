using Common;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace MainPro
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// 当前（主）程序的双开标识
        /// </summary>
        private static Mutex mutex;
        protected override void OnStartup(StartupEventArgs e)
        {
            #region 避免双开
            //避免双开
            mutex = new System.Threading.Mutex(true, "MUTEX_MAIN");
            if (mutex.WaitOne(0, false))
            {
                RunMonitorTimer();
                base.OnStartup(e);
            }
            else
            {
                MessageBox.Show($"无法双开主程序");
                this.Shutdown();    //关闭此次打开的程序
            }
            #endregion
        }

        #region 监视并启动守护进程
        /// <summary>
        /// 守护进程互斥量
        /// </summary>
        private static Mutex mutex_daemon;

        /// <summary>
        /// 打开监视定时器
        /// </summary>
        public void RunMonitorTimer()
        {
            System.Timers.Timer timer = new System.Timers.Timer();
            timer.Elapsed += timer_Elapsed;
            timer.Interval = 5000;          //主进程启动后5秒  开始启动守护进程，后续每5秒检测一次守护进程是否存在,若不存在则启动守护进程
            timer.Start();
        }
        /// <summary>
        /// 守护进程是否已经启动过了
        /// </summary>
        private bool isStarted { get; set; } = false;

        /// <summary>
        /// 定时任务检测守护进程是否存在
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (mutex_daemon == null)
            {
                mutex_daemon = new Mutex(true, "MUTEX_DAEMON");
            }
            if (mutex_daemon.WaitOne(0, false))
            {
                try
                {
                    //必须释放mutex，否则将导致mutex被占用，主程序不能允许
                    mutex_daemon.Dispose();
                    mutex_daemon = null;
                    if (!isStarted)
                    {
                        isStarted = true;
                        RunProcess();
                    }
                    isStarted = false;
                }
                catch (Exception ex)
                {
                    isStarted = false;
                    MessageBox.Show($"【{DateTime.Now}】守护程序重启失败！" + ex.ToString());
                }
            }
        }

        /// <summary>
        /// 打开守护程序 MainProd
        /// </summary>
        public void RunProcess()
        {
            Process m_Process = new Process();
            var fileName = Process.GetCurrentProcess().MainModule.FileName; //当前程序exe完整路径
            m_Process.StartInfo.FileName = fileName.Replace("MainPro.exe", "MainProd.exe");
            //严谨一点可以判断进程名称  但是进程名称可能会冲突。。。
            //if(!ProcessHelper.CheckProcess("MainProd")) m_Process.Start();
            m_Process.Start();
        }
        #endregion
    }
}
