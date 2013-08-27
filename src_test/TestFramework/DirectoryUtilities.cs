using System;
using System.IO;
using System.Linq;

namespace AdSage.Concert.Test.Framework
{
    /// <summary>
    /// Static class that contains helper methods which act upon directories,
    /// in much the same manner as the System.IO.Directory class.
    /// </summary>
    public static class DirectoryUtilities
    {
        /// <summary>
        /// Delete any of the specified files which exist in the given directory.
        /// </summary>
        public static void DeleteFilesInDir(string path, params string[] files)
        {
            FileUtilities.DeleteFiles(from f in files select Path.Combine(path, f));
        }

        /// <summary>
        /// Delete any of the specified files which exist in the given directory.
        /// </summary>
        public static void DeleteFilesInRemoteDir(string location, string path, params string[] files)
        {
            FileUtilities.DeleteFiles(from f in files select Path.Combine(path, f));
        }

        /// <summary>
        /// Delete every file and subfolder in the given directory
        /// Keep the given directory if keepDir is set to true
        /// </summary>
        /// <param name="dir">The path of the directory to be deleted</param>
        /// <param name="keepDir">Whether to keep the directory itself</param>
        public static void DeleteDirectory(bool keepDir, string dir)
        {
            if (dir != null && Directory.Exists(dir))
            {
                DeleteInput(dir, dir, keepDir);
            }
        }

        /// <summary>
        /// Delete every file and subfolder in the given directory
        /// Keep the given directory if keepDir is set to true
        /// </summary>
        /// <param name="location">Component or machine</param>
        /// <param name="dir">The path of the directory to be deleted</param>
        /// <param name="keepDir">Whether to keep the directory itself</param>
        public static void DeleteRemoteDirectory(bool keepDir, string location, string dir)
        {
            if (dir != null && Directory.Exists(dir))
            {
                DeleteInput(dir, dir, keepDir);
            }
        }

        /// <summary>
        /// Cleanup all the directories in the input list
        /// </summary>
        /// <param name="keepDirectories">false means the directories will be deleted, true means only the content in the directories will be deleted</param>
        /// <param name="directories">the directories to be cleaned</param>
        public static void DeleteDirectories(bool keepDirectories, params string[] directories)
        {
            foreach (string dir in directories)
            {
                DeleteDirectory(keepDirectories, dir);
            }
        }

        /// <summary>
        /// Cleanup all the directories in the input list
        /// </summary>
        /// <param name="keepDirectories">false means the directories will be deleted, true means only the content in the directories will be deleted</param>
        /// <param name="location">Component or machine</param>
        /// <param name="directories">the directories to be cleaned</param>
        public static void DeleteRemoteDirectories(bool keepDirectories, string location, params string[] directories)
        {
            foreach (string dir in directories)
            {
                DeleteRemoteDirectory(keepDirectories, location, dir);
            }
        }

        /// <summary>
        /// Delete everything inside destination directory where a same name item exists in source directory
        /// Keep the destination directory if keepDestination is set to true
        /// </summary>
        /// <param name="source">Source directory</param>
        /// <param name="destination">Destination directory</param>
        /// <param name="keepDestination">Whether to keep the Destination directory itself</param>
        public static void DeleteInput(string source, string destination, bool keepDestination)
        {
            if (!Directory.Exists(source))
            {
                return;
            }

            DirectoryInfo sourceInfo = new DirectoryInfo(source);
            FileInfo[] files = sourceInfo.GetFiles();

            if (files != null)
            {
                foreach (FileInfo s in files)
                {
                    string fileName = s.Name;
                    string destFile = Path.Combine(destination, fileName);
                    if (File.Exists(destFile))
                    {
                        FileInfo destFileInfo = new FileInfo(destFile);
                        if (destFileInfo.IsReadOnly)
                        {
                            destFileInfo.IsReadOnly = false;
                        }
                        destFileInfo.Delete();
                    }
                }
            }

            DirectoryInfo[] subDirs = sourceInfo.GetDirectories();

            if (subDirs != null)
            {
                foreach (DirectoryInfo dir in subDirs)
                {
                    string subDirName = dir.Name;
                    string destSubDir = Path.Combine(destination, subDirName);
                    DeleteInput(dir.FullName, destSubDir, false);
                }
            }
            if (!keepDestination)
            {
                try
                {
                    Directory.Delete(destination);
                }
                catch (IOException)
                {
                    // let it be...
                }
            }
        }

