using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Windows;
using System.Windows.Media.Imaging;
using AtoiHomeServiceLib;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows.Controls;
using MahApps.Metro.Controls;
using System.Collections.Generic;
using System.Windows.Shapes;
using System.Windows.Markup;
using System.Windows.Data;
using System.Globalization;
using System.Net.Mail;
using System.Net;

namespace AtoiHomeManager
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
#if DEBUG
        public static string HostAddr = "net.pipe://localhost/test/Notify";
#else
        public static string HostAddr = "net.pipe://localhost/Notify";
#endif
        private static INotifyService IPCNotify;
        private static int iCurImageRotation=0;

        private void ModelEventHandler(Object sender, ModelContextArgs e)
        {
            try
            {
                switch (e.MsgType)
                {
                    case MessageType.DOWNLOAD_IMAGE:
                        DrawScreenshotImageAndSave(e.Message);
                        //DrawImageFromFile(e.Message);
                        break;
                    case MessageType.NOTIFYSERVICE_CLOSING:
                        TextTransferEventArgs disconnect = new TextTransferEventArgs("atoi", MessageType.NOTIFYSERVICE_CLOSING, "Bye", null);
                        // 알림서브스에서 중지 메시지를 받으면 클라이언트에서 연결을 끊겠다는 메시지를 반송하여 서비스 종료를 명확하게 한다.
                        // 이를 하지 않으면 서비스 종료가 지연됨
                        try
                        {
                            IPCNotify.Disconnect(new TextTransferEventArgs("atoi", MessageType.GET_DATA, "bye", null));
                            (Application.Current as App).bConnected = false;
                            (buttonConnect.Content as StackPanel).FindChild<Image>("buttonConnectImage").Source = new BitmapImage(new Uri(Properties.Resources.DisconnectImagePath));
                            (buttonConnect.Content as StackPanel).FindChild<TextBlock>("tbConnect").Text = "Disconnected";
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message);
                        }

                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        System.Windows.Forms.Screen secondaryScreen = System.Windows.Forms.Screen.AllScreens.Where(s => !s.Primary).FirstOrDefault();
        MainWindowViewModel _MainWindowViewModel = new MainWindowViewModel();

        public MainWindow()
        {
            InitializeComponent();
            (Application.Current as App).onModelContextEvent += new ModelContextEvent(ModelEventHandler);
            DataContext = _MainWindowViewModel;
        }

        private void ButtonConnect_Click(object sender, RoutedEventArgs e)
        {
            if ((Application.Current as App).bConnected == false)
            {
                try
                {
                    if ((Application.Current as App).bConnected = ConnectToIPCService())
                    {

                        ((sender as Button).Content as StackPanel).FindChild<Image>("buttonConnectImage").Source = new BitmapImage(new Uri(Properties.Resources.ConnectImagePath));
                        ((sender as Button).Content as StackPanel).FindChild<TextBlock>("tbConnect").Text = "Connected";
                    }
                        
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else
            {
                try
                {
                    IPCNotify.Disconnect(new TextTransferEventArgs("atoi", MessageType.GET_DATA, "bye", null));
                    (Application.Current as App).bConnected = false;
                    ((sender as Button).Content as StackPanel).FindChild<Image>("buttonConnectImage").Source = new BitmapImage(new Uri(Properties.Resources.DisconnectImagePath));
                    ((sender as Button).Content as StackPanel).FindChild<TextBlock>("tbConnect").Text = "Disconnected";
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }

            }
        }

        private void ButtonDownloadImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                DrawImageFromFile(openFileDialog.FileName);
            }
        }

        public void DrawImageFromFile(string strImageFilePath)
        {
            try
            {
                MemoryStream ms = new MemoryStream();
                BitmapImage bi = new BitmapImage();
                byte[] arrbytFileContent = File.ReadAllBytes(strImageFilePath);
                ms.Write(arrbytFileContent, 0, arrbytFileContent.Length);
                ms.Position = 0;
                bi.BeginInit();
                bi.StreamSource = ms;
                bi.EndInit();
                imageDownload.Source = bi;

                iCurImageRotation = 0;
                imageDownload.RenderTransform = new RotateTransform(iCurImageRotation);

                slider.Value = 0;
                if (bi.PixelWidth > bi.PixelHeight)
                {
                    if (bZoomView == true)
                        return;
                    bZoomView = true;
                    scrollViewer.PreviewMouseWheel += OnPreviewMouseWheel;
                    (buttonViewMode.Content as StackPanel).FindChild<Image>("buttonViewModeImage").Source = new BitmapImage(new Uri(Properties.Resources.ZoomViewImagePath));
                    (buttonViewMode.Content as StackPanel).FindChild<TextBlock>("tbViewMode").Text = "ZoomView";


                }
                else
                {
                    if (bZoomView == false)
                        return;
                    bZoomView = false;
                    scrollViewer.PreviewMouseWheel -= OnPreviewMouseWheel;
                    (buttonViewMode.Content as StackPanel).FindChild<Image>("buttonViewModeImage").Source = new BitmapImage(new Uri(Properties.Resources.ScrollViewImagePath));
                    (buttonViewMode.Content as StackPanel).FindChild<TextBlock>("tbViewMode").Text = "ScrollView";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }

 
        public void DrawScreenshotImageAndSave(string strFilename)
        {
            //creating the object of WCF service client       
#if DEBUG
            TestTextTransferServiceSoap.TextTransferSoapClient WCFClient = new TestTextTransferServiceSoap.TextTransferSoapClient();
#else
            TextTransferServiceSoap.TextTransferSoapClient WCFClient = new TextTransferServiceSoap.TextTransferSoapClient();
#endif
            try
            {
                Stream streamInput = WCFClient.DownloadImage(strFilename);
                String strSaveFullPath = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures) + "\\" + strFilename;

                FileStream fs = new FileStream(strSaveFullPath, FileMode.Create);
                int b, i = 0;
                do
                {
                    b = streamInput.ReadByte(); //read next byte from stream  
                    fs.WriteByte((byte)b); //write byte to local file  
                    i++;
                } while (b != -1);
                streamInput.Close();
                fs.Close();
                DrawImageFromFile(strSaveFullPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
                Console.Write(ex.Message);
            }
            finally
            {
                WCFClient.Close();
            }
        }


        Point? lastCenterPositionOnTarget;
        Point? lastMousePositionOnTarget;
        Point? lastDragPoint;

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if ((Application.Current as App).bConnected)
                {
                    (buttonConnect.Content as StackPanel).FindChild<Image>("buttonConnectImage").Source = new BitmapImage(new Uri(Properties.Resources.ConnectImagePath));
                    (buttonConnect.Content as StackPanel).FindChild<TextBlock>("tbConnect").Text = "Connected";
                }
                else
                {
                    (buttonConnect.Content as StackPanel).FindChild<Image>("buttonConnectImage").Source = new BitmapImage(new Uri(Properties.Resources.DisconnectImagePath));
                    (buttonConnect.Content as StackPanel).FindChild<TextBlock>("tbConnect").Text = "Disconnected";
                }

                if (secondaryScreen != null)
                {
                    this.Left = (int)SystemParameters.PrimaryScreenWidth + GetWorkingArea().Width - ActualWidth;
                }
                else
                {
                    this.Left = (int)SystemParameters.PrimaryScreenWidth - ActualWidth;
                }


#if DEBUG
                this.Title += " - Debug";
#else
                this.Title += " - Release";
#endif

                scrollViewer.ScrollChanged += OnScrollViewerScrollChanged;
                scrollViewer.MouseLeftButtonUp += OnMouseLeftButtonUp;
                scrollViewer.PreviewMouseLeftButtonUp += OnMouseLeftButtonUp;

                scrollViewer.PreviewMouseLeftButtonDown += OnMouseLeftButtonDown;
                scrollViewer.MouseMove += OnMouseMove;

                slider.ValueChanged += OnSliderValueChanged;

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }


        void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (lastDragPoint.HasValue)
            {
                Point posNow = e.GetPosition(scrollViewer);

                double dX = posNow.X - lastDragPoint.Value.X;
                double dY = posNow.Y - lastDragPoint.Value.Y;

                lastDragPoint = posNow;

                scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset - dX);
                scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - dY);
            }
        }

        void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var mousePos = e.GetPosition(scrollViewer);
            if (mousePos.X <= scrollViewer.ViewportWidth && mousePos.Y < scrollViewer.ViewportHeight) //make sure we still can use the scrollbars
            {
                scrollViewer.Cursor = Cursors.SizeAll;
                lastDragPoint = mousePos;
                Mouse.Capture(scrollViewer);
            }
        }

        void OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            lastMousePositionOnTarget = Mouse.GetPosition(gridImage);

            if (e.Delta > 0)
            {
                slider.Value += 1;
            }
            if (e.Delta < 0)
            {
                slider.Value -= 1;
            }

            e.Handled = true;
        }

        void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            scrollViewer.Cursor = Cursors.Arrow;
            scrollViewer.ReleaseMouseCapture();
            lastDragPoint = null;
        }

        void OnSliderValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            scaleTransform.ScaleX = e.NewValue;
            scaleTransform.ScaleY = e.NewValue;

            var centerOfViewport = new Point(scrollViewer.ViewportWidth / 2, scrollViewer.ViewportHeight / 2);
            lastCenterPositionOnTarget = scrollViewer.TranslatePoint(centerOfViewport, gridImage);
        }

        void OnScrollViewerScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (e.ExtentHeightChange != 0 || e.ExtentWidthChange != 0)
            {
                Point? targetBefore = null;
                Point? targetNow = null;

                if (!lastMousePositionOnTarget.HasValue)
                {
                    if (lastCenterPositionOnTarget.HasValue)
                    {
                        var centerOfViewport = new Point(scrollViewer.ViewportWidth / 2,
                                                         scrollViewer.ViewportHeight / 2);
                        Point centerOfTargetNow =
                              scrollViewer.TranslatePoint(centerOfViewport, gridImage);

                        targetBefore = lastCenterPositionOnTarget;
                        targetNow = centerOfTargetNow;
                    }
                }
                else
                {
                    targetBefore = lastMousePositionOnTarget;
                    targetNow = Mouse.GetPosition(gridImage);

                    lastMousePositionOnTarget = null;
                }

                if (targetBefore.HasValue)
                {
                    double dXInTargetPixels = targetNow.Value.X - targetBefore.Value.X;
                    double dYInTargetPixels = targetNow.Value.Y - targetBefore.Value.Y;

                    double multiplicatorX = e.ExtentWidth / gridImage.Width;
                    double multiplicatorY = e.ExtentHeight / gridImage.Height;

                    double newOffsetX = scrollViewer.HorizontalOffset -
                                        dXInTargetPixels * multiplicatorX;
                    double newOffsetY = scrollViewer.VerticalOffset -
                                        dYInTargetPixels * multiplicatorY;

                    if (double.IsNaN(newOffsetX) || double.IsNaN(newOffsetY))
                    {
                        return;
                    }

                    scrollViewer.ScrollToHorizontalOffset(newOffsetX);
                    scrollViewer.ScrollToVerticalOffset(newOffsetY);
                }
            }
        }
        public static bool ConnectToIPCService()
        {
            try
            {
                var callback = new App.NotifyCallback();
                var context = new InstanceContext(callback);
                NetNamedPipeBinding IPCBinding = new NetNamedPipeBinding(NetNamedPipeSecurityMode.None);
                IPCBinding.ReceiveTimeout = TimeSpan.MaxValue;
                var pipeFactory =
                     new DuplexChannelFactory<INotifyService>(context,
                     IPCBinding,
                new EndpointAddress(HostAddr));
                IPCNotify = pipeFactory.CreateChannel();

                TextTransferEventArgs e = new TextTransferEventArgs("atoi", MessageType.GET_DATA, "Hi", null);
                IPCNotify.Connect(e);
                return true;

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {

#if DEBUG
                if ((Application.Current as App).bConnected == true)
                {
                    IPCNotify.Disconnect(new TextTransferEventArgs("atoi", MessageType.GET_DATA, "bye", null));
                    Application.Current.Shutdown();
                }
#endif
#if APPBAR
                UnregisterBar();
#endif
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void ButtonImageRotation_Click(object sender, RoutedEventArgs e)
        {
            if (iCurImageRotation >= 270)
                iCurImageRotation = 0;
            else
                iCurImageRotation += 90;
            
            imageDownload.RenderTransform = new RotateTransform(iCurImageRotation);
        }

        private bool bZoomView = false;
        private void ButtonViewMode_Click(object sender, RoutedEventArgs e)
        {
            bZoomView = !bZoomView;
            if (bZoomView == true)
            {
                try
                {
                    scrollViewer.PreviewMouseWheel += OnPreviewMouseWheel;
                    ((sender as Button).Content as StackPanel).FindChild<Image>("buttonViewModeImage").Source = new BitmapImage(new Uri(Properties.Resources.ZoomViewImagePath));
                    (buttonViewMode.Content as StackPanel).FindChild<TextBlock>("tbViewMode").Text = "ZoomView";
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message.ToString());
                }
            }
            else
            {
                try
                {
                    scrollViewer.PreviewMouseWheel -= OnPreviewMouseWheel;
                    ((sender as Button).Content as StackPanel).FindChild<Image>("buttonViewModeImage").Source = new BitmapImage(new Uri(Properties.Resources.ScrollViewImagePath));
                    (buttonViewMode.Content as StackPanel).FindChild<TextBlock>("tbViewMode").Text = "ScrollView";
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message.ToString());
                }
            }
        }

        private void ButtonGetPublicIP_Click(object sender, RoutedEventArgs e)
        {
            if ((Application.Current as App).bConnected == true)
            {
                try
                {
                    IpInfo ipInfo;
                    ipInfo = IPCNotify.GetHostPublicIP();
                    MessageBox.Show(this, "Public IPAddress : " + ipInfo.strPublicIP + "\r\n" + "Local IPAddress  : " + ipInfo.strLocalIP, "AtoiHomeManager");
                }
                catch (FaultException<CustomerServiceFault> fault)
                {
                    MessageBox.Show($"Fault received: {fault.Detail.ErrorMessage}");
                }
            }
        }
    }
}
