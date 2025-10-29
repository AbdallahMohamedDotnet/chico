using System;
using System.IO;

namespace ChicoDesktopApp
{
    public class BackupHelper
    {
        private readonly string _databasePath;
        private readonly string _backupFolder;

        public BackupHelper(string databasePath)
        {
            _databasePath = databasePath;
            
            // Create Backups folder in the same directory as Data folder
            var dataFolder = Path.GetDirectoryName(databasePath);
            var baseFolder = Path.GetDirectoryName(dataFolder);
            _backupFolder = Path.Combine(baseFolder!, "Backups");
            
            if (!Directory.Exists(_backupFolder))
            {
                Directory.CreateDirectory(_backupFolder);
            }
        }

        /// <summary>
        /// Create a backup of the database with timestamp
        /// </summary>
        public string CreateBackup()
        {
            if (!File.Exists(_databasePath))
            {
                throw new FileNotFoundException("Database file not found", _databasePath);
            }

            string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            string backupFileName = $"chico_backup_{timestamp}.db";
            string backupPath = Path.Combine(_backupFolder, backupFileName);

            try
            {
                File.Copy(_databasePath, backupPath, overwrite: false);
                return backupPath;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to create backup: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Restore database from a backup file
        /// </summary>
        public void RestoreBackup(string backupPath)
        {
            if (!File.Exists(backupPath))
            {
                throw new FileNotFoundException("Backup file not found", backupPath);
            }

            try
            {
                // Create a backup of current database before restoring
                if (File.Exists(_databasePath))
                {
                    string preRestoreBackup = Path.Combine(_backupFolder, 
                        $"pre_restore_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.db");
                    File.Copy(_databasePath, preRestoreBackup, overwrite: true);
                }

                // Restore the backup
                File.Copy(backupPath, _databasePath, overwrite: true);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to restore backup: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Get list of all backup files
        /// </summary>
        public string[] GetBackupFiles()
        {
            if (!Directory.Exists(_backupFolder))
            {
                return Array.Empty<string>();
            }

            var backups = Directory.GetFiles(_backupFolder, "*.db");
            Array.Sort(backups);
            Array.Reverse(backups); // Most recent first
            return backups;
        }

        /// <summary>
        /// Delete old backups, keeping only the specified number of most recent backups
        /// </summary>
        public int CleanupOldBackups(int keepCount = 10)
        {
            var backups = GetBackupFiles();
            int deletedCount = 0;

            if (backups.Length <= keepCount)
            {
                return 0;
            }

            // Delete older backups
            for (int i = keepCount; i < backups.Length; i++)
            {
                try
                {
                    File.Delete(backups[i]);
                    deletedCount++;
                }
                catch
                {
                    // Ignore errors when deleting old backups
                }
            }

            return deletedCount;
        }

        /// <summary>
        /// Get the size of a backup file in MB
        /// </summary>
        public double GetBackupSizeMB(string backupPath)
        {
            if (!File.Exists(backupPath))
            {
                return 0;
            }

            FileInfo fileInfo = new FileInfo(backupPath);
            return fileInfo.Length / (1024.0 * 1024.0);
        }

        /// <summary>
        /// Get total size of all backups in MB
        /// </summary>
        public double GetTotalBackupSizeMB()
        {
            var backups = GetBackupFiles();
            double totalSize = 0;

            foreach (var backup in backups)
            {
                totalSize += GetBackupSizeMB(backup);
            }

            return totalSize;
        }

        public string GetBackupFolder() => _backupFolder;
    }
}
