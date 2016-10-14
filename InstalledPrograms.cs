using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Inspectify
{
    public class InstalledPrograms
    {
        public List<FileInformation> GetInstalledPrograms()
        {
            var result = new List<FileInformation>();

            if (!Environment.Is64BitOperatingSystem)
            {
                result.AddRange(GetInstalledProgramsFromRegistry(RegistryView.Registry32));
            }
            else
            {
                result.AddRange(GetInstalledProgramsFromRegistry(RegistryView.Registry64));
            }

            return result;
        }

        private List<FileInformation> GetInstalledProgramsFromRegistry(RegistryView registryView)
        {
            List<FileInformation> result = new List<FileInformation>();

            List<string> locations = new List<string>();
            locations.Add(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall");

            if (Environment.Is64BitOperatingSystem)
            {
                locations.Add(@"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall");
            }

            foreach (string registryKey in locations)
            {
                using (RegistryKey key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, registryView).OpenSubKey(registryKey))
                {
                    if (key != null)
                    {
                        foreach (string subKeyName in key.GetSubKeyNames())
                        {
                            using (RegistryKey subkey = key.OpenSubKey(subKeyName))
                            {
                                if (this.IsProgramVisible(subkey))
                                {
                                    object name = subkey.GetValue("DisplayName");
                                    object icon = subkey.GetValue("DisplayIcon");

                                    if (name != null && icon != null)
                                    {
                                        result.Add(new FileInformation(name.ToString(), icon.ToString()));
                                    }
                                }
                            }
                        }
                    }
                }

                using (RegistryKey key = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, registryView).OpenSubKey(registryKey))
                {
                    if (key != null)
                    {
                        foreach (string subkeyName in key.GetSubKeyNames())
                        {
                            using (RegistryKey subkey = key.OpenSubKey(subkeyName))
                            {
                                if (this.IsProgramVisible(subkey))
                                {
                                    object name = subkey.GetValue("DisplayName");
                                    object icon = subkey.GetValue("DisplayIcon");

                                    if (name != null && icon != null)
                                    {
                                        result.Add(new FileInformation(name.ToString(), icon.ToString()));
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return result;
        }

        private bool IsProgramVisible(RegistryKey subkey)
        {
            var name = (string)subkey.GetValue("DisplayName");
            var releaseType = (string)subkey.GetValue("ReleaseType");
            var parentName = (string)subkey.GetValue("ParentDisplayName");
            var systemComponent = subkey.GetValue("SystemComponent");

            return !string.IsNullOrEmpty(name) && string.IsNullOrEmpty(releaseType) && string.IsNullOrEmpty(parentName) && systemComponent == null;
        }
    }

}
