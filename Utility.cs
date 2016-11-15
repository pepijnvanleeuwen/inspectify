using Hardcodet.Wpf.TaskbarNotification;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Inspectify
{
    /// <summary>
    /// Utilities that are widely used in this application and do not belong in a specific class.
    /// </summary>
    public class Utility
    {
        #region General

        #region Constants
        #endregion

        private static Utility current;
        /// <summary>
        /// Gets a static instance of Utility.
        /// </summary>
        public static Utility Current
        {
            get
            {
                if (current == null)
                {
                    current = new Utility();
                }

                return current;
            }
        }

        /// <summary>
        /// Gets or sets a task bar icon.
        /// </summary>
        public TaskbarIcon TaskbarIcon
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the name of the application.
        /// </summary>
        public string ApplicationName
        {
            get { return "Inspectify"; }
        }
        #endregion

        #region Exceptions
        /// <summary>
        /// Handle an exception.
        /// </summary>
        /// <param name="ex">The exception to handle.</param>
        public void HandleException(Exception ex)
        {
            try
            {
                Trace.WriteLine("An error occurred:");
                Trace.WriteLine(ex);

                string title = this.ApplicationName;
                string message;

                if (ex is AggregateException)
                {
                    AggregateException aex = ex as AggregateException;

                    StringBuilder messageBuilder = new StringBuilder();

                    foreach (Exception exception in aex.InnerExceptions)
                    {
                        messageBuilder.AppendLine(exception.Message);
                    }

                    message = messageBuilder.ToString();
                }
                else
                {
                    message = ex.Message;
                }

                if (this.TaskbarIcon != null)
                {
                    Application.Current.Dispatcher.Invoke( () => TaskbarIcon.ShowBalloonTip($"Inspectify", message, BalloonIcon.Error));
                }
                else
                {
                    Application.Current.Dispatcher.Invoke( () => new ErrorWindow().Show());
                }
            }
            catch (Exception hex)
            {
                //
                // Attempt to log the original exception and the newly raised exception. When it does happen, something is has gone quite wrong.
                //
                Trace.WriteLine(ex);
                Trace.WriteLine(hex);

                MessageBox.Show($"An error occurred: {hex}.{Environment.NewLine}{Environment.NewLine}Inner exception: {ex}");
            }
        }
        #endregion

        #region Configuration
        /// <summary>
        /// Gets the path of the configuration file.
        /// </summary>
        public string ConfigurationPath
        {
            get
            {
                string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "scanbadge");

                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                return path;
            }
        }

        /// <summary>
        /// Gets full file location of the configuration file.
        /// </summary>
        public string ConfigurationFileName
        {
            get
            {
                return Path.Combine(this.ConfigurationPath, "config.json");
            }
        }
        #endregion

        #region WPF
        /// <summary>
        /// Shows the <see cref="SettingsWindow"/>.
        /// </summary>
        /// <returns></returns>
        public bool ShowSettingsWindow()
        {
            SettingsWindow window = new SettingsWindow();
            
            return window.ShowDialog().GetValueOrDefault(false);
        }
        #endregion

        #region IO
        /// <summary>
        /// Saves the provided contents to the provided path as a file. 
        /// </summary>
        /// <param name="path">The path of the file.</param>
        /// <param name="contents">The contents to save as a file.</param>
        public void SaveFile(string path, string contents)
        {
            try
            {
                if (!string.IsNullOrEmpty(path))
                {
                    File.WriteAllText(path, contents);
                }
                else
                {
                    throw new Exception("Cannot save file, because path is not provided.");
                }
            }
            catch (Exception ex)
            {
                this.HandleException(ex);
            }
        }

        /// <summary>
        /// Reads a file.
        /// </summary>
        /// <param name="path">The path of the file to read.</param>
        public string ReadFile(string path)
        {
            try
            {
                if (!string.IsNullOrEmpty(path))
                {
                    return File.ReadAllText(path);
                }
                else
                {
                    throw new Exception("Cannot read file, because path is not provided.");
                }
            }
            catch (Exception ex)
            {
                this.HandleException(ex);
            }

            return null;
        }

        /// <summary>
        /// Opens the provided file location if it exists.
        /// </summary>
        /// <param name="fileName">An application with which to start a process.</param>
        /// <param name="arguments">Command-line arguments to pass to the application when the process starts.</param>
        /// <returns>If true, the file location has been opened; otherwise, false.</returns>
        public Process OpenFile(string fileName, string arguments = null)
        {
            Process process = null;

            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo(fileName, arguments);

                startInfo.UseShellExecute = true;

                //
                // The Windows error dialog does not allow you to choose a program anymore.
                //
                startInfo.ErrorDialog = false;

                //
                // First try to start and have Windows it's way.
                //
                process = Process.Start(startInfo);
            }
            catch (Exception ex)
            {
                this.HandleException(ex);
            }

            return process;
        }
        #endregion

        #region Images
        /// <summary>
        /// Gets a WPF image source located in this project, based on the file name in the '/Resources/' folder.
        /// </summary>
        /// <param name="fileName">The file name to search for.</param>
        /// <returns>An image source containing the found image if the file exists, otherwise; null.</returns>
        public ImageSource GetWpfImageSource(string fileName)
        {
            if(!string.IsNullOrEmpty(fileName))
            {
                return new BitmapImage(new Uri($"pack://application:,,,/Inspectify;component/Resources/{fileName}", UriKind.Absolute));
            }
            else
            {
                return null;
            }
        }
        #endregion
    }
}