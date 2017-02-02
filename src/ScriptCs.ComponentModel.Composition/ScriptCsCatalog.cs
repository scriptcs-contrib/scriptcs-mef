using ScriptCs.Contracts;
using ScriptCs.Hosting;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using IOPath = System.IO.Path;

namespace ScriptCs.ComponentModel.Composition
{
    /// <summary>
    /// Catalog using ScriptCs scripts>
    /// </summary>
    public class ScriptCsCatalog : ComposablePartCatalog, INotifyComposablePartCatalogChanged, ICompositionElement
    {
        private readonly object _thisLock = new object();
        private readonly ScriptCsCatalogOptions _options;
        private bool _isDisposed;
        private AssemblyCatalog _catalog;
        private ReadOnlyCollection<string> _loadedFiles;

        /// <summary>
        ///     Translated absolute path of the path passed into the constructor of <see cref="DirectoryCatalog"/>.
        /// </summary>
        public string FullPath { get; private set; }

        /// <summary>
        ///     Path passed into the constructor of <see cref="DirectoryCatalog"/>.
        /// </summary>
        public string Path { get; private set; }

        /// <summary>
        ///   SearchPattern passed into the constructor of <see cref="DirectoryCatalog"/>, or the default *.dll.
        /// </summary>
        public string SearchPattern { get; private set; }

        /// <summary>
        ///     Set of files that have currently been loaded into the catalog.
        /// </summary>
        public ReadOnlyCollection<string> LoadedFiles
        {
            get
            {
                lock (_thisLock)
                {
                    return _loadedFiles;
                }
            }
        }

        /// <summary>
        /// Display name of the <see cref="ICompositionElement"/>
        /// </summary>
        public string DisplayName
        {
            get { return GetDisplayName(); }
        }

        /// <summary>
        /// Origin of the <see cref="ICompositionElement"/>
        /// </summary>
        public ICompositionElement Origin
        {
            get { return null; }
        }

        /// <summary>
        /// Notify when the contents of the Catalog has changed.
        /// </summary>summary>
        public event EventHandler<ComposablePartCatalogChangeEventArgs> Changed;

        /// <summary>
        /// Notify when the contents of the Catalog has changing.
        /// </summary>summary>
        public event EventHandler<ComposablePartCatalogChangeEventArgs> Changing;

        /// <summary>
        /// Creates a catalog of <see cref="ComposablePartDefinition"/>s based on all the *.csx files
        ///     in the given directory path.
        /// </summary>
        /// <param name="path">
        ///     Path to the directory to scan for assemblies to add to the catalog.
        ///     The path needs to be absolute or relative to <see cref="AppDomain.BaseDirectory"/>
        /// </param>
        public ScriptCsCatalog(string path)
            : this(path, "*.csx", new ScriptCsCatalogOptions())
        {
        }

        /// <summary>
        /// Creates a catalog of <see cref="ComposablePartDefinition"/>s based on all the *.csx files
        ///     in the given directory path.
        /// </summary>
        /// <param name="path">
        ///     Path to the directory to scan for assemblies to add to the catalog.
        ///     The path needs to be absolute or relative to <see cref="AppDomain.BaseDirectory"/>
        /// </param>
        /// <param name="options">
        ///     Options of the ScriptCsCatalog.
        /// </param>
        public ScriptCsCatalog(string path, ScriptCsCatalogOptions options)
            : this(path, "*.csx", options)
        {
        }

        /// <summary>
        /// Creates a catalog of <see cref="ComposablePartDefinition"/>s based on all the files corresponding to <paramref name="searchPattern"/>
        ///     in the given directory path.
        /// </summary>
        /// <param name="path">
        ///     Path to the directory to scan for assemblies to add to the catalog.
        ///     The path needs to be absolute or relative to <see cref="AppDomain.BaseDirectory"/>
        /// </param>
        /// <param name="searchPattern">
        ///     Search pattern to get all scripts files from the directory.
        /// </param>
        public ScriptCsCatalog(string path, string searchPattern)
            : this(path, searchPattern, new ScriptCsCatalogOptions())
        {
        }

