using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Inspectify
{
    /// <summary>
    /// Describes file information, that is retrieved from the file system.
    /// </summary>
    public class FileInformation
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="displayName">The friendly name of the found file.</param>
        /// <param name="fileLocation">The location of the found file.</param>
        public FileInformation(string displayName, string fileLocation)
        {
            this.DisplayName = displayName;
            this.FileLocation = fileLocation;
        }

        private ImageSource icon;
        /// <summary>
        /// Gets the icon of the file, if available.
        /// </summary>
        public ImageSource Icon
        {
            get
            {
                if (this.icon == null)
                {
                    this.icon = this.IconForExtension(this.Extension);
                }

                return this.icon;
            }
        }

        /// <summary>
        /// Gets or sets the friendly name of the file.
        /// </summary>
        public string DisplayName
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the location of the file.
        /// </summary>
        public string FileLocation
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the real file location, e.g. in the case of a link (.lnk) file.
        /// </summary>
        public string RealFileLocation
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        public string Arguments
        {
            get;
            set;
        }

        private string extension;
        /// <summary>
        /// Gets the extension of the file, based on its file location.
        /// </summary>
        public string Extension
        {
            get
            {
                if (string.IsNullOrEmpty(this.extension))
                {
                    this.extension = Path.GetExtension(this.FileLocation);
                }

                return this.extension;
            }
        }

        const string EXTENSION_EXE = ".exe";
        const string EXTENSION_LNK = ".lnk";

        private ImageSource IconForExtension(string extension)
        {
            switch(extension)
            {
                case EXTENSION_EXE:
                    {
                        this.RealFileLocation = this.FileLocation;

                        return this.ExtractIconFromFileLocation(this.RealFileLocation);
                    }
                case EXTENSION_LNK:
                    {
                        IWshRuntimeLibrary.WshShell shell = new IWshRuntimeLibrary.WshShell();
                        IWshRuntimeLibrary.IWshShortcut link = shell.CreateShortcut(this.FileLocation) as IWshRuntimeLibrary.IWshShortcut;

                        //
                        // See: https://msdn.microsoft.com/en-us/library/xk6kst2k(v=vs.84).aspx
                        // TODO: implement proper handling of arguments/windowsstyle etc.
                        //
                        if (link != null)
                        {
                            if (string.IsNullOrEmpty(link.TargetPath))
                            {
                                this.RealFileLocation = this.FileLocation;
                            }
                            else
                            {
                                if (File.Exists(link.TargetPath))
                                {
                                    string targetPathExtension = Path.GetExtension(link.TargetPath);

                                    if (targetPathExtension.Equals(EXTENSION_EXE, StringComparison.InvariantCultureIgnoreCase))
                                    {
                                        this.RealFileLocation = link.TargetPath;
                                    }
                                    else
                                    {
                                        //
                                        // Prevent that non-executable files are opened.
                                        //
                                        this.RealFileLocation = this.FileLocation;
                                    }
                                }
                                else
                                {
                                    //
                                    // Workaround for issue as described in: http://stackoverflow.com/q/7120583/4253002.
                                    //
                                    string workingDirectory = link.WorkingDirectory;
                                    string fileName = Path.GetFileName(link.TargetPath);

                                    if (!string.IsNullOrEmpty(link.WorkingDirectory))
                                    {
                                        string targetFile = Path.Combine(workingDirectory, fileName);

                                        if (!Path.GetExtension(targetFile).Equals(EXTENSION_EXE, StringComparison.InvariantCultureIgnoreCase) || !File.Exists(targetFile))
                                        {
                                            //
                                            // Prevent that non-executable (or not existing) files can be opened.
                                            //
                                            this.RealFileLocation = this.FileLocation;
                                        }
                                        else
                                        {
                                            this.RealFileLocation = targetFile;
                                        }
                                    }
                                    else
                                    {
                                        //
                                        // Hmm, ok. The x86 folder does not exist, and there is no working directory.
                                        // It might be useful to verify that we have a 'Program Files (x86)' folder. If so, attempt to rewrite it to the x64 variant.
                                        //
                                        string x86Folder = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);

                                        if (Environment.Is64BitOperatingSystem && link.TargetPath.StartsWith(x86Folder))
                                        {
                                            if (Environment.Is64BitProcess)
                                            {
                                                string x64Folder = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);

                                                this.RealFileLocation = link.TargetPath.Replace(x86Folder, x64Folder);
                                            }
                                            else
                                            {
                                                string baseDirectory = Path.GetPathRoot(Environment.GetFolderPath(Environment.SpecialFolder.System));
                                                const string PROGRAM_FILES_X64 = "Program Files";

                                                string x64Folder = Path.Combine(baseDirectory, PROGRAM_FILES_X64);

                                                if (Directory.Exists(x64Folder))
                                                {
                                                    this.RealFileLocation = link.TargetPath.Replace(x86Folder, x64Folder);
                                                }
                                                else
                                                {
                                                    this.RealFileLocation = link.TargetPath;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            //
                                            // FileLocation cannot be determined... :(
                                            //
                                            this.RealFileLocation = link.TargetPath;
                                        }
                                    }
                                }
                            }

                            if (!string.IsNullOrEmpty(link.Arguments))
                            {
                                this.Arguments = link.Arguments;
                            }

                            return this.ExtractIconFromFileLocation(this.RealFileLocation);
                        }
                        else
                        {
                            goto default;
                        }
                    }
                default:
                    {
                        return Utility.Current.GetWpfImageSource("default_32.png");
                    }
            }
        }

        private ImageSource ExtractIconFromFileLocation(string fileLocation)
        {
            Icon icon;

            try
            {
                icon = System.Drawing.Icon.ExtractAssociatedIcon(fileLocation);
            }
            catch (Exception)
            {
                try
                {
                    //
                    // Heuristic approach. Rewrite path to allow possible extraction of the icon.
                    //
                    icon = System.Drawing.Icon.ExtractAssociatedIcon(this.FileLocation);
                }
                catch (Exception)
                {
                    //
                    // Fall back.
                    //
                    icon = null;
                }
            }

            if (icon != null)
            {
                Bitmap bitmap = icon.ToBitmap();
                IntPtr hBitmap = bitmap.GetHbitmap();

                return Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            }
            else
            {
                return Utility.Current.GetWpfImageSource("default_32.png");
            }
        }
    }
}
