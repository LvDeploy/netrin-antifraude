IF OBJECT_ID(N'dbo.Transactions', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Transactions]
    (
        [Id] UNIQUEIDENTIFIER NOT NULL,
        [Value] DECIMAL(18, 2) NOT NULL,
        [Status] INT NOT NULL,
        [IdEmpotency] NVARCHAR(MAX) NOT NULL,
        [CreatedAt] DATETIME2(7) NOT NULL,
        CONSTRAINT [PK_Transactions] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [CK_Transactions_Status] CHECK ([Status] IN (0, 1, 2))
    );
END;

IF OBJECT_ID(N'dbo.TransactionHistorics', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[TransactionHistorics]
    (
        [Id] UNIQUEIDENTIFIER NOT NULL,
        [IdEmpotency] NVARCHAR(MAX) NOT NULL,
        [Status] INT NOT NULL,
        [EventTime] DATETIME2(7) NOT NULL,
        [TransactionId] UNIQUEIDENTIFIER NOT NULL,
        [StatusMessage] NVARCHAR(MAX) NOT NULL,
        CONSTRAINT [PK_TransactionHistorics] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [CK_TransactionHistorics_Status] CHECK ([Status] IN (0, 1, 2)),
        CONSTRAINT [FK_TransactionHistorics_Transactions_TransactionId]
            FOREIGN KEY ([TransactionId])
            REFERENCES [dbo].[Transactions] ([Id])
            ON DELETE NO ACTION
    );
END;

IF OBJECT_ID(N'dbo.TransactionHistorics', N'U') IS NOT NULL
   AND NOT EXISTS (
        SELECT 1
        FROM sys.indexes
        WHERE [name] = N'IX_TransactionHistorics_TransactionId'
          AND [object_id] = OBJECT_ID(N'dbo.TransactionHistorics', N'U')
   )
BEGIN
    CREATE NONCLUSTERED INDEX [IX_TransactionHistorics_TransactionId]
        ON [dbo].[TransactionHistorics] ([TransactionId] ASC);
END;
