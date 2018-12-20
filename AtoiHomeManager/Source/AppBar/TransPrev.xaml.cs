using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Appbar
{
    /// <summary>
    /// Interaction logic for TransPrev.xaml
    /// </summary>
    public partial class TransPrev : Window
    {
        public TransPrev()
        {
            InitializeComponent();
        }

        const int ABE_LEFT = 0;
        const int ABE_TOP = 1;
        const int ABE_RIGHT = 2;
        const int ABE_BOTTOM = 3;

        public void SetArrow(int uEdge)
        {
            if (uEdge == ABE_LEFT)
            {
                right.Visibility = System.Windows.Visibility.Collapsed;
                left.Visibility = System.Windows.Visibility.Visible;
            }
            else if (uEdge == ABE_RIGHT)
            {
                left.Visibility = System.Windows.Visibility.Collapsed;
                right.Visibility = System.Windows.Visibility.Visible;
            }
        }
    }
}
