using System;
using System.IO;
using System.Diagnostics;

namespace AdSage.Concert.Test.Framework
{
    ///<summary>
    /// Robocopy wrapper for copying directories
    ///</summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Robocopy")]
    public static class RobocopyUtilities
    {
        private const string RobocopyExecutableName = "robocopy.exe";

        private static FileInfo FindRobocopyLocation()
        {
            DirectoryInfo windowsDirectory = new DirectoryInfo(Environment.SystemDirectory);
            FileInfo fullRobocopyLocation = new FileInfo(Path.Combine(windowsDirectory.FullName, RobocopyExecutableName));
            if (fullRobocopyLocation.Exists)
            {
                return fullRobocopyLocation;
            }
            
            DirectoryInfo windowsDir = windowsDirectory.Parent;
            if(windowsDir != null)
            {
                fullRobocopyLocation = new FileInfo(Path.Combine(windowsDir.FullName, RobocopyExecutableName));
                if (fullRobocopyLocation.Exists)
                {
                    return fullRobocopyLocation;
                }
            }
            
            fullRobocopyLocation = new FileInfo(Path.Combine(Environment.CurrentDirectory, RobocopyExecutableName));
            if (fullRobocopyLocation.Exists)
            {
                return fullRobocopyLocation;
            }

            throw new FileNotFoundException("Could not find " + RobocopyExecutableName + " in either Windows directory nor System directory nor the current folder");
        }

        ///<summary>
        /// Copies all files and subdirectories from source directory to target directory.
        ///</summary>
        ///<param name="sourceDir">Source directory</param>
        ///<param name="targetDir">Target directory</param>
        ///<param name="disableRecursive">Disable recursive copying</param>
        public static void CopyDirectory(string sourceDir, string targetDir, bool disableRecursive)
        {
            FileInfo robocopyLocation = FindRobocopyLocation();

            // Using 2 retries on failed copies with a default wait interval of 30 seconds.
            string arguments = "\"" + sourceDir + "\" \"" + targetDir + "\" /R:2";
            if(!disableRecursive)
            {
                arguments += " /E";
            }
            
            // robocopy.exe's exit code is a bitmap defined as follows:
            //
            //  Hex Bit Value Decimal Value Meaning If Set
            //  ------------- ------------- --------------------------------------------------------------------------------------------------------------------------------------------------------------------------
            //  0x10          16            Serious error. Robocopy did not copy any files. This is either a usage error or an error due to insufficient access privileges on the source or destination directories.
            //  0x08          8             Some files or directories could not be copied (copy errors occurred and the retry limit was exceeded). Check these errors further.
            //  0x04          4             Some Mismatched files or directories were detected. Examine the output log. Housekeeping is probably necessary.
            //  0x02          2             Some Extra files or directories were detected. Examine the output log. Some housekeeping may be needed.
            //  0x01          1             One or more files were copied successfully (that is, new files have arrived).
            //  0x00          0             No errors occurred, and no copying was done. The source and destination directory trees are completely synchronized.
           // int robocopyExitCode = ProcessUtilities.ExecuteCommand(".", Environment.CurrentDirectory, robocopyLocation.FullName, arguments, null);;
            Process roboProcess = Process.Start(robocopyLocation.FullName, arguments);
            roboProcess.WaitForExit();
            int robocopyExitCode = roboProcess.ExitCode;

            if(robocopyExitCode > 7)
            {
                throw new IOException("Robocopy failed to copy some files, error code is " +
                                      robocopyExitCode + ". See log for more info.");
            }
        }
    }
}
