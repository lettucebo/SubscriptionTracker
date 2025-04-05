IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [Categories] (
    [Id] int NOT NULL IDENTITY,
    [ColorCode] nvarchar(7) NOT NULL DEFAULT N'#3A86FF',
    [Name] nvarchar(50) NOT NULL,
    [Description] nvarchar(200) NOT NULL,
    [IsDelete] bit NOT NULL DEFAULT CAST(0 AS bit),
    [DeleteAt] datetime2 NULL,
    CONSTRAINT [PK_Categories] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [Subscriptions] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(max) NOT NULL,
    [Amount] decimal(18,2) NOT NULL,
    [Fee] decimal(18,2) NOT NULL,
    [BillingCycle] nvarchar(10) NOT NULL,
    [DiscountRate] decimal(3,2) NOT NULL DEFAULT 0.0,
    [StartDate] date NOT NULL,
    [EndDate] date NULL,
    [CategoryId] int NOT NULL,
    [IsShared] bit NOT NULL DEFAULT CAST(0 AS bit),
    [ContactInfo] nvarchar(500) NULL,
    CONSTRAINT [PK_Subscriptions] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Subscriptions_Categories_CategoryId] FOREIGN KEY ([CategoryId]) REFERENCES [Categories] ([Id]) ON DELETE NO ACTION
);
GO

CREATE INDEX [IX_Subscriptions_CategoryId] ON [Subscriptions] ([CategoryId]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250404160407_InitialSchema', N'8.0.3');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Subscriptions] ADD [UserId] int NOT NULL DEFAULT 0;
GO

ALTER TABLE [Categories] ADD [UserId] int NOT NULL DEFAULT 0;
GO

CREATE TABLE [Users] (
    [Id] int NOT NULL IDENTITY,
    [ObjectId] nvarchar(128) NOT NULL,
    [DisplayName] nvarchar(100) NOT NULL,
    [Email] nvarchar(256) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_Users] PRIMARY KEY ([Id])
);
GO

CREATE INDEX [IX_Subscriptions_UserId] ON [Subscriptions] ([UserId]);
GO

CREATE INDEX [IX_Categories_UserId] ON [Categories] ([UserId]);
GO

CREATE INDEX [IX_Users_Email] ON [Users] ([Email]);
GO

CREATE UNIQUE INDEX [IX_Users_ObjectId] ON [Users] ([ObjectId]);
GO

ALTER TABLE [Categories] ADD CONSTRAINT [FK_Categories_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE;
GO

ALTER TABLE [Subscriptions] ADD CONSTRAINT [FK_Subscriptions_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250405052337_AddUserEntity', N'8.0.3');
GO

COMMIT;
GO

