using System;
using System.Collections.Generic;
using System.IO;
using Dnt.Commands.Infrastructure;
using NConsole;
using Newtonsoft.Json;

namespace Dnt.Commands.Packages.Switcher
{
    public class ReferenceSwitcherConfiguration
    {
        public class RestoreFile
        {
            [JsonProperty("restore", NullValueHandling = NullValueHandling.Ignore)]
            public List<RestoreProjectInformation> Restore { get; set; } = new List<RestoreProjectInformation>();
        }

        [JsonIgnore]
        internal string Path { get; set; }

        [JsonProperty("solution")]
        public string Solution { get; set; }

        [JsonProperty("mappings")]
        [JsonConverter(typeof(SingleOrArrayConverter))]
        public Dictionary<string, List<string>> Mappings { get; set; }

        [JsonProperty("restore", NullValueHandling = NullValueHandling.Ignore)]
        public List<RestoreProjectInformation> Restore { get; set; } = new List<RestoreProjectInformation>();

        [JsonIgnore]
        public string ActualSolution => PathUtilities.ToAbsolutePath(Solution, System.IO.Path.GetDirectoryName(Path));

        [JsonProperty("removeProjects", NullValueHandling = NullValueHandling.Ignore)]
        public bool RemoveProjects { get; set; } = true;

        [JsonProperty("useSeparateRestoreFile", NullValueHandling = NullValueHandling.Ignore)]
        public bool UseSeparateRestoreFile { get; set; } = true;

        public string GetActualPath(string path)
        {
            return PathUtilities.ToAbsolutePath(path, System.IO.Path.GetDirectoryName(Path));
        }

        public static string GetRestorePath(string path)
        {
            var dir = System.IO.Path.GetDirectoryName(path);
            var file = System.IO.Path.GetFileNameWithoutExtension(path);
            var ext = System.IO.Path.GetExtension(path);
            var rp = System.IO.Path.Combine(dir, $"{file}.restore{ext}");
            return rp;
        }

        public static ReferenceSwitcherConfiguration Load(string fileName, IConsoleHost host)
        {
            if (!File.Exists(fileName))
            {
                host.WriteError($"File '{fileName}' not found.");
                return null;
            }

            var c = JsonConvert.DeserializeObject<ReferenceSwitcherConfiguration>(File.ReadAllText(fileName));
            c.Path = PathUtilities.ToAbsolutePath(fileName, Directory.GetCurrentDirectory());

            if (!c.UseSeparateRestoreFile)
                return c;

            var restorePath = GetRestorePath(c.Path);

            Console.WriteLine($"rp: {restorePath}");

            if (!File.Exists(restorePath))
              return c;

            var cr =  JsonConvert.DeserializeObject<RestoreFile>(File.ReadAllText(restorePath));
            c.Restore = cr.Restore;

            return c;
        }

        public void Save()
        {
            if (UseSeparateRestoreFile)
            {
                var restore = new RestoreFile { Restore = this.Restore };
                var restorePath = GetRestorePath(Path);

                var json = JsonConvert.SerializeObject(restore, Formatting.Indented);
                File.WriteAllText(restorePath, json);
            }
            else
            {
                var json = JsonConvert.SerializeObject(this, Formatting.Indented);
                File.WriteAllText(Path, json);
            }
        }
    }
}