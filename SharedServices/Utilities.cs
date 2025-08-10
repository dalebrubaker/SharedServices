using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using BruSoftware.ListMmf;
using NLog;

namespace BruSoftware.SharedServices;

/// <summary>
/// Some utility helpers
/// </summary>
public static class Utilities
{
    private static readonly Logger s_logger = LogManager.GetCurrentClassLogger();

    public static string AssemblyDirectory
    {
        get
        {
            var codeBase = Assembly.GetExecutingAssembly().Location;
            var uri = new UriBuilder(codeBase);
            var path = Uri.UnescapeDataString(uri.Path);
            return Path.GetDirectoryName(path);
        }
    }

    /// <summary>
    /// method to set the name of the current thread
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public static bool SetThreadName(string name)
    {
        if (Thread.CurrentThread.Name != null)
        {
            return false;
        }

        //variable to hold our return value
        var success = false;
        try
        {
            //get the current thread
            var current = Thread.CurrentThread;
            current.Name = name;
            success = true;
            // s_logger.Error($"SetThreadName {name} CurrentManagedThreadId={Environment.CurrentManagedThreadId}");
        }
        catch (ThreadStateException ex)
        {
            success = false;
            var _ = ex.ToString();
            s_logger.Error(ex, ex.Message);

            //MessageBox.Show(ex.Message);
        }
        catch (Exception ex)
        {
            success = false;
            var _ = ex.ToString();
            s_logger.Error(ex, ex.Message);

            //MessageBox.Show(ex.Message);
        }
        return success;
    }

    public static void ThrowIfIsUIThread()
    {
        if (GlobalsShared.IsUnitTesting)
        {
            return;
        }
        var threadId = Environment.CurrentManagedThreadId;
        if (threadId == GlobalsShared.UIThreadId)
        {
            s_logger.Error("ThrowIfIsUIThread called on the UI thread. Locking is not allowed on the UI thread because we don't want UI hangs");
            throw new InvalidOperationException("Locking is not allowed on the UI thread because we don't want UI hangs");
        }
    }

    public static void ThrowIfIsNotUIThread()
    {
        if (GlobalsShared.IsUnitTesting)
        {
            return;
        }
        var threadId = Environment.CurrentManagedThreadId;
        if (threadId != GlobalsShared.UIThreadId)
        {
            s_logger.Error("ThrowIfIsNotUIThread called from off the UI thread. Pego must be on the UI thread");
            throw new InvalidOperationException("ThrowIfIsNotUIThread called from off the UI thread. Pego must be on the UI thread");
        }
    }

    public static void DeleteDirectoryAndAllFiles(string directory)
    {
        if (!Directory.Exists(directory))
        {
            return;
        }
        DeleteSubDirectoriesAndAllFiles(directory);
        DeleteWithRetry(directory);
    }

    public static void DeleteSubDirectoriesAndAllFiles(string directory)
    {
        var subDirectories = Directory.GetDirectories(directory, "*", SearchOption.AllDirectories);
        Array.Reverse(subDirectories);
        foreach (var subDirectory in subDirectories)
        {
            DeleteWithRetry(subDirectory);
        }
    }

    private static void DeleteWithRetry(string directory)
    {
        if (!Directory.Exists(directory))
        {
            // nothing to delete
            return;
        }
        try
        {
            //s_logger.ConditionalDebug($"Deleting {directory}");
            Directory.Delete(directory, true);
        }
        catch (Exception ex)
        {
            s_logger.Error(ex, " Trying again... {Message} {Directory}", ex.Message, directory);
            Thread.Sleep(1000);
            try
            {
                //s_logger.ConditionalDebug($"Deleting {directory}");
                Directory.Delete(directory, true);
            }
            catch (Exception ex2)
            {
                // var di = new DirectoryInfo(directory);
                // var files = di.GetFiles("*.*", SearchOption.AllDirectories);
                // if (files.Length == 1)
                // {
                //     var file = files.FirstOrDefault();
                //     if (file != null && file.Name.EndsWith("sqlite"))
                //     {
                //         // HACK! Not needed now that I am using :memory: files for unit testing, also maybe because I am using FastCRUD and disposing the connection.
                //         s_logger.ConditionalDebug($"Ignoring inability to close {directory} because of {file} still in use, a System.Data.Sqlite issue.");
                //         return;
                //     }
                // }
                var msg = $"{directory} {ex2.Message}";
                s_logger.Error(ex2, msg);
                throw new SharedServicesException(msg);
            }
        }
    }

