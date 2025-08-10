using System.IO;

namespace BruSoftware.SharedServices.ExtensionMethods;

public static class IOExtensions
{
    /// <summary>
    /// Thanks to https://stackoverflow.com/questions/876473/is-there-a-way-to-check-if-a-file-is-in-use
    /// </summary>
    /// <param name="file"></param>
    /// <returns></returns>
    public static bool IsFileLocked(this FileInfo file)
    {
        FileStream stream = null;
        try
        {
            stream = file.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
        }
        catch (IOException)
        {
            //the file is unavailable because it is:
            //still being written to
            //or being processed by another thread
            //or does not exist (has already been processed)
            return true;
        }
        finally
        {
            stream?.Close();
        }

        //file is not locked
        return false;
    }
}