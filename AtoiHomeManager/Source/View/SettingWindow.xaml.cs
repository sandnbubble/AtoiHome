using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using AtoiHomeManager.Source.ViewModel;
using MahApps.Metro.Controls.Dialogs;

namespace AtoiHomeManager.Source.View
{
    /// <summary>
    /// Interaction logic for SettingWindow.xaml
    /// </summary>
    public partial class SettingWindow : UserControl
    {
        SettingWindowViewModel vm = new SettingWindowViewModel(DialogCoordinator.Instance);
        public SettingWindow()
        {
            InitializeComponent();
            DataContext = vm;
            vm.PropertyChanged += OnPropertyChanged;
        }

        protected void OnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == "CloseDialogResult")
            {
                Window window = Window.GetWindow(this);
                window.DialogResult = vm.CloseDialogResult;
                window.Close();
            }
        }
    }
}