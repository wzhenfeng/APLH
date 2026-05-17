-- Grant permissions to your Windows user
USE webapplication_db;
GO

-- Create login for your Windows user if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.server_principals WHERE name = 'LAPTOP-25EP612F\bennc')
BEGIN
    CREATE LOGIN [LAPTOP-25EP612F\bennc] FROM WINDOWS;
END
GO

-- Create user in the database
IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = 'LAPTOP-25EP612F\bennc')
BEGIN
    CREATE USER [LAPTOP-25EP612F\bennc] FOR LOGIN [LAPTOP-25EP612F\bennc];
END
GO

-- Grant full permissions
ALTER ROLE db_owner ADD MEMBER [LAPTOP-25EP612F\bennc];
GO

PRINT 'Permissions granted successfully!';