        /// <summary>
        /// Creates a catalog of <see cref="ComposablePartDefinition"/>s based on all the files corresponding to <paramref name="searchPattern"/>
        ///     in the given directory path.
        /// </summary>
        /// <param name="path">
        ///     Path to the directory to scan for assemblies to add to the catalog.
        ///     The path needs to be absolute or relative to <see cref="AppDomain.BaseDirectory"/>
        /// </param>
        /// <param name="searchPattern">
        ///     Search pattern to get all scripts files from the directory.
        /// </param>
        /// <param name="options">
        ///     Options of the ScriptCsCatalog.
        /// </param>
        public ScriptCsCatalog(string path, string searchPattern, ScriptCsCatalogOptions options)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException("path");
            }

            if (string.IsNullOrEmpty(searchPattern))
            {
                throw new ArgumentNullException("searchPattern");
            }

            if (options == null)
            {
                throw new ArgumentNullException("options");
            }

            _options = options.OverridesNullByDefault();

            Initialize(path, searchPattern);
        }

        /// <summary>
        ///  Creates a catalog of <see cref="ComposablePartDefinition"/>s based on all the <paramref name="scriptFiles"/>.
        /// </summary>
        /// <param name="scriptFiles">
        ///     Scripts files to be executed and added to the catalog.
        /// </param>
        public ScriptCsCatalog(IEnumerable<string> scriptFiles)
            : this(scriptFiles, new ScriptCsCatalogOptions())
        {
        }

        /// <summary>
        ///  Creates a catalog of <see cref="ComposablePartDefinition"/>s based on all the <paramref name="scriptFiles"/>.
        /// </summary>
        /// <param name="scriptFiles">
        ///     Scripts files to be executed and added to the catalog.
        /// </param>
        /// <param name="options">
        ///     Options of the ScriptCsCatalog.
        /// </param>
        public ScriptCsCatalog(IEnumerable<string> scriptFiles, ScriptCsCatalogOptions options)
        {
            if (scriptFiles == null || !scriptFiles.Any())
            {
                throw new ArgumentNullException("scriptFiles");
            }

            if (options == null)
            {
                throw new ArgumentNullException("options");
            }

            _options = options.OverridesNullByDefault();

            Initialize(scriptFiles);
        }

        /// <summary>
        ///     Returns a string representation of the directory catalog.
        /// </summary>
        /// <returns>
        ///     A <see cref="String"/> containing the string representation of the <see cref="ScriptCsCatalog"/>.
        /// </returns>
        public override string ToString()
        {
            return GetDisplayName();
        }

        /// <summary>
        /// Returns an enumerator that iterates through the catalog
        /// </summary>
        /// <returns></returns>
        public override IEnumerator<ComposablePartDefinition> GetEnumerator()
        {
            return _catalog.GetEnumerator();
        }

        /// <summary>
        /// Get a list of export definitions that match the constraint defined by the specified <see cref="ImportDefinition"/> object.
        /// </summary>
        /// <param name="definition">The condition of the <see cref="ExportDefinition"/> objects to be returned.</param>
        /// <returns></returns>
        public override IEnumerable<Tuple<ComposablePartDefinition, ExportDefinition>> GetExports(ImportDefinition definition)
        {
            return _catalog.GetExports(definition);
        }

        /// <summary>
        ///     Refreshes the <see cref="ComposablePartDefinition"/>s with the latest files in the directory that match
        ///     the searchPattern. If any files have been added they will be added to the catalog and if any files were
        ///     removed they will be removed from the catalog. For files that have been removed keep in mind that the
        ///     assembly cannot be unloaded from the process so <see cref="ComposablePartDefinition"/>s for those files
        ///     will simply be removed from the catalog.
        ///
        ///     Possible exceptions that can be thrown are any that <see cref="Directory.GetFiles(string, string)"/>.
        /// </summary>
        /// <exception cref="DirectoryNotFoundException">
        ///     The specified path has been removed since object construction.
        /// </exception>
        public void Refresh()
        {
            ThrowIfDisposed();

            ComposablePartDefinition[] addedDefinitions;
            ComposablePartDefinition[] removedDefinitions;
            object changeReferenceObject;
            string[] afterFiles;
            string[] beforeFiles;

            while (true)
            {
                afterFiles = GetFiles();

                lock (_thisLock)
                {
                    changeReferenceObject = _loadedFiles;
                    beforeFiles = _loadedFiles.ToArray();
                }

                var newCatalog = ExecuteScripts(afterFiles);

                // Notify listeners to give them a preview before completing the changes
                addedDefinitions = newCatalog.ToArray();

                removedDefinitions = _catalog.ToArray();

                using (var atomicComposition = new AtomicComposition())
                {
                    var changingArgs = new ComposablePartCatalogChangeEventArgs(addedDefinitions, removedDefinitions, atomicComposition);
                    OnChanging(changingArgs);

                    // if the change went through then write the catalog changes
                    lock (_thisLock)
                    {
                        if (changeReferenceObject != _loadedFiles)
                        {
                            // Someone updated the list while we were diffing so we need to try the diff again
                            continue;
                        }

                        _catalog = newCatalog;
                        _loadedFiles = afterFiles.ToReadOnlyCollection();

                        // Lastly complete any changes added to the atomicComposition during the change event
                        atomicComposition.Complete();

                        // Break out of the while(true)
                        break;
                    } // WriteLock
                } // AtomicComposition
            }   // while (true)

            var changedArgs = new ComposablePartCatalogChangeEventArgs(addedDefinitions, removedDefinitions, null);
            OnChanged(changedArgs);
        }

        /// <summary>
        ///     Raises the <see cref="INotifyComposablePartCatalogChanged.Changed"/> event.
        /// </summary>
        /// <param name="e">
        ///     An <see cref="ComposablePartCatalogChangeEventArgs"/> containing the data for the event.
        /// </param>
        protected virtual void OnChanged(ComposablePartCatalogChangeEventArgs e)
        {
            EventHandler<ComposablePartCatalogChangeEventArgs> changedEvent = Changed;
            if (changedEvent != null)
            {
                changedEvent(this, e);
            }
        }

        /// <summary>
        ///     Raises the <see cref="INotifyComposablePartCatalogChanged.Changing"/> event.
        /// </summary>
        /// <param name="e">
        ///     An <see cref="ComposablePartCatalogChangeEventArgs"/> containing the data for the event.
        /// </param>
        protected virtual void OnChanging(ComposablePartCatalogChangeEventArgs e)
        {
            EventHandler<ComposablePartCatalogChangeEventArgs> changingEvent = Changing;
            if (changingEvent != null)
            {
                changingEvent(this, e);
            }
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="ComposablePartCatalog" /> and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing && !_isDisposed)
                {
                    AssemblyCatalog catalogs = null;

                    try
                    {
                        lock (_thisLock)
                        {
                            if (!_isDisposed)
                            {
                                catalogs = _catalog;
                                _catalog = null;
                                _isDisposed = true;
                            }
                        }
                    }
                    finally
                    {
                        if (catalogs != null)
                        {
                            catalogs.Dispose();
                        }
                    }
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        private void ThrowIfDisposed()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException("ScriptCsCatalog");
            }
        }

        private void Initialize(string path, string searchPattern)
        {
            Path = path;
            FullPath = GetFullPath(path);

            SearchPattern = searchPattern;

            var scriptFiles = GetFiles();
            _loadedFiles = scriptFiles.ToReadOnlyCollection();

            _catalog = ExecuteScripts(scriptFiles);
        }

        private void Initialize(IEnumerable<string> scriptFiles)
        {
            foreach (var scriptFile in scriptFiles)
            {
                if (!_options.FileSystem.FileExists(scriptFile))
                {
                    throw new FileNotFoundException(string.Format("Script: '{0}' does not exist", scriptFile), scriptFile);
                }
            }

            _loadedFiles = scriptFiles.ToReadOnlyCollection();

            _catalog = ExecuteScripts(scriptFiles);
        }

        private AssemblyCatalog ExecuteScripts(IEnumerable<string> scriptFiles)
        {
            var services = CreateScriptServices();

            ScriptResult result = null;
            if (_options.KeepScriptsSeparated)
            {
                foreach (var scriptFile in scriptFiles)
                {
                    var loader = GetLoader(new[] { scriptFile });
                    result = ExecuteScript(services, loader);
                }
            }
            else
            {
                var loader = GetLoader(scriptFiles);
                result = ExecuteScript(services, loader);
            }

            AssemblyCatalog catalog = null;
            var marker = result.ReturnValue as Type;
            if (marker != null)
            {
                catalog = new AssemblyCatalog(marker.Assembly, this);
            }

            return catalog;
        }

        private ScriptResult ExecuteScript(ScriptServices services, string loader)
        {
            var result = services.Executor.ExecuteScript(loader);

            if (result.CompileExceptionInfo != null)
            {
                result.CompileExceptionInfo.Throw();
            }

            if (result.ExecuteExceptionInfo != null)
            {
                result.ExecuteExceptionInfo.Throw();
            }

            return result;
        }

        private string GetFullPath(string path)
        {
            if (!IOPath.IsPathRooted(path) && _options.FileSystem.CurrentDirectory != null)
            {
                path = IOPath.Combine(_options.FileSystem.CurrentDirectory, path);
            }

            return IOPath.GetFullPath(path);
        }

        private string[] GetFiles()
        {
            if (!_options.FileSystem.DirectoryExists(FullPath))
            {
                throw new DirectoryNotFoundException(string.Format("Scripts folder: '{0}' does not exist", FullPath));
            }

            return _options.FileSystem.EnumerateFiles(FullPath, SearchPattern).ToArray();
        }

        private string GetDisplayName()
        {
            return string.Format(CultureInfo.CurrentCulture, "{0} (Path=\"{1}\")", GetType().Name, Path);
        }

        private string GetLoader(IEnumerable<string> scriptFiles)
        {
            var builder = new StringBuilder();

            foreach (var scriptFile in scriptFiles)
            {
                builder.AppendFormat("#load {0}{1}", scriptFile, Environment.NewLine);
            }
            builder.AppendLine("public class Marker {}");
            builder.AppendLine("typeof(Marker)");

            return builder.ToString();
        }

        private ScriptServices CreateScriptServices()
        {
            var console = new ScriptConsole();
            var logProvider = new ColoredConsoleLogProvider(LogLevel.Info, console);

            var initializationServices = new InitializationServices(logProvider);
            initializationServices.GetAppDomainAssemblyResolver().Initialize();

            var scriptServicesBuilder = new ScriptServicesBuilder(console, logProvider, null, null, initializationServices);
            scriptServicesBuilder.Overrides[typeof(IFileSystem)] = _options.FileSystem;
            scriptServicesBuilder.LoadScriptPacks();
            scriptServicesBuilder.LoadModules(".csx", _options.Modules);
            
            var scriptServices = scriptServicesBuilder.Build();

            var assemblies = scriptServices.AssemblyResolver.GetAssemblyPaths(_options.FileSystem.CurrentDirectory, true);
            var packs = scriptServices.ScriptPackResolver.GetPacks();
            scriptServices.Executor.Initialize(assemblies, packs, _options.ScriptArgs);
            scriptServices.Executor.AddReferences(typeof(Attribute), typeof(ExportAttribute));
            scriptServices.Executor.ImportNamespaces("System.ComponentModel.Composition");

            if (_options.References != null)
            {
                scriptServices.Executor.AddReferenceAndImportNamespaces(_options.References);
            }

            return scriptServices;
        }
    }
}
