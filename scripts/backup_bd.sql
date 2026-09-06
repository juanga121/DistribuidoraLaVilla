-- Backup script for DistribuidoraLaVilla
-- Creates a full database backup with timestamp and prunes old backups.
--
-- Usage (from the repo root, or any machine with sqlcmd + access to the DB):
--   sqlcmd -S localhost -E -i scripts\backup_bd.sql
--
-- Optional: customize @BackupFolder to the desired backup location.

SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @BackupFolder nvarchar(500) = N'C:\Users\juanp\source\repos\ProyectoDlv\backups\';
DECLARE @DatabaseName sysname = N'DistribuidoraLaVilla';
DECLARE @Timestamp nvarchar(20) = REPLACE(REPLACE(REPLACE(CONVERT(nvarchar(20), GETDATE(), 120), ':', ''), '-', ''), ' ', '_');
DECLARE @FileName nvarchar(600) = @BackupFolder + @DatabaseName + N'_' + @Timestamp + N'.bak';
DECLARE @RetentionDays int = 14;

-- Create the backup folder if it does not exist.
EXEC master.dbo.xp_create_subdir @BackupFolder;

-- Full database backup.
BACKUP DATABASE @DatabaseName
TO DISK = @FileName
WITH INIT, STATS = 10;

PRINT N'';
PRINT N'Backup created: ' + @FileName;