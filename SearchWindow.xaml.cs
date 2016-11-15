using Microsoft.Win32;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Diagnostics;
using System.Globalization;
using System.IO;
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

namespace Inspectify
{
    /// <summary>
    /// Interaction logic for SearchWindow.xaml
    /// </summary>
    public partial class SearchWindow : Window
    {
        #region General
        /// <summary>
        /// Registers the PreferredImage dependency property.
        /// </summary>
        public static readonly DependencyProperty PreferredImageProperty = DependencyProperty.Register(nameof(PreferredImage), typeof(ImageSource), typeof(SearchWindow));

        /// <summary>
        /// Registers the SearchQuery dependency property.
        /// </summary>
        public static readonly DependencyProperty SearchQueryProperty = DependencyProperty.Register(nameof(SearchQuery), typeof(string), typeof(SearchWindow), new PropertyMetadata(new PropertyChangedCallback(SearchQuery_PropertyChanged)));

        /// <summary>
        /// Registers the AlternativeAccentBrush dependency property.
        /// </summary>
        public static readonly DependencyProperty AlternativeAccentBrushProperty = DependencyProperty.Register(nameof(AlternativeAccentBrush), typeof(Brush), typeof(SearchWindow));

        /// <summary>
        /// Registers the TransparentAlternativeAccentBrush dependency property.
        /// </summary>
        public static readonly DependencyProperty TransparentAlternativeAccentBrushProperty = DependencyProperty.Register(nameof(TransparentAlternativeAccentBrush), typeof(Brush), typeof(SearchWindow));

        /// <summary>
        /// Registers the the FileInformationCollection dependency property.
        /// </summary>
        public static readonly DependencyProperty FileInformationCollectionProperty = DependencyProperty.Register(nameof(FilesResult), typeof(List<FileInformation>), typeof(SearchWindow));

        /// <summary>
        /// Describes an Escape <see cref="RoutedCommand"/>.
        /// </summary>
        public static RoutedCommand EscapeCommand = new RoutedCommand();

        /// <summary>
        /// Describes an Escape <see cref="RoutedCommand"/>.
        /// </summary>
        public static RoutedCommand EnterCommand = new RoutedCommand();

        /// <summary>
        /// Constructor.
        /// </summary>
        public SearchWindow()
        {
            try
            {
                this.InitializeComponent();

                this.DataContext = this;

                this.Files = this.SearchProgramsFiles();

                this.DetermineAlternativeAccentBrush();
                this.DetermineTransparentAlternativeAccentBrush();

                EscapeCommand.InputGestures.Add(new KeyGesture(Key.Escape));
                EnterCommand.InputGestures.Add(new KeyGesture(Key.Enter));
            }
            catch (Exception ex)
            {
                Utility.Current.HandleException(ex);
            }
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the image source of the preferred search result.
        /// </summary>
        public ImageSource PreferredImage
        {
            get { return (ImageSource)this.GetValue(PreferredImageProperty); }
            set { this.SetValue(PreferredImageProperty, value); }
        }

        /// <summary>
        /// Gets or sets the search query.
        /// </summary>
        public string SearchQuery
        {
            get { return (string)this.GetValue(SearchQueryProperty); }
            set { this.SetValue(SearchQueryProperty, value); }
        }

        /// <summary>
        /// Gets or sets an alternative (more brighter) Windows accent brush.
        /// </summary>
        public Brush AlternativeAccentBrush
        {
            get { return (Brush)this.GetValue(AlternativeAccentBrushProperty); }
            set { this.SetValue(AlternativeAccentBrushProperty, value); }
        }

        public Brush TransparentAlternativeAccentBrush
        {
            get { return (Brush)this.GetValue(TransparentAlternativeAccentBrushProperty); }
            set { this.SetValue(TransparentAlternativeAccentBrushProperty, value); }
        }

        /// <summary>
        /// Gets or sets a collection of <see cref="FileInformation"/>.
        /// </summary>
        public List<FileInformation> FilesResult
        {
            get { return (List<FileInformation>)this.GetValue(FileInformationCollectionProperty); }
            set { this.SetValue(FileInformationCollectionProperty, value); }
        }

        /// <summary>
        /// Gets or sets all found files.
        /// </summary>
        public ConcurrentBag<FileInformation> Files
        {
            get;
            set;
        }
        #endregion

        #region Methods
        private ConcurrentBag<FileInformation> SearchProgramsFiles()
        {
            ConcurrentBag<FileInformation> files = new ConcurrentBag<FileInformation>();

            List<string> paths = new List<string>();

            paths.Add(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu));
            paths.Add(Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu));

            foreach (string path in paths)
            {
                foreach (var file in this.WalkPath(path, "*.lnk"))
                {
                    files.Add(file);
                }
            }

            return new ConcurrentBag<FileInformation>(files.GroupBy(x => x.DisplayName).Select(y => y.First()));
        }

        private ConcurrentBag<FileInformation> WalkPath(string path, string extension = null)
        {
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }

            if (string.IsNullOrEmpty(extension))
            {
                //
                // Include all files.
                //
                extension = "*.*";
            }

            if (!Directory.Exists(path))
            {
                throw new Exception($"Cannot locate the folder '{path}'.");
            }

            var files = Directory.EnumerateFiles(path, extension, SearchOption.AllDirectories)
                                 .Select(p => new FileInformation(Path.GetFileNameWithoutExtension(p), p));

            return new ConcurrentBag<FileInformation>(files);
        }

        #pragma warning disable 1998
        private async Task<List<FileInformation>> GetSearchResults(string query)
        {
            List<FileInformation> filesResult = new List<FileInformation>();

            if (!string.IsNullOrEmpty(query))
            {
                //
                // Wikipedia search example - work in progress :)
                //
                //Modules.WikipediaSearchModule wikipedia = new Modules.WikipediaSearchModule();
                //wikipedia.RetrieveResultsAsync(query);

                filesResult = this.Files
                              .Where(x => x.DisplayName.Contains(query, StringComparison.CurrentCultureIgnoreCase))
                              .OrderBy(x => x.DisplayName?.Length) // In most cases, the desired application's name is the shortest (e.g. 'Command Prompt' vs 'Developer Command Prompt for VS2015').
                              .ToList();

                if (filesResult.Count > 0)
                {
                    //
                    // Get icon for first result.
                    //
                    this.PreferredImage = filesResult.First().Icon;
                }
                else
                {
                    filesResult = null;
                    this.PreferredImage = null;
                }
            }
            else
            {
                filesResult = null;
                this.PreferredImage = null;
            }

            return filesResult;
        }
        #pragma warning restore

        /// <summary>
        /// Determines the alternative (more brighter) Windows accent brush. The accent will be made about 60% brighter.
        /// </summary>
        private void DetermineAlternativeAccentBrush()
        {
            try
            {
                var currentAccent = SystemParameters.WindowGlassColor;

                if (currentAccent != null)
                {
                    float correctionFactor = 0.8f;
                    float red = (255 - currentAccent.R) * correctionFactor + currentAccent.R;
                    float green = (255 - currentAccent.G) * correctionFactor + currentAccent.G;
                    float blue = (255 - currentAccent.B) * correctionFactor + currentAccent.B;

                    Color lighterAccent = Color.FromArgb(currentAccent.A, (byte)red, (byte)green, (byte)blue);
                    this.AlternativeAccentBrush = new SolidColorBrush(lighterAccent);
                }
                else
                {
                    this.AlternativeAccentBrush = new SolidColorBrush(currentAccent);
                }
            }
            catch(Exception ex)
            {
                Utility.Current.HandleException(ex);
            }
        }

        private void DetermineTransparentAlternativeAccentBrush()
        {
            var accentBrush = this.AlternativeAccentBrush;

            if (accentBrush != null)
            {
                byte a = ((Color)accentBrush.GetValue(SolidColorBrush.ColorProperty)).A;
                byte g = ((Color)accentBrush.GetValue(SolidColorBrush.ColorProperty)).G;
                byte r = ((Color)accentBrush.GetValue(SolidColorBrush.ColorProperty)).R;
                byte b = ((Color)accentBrush.GetValue(SolidColorBrush.ColorProperty)).B;

                this.TransparentAlternativeAccentBrush = new SolidColorBrush(Color.FromArgb(0x80, r, g, b));
            }
        }
        #endregion

        #region Events
        private async static void SearchQuery_PropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                SearchWindow self = sender as SearchWindow;

                if (self != null)
                {
                    self.FilesResult = await self.GetSearchResults(self.SearchQuery);
                }
            }
            catch (Exception ex)
            {
                Utility.Current.HandleException(ex);
            }
        }

        private async void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SearchWindow self = sender as SearchWindow;

                if (self != null)
                {
                    self.FilesResult = await self.GetSearchResults(self.SearchQuery);
                }
            }
            catch (Exception ex)
            {
                Utility.Current.HandleException(ex);
            }
        }

        private void OpenFileLocation_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button button = sender as Button;

                if (button != null && button.DataContext != null)
                {
                    FileInformation fileInfo = button.DataContext as FileInformation;

                    Utility.Current.OpenFile(fileInfo.RealFileLocation, fileInfo.Arguments);
                }
            }
            catch (Exception ex)
            {
                Utility.Current.HandleException(ex);
            }
        }

        private void EscapeCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                SearchWindow self = sender as SearchWindow;

                if (self != null)
                {
                    self.SearchQuery = null;

                    self.Hide();
                }
            }
            catch (Exception ex)
            {
                Utility.Current.HandleException(ex);
            }
        }

        private void EnterCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                SearchWindow self = sender as SearchWindow;

                if (self != null)
                {
                    if (self.FilesResult != null)
                    {
                        Utility.Current.OpenFile(self.FilesResult.FirstOrDefault()?.RealFileLocation, self.FilesResult.FirstOrDefault()?.Arguments);

                        self.Hide();
                    }
                }
            }
            catch (Exception ex)
            {
                Utility.Current.HandleException(ex);
            }
        }

        private void SearchWindow_Deactivated(object sender, EventArgs e)
        {
            try
            {
                SearchWindow self = sender as SearchWindow;

                if (self != null)
                {
                    self.SearchQuery = null;

                    self.Hide();
                }
            }
            catch (Exception ex)
            {
                Utility.Current.HandleException(ex);
            }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

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

        #region Interop
        [DllImport("User32.dll")]
        private static extern bool RegisterHotKey([In] IntPtr hWnd, [In] int id, [In] uint fsModifiers, [In] uint vk);

        [DllImport("User32.dll")]
        private static extern bool UnregisterHotKey([In] IntPtr hWnd, [In] int id);

        private HwndSource source;
        private const int HOTKEY_ID = 9001;

        /// <summary>
        /// Raises the <see cref="System.Windows.Window.SourceInitialized"/> event.
        /// </summary>
        /// <param name="e">An <see cref="System.EventArgs"/> that contains the event data.</param>
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            WindowInteropHelper helper = new WindowInteropHelper(this);

            this.source = HwndSource.FromHwnd(helper.Handle);
            this.source.AddHook(HwndHook);

            this.RegisterHotKey();
        }

        /// <summary>
        /// Raises the <see cref="System.Windows.Window.Closed"/> event.
        /// </summary>
        /// <param name="e">An <see cref="System.EventArgs"/> that contains the event data.</param>
        protected override void OnClosed(EventArgs e)
        {
            this.source.RemoveHook(HwndHook);
            this.source = null;

            this.UnregisterHotKey();

            base.OnClosed(e);
        }

        private void RegisterHotKey()
        {
            try
            {
                var helper = new WindowInteropHelper(this);

                const uint VK_SPACE = 0x20;
                const uint MOD_ALT = 0x0001;
                const uint MOD_WIN = 0x0008;

                Trace.WriteLine("Trying to register hotkey 'ALT+WIN+SPACEBAR'...");

                if (RegisterHotKey(helper.Handle, HOTKEY_ID, MOD_ALT | MOD_WIN, VK_SPACE))
                {
                    Trace.WriteLine("Registered hotkey 'ALT+WIN+SPACEBAR succesfully.");
                }
                else
                {
                    Trace.WriteLine("Cannot register hotkey 'ALT+WIN+SPACEBAR'.");
                }
            }
            catch (Exception ex)
            {
                Utility.Current.HandleException(ex);
            }
        }

        private void UnregisterHotKey()
        {
            WindowInteropHelper helper = new WindowInteropHelper(this);

            Trace.WriteLine("Unregistering hotkey 'ALT+WIN+SPACEBAR'...");

            UnregisterHotKey(helper.Handle, HOTKEY_ID);
        }

        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WM_HOTKEY = 0x0312;

            switch (msg)
            {
                case WM_HOTKEY:
                    {
                        switch (wParam.ToInt32())
                        {
                            case HOTKEY_ID:
                                {
                                    this.OnHotKeyPressed();

                                    handled = true;

                                    break;
                                }
                        }

                        break;
                    }
            }
            return IntPtr.Zero;
        }

        private void OnHotKeyPressed()
        {
            try
            {
                this.ShowWindow();
            }
            catch (Exception ex)
            {
                Utility.Current.HandleException(ex);
            }
        }

        private void ShowWindow()
        {
            if (this.IsVisible)
            {
                if (this.WindowState == WindowState.Minimized)
                {
                    this.WindowState = WindowState.Normal;
                }

                this.Activate();
            }
            else
            {
                this.Show();
            }
        }
        #endregion
    }
}
