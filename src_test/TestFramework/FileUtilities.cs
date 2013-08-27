using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AdSage.Concert.Test.Framework
{
    /// <summary>
    /// Collection of utility methods to work on files, an extension to what
    /// the .Net framework provides with System.IO.File.  Some of these 
    /// methods also work with TestBase to automatically undo their operations
    /// when the test is complete.
    /// </summary>
    public static class FileUtilities
    {
        /// <summary>
        /// A short period of interval to keep monitor file existence
        /// </summary>
        public static readonly int SmallMonitorInterval = 1;

        /// <summary>
        /// A relative long period of interval to keep monitor file existence
        /// </summary>
        public static readonly int LargeMonitorInterval = 5;

        /// <summary>
        /// Delete all files and subdirectories from the directory specified
        /// </summary>
        /// <param name="directory"></param>
        public static void DeleteAllFromDirectory(string directory)
        {
            DeleteAllFromDirectoryExceptFile(directory, string.Empty);
        }

        public static void DeleteAllFromDirectoryExceptFile(string directory, string exceptFileName)
        {
            if (directory == null)
            {
                throw new ArgumentNullException("directory");
            }
            if (Directory.Exists(directory))
            {
                List<string> dirs = new List<string>();
                dirs.Add(directory);
                while (dirs.Count > 0)
                {
                    foreach (string file in Directory.GetFiles(dirs[0]))
                    {
                        FileInfo fi = new FileInfo(file);
                        if (fi.IsReadOnly) fi.IsReadOnly = false;
                        if (string.IsNullOrEmpty(exceptFileName) || !string.Equals(fi.Name, exceptFileName, StringComparison.OrdinalIgnoreCase))
                        {
                            File.Delete(file);
                        }
                    }
                    foreach (string dir in Directory.GetDirectories(dirs[0]))
                    {
                        dirs.Add(dir);
                    }
                    dirs.RemoveAt(0);
                }
                // Delete the empty sub directories
                foreach (string dir in Directory.GetDirectories(directory))
                {
                    Directory.Delete(dir, true);
                }
            }
            else
            {
                throw new DirectoryNotFoundException(directory);
            }
        }

        /// <summary>
        /// Deletes the specified files if they exist.
        /// </summary>
        public static void DeleteFiles(params string[] files)
        {
            DeleteFiles((IEnumerable<string>)files);
        }

        /// <summary>
        /// Deletes the specified files if they exist.
        /// </summary>
        public static void DeleteFiles(IEnumerable<string> files)
        {
            foreach (string file in files)
            {
                DeleteFile(file);
            }
        }

        /// <summary>
        /// Deletes the specified file if it exists.
        /// </summary>
        public static void DeleteFile(string file)
        {
            if (File.Exists(file))
            {
                File.Delete(file);
            }
        }

        /// <summary>
        /// Reads the given file into an enumerable of split strings.
        /// Does checking to ensure that each input line has the same number
        /// of columns.
        /// </summary>
        public static IEnumerable<string[]> ReadAsCsv(string fileName)
        {
            return ReadAsCsv(fileName, true);
        }

        /// <summary>
        /// Reads the given file into an enumerable of split strings.
        /// User specify whether to check and ensure that each input line has the same number
        /// of columns.
        /// </summary>
        public static IEnumerable<string[]> ReadAsCsv(string fileName, bool lineShouldHaveSameLength)
        {
            using (StreamReader reader = new StreamReader(fileName))
            {
                int columnCount = 0;
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    string[] split = line.Split(',');
                    if (lineShouldHaveSameLength)
                    {
                        if (columnCount == 0)
                        {
                            columnCount = split.Length;
                        }
                        else
                        {
                            Assert.AreEqual(columnCount, split.Length, "This line has the wrong number of columns: " + line);
                        }
                    }
                    yield return split;
                }
            }
        }
    }
}
