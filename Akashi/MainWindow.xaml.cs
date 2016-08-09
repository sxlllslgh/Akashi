using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows;

namespace Akashi
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool status = false;
        private String phpcgiUri;

        public MainWindow()
        {
            InitializeComponent();
            if (CultureInfo.CurrentCulture.Name == "zh-CN")
            {
                Application.Current.Resources.MergedDictionaries[0] = new ResourceDictionary()
                {
                    Source = new Uri("lang\\zh-CN.xaml", UriKind.RelativeOrAbsolute)
                };  
            }
            PHPCGIPath.Text = Properties.Settings.Default.PHPCGIPath;
            NginxPath.Text = Properties.Settings.Default.NginxPath;
            phpcgiUri = getNginxCgiConfig(NginxPath.Text + "\\conf\\nginx.conf");
            if (isProcsRunning())
            {
                changeButtonStatus(true);
                status = true;
            }
        }

        private bool isProcsRunning()
        {
            Process[] phpProc = Process.GetProcessesByName("php-cgi");
            Process[] nginxProc = Process.GetProcessesByName("nginx");
            if (phpProc.Length == 0 && nginxProc.Length == 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        private void changeButtonStatus(bool flag)
        {
            if (flag == true)
            {
                RunStopButton.Content = Application.Current.FindResource("stop").ToString();
                RestartButton.Visibility = Visibility.Visible;
            }
            else
            {
                RunStopButton.Content = Application.Current.FindResource("run").ToString();
                RestartButton.Visibility = Visibility.Collapsed;
            }
        }

        private void changeStatus(bool flag)
        {
            changeButtonStatus(flag);
            if (flag == true)
            {
                Process phpCgiProc = new Process();
                phpCgiProc.StartInfo.WorkingDirectory = PHPCGIPath.Text;
                phpCgiProc.StartInfo.FileName = "php-cgi.exe";
                phpCgiProc.StartInfo.Arguments = phpcgiUri;
                phpCgiProc.StartInfo.UseShellExecute = false;
                phpCgiProc.StartInfo.CreateNoWindow = true;
                phpCgiProc.Start();
                Process nginxProc = new Process();
                nginxProc.StartInfo.WorkingDirectory = NginxPath.Text;
                nginxProc.StartInfo.FileName = "nginx.exe";
                nginxProc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                nginxProc.Start();
                
            }
            else
            {
                Process[] phpProc = Process.GetProcessesByName("php-cgi");
                foreach (Process item in phpProc)
                {
                    item.Kill();
                }
                Process nginxProc = new Process();
                nginxProc.StartInfo.WorkingDirectory = NginxPath.Text;
                nginxProc.StartInfo.FileName = "nginx.exe";
                nginxProc.StartInfo.Arguments = "-s quit";
                nginxProc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                nginxProc.Start();
            }
            status = flag;
        }

        private String getNginxCgiConfig(String path)
        {
            StreamReader sr = new StreamReader(path, Encoding.Default);
            String line, addr = "";
            while ((line = sr.ReadLine()) != null)
            {
                if ((addr = line.Trim().Split(' ')[0]) == "fastcgi_pass")
                {
                    addr = line.Trim().Split(' ')[line.Trim().Split(' ').Length - 1];
                    break;
                }
            }
            return "-b " + addr.Replace(";", "");;
        }

        private void RunStopButton_Click(object sender, RoutedEventArgs e)
        {
            changeStatus(!status);
        }

        private void RestartButton_Click(object sender, RoutedEventArgs e)
        {
            changeStatus(false);
            while (isProcsRunning()) { }
            changeStatus(true);
        }

        private void SaveConfig(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.PHPCGIPath = PHPCGIPath.Text;
            Properties.Settings.Default.NginxPath = NginxPath.Text;
            Properties.Settings.Default.Save();
        }
    }
}
