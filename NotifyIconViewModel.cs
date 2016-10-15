using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;

namespace Inspectify
{
    /// <summary>
    /// Provides bindable properties and commands for the NotifyIcon. In this sample, the
    /// view model is assigned to the NotifyIcon in XAML. Alternatively, the startup routing
    /// in App.xaml.cs could have created this view model, and assigned it to the NotifyIcon.
    /// </summary>
    public class NotifyIconViewModel
    {
        /// <summary>
        /// Shuts down the application.
        /// </summary>
        public ICommand OpenSettingsCommand
        {
            get
            {
                return new DelegateCommand { CommandAction = () => Utility.Current.ShowSettingsWindow() };
            }
        }

        /// <summary>
        /// Shuts down the application.
        /// </summary>
        public ICommand OpenAboutCommand
        {
            get
            {
                return new DelegateCommand { CommandAction = () => Utility.Current.OpenFile("https://github.com/pepijnvanleeuwen/inspectify")};
            }
        }

        /// <summary>
        /// Shows a window, if none is already open.
        /// </summary>
        public ICommand ShowWindowCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CanExecuteFunc = () => Application.Current.MainWindow != null && Application.Current.MainWindow.Visibility != Visibility.Visible,
                    CommandAction = () =>
                    {
                        Application.Current.MainWindow.Show();
                    }
                };
            }
        }

        /// <summary>
        /// Shuts down the application.
        /// </summary>
        public ICommand ExitApplicationCommand
        {
            get
            {
                return new DelegateCommand { CommandAction = () => Application.Current.Shutdown() };
            }
        }
    }


    /// <summary>
    /// Simplistic delegate command.
    /// </summary>
    public class DelegateCommand : ICommand
    {
        /// <summary>
        /// The action to perform.
        /// </summary>
        public Action CommandAction
        {
            get;
            set;
        }

        /// <summary>
        /// A function containing a boolean value indicating whether or not the delegate command can execute. 
        /// </summary>
        public Func<bool> CanExecuteFunc
        {
            get;
            set;
        }

        /// <summary>
        /// Occurs when changes occur that affect whether or not the command should execute.
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        /// <summary>
        /// Defines the method that determines whether the command can execute in its current state.
        /// </summary>
        /// <param name="parameter">Data used by the command. If the command does not require data to be passed, this object can be set to null.</param>
        /// <returns>True if this command can be executed; otherwise, false.</returns>
        public bool CanExecute(object parameter)
        {
            return this.CanExecuteFunc == null || this.CanExecuteFunc();
        }

        /// <summary>
        /// Defines the method to be called when the command is invoked.
        /// </summary>
        /// <param name="parameter">Data used by the command. If the command does not require data to be passed, this object can be set to null.</param>
        public void Execute(object parameter)
        {
            this.CommandAction();
        }
    }
}
