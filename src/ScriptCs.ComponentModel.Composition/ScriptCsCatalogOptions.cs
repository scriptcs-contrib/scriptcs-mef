using ScriptCs.Contracts;
using System;

namespace ScriptCs.ComponentModel.Composition
{
    /// <summary>
    /// ScriptCsCatalog options
    /// </summary>
    public class ScriptCsCatalogOptions
    {
        /// <summary>
        /// Arguments passed to the scripts.
        /// </summary>
        public string[] ScriptArgs { get; set; }

        /// <summary>
        /// References to add automatically to all the scripts.
        /// </summary>
        public Type[] References { get; set; }

        /// <summary>
        /// File system used to get the scripts files.
        /// </summary>
        public IFileSystem FileSystem { get; set; }

        /// <summary>
        /// Modules to load in scripts.
        /// </summary>
        public string[] Modules { get; set; }

        internal ScriptCsCatalogOptions OverridesNullByDefault()
        {
            return new ScriptCsCatalogOptions
            {
                References = References,
                ScriptArgs = ScriptArgs ?? new string[0],
                FileSystem = FileSystem ?? new FileSystem(),
                Modules = Modules ?? new string[0]
            };
        }
    }
}