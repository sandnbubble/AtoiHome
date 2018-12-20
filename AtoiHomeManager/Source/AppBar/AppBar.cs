using Appbar;
using MahApps.Metro.Controls;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Shapes;

namespace AtoiHomeManager
{
    public partial class MainWindow : MetroWindow
    {
        #region Window Docking

        [DllImport("SHELL32", CallingConvention = CallingConvention.StdCall)]
        static extern uint SHAppBarMessage(int dwMessage, ref APPBARDATA pData);
        [DllImport("User32.dll", ExactSpelling = true, CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        private static extern bool MoveWindow(IntPtr hWnd, int x, int y, int cx, int cy, bool repaint);
        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        private static extern int RegisterWindowMessage(string msg);
        const int ABM_NEW = 0;
        const int ABM_REMOVE = 1;
        const int ABM_QUERYPOS = 2;
        const int ABM_SETPOS = 3;
        const int ABM_GETSTATE = 4;
        const int ABM_GETTASKBARPOS = 5;
        const int ABM_ACTIVATE = 6;
        const int ABM_GETAUTOHIDEBAR = 7;
        const int ABM_SETAUTOHIDEBAR = 8;
        const int ABM_WINDOWPOSCHANGED = 9;
        const int ABM_SETSTATE = 10;
        const int ABN_STATECHANGE = 0;
        const int ABN_POSCHANGED = 1;
        const int ABN_FULLSCREENAPP = 2;
        const int ABN_WINDOWARRANGE = 3;
        const int ABE_LEFT = 0;
        const int ABE_TOP = 1;
        const int ABE_RIGHT = 2;
        const int ABE_BOTTOM = 3;
        [StructLayout(LayoutKind.Sequential)]
        struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }
        [StructLayout(LayoutKind.Sequential)]
        struct APPBARDATA
        {
            public int cbSize;
            public IntPtr hWnd;
            public int uCallbackMessage;
            public int uEdge;
            public RECT rc;
            public IntPtr lParam;
        }

        // Is AppBar registered?
        bool fBarRegistered = false;

        // Number of AppBar's message for WndProc
        int uCallBack;
        Rect rectWorkingArea = new Rect();

        // Register AppBar
        void RegisterBar()
        {
            WindowInteropHelper helper = new WindowInteropHelper(this);
            HwndSource mainWindowSrc = (HwndSource)HwndSource.FromHwnd(helper.Handle);

            APPBARDATA abd = new APPBARDATA();
            abd.cbSize = Marshal.SizeOf(abd);
            abd.hWnd = mainWindowSrc.Handle;

            if (!fBarRegistered)
            {
                uCallBack = RegisterWindowMessage("AppBarMessage");
                abd.uCallbackMessage = uCallBack;

                uint ret = SHAppBarMessage(ABM_NEW, ref abd);
                fBarRegistered = true;

                ABSetPos();
            }
        }
        // Unregister AppBar
        void UnregisterBar()
        {
            WindowInteropHelper helper = new WindowInteropHelper(this);
            HwndSource mainWindowSrc = (HwndSource)HwndSource.FromHwnd(helper.Handle);

            APPBARDATA abd = new APPBARDATA();
            abd.cbSize = Marshal.SizeOf(abd);
            abd.hWnd = mainWindowSrc.Handle;

            if (fBarRegistered)
            {
                SHAppBarMessage(ABM_REMOVE, ref abd);
                fBarRegistered = false;
            }
        }
        // Set position of AppBar
        void ABSetPos()
        {
            if (fBarRegistered)
            {
                WindowInteropHelper helper = new WindowInteropHelper(this);
                HwndSource mainWindowSrc = (HwndSource)HwndSource.FromHwnd(helper.Handle);

                APPBARDATA abd = new APPBARDATA();
                abd.cbSize = Marshal.SizeOf(abd);
                abd.hWnd = mainWindowSrc.Handle;
                abd.uEdge = Properties.Settings.Default.uEdge;

                if (abd.uEdge == ABE_LEFT || abd.uEdge == ABE_RIGHT)
                {
                    abd.rc.top = 0;
                    abd.rc.bottom = (int)SystemParameters.PrimaryScreenHeight;
                    if (abd.uEdge == ABE_LEFT)
                    {
                        abd.rc.left = 0;
                        abd.rc.right = (int)this.ActualWidth;
                    }
                    else
                    {
                        abd.rc.right = (int)SystemParameters.PrimaryScreenWidth + (int)rectWorkingArea.Width;
                        abd.rc.left = abd.rc.right - (int)this.ActualWidth;
                    }
                }
                else
                {
                    abd.rc.left = 0;
                    abd.rc.right = (int)SystemParameters.PrimaryScreenWidth + (int)rectWorkingArea.Width;

                    if (abd.uEdge == ABE_TOP)
                    {
                        abd.rc.top = 0;
                        abd.rc.bottom = (int)this.ActualHeight;
                    }
                    else
                    {
                        abd.rc.bottom = (int)SystemParameters.PrimaryScreenHeight;
                        abd.rc.top = abd.rc.bottom - (int)this.ActualHeight;
                    }
                }
                SHAppBarMessage(ABM_QUERYPOS, ref abd);
                SHAppBarMessage(ABM_SETPOS, ref abd);
                MoveWindow(abd.hWnd, abd.rc.left, abd.rc.top, abd.rc.right - abd.rc.left, abd.rc.bottom - abd.rc.top, true);
            }
        }

        #endregion

        #region Glass extending

        [StructLayout(LayoutKind.Sequential)]
        struct MARGINS
        {
            public int cxLeftWidth;
            public int cxRightWidth;
            public int cyTopHeight;
            public int cyBottomHeight;
        }
        [DllImport("dwmapi.dll")]
        static extern int DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS pMarInset);
        [DllImport("dwmapi.dll")]
        extern static int DwmIsCompositionEnabled(ref int en);
        const int WM_DWMCOMPOSITIONCHANGED = 0x031E;

