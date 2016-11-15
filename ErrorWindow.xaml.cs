using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Inspectify
{
    /// <summary>
    /// Interaction logic for ErrorWindow.xaml
    /// </summary>
    public partial class ErrorWindow : MetroWindow
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public ErrorWindow()
        {
            this.InitializeComponent();

            //
            // Workaround for issue described here: https://github.com/MahApps/MahApps.Metro/issues/1366
            //
            this.AllowsTransparency = true;
        }

        #region Events
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.EnableBlur();
        }
        #endregion

        #region Blur
        private void EnableBlur()
        {
            try
            {
                var windowHelper = new WindowInteropHelper(this);

                var accent = new NativeMethods.AccentPolicy();
                var accentStructSize = Marshal.SizeOf(accent);
                accent.AccentState = NativeMethods.AccentState.ACCENT_ENABLE_BLURBEHIND;

                var accentPtr = Marshal.AllocHGlobal(accentStructSize);
                Marshal.StructureToPtr(accent, accentPtr, false);

                var data = new NativeMethods.WindowCompositionAttributeData();
                data.Attribute = NativeMethods.WindowCompositionAttribute.WCA_ACCENT_POLICY;
                data.SizeOfData = accentStructSize;
                data.Data = accentPtr;

                NativeMethods.SetWindowCompositionAttribute(windowHelper.Handle, ref data);

                Marshal.FreeHGlobal(accentPtr);
            }
            catch (Exception ex)
            {
                Utility.Current.HandleException(ex);
            }
        }
        #endregion
    }
}
