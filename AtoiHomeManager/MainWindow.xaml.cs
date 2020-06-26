using MahApps.Metro.Controls;
using System;
using System.Linq;
using System.Windows;
using AtoiHomeManager.Source.Utils;
namespace AtoiHomeManager
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public static System.Windows.Forms.Screen secondaryScreen = System.Windows.Forms.Screen.AllScreens.Where(s => !s.Primary).FirstOrDefault();
        MainWindowViewModel _MainWindowViewModel = new MainWindowViewModel();

        public MainWindow()
        {
            InitializeComponent();
            DataContext = _MainWindowViewModel;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (secondaryScreen != null)
                {
                    Left = (int)SystemParameters.PrimaryScreenWidth + Utility.GetWorkingArea().Width - ActualWidth;
                }
                else
                {
                    Left = (int)SystemParameters.PrimaryScreenWidth - ActualWidth;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                if ((Application.Current as App).bConnected == true)
                {
                    Network.DisconnectService();
#if DEBUG
                    Application.Current.Shutdown();
#endif
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
