-- Create PaymentDetails Table with support for encrypted sensitive data
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PaymentDetails]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[PaymentDetails](
        [Id] [uniqueidentifier] NOT NULL,
        [UserId] [uniqueidentifier] NOT NULL,
        [PaymentMethodType] [int] NOT NULL,
        [CardholderName] [nvarchar](500) NULL, -- Encrypted field, needs extra length
        [MaskedCardNumber] [nvarchar](20) NULL, -- Only last 4 digits, not encrypted
        [ExpirationMonth] [int] NOT NULL,
        [ExpirationYear] [int] NOT NULL,
        [BillingAddress] [nvarchar](1000) NULL, -- Encrypted field, needs extra length
        [CreatedDate] [datetime2](7) NOT NULL,
        [UpdatedDate] [datetime2](7) NULL,
        [PaymentToken] [nvarchar](500) NULL, -- Encrypted field, needs extra length
        CONSTRAINT [PK_PaymentDetails] PRIMARY KEY CLUSTERED 
        (
            [Id] ASC
        ),
        CONSTRAINT [FK_PaymentDetails_Users] FOREIGN KEY ([UserId])
        REFERENCES [dbo].[Users] ([Id])
    )
    
    -- Create index on UserId for faster lookups
    CREATE NONCLUSTERED INDEX [IX_PaymentDetails_UserId] ON [dbo].[PaymentDetails]
    (
        [UserId] ASC
    )
    
    PRINT 'Created PaymentDetails table with encrypted field support'
END
ELSE
BEGIN
    PRINT 'PaymentDetails table already exists'
END
GO

-- Add encryption settings to application configuration if they don't exist
IF NOT EXISTS (SELECT * FROM AppSettings WHERE [Key] = 'Encryption:Key')
BEGIN
    -- Note: In a real application, you wouldn't store encryption keys in the database
    -- This is just a placeholder to remind developers to set up proper encryption keys
    INSERT INTO AppSettings ([Key], [Value], [Description])
    VALUES 
        ('Encryption:Key', 'PLACEHOLDER_MUST_BE_REPLACED_WITH_ACTUAL_KEY', 'AES-256 encryption key (Base64 encoded) - MUST be set in environment variables or secure key vault'),
        ('Encryption:IV', 'PLACEHOLDER_MUST_BE_REPLACED_WITH_ACTUAL_IV', 'AES-256 initialization vector (Base64 encoded) - MUST be set in environment variables or secure key vault');
    
    PRINT 'Added placeholder encryption configuration settings - MUST be replaced with secure values'
END
ELSE
BEGIN
    PRINT 'Encryption configuration settings already exist'
END
GO

-- Add a comment in the database to remind about encrypted fields
IF NOT EXISTS (SELECT * FROM sys.extended_properties WHERE [major_id] = OBJECT_ID('PaymentDetails') AND [name] = 'ContainsEncryptedData')
BEGIN
    EXEC sp_addextendedproperty
        @name = N'ContainsEncryptedData',
        @value = N'This table contains encrypted fields: CardholderName, BillingAddress, PaymentToken',
        @level0type = N'SCHEMA', @level0name = N'dbo',
        @level1type = N'TABLE',  @level1name = N'PaymentDetails';
    
    PRINT 'Added database documentation for encrypted fields'
END
GO 