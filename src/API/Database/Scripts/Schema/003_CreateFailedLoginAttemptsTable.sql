-- Create FailedLoginAttempts Table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[FailedLoginAttempts]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[FailedLoginAttempts](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [Username] [nvarchar](50) NOT NULL,
        [FailedCount] [int] NOT NULL DEFAULT(0),
        [LastFailedAttempt] [datetime2](7) NOT NULL,
        CONSTRAINT [PK_FailedLoginAttempts] PRIMARY KEY CLUSTERED 
        (
            [Id] ASC
        ),
        CONSTRAINT [UK_FailedLoginAttempts_Username] UNIQUE NONCLUSTERED
        (
            [Username] ASC
        )
    )
    
    PRINT 'Created FailedLoginAttempts table'
END
ELSE
BEGIN
    PRINT 'FailedLoginAttempts table already exists'
END
GO

-- Create index on Username for faster lookups
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name='IX_FailedLoginAttempts_Username' AND object_id = OBJECT_ID('FailedLoginAttempts'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_FailedLoginAttempts_Username] ON [dbo].[FailedLoginAttempts]
    (
        [Username] ASC
    )
    
    PRINT 'Created index IX_FailedLoginAttempts_Username'
END
GO 