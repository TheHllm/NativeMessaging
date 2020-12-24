using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace NativeMessaging
{
    [DebuggerDisplay("{Name}")]
    public class Host
    {
        private Host() { }

        public ManifestFile Manifest { get; private set; }
        public string Name { get { return Manifest.Name; } }
        public string ExecutablePath { get { return Manifest.Path; } }
        public string ManifestPath { get; private set; }

        private static string[] RegistryLocations = new string[] { @"Software\\Google\\Chrome\\NativeMessagingHosts" };


        public static async Task<Host> LoadFromManifest(string path)
        {
            ManifestFile mani = await ManifestFile.LoadFile(path);
            return new Host() { ManifestPath = path, Manifest = mani };
        }


        private static List<Host> Hosts = null;
        /// <summary>
        /// Currently on works on windows
        /// </summary>
        /// <returns></returns>
        public static async Task<IEnumerable<Host>> GetAvailableHosts()
        {
            if (Hosts == null)
            {
                Hosts = new();

                foreach (string loc in RegistryLocations)
                {
                    RegistryKey regLoc = Registry.CurrentUser.OpenSubKey(loc);
                    string[] hosts = regLoc.GetSubKeyNames();
                    foreach (string host in hosts)
                    {
                        string maniLoc = (string)regLoc.OpenSubKey(host).GetValue("");
                        if (!String.IsNullOrWhiteSpace(maniLoc))
                        {
                            Hosts.Add(await LoadFromManifest(maniLoc));
                        }
                    }

                }
            }

            return Hosts;
        }

        public static async Task<Host> GetByName(string name)
        {
            var hosts = await GetAvailableHosts();

            foreach(var host in hosts)
            {
                if(host.Name == name)
                {
                    return host;
                }
            }

            throw new KeyNotFoundException();
        }
    }
}
