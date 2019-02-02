using System;
using System.Windows.Input;

namespace AtoiHomeManager.Source.ViewModel
{
    internal class RelayCommand<T> : ICommand
    {
        private Func<object, object> p1;
        private Func<object, bool> p2;

        public RelayCommand(Func<object, object> p1, Func<object, bool> p2)
        {
            this.p1 = p1;
            this.p2 = p2;
        }
    }
}