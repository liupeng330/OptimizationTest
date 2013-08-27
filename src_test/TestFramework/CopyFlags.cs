using System;

namespace AdSage.Concert.Test.Framework
{
    /// <summary>
    /// Options to use with DirectoryUtilities and FileUtilities copy operations
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "Flags"), Flags]
    public enum CopyFlags
    {
        /// <summary>
        /// Default flags
        /// </summary>
        None = 0x00000000,

        /// <summary>
        /// Do not copy recursively
        /// </summary>
        DisableRecursive = 0x00000001,

        /// <summary>
        /// Force overwriting the destination
        /// </summary>
        ForceOverwrite = 0x00000002,

        /// <summary>
        /// Clear the destination directory first
        /// </summary>
        ClearDestinationDirectoryFirst = 0x00000004,
    }
}
