using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.IO;
using System.Management;
using System.Threading.Tasks;

namespace Disc_Control
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            int interval = 10;
            while (true)
            {
                Console.Clear();
                ReloadDrives();
                await Task.Delay(interval * 1000); // Wait for the specified interval in seconds
            }
        }

        static void ReloadDrives()
        {
            string skeleton = Drives();
            Console.WriteLine(skeleton);
        }

        static string Drives()
        {
            string information = "Drive         FreeSpace (GB)       %FreeSpace       UsedSpace (GB)       TotalSpace (GB)     FileSystem     DriveType          Name       Serial Number\n";
            information += "- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -\r\n";
            try
            {
                DriveInfo[] drives = DriveInfo.GetDrives();
                if (drives.Length == 0)
                {
                    information += "No drives found.\n";
                }
                foreach (DriveInfo drive in drives)
                {
                    string driveInfo;
                    string serialNumber = GetDriveSerialNumber(drive.Name);
                    if (drive.IsReady)
                    {
                        double freeSpace = drive.AvailableFreeSpace / (1024 * 1024 * 1024.0);
                        double totalSpace = drive.TotalSize / (1024 * 1024 * 1024.0);
                        double usedSpace = totalSpace - freeSpace;
                        double fsPercentage = (freeSpace / totalSpace) * 100;
                        string fileSystem = drive.DriveFormat;
                        string driveType = drive.DriveType.ToString();
                        string driveName = string.IsNullOrEmpty(drive.VolumeLabel) ? "Unnamed" : drive.VolumeLabel;

                        driveInfo = $"{drive.Name,-10} {freeSpace,15:F2}  {fsPercentage,15:F2}% {usedSpace,20:F2} {totalSpace,15:F2}  {fileSystem,15}  {driveType,15}  {driveName,15}  {serialNumber,15}";

                        // Check if free space percentage is less than or equal to 10%
                        if (fsPercentage >= 20)
                        {
                            // Trigger notification
                            Notification.Show(drive.Name, fsPercentage);
                        }
                    }
                    else
                    {
                        driveInfo = $"{drive.Name,-10} {"N/A",15}  {"N/A",15} {"N/A",20} {"N/A",15}  {"N/A",15}  {drive.DriveType.ToString(),15}  {"Not Ready",15}  {serialNumber,15}";
                    }
                    information += driveInfo + "\n";
                }
            }
            catch (Exception ex)
            {
                information += $"An error occurred: {ex.Message}\n";
            }
            return information;
        }

        static string GetDriveSerialNumber(string driveName)
        {
            try
            {
                if (!driveName.EndsWith("\\"))
                {
                    driveName += "\\";
                }

                ManagementObjectSearcher searcher = new ManagementObjectSearcher($"SELECT VolumeSerialNumber FROM Win32_LogicalDisk WHERE DeviceID = '{driveName.Substring(0, 2)}'");
                foreach (ManagementObject vol in searcher.Get())
                {
                    return vol["VolumeSerialNumber"]?.ToString() ?? "Unknown";
                }
            }
            catch (Exception)
            {
                return "Error";
            }
            return "Unknown";
        }
    }

    internal static class Notification
    {
        public static void Show(string driveName, double fsPercentage)
        {
            new ToastContentBuilder()
                .AddArgument("action", "viewConversation")
                .AddArgument("conversationId", 9813)
                .AddText($"Drive '{driveName}' has reached a critical threshold of {fsPercentage}% free space.")
                .Show();
        }
    }
}