    /// <summary>
    /// Thanks to https://stackoverflow.com/questions/2811509/c-sharp-remove-all-empty-subdirectories
    /// </summary>
    /// <param name="startLocation"></param>
    public static void DeleteEmptyDirectoryAndAllEmptySubdirectories(string startLocation)
    {
        try
        {
            foreach (var directory in Directory.GetDirectories(startLocation))
            {
                DeleteEmptyDirectoryAndAllEmptySubdirectories(directory);
                if (Directory.GetFiles(directory).Length == 0
                    && Directory.GetDirectories(directory).Length == 0)
                {
                    //s_logger.ConditionalDebug($"Deleting {directory}");
                    Directory.Delete(directory, false);
                }
            }
        }
        catch (Exception ex)
        {
            s_logger.Warn("Ignoring error {0}:{1}", nameof(DeleteDirectoryAndAllFiles), ex.Message);
        }
    }

    /// <summary>
    /// Thanks to Jon Skeet http://stackoverflow.com/questions/466946/how-to-initialize-a-listt-to-a-given-size-as-opposed-to-capacity
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="count"></param>
    /// <returns></returns>
    public static List<T> RepeatedDefault<T>(int count)
    {
        return Repeated(default(T), count);
    }

    /// <summary>
    /// Thanks to Jon Skeet http://stackoverflow.com/questions/466946/how-to-initialize-a-listt-to-a-given-size-as-opposed-to-capacity
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="value"></param>
    /// <param name="count"></param>
    /// <returns></returns>
    public static List<T> Repeated<T>(T value, int count)
    {
        var ret = new List<T>(count);
        ret.AddRange(Enumerable.Repeat(value, count));
        return ret;
    }

    public static object[] Args(params object[] args)
    {
        return args;
    }

    public static T Convert<T>(string value)
    {
        if (typeof(T).IsEnum)
        {
            return (T)Enum.Parse(typeof(T), value);
        }
        return (T)System.Convert.ChangeType(value, typeof(T));
    }

    public static T Convert<T>(double value)
    {
        return (T)System.Convert.ChangeType(value, typeof(T));
    }

    /// <summary>
    /// Thanks to http://stackoverflow.com/questions/278439/creating-a-temporary-directory-in-windows
    /// </summary>
    /// <returns></returns>
    public static string GetTemporaryDirectory(string callerName)
    {
        var tempDirectory = Path.Combine(Path.GetTempPath(), "_" + "BruTrader22", callerName + "_" + Path.GetRandomFileName());
        CreateDirectory(tempDirectory);
        //s_logger.ConditionalDebug($"Created {tempDirectory}");
        return tempDirectory;
    }

    /// <summary>
    /// Delay for msec milliseconds
    /// </summary>
    /// <param name="msec"></param>
    public static void Wait(int msec)
    {
        var timeWas = DateTime.Now;
        long ticks = msec * 10000; // a tick is 100 nanoseconds
        var targetTime = timeWas + new TimeSpan(ticks);
        while (DateTime.Now < targetTime)
        {
            // wait
        }
    }

    /// <summary>
    /// Enclose sendStr in double quotes and append a comma
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static string EncloseForCSV(string str)
    {
        var result = "\"" + str + "\",";
        return result;
    }

    /// <summary>
    /// append a comma
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static string AppendComma(string str)
    {
        var result = str + ",";
        return result;
    }

    /// <summary>
    /// append a tab
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static string AppendTab(string str)
    {
        var result = str + '\x09';
        return result;
    }

    /// <summary>
    /// Return a string that has a tab after each string in strs
    /// </summary>
    /// <param name="strs"></param>
    /// <returns></returns>
    public static string EnTab(List<string> strs)
    {
        var sb = new StringBuilder();
        foreach (var str in strs)
        {
            sb.Append(str);
            sb.Append('\x09');
        }
        return sb.ToString();
    }

    public static bool MutexNameExists(string mutexName)
    {
        var exists = Mutex.TryOpenExisting(mutexName, out var mutex);
        if (exists)
        {
            // We didn't really want to open one.
            mutex.Close();
        }
        return exists;
    }

    /// <summary>
    /// Return the fully-qualified path
    /// </summary>
    /// <param name="directory">Created if not null and does not exist.</param>
    /// <param name="fileName">fileName and extension -- everything after directory</param>
    /// <returns></returns>
    public static string GetPathWithCreateDirectory(string directory, string fileName)
    {
        if (string.IsNullOrEmpty(directory))
        {
            return fileName;
        }
        if (!Directory.Exists(directory))
        {
            CreateDirectory(directory);
        }
        return Path.Combine(directory, fileName);
    }