        /// <summary>
        /// This method will return the latest created file (looking back one hour from runtime)
        /// that matches the search pattern parameter.
        /// </summary>
        /// <param name="dir">directory to look in</param>
        /// <param name="searchPattern">Search pattern can use * as a wildcard, and ? for exactly zero or one character</param>
        /// <returns>the name of the file in the specified directory (fileDir)</returns>
        public static string GetLatestFile(string dir, string searchPattern)
        {
            string latest = Directory.GetFiles(dir, searchPattern).OrderByDescending(f => File.GetCreationTime(f)).FirstOrDefault();

            if (latest == null)
            {
                throw new FileNotFoundException("No files in \"" + dir + "\" match \"" + searchPattern + "\".");
            }

            return latest;
        }

        /// <summary>
        /// Creates a directory, then deletes it when the test is done.
        /// If the directory exists at the start of the test, it will be first deleted (including the files in it).
        /// </summary>        
        public static void CreateDirectory(TestBase test, string dir)
        {
            CreateDirectory(test, dir, "Delete \"" + dir + " and every file and subfolder in the given directory \".");
        }

        /// <summary>
        /// Creates a directory, then deletes it when the test is done.
        /// If the directory exists at the start of the test, it will be first deleted (including the files in it).
        /// Includes the option of adding to test cleanup stack or not.
        /// </summary>        
        public static void CreateDirectory(TestBase test, string dir, bool addToTestCleanup)
        {
            CreateDirectory(test, dir, "Delete \"" + dir + " and every file and subfolder in the given directory \".", addToTestCleanup);
        }

        /// <summary>
        /// Creates a directory, then deletes it when the test is done.
        /// If the directory exists at the start of the test, it will be first deleted (including the files in it).
        /// There is an option here to define the cleanup name
        /// </summary>
        public static void CreateDirectory(TestBase test, string dir, string cleanupName)
        {
            CreateDirectory(test, dir, cleanupName, true);
        }

        /// <summary>
        /// Creates a directory, then deletes it when the test is done.
        /// If the directory exists at the start of the test, it will be first deleted (including the files in it).
        /// There is an option here to define the cleanup name and also to add to cleanup stack or not.
        /// </summary>        
        public static void CreateDirectory(TestBase test, string dir, string cleanupName, bool addToTestCleanup)
        {
            if (Directory.Exists(dir))
            {
                DeleteDirectory(true, dir);
            }
            else
            {
                Directory.CreateDirectory(dir);
            }

            if (addToTestCleanup)
            {
                // Set up cleanup
                test.AddTestCleanup(
                    cleanupName,
                    () =>
                    {
                        DeleteDirectory(false, dir);
                    });
            }
        }

                /// <summary>
        /// Compare the source with destination
        /// If they are the same path, do nothing
        /// Otherwise, copy everything in source to destination that doesn't already
        /// exist there, or has a different date/time or length.  Note that setting
        /// clearDestination to true forces a full copy, regardless of file matching.
        /// </summary>
        /// <param name="source">Source directory</param>
        /// <param name="destination">Destination directory</param>
        /// <param name="options">options</param>
        public static void Copy(string source, string destination, CopyFlags options)
        {
            if (source == null && !Directory.Exists(source))
            {
                throw new ArgumentException("Invalid source path", source);
            }

            if (destination == null)
            {
                throw new ArgumentNullException(destination, "Null destination path");
            }

            // Copying onto yourself does nothing
            if (source.Equals(destination, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            // Cleanup existing destination folder if clearDestination flag is set to true
            if (Directory.Exists(destination) && (options & CopyFlags.ClearDestinationDirectoryFirst) != 0)
            {
                DeleteDirectory(false, destination);
            }

            RobocopyUtilities.CopyDirectory(source, destination, (options & CopyFlags.DisableRecursive) != 0);
        }

    }
}
