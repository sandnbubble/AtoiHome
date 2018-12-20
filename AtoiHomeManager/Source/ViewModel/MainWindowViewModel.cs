using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AtoiHomeManager
{
    class MainWindowViewModel : INotifyPropertyChanged
    {
        #region 생성자
        public MainWindowViewModel()
        {
            MainWindowContext = new MainWindowContext();
        }
        #endregion
        private MainWindowContext MainWindowContext { get; set; }

        #region 이벤트 처리
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public bool bConnectButtonEnable
        {
            get { return MainWindowContext.bConnectButtonEnable; }
            set
            {
                MainWindowContext.bConnectButtonEnable = value;
                OnPropertyChanged("bConnectButtonEnable");
            }
        }
    }
}