    /// <summary>
    /// Open or Create an empty file with name directory\prefixNN.extension. Return the prefixNN used.
    /// </summary>
    /// <param name="directory">Created if not null and does not exist.</param>
    /// <param name="prefix">The file name before the numeric counter that makes it unique</param>
    /// <param name="extension">The extension, including the preceding ., like .txt</param>
    /// <param name="fileMode"></param>
    /// <exception cref="SharedServicesException">if no file can be opened with the giving fileMode</exception>
    public static FileStream OpenUniqueFileNameSequential(string directory, string prefix, string extension, FileMode fileMode)
    {
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            CreateDirectory(directory);
        }
        for (var i = 0; i < 1000000; i++)
        {
            var filename = prefix + i;
            var path = string.IsNullOrEmpty(directory) ? filename + extension : Path.Combine(directory, filename + extension);
            if (File.Exists(path))
            {
                continue;
            }

            // Create the empty file
            try
            {
                FileStream stream = null;
                Retry.Do(() => { stream = new FileStream(path, fileMode); }, TimeSpan.FromSeconds(1));
                return stream;
            }
            catch (AggregateException aex)
            {
                aex.Flatten().Handle(ex =>
                {
                    s_logger.Error(ex, "{Message}", ex.Message);
                    return false;
                });
                throw;
            }
            catch (Exception ex)
            {
                s_logger.Error(ex, "{Message}", ex.Message);
                throw;
            }
        }
        throw new SharedServicesException("Unable to open unique file.");
    }

    /// <summary>
    /// From https://docs.microsoft.com/en-us/dotnet/standard/io/how-to-copy-directories
    /// </summary>
    /// <param name="sourceDirName"></param>
    /// <param name="destDirName"></param>
    /// <param name="copySubDirs"></param>
    public static void CopyDirectory(string sourceDirName, string destDirName, bool copySubDirs)
    {
        // Get the subdirectories for the specified directory.
        var dir = new DirectoryInfo(sourceDirName);

        if (!dir.Exists)
        {
            throw new DirectoryNotFoundException(
                "Source directory does not exist or could not be found: "
                + sourceDirName);
        }

        var dirs = dir.GetDirectories();
        // If the destination directory doesn't exist, create it.
        if (!Directory.Exists(destDirName))
        {
            CreateDirectory(destDirName);
        }

        // Get the files in the directory and copy them to the new location.
        var files = dir.GetFiles();
        foreach (var file in files)
        {
            // Skip lock files - they shouldn't exist unless crashes occured and shouldn't be copied
            if (file.Extension == UtilsListMmf.LockFileExtension)
            {
                continue;
            }

            if (destDirName != null)
            {
                var tempPath = Path.Combine(destDirName, file.Name);
                file.CopyTo(tempPath, false);
            }
        }

        // If copying subdirectories, copy them and their contents to new location.
        if (copySubDirs)
        {
            foreach (var subDirectory in dirs)
            {
                if (destDirName != null)
                {
                    var tempPath = Path.Combine(destDirName, subDirectory.Name);
                    CopyDirectory(subDirectory.FullName, tempPath, copySubDirs);
                }
            }
        }
    }

    public static DirectoryInfo CreateDirectory(string directoryName)
    {
        return Directory.CreateDirectory(directoryName);
    }

    /// <summary>
    /// Return <c>true</c> when time is in the range from beginTimeHHMM up through endTimeHHMM
    /// This handles sessions e.g. for futures where the start of the day is later timeOfDay than the end
    /// </summary>
    /// <param name="time"></param>
    /// <param name="begin"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    public static bool IsInRange(DateTime time, DateTime begin, DateTime end)
    {
        var isReversed = begin > end;
        bool isInRange;
        if (isReversed)
        {
            isInRange = time >= begin || time <= end;
        }
        else // not _isReversed
        {
            isInRange = time >= begin && time <= end;
        }
        return isInRange;
    }

    public static void ValidateSqlitePath(string path)
    {
        if (!File.Exists(path))
        {
            throw new SharedServicesException($"SqLite database file {path} does not exist.");
        }

        // Also be sure the file is not zero length, because Sqlite auto-creates such a file and the complains the table is missing 
        var fileInfo = new FileInfo(path);
        if (fileInfo.Length == 0)
        {
            throw new SharedServicesException($"SqLite database file {path} has zero length.\nProbably auto-created by SqLite.");
        }
    }
}