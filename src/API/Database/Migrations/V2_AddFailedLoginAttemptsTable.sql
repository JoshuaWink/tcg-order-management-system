-- Migration: V2_AddFailedLoginAttemptsTable.sql
-- Description: Adds table for tracking failed login attempts to implement account lockout functionality
-- Date: 2023-08-21

-- Create table for tracking failed login attempts
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'FailedLoginAttempts')
BEGIN
    CREATE TABLE FailedLoginAttempts (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Username NVARCHAR(100) NOT NULL,
        FailedCount INT NOT NULL DEFAULT 0,
        LastFailedAttempt DATETIME2 NOT NULL,
        CONSTRAINT UQ_FailedLoginAttempts_Username UNIQUE (Username)
    );
    
    -- Create index for faster lookups
    CREATE INDEX IX_FailedLoginAttempts_Username ON FailedLoginAttempts (Username);
    
    PRINT 'Created FailedLoginAttempts table';
END
ELSE
BEGIN
    PRINT 'FailedLoginAttempts table already exists';
END

-- Add lockout settings to application configuration if not exists
IF NOT EXISTS (SELECT * FROM AppSettings WHERE [Key] = 'Security:MaxFailedAttempts')
BEGIN
    INSERT INTO AppSettings ([Key], [Value], [Description])
    VALUES 
        ('Security:MaxFailedAttempts', '5', 'Maximum number of failed login attempts before account lockout'),
        ('Security:LockoutMinutes', '15', 'Duration of account lockout in minutes');
    
    PRINT 'Added account lockout configuration settings';
END
ELSE
BEGIN
    PRINT 'Account lockout configuration settings already exist';
END 