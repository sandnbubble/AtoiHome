using MahApps.Metro.Controls.Dialogs;
using System;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows;

namespace AtoiHomeManager.Source.ViewModel
{
    class SettingWindowViewModel : BaseViewModel
    {
        // Variable
        private IDialogCoordinator dialogCoordinator;

        // Constructor
        public SettingWindowViewModel(IDialogCoordinator instance)
        {
            dialogCoordinator = instance;
            UploadPath = Utils.Utility.getRegValue("atoihome", "UploadPath");
        }

        // Methods
        private async void FooMessageAsync()
        {
            await dialogCoordinator.ShowMessageAsync(this, "HEADER", "MESSAGE");
        }

        private async void FooProgressAsync()
        {
            // Show...
            ProgressDialogController controller = await dialogCoordinator.ShowProgressAsync(this, "HEADER", "MESSAGE");
            controller.SetIndeterminate();

            // Do your work... 

            // Close...
            await controller.CloseAsync();
        }


        private string _UploadPath;
        public string UploadPath
        {
            get { return _UploadPath; }
            set
            {
                _UploadPath = value;
                OnPropertyChanged("UploadPath");
            }
        }

        private RelayCommand _ChooseImagFolder;
        public ICommand ChooseImagFolder
        {
            get
            {
                return _ChooseImagFolder ?? (_ChooseImagFolder = new RelayCommand(ChooseImagFolderAction, CanExecuteChooseImageFolderAction));
            }
        }

        private void ChooseImagFolderAction(object args)
        {
            try
            {
                using (var fbd = new System.Windows.Forms.FolderBrowserDialog())
                {
                    fbd.SelectedPath = Utils.Utility.getRegValue("atoihome", "UploadPath");
                    System.Windows.Forms.DialogResult result = fbd.ShowDialog();

                    if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                    {
                        UploadPath = fbd.SelectedPath;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private bool CanExecuteChooseImageFolderAction(object args)
        {
            return true;
        }

        private bool _CloseDialogResult;
        public bool CloseDialogResult
        {
            get { return _CloseDialogResult; }
            set { _CloseDialogResult = value; OnPropertyChanged("CloseDialogResult"); }
        }


        private RelayCommand _CloseDialog;
        public ICommand CloseDialog
        {
            get
            {
                return _CloseDialog ?? (_CloseDialog = new RelayCommand(CloseDialogAction, CanExecuteCloseDialogAction));
            }
        }
        private void CloseDialogAction(object args)
        {
            if ((bool)args == true)
                CloseDialogResult = true;
            else
                CloseDialogResult = false;

        }
        private bool CanExecuteCloseDialogAction(object args)
        {
            return true;
        }
    }
}
