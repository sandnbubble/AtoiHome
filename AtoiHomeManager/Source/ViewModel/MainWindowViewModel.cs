using AtoiHomeManager.Source.Utils;
using AtoiHomeManager.Source.View;
using AtoiHomeServiceLib;
using MahApps.Metro.Controls;
using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using static AtoiHomeManager.Source.Utils.Utility;

namespace AtoiHomeManager
{
    class BaseViewModel : INotifyPropertyChanged
    {
        #region 이벤트 처리
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region RelayCommand
        public class RelayCommand : ICommand
        {
            private Action<object> _action;
            private Func<object, bool> _canExecute;

            public RelayCommand(Action<object> action, Func<object,bool> canExecute=null)
            {
                _action = action;
                _canExecute = canExecute;
            }

            public event EventHandler CanExecuteChanged
            {
                add { CommandManager.RequerySuggested += value; }
                remove { CommandManager.RequerySuggested -= value; }
            }

            public bool CanExecute(object parameter)
            {
                return _canExecute(parameter);
            }

            public void Execute(object parameter)
            {
                _action(parameter);
            }
        }
        #endregion

    }

    class MainWindowViewModel : BaseViewModel
    {
        //App에서 발행한 이벤트를 처리하기 위한 이벤트핸들러
        //구독은 생성자에서 하고 있다.
        private void ModelEventHandler(object sender, ModelContextArgs e)
        {
            try
            {
                switch (e.MsgType)
                {
                    case MessageType.DOWNLOAD_IMAGE:
                        ImagePath = ImageFilesFolderPath + "\\" + e.Message;
                        break;
                    case MessageType.NOTIFYSERVICE_CLOSING:
                        OneClickShotEventArgs disconnect = new OneClickShotEventArgs(Network.strEmail, null, MessageType.NOTIFYSERVICE_CLOSING, "Bye", null);
                        // 알림서비스에서 서비스종료메시지를 받으면 서비스 종료가 지연되지 않도록 연결을 끊는다
                        try
                        {
                            Network.NotificationService.Disconnect(new OneClickShotEventArgs(Network.strEmail, null, MessageType.DISCONNECTED_CLIENT, "bye", null));
                            (Application.Current as App).bConnected = false;
                            ServiceStatus = false;
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

        #region 생성자
        public MainWindowViewModel()
        {
            ImageFilesFolderPath = Utility.getRegValue("atoihome", "UploadPath");
            (Application.Current as App).onModelContextEvent += new ModelContextEvent(ModelEventHandler);
        }
        #endregion


        private string _ImageFilesFolderPath;
        public string ImageFilesFolderPath
        {
            get { return _ImageFilesFolderPath; }
            set
            {
                _ImageFilesFolderPath = value;
                OnPropertyChanged("ImageFilesFolderPath");
            }
        }

        #region LoadedCommand for View Loaded_event 
        private RelayCommand _LoadedComand;
        public ICommand LoadedCommand
        {
            get
            {
                return _LoadedComand ?? (_LoadedComand = new RelayCommand(LoadedCommandAction, CanExecuteLoadedCommand));
            }
        }

        private void LoadedCommandAction(object args)
        {
            try
            {
                if ((Application.Current as App).bConnected)
                {
                    ServiceStatus = true;
                }
                else
                {
                    ServiceStatus = false;
                }
#if _TEST_SERVICE_
                this.Title += " - TEST";
#endif
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }
        private bool CanExecuteLoadedCommand(object args)
        {
            return true;
        }
        #endregion

        #region SettingWindow
        private RelayCommand _showSettingDialogCommand;
        public ICommand ShowSettingDialogCommand
        {
            get
            {
                return _showSettingDialogCommand ?? 
                    (_showSettingDialogCommand = new RelayCommand(ShowSettingDialogAction, CanExecuteSHowSettingDialogAction));
            }
        }

        private async void ShowSettingDialogAction(object args)
        {
            try
            {
                MetroWindow SettingDlg = new MetroWindow();
                SettingDlg.Title = "Settings";
                SettingDlg.Content = new SettingWindow();
                SettingDlg.SizeToContent = SizeToContent.WidthAndHeight;
                SettingDlg.ResizeMode = ResizeMode.NoResize;
                SettingDlg.Topmost = true;
                SettingDlg.Owner = Application.Current.MainWindow;
                SettingDlg.WindowStartupLocation = WindowStartupLocation.CenterOwner;

                if (SettingDlg.ShowDialog() == true)
                {
                    string tempPath = (SettingDlg.Content as SettingWindow).LabelUploadPath.Content.ToString();
                    string CurPath = getRegValue("atoihome", "UploadPath");
                    if (CurPath.Equals(tempPath))
                    {
                        return;
                    }
                    ImageFilesFolderPath = tempPath;
                    Utility.setRegValue("atoihome", "UploadPath", ImageFilesFolderPath);

                    Mouse.OverrideCursor = Cursors.AppStarting;
                    IsEnabledServiceControl = false;
                    Network.DisconnectService();
                    ServiceStatus = false;
                    await Task<TaskArgs>.Run(() => Utility.RestartService(new TaskArgs("atoihomeservice", 30 * 1000)));
                    Network.ConnectService();
                    ServiceStatus = true;
                    IsEnabledServiceControl = true;
                    Mouse.OverrideCursor = Cursors.Arrow;
                }
                SettingDlg = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private bool CanExecuteSHowSettingDialogAction(object args)
        {
            return true;
        }
        #endregion

        #region ServiceControl
        /// <summary>
        /// 서비스 연결상태를 표현하는 bool 프로퍼티
        /// </summary>
        private bool _ServiceStatus;
        public bool ServiceStatus
        {
            get { return _ServiceStatus; }
            set
            {
                // ServiceStatus가 변경되면 종속된 문자열도 변경한다.
                _ServiceStatus = value;
                if (_ServiceStatus == false)
                {
                    ServiceStatusString = "Stoped service";
                }
                else
                {
                    ServiceStatusString = "Started service";
                }
                OnPropertyChanged("ServiceStatus");
            }
        }

        private bool _IsEnabledServiceControl=true;
        public bool IsEnabledServiceControl
        {
            get { return _IsEnabledServiceControl; }
            set
            {
                // ServiceStatus가 변경되면 종속된 문자열도 변경한다.
                _IsEnabledServiceControl = value;
                OnPropertyChanged("IsEnabledServiceControl");
            }
        }
        /// <summary>
        /// 버튼이미지의 하단에 서비스 상태를 표시하는 문자열 프로퍼티
        /// </summary>
        private string _ServiceStatusString="Stoped service";
        public string ServiceStatusString
        {
            get { return _ServiceStatusString; }
            set
            {
                _ServiceStatusString = value;
                OnPropertyChanged("ServiceStatusString");
            }
        }

        private RelayCommand _serviceControlCommand;
        public ICommand ServiceControlCommand
        {
            get
            {
                return _serviceControlCommand ?? (_serviceControlCommand = new RelayCommand(ServiceControlAction, CanExecuteServiceControl));
            }
        }

        private void ServiceControlAction(object args)
        {
            try
            {
                if ((Application.Current as App).bConnected == false)
                {
#if !_TEST_SERVICE_
                    if (Utility.GetServiceStatus("OneClickShot") == false)
                        Utility.StartService("OneClickShot", 30 * 1000);
#endif
                    (Application.Current as App).bConnected = Network.ConnectService();
                }
                else
                {
                    Network.NotificationService.Disconnect(new OneClickShotEventArgs(Network.strEmail, null, MessageType.GET_DATA, "bye", null));
#if (!_TEST_SERVICE_)
                    Utility.StopService("AtoiHomeService", 30 * 1000);
#endif
                    (Application.Current as App).bConnected = false;
                }
                if (ServiceStatus = (Application.Current as App).bConnected)
                    ServiceStatusString = "Started service";
                else
                    ServiceStatusString = "Stoped service";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private bool CanExecuteServiceControl(object args)
        {
            return true;
        }

        #endregion

        #region DrawImage
        private string _ImagePath;
        public string ImagePath
        {
            get { return _ImagePath; }
            set
            {
                _ImagePath = value;
                OnPropertyChanged("ImagePath");
            }
        }

        private RelayCommand _DrawImageCommand;
        public ICommand DrawImageCommand
        {
            get
            {
                return _DrawImageCommand ?? (_DrawImageCommand = new RelayCommand(DrawImageAction, CanExecuteDrawImageAction));
            }
        }

        private void DrawImageAction(object args)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            {
                openFileDialog.Reset();
                if (!string.IsNullOrEmpty(ImageFilesFolderPath))
                    openFileDialog.InitialDirectory = ImageFilesFolderPath;
                else
                    openFileDialog.InitialDirectory = "c:\\";
                openFileDialog.Filter = "Image files (*.png or *.jpg)|*.png;*.jpg";
                openFileDialog.FilterIndex = 2;
                openFileDialog.RestoreDirectory = false;
                openFileDialog.CheckFileExists = true;
                openFileDialog.CheckPathExists = true;
                if (openFileDialog.ShowDialog() == true)
                {
                    ImagePath = openFileDialog.FileName;
                }
            }
            openFileDialog = null;
        }
        private bool CanExecuteDrawImageAction(object args)
        {
            return true;
        }
        #endregion

        #region RotateImage
        private int _RotateAngle;
        public int RotateAngle
        {
            get { return _RotateAngle; }
            set { _RotateAngle = value; OnPropertyChanged("RotateAngle"); }
        }

        private RelayCommand _RotateImageCommand;
        public ICommand RotateImageCommand
        {
            get
            {
                return _RotateImageCommand ?? (_RotateImageCommand = new RelayCommand(RotateImageAction, CanExecuteRotateImageAction));
            }
        }

        private void RotateImageAction(object args)
        {
            if (RotateAngle >= 270)
                RotateAngle = 0;
            else
                RotateAngle += 90;
        }
        private bool CanExecuteRotateImageAction(object args)
        {
            return true;
        }
        #endregion

        #region ViewMode Chooese scrollView, zoomView 
        private string _ViewModeName = "ScrollView";
        public string ViewModeName
        {
            get { return _ViewModeName; }
            set { _ViewModeName = value; OnPropertyChanged("ViewModeName"); }
        }

        private bool _bScrollViewMode=true;
        public bool bScrollViewMode
        {
            get { return _bScrollViewMode; }
            set { _bScrollViewMode = value; OnPropertyChanged("bScrollViewMode"); }
        }

        private RelayCommand _DrawModeCommand;
        public ICommand DrawModeCommand
        {
            get
            {
                return _DrawModeCommand ?? (_DrawModeCommand = new RelayCommand(DrawModeAction, CanExecuteDrawModeAction));
            }
        }

        private void DrawModeAction(object args)
        {
            bScrollViewMode = !bScrollViewMode;
            if (bScrollViewMode == true)
            {
                ViewModeName = "ScrollView";
            }
            else
            {
                ViewModeName = "ZoomView";
            }
        }
        private bool CanExecuteDrawModeAction(object args)
        {
            return true;
        }
        #endregion

        /// <summary>
        /// REMOTE_SERVICE에서 이미지파일을 다운로드 받아 사용자폴더에 저장하는 메서드
        /// </summary>
        /// <param name="strFilename"></param>
        public void DownloadImageAndSave(string strFilename)
        {
            //creating the object of WCF service client       
#if _TEST_SERVICE_
                    TestOneClickShotServiceSoap.OneClickShotSoapClient WCFClient = new TestOneClickShotServiceSoap.OneClickShotSoapClient();
#else
            OneClickShotServiceSoap.OneClickShotSoapClient WCFClient = new OneClickShotServiceSoap.OneClickShotSoapClient();
#endif
            try
            {
                Stream streamInput = WCFClient.DownloadImage(strFilename);
                string strSaveFullPath = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures) + "\\" + strFilename;

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
                ImagePath = strSaveFullPath;
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
    }
}
