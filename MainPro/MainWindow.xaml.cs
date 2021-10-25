using Common;
using System;
using System.Collections.Generic;
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

namespace MainPro
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            new Thread(() => {
                string ss = null;
                var sss = ss.ToString();    //空指针异常
            }).Start();

            Thread.Sleep(1000);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //关闭主程序时，先关闭守护进程再关闭主程序
            ProcessHelper.KillProcess("MainProd", "MainPro");   //注意不要加exe
        }
    }
}