        void ExtendGlass()
        {
            try
            {
                int isGlassEnabled = 0;
                DwmIsCompositionEnabled(ref isGlassEnabled);
                if (Environment.OSVersion.Version.Major > 5 && isGlassEnabled > 0)
                {
                    WindowInteropHelper helper = new WindowInteropHelper(this);
                    HwndSource mainWindowSrc = (HwndSource)HwndSource.FromHwnd(helper.Handle);

                    mainWindowSrc.CompositionTarget.BackgroundColor = Colors.Transparent;
                    this.Background = Brushes.Transparent;

                    MARGINS margins = new MARGINS();
                    margins.cxLeftWidth = -1;
                    margins.cxRightWidth = -1;
                    margins.cyBottomHeight = -1;
                    margins.cyTopHeight = -1;

                    DwmExtendFrameIntoClientArea(mainWindowSrc.Handle, ref margins);
                }
            }
            catch (DllNotFoundException) { }
        }

        #endregion
  
        const int WM_NCLBUTTONDOWN = 0x00A1;
        const int WM_EXITSIZEMOVE = 0x0232;

        TransPrev tp = new TransPrev();
        bool nclbd = false;

        IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == uCallBack && wParam.ToInt32() == ABN_POSCHANGED)
            {
                ABSetPos();
                handled = true;
            }
            else if (msg == WM_DWMCOMPOSITIONCHANGED)
            {
                ExtendGlass();
                handled = true;
            }
            else if (msg == WM_NCLBUTTONDOWN)
            {
                nclbd = true;
            }
            else if (msg == WM_EXITSIZEMOVE)
            {
                nclbd = false;
                tp.Hide();
                CalculateHorizontalEdge();
                RegisterBar();
            }
            return IntPtr.Zero;
        }

        private void Window_LocationChanged(object sender, EventArgs e)
        {
            if (nclbd)
            {
                if (fBarRegistered)
                {
                    UnregisterBar();
                    tp.Show();
                }
                RefreshTransPrev();
            }
        }

        void RefreshTransPrev()
        {
            CalculateHorizontalEdge();
            tp.SetArrow(Properties.Settings.Default.uEdge);
        }

        void CalculateHorizontalEdge()
        {
            if ((SystemParameters.PrimaryScreenWidth + rectWorkingArea.Width) / 2 > this.Left)
                Properties.Settings.Default.uEdge = ABE_LEFT;
            else
                Properties.Settings.Default.uEdge = ABE_RIGHT;
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

#if DEBUG
            return;
#else 
            rectWorkingArea = GetWorkingArea();
            IntPtr hwnd = new WindowInteropHelper(this).Handle;
            HwndSource source = HwndSource.FromHwnd(hwnd);
            source.AddHook(new HwndSourceHook(WndProc));

            ExtendGlass();
            RegisterBar();
#endif
        }
        public Rect GetWorkingArea()
        {
            if (secondaryScreen == null)
            {
                return new Rect(0, 0, (int)SystemParameters.PrimaryScreenWidth, SystemParameters.PrimaryScreenHeight);
            }
            else
            {
                int left = System.Windows.Forms.Screen.AllScreens.FirstOrDefault(x => !x.Primary).WorkingArea.Left;
                int top = System.Windows.Forms.Screen.AllScreens.FirstOrDefault(x => !x.Primary).WorkingArea.Top;
                int width = System.Windows.Forms.Screen.AllScreens.FirstOrDefault(x => !x.Primary).WorkingArea.Width;
                int height = System.Windows.Forms.Screen.AllScreens.FirstOrDefault(x => !x.Primary).WorkingArea.Height;
                Rect WorkingArea = new Rect(left, top, width, height);
                return WorkingArea;
            }
        }
    }
}