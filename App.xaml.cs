using Hardcodet.Wpf.TaskbarNotification;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Inspectify
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private TaskbarIcon notifyIcon;

        /// <summary>
        /// Raises the <see cref="System.Windows.Application.Startup"/> event.
        /// </summary>
        /// <param name="e">A <see cref="System.Windows.StartupEventArgs"/> that contains the event data.</param>
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            this.notifyIcon = (TaskbarIcon)FindResource("NotifyIcon");
        }

        /// <summary>
        /// Raises the <see cref="System.Windows.Application.Exit"/> event.
        /// </summary>
        /// <param name="e">An <see cref="System.Windows.ExitEventArgs"/> that contains the event data.</param>
        protected override void OnExit(ExitEventArgs e)
        {
            this.notifyIcon.Dispose();

            base.OnExit(e);
        }
    }
}
