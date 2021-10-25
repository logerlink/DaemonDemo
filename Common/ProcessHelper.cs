using System;
using System.Diagnostics;

namespace Common
{
    public class ProcessHelper
    {
        /// <summary>
        /// 强制杀死进程
        /// </summary>
        /// <param name="processName"></param>
        /// <returns>全部杀死则返回true</returns>
        public static bool KillProcess(params string[] processNameArr)
        {
            bool isAllKill = false;
            foreach (var item in processNameArr)
            {
                try
                {
                    isAllKill = false;
                    Process[] p2 = Process.GetProcessesByName(item);
                    foreach (var process in p2)
                    {
                        if (process.MainWindowTitle != "信息")
                        {
                            process.Kill();
                            isAllKill = true;
                        }
                    }

                    //p[0].Kill();
                    //MessageBox.Show("进程关闭成功！");
                }
                catch (Exception ex)
                {
                    LogHelper.WriteLog($"杀死进程{item}失败！" + ex.ToString());
                    //MessageBox.Show("无法关闭此进程！");
                }
            }
            if (isAllKill == true) return true;
            return false;
        }


        /// <summary>
        /// 检测是否存在某线程
        /// </summary>
        /// <param name="processName"></param>
        /// <returns>存在则返回true</returns>
        public static bool CheckProcess(string processName)
        {
            Process[] p2 = Process.GetProcessesByName(processName);
            return p2.Length > 0;
        }

    }
}
