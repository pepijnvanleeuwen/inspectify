﻿using Hardcodet.Wpf.TaskbarNotification;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Squirrel;

namespace Inspectify
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Raises the <see cref="System.Windows.Application.Startup"/> event.
        /// </summary>
        /// <param name="e">A <see cref="System.Windows.StartupEventArgs"/> that contains the event data.</param>
        protected async override void OnStartup(StartupEventArgs e)
        {
            Utility.Current.TaskbarIcon = (TaskbarIcon)FindResource("NotifyIcon");

            using (var updateManager = UpdateManager.GitHubUpdateManager("https://github.com/pepijnvanleeuwen/inspectify", prerelease: true))
            {
                await Task.Run( () =>
                              {
                                  try
                                  {
                                      updateManager.Result.UpdateApp();
                                  }
                                  catch (Exception ex)
                                  {
                                      Utility.Current.HandleException(ex);
                                  }
                              });
            }

            base.OnStartup(e);
        }

        /// <summary>
        /// Raises the <see cref="System.Windows.Application.Exit"/> event.
        /// </summary>
        /// <param name="e">An <see cref="System.Windows.ExitEventArgs"/> that contains the event data.</param>
        protected override void OnExit(ExitEventArgs e)
        {
            Utility.Current.TaskbarIcon.Dispose();

            base.OnExit(e);
        }
    }
}
