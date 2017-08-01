using MockEngine.Configuration;
using MockEngine.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MockEngine.Resolvers
{
    public class FileScenarioResolver : IScenarioResolver
    {
        private ILogProvider _logProvider;
        private string _pathBase;
        private string _pathSuffix;

        private class ScenarioChange : IScenarioChange
        {
            public ScenarioChangeTypes ChangeType { get; set; }

            public string Name { get; set; }

            public string OldName { get; set; }
        }
        public FileScenarioResolver( )
        {
            _pathBase = "";

            _pathSuffix = ".yaml";

        }
        public void Initialize( IMockContext context )
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            if (context.Settings == null)
            {
                throw new ArgumentException("Settings are not null", nameof(context));
            }
            if (context.Settings.ScenarioResolverSettings == null)
            {
                throw new ArgumentException("Settings.ScenarioResolverSettings are not null", nameof(context));
            }
            this._logProvider = context.LogProvider;
            if (!string.IsNullOrEmpty(context.Settings.ScenarioResolverSettings.PathBase))
            {
                _pathBase = context.Settings.ScenarioResolverSettings.PathBase;
                _pathBase = _pathBase.Replace("/", "\\");
                _pathBase = _pathBase.Replace("~", context.RootPath);
            }
            if (!string.IsNullOrEmpty(context.Settings.ScenarioResolverSettings.PathSuffix))
            {
                _pathSuffix = context.Settings.ScenarioResolverSettings.PathSuffix;
            }
        }
        private string JoinPath(string pathA, string pathB, string pathSepparator = "\\")
        {
            if (string.IsNullOrWhiteSpace(pathA))
            {
                return pathB;
            }
            else if (string.IsNullOrWhiteSpace(pathB))
            {
                return pathA;
            }
            else if (pathA.EndsWith(pathSepparator))
            {
                if (pathB.StartsWith(pathSepparator))
                {
                    return pathA + pathB.Substring(pathSepparator.Length);
                }
                else
                {
                    return pathA + pathB;
                }
            }
            else if (pathB.StartsWith(pathSepparator))
            {
                return pathA + pathB;
            }
            else
            {
                return pathA + pathSepparator + pathB;
            }
        }
        public Stream Resolve(string scenarioName)
        {
            var filePath = JoinPath( _pathBase,scenarioName.Replace("/","\\") + _pathSuffix);
            return new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite|FileShare.Delete);
        }

        public IEnumerable<string> GetScenarioNames()
        {
            List<string> scenarioNames = new List<string>();
            GetScenarioNames(_pathBase, scenarioNames);
            return scenarioNames;
        }
        private void GetScenarioNames( string directory, List<string> scenarioNames)
        {
            foreach (var filepath in Directory.EnumerateFiles(directory))
            {
                if (filepath.EndsWith(_pathSuffix, StringComparison.InvariantCultureIgnoreCase))
                {
                    var filename = filepath.Substring(_pathBase.Length); //remove the _pathBase port of the path

                    var scenarioName = filename.Substring(0, filename.Length - _pathSuffix.Length); //remove the suffix part of the path

                    scenarioName = scenarioName.Replace("\\", "/");
                    scenarioNames.Add(scenarioName);
                }
            }
            foreach (var directoryPath in Directory.EnumerateDirectories(directory))
            {
                GetScenarioNames(directoryPath, scenarioNames);
            }
        }
        private FileSystemWatcher _fileSystemWatcher;
        private List<Action<IScenarioChange>> _changeHandlers = new List<Action<IScenarioChange>>();
        public void RegisterChangeHandler(Action<IScenarioChange> onChange)
        {
            if ( _fileSystemWatcher == null)
            {
                _fileSystemWatcher = new FileSystemWatcher(_pathBase);
                _fileSystemWatcher.Changed += Scenario_Changed;
                _fileSystemWatcher.Deleted += Scenario_Deleted;
                _fileSystemWatcher.Created += Scenario_Created;
                _fileSystemWatcher.Renamed += Scenario_Renamed;
                _fileSystemWatcher.Error += Scenario_Error;
                _fileSystemWatcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.DirectoryName;
                _fileSystemWatcher.IncludeSubdirectories = true;
                _fileSystemWatcher.EnableRaisingEvents = true;
            }
            _changeHandlers.Add(onChange);
        }

        private void Scenario_Error(object sender, ErrorEventArgs e)
        {
            _logProvider.Warning(e.GetException(), "Scenario file watcher reported error: {reason}", e.GetException().Message);
        }

        private void PostChange(IScenarioChange change)
        {
            for( int i = 0; i < _changeHandlers.Count;i++ )
            {
                try
                {
                    _changeHandlers[i](change);
                }
                catch(Exception e)
                {
                    _logProvider.Warning(e,"Error reported from change handler: {reason}",e.Message);
                }
            }
        }
        private void Scenario_Renamed(object sender, RenamedEventArgs e)
        {
            if (e.OldName.EndsWith(_pathSuffix))
            {
                var oldName = e.OldName.Substring(0, e.Name.Length - _pathSuffix.Length);
                if (e.Name.EndsWith(_pathSuffix))
                {
                    var name = e.Name.Substring(0, e.Name.Length - _pathSuffix.Length);
                    PostChange(new ScenarioChange { Name = name, ChangeType = ScenarioChangeTypes.Renamed, OldName = oldName });
                }
                else
                {
                    PostChange(new ScenarioChange { Name = oldName, ChangeType = ScenarioChangeTypes.Deleted });
                }
            }
            else if (e.Name.EndsWith(_pathSuffix))
            {
                var name = e.Name.Substring(0, e.Name.Length - _pathSuffix.Length);
                PostChange(new ScenarioChange { Name = name, ChangeType = ScenarioChangeTypes.Created });
            }
        }

        private void Scenario_Created(object sender, FileSystemEventArgs e)
        {
            if (e.Name.EndsWith(_pathSuffix))
            {
                var name = e.Name.Substring(0, e.Name.Length - _pathSuffix.Length);
                PostChange(new ScenarioChange { Name = name, ChangeType = ScenarioChangeTypes.Created });
            }
        }

        private void Scenario_Deleted(object sender, FileSystemEventArgs e)
        {
            if (e.Name.EndsWith(_pathSuffix))
            {
                var name = e.Name.Substring(0, e.Name.Length - _pathSuffix.Length);
                PostChange(new ScenarioChange { Name = name, ChangeType = ScenarioChangeTypes.Deleted });
            }
        }

        private void Scenario_Changed(object sender, FileSystemEventArgs e)
        {
            if (e.Name.EndsWith(_pathSuffix))
            {
                var name = e.Name.Substring(0, e.Name.Length - _pathSuffix.Length);
                PostChange(new ScenarioChange { Name = name, ChangeType = ScenarioChangeTypes.Changed });
            }
        }
    }
}
