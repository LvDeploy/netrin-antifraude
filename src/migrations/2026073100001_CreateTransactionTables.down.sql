IF OBJECT_ID(N'dbo.TransactionHistorics', N'U') IS NOT NULL
BEGIN
    DROP TABLE [dbo].[TransactionHistorics];
END;

IF OBJECT_ID(N'dbo.Transactions', N'U') IS NOT NULL
BEGIN
    DROP TABLE [dbo].[Transactions];
END;
