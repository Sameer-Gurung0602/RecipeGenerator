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

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260128144431_InitialCreate'
)
BEGIN
    CREATE TABLE [DietaryRestrictions] (
        [DietaryRestrictionsId] int NOT NULL IDENTITY,
        [Name] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_DietaryRestrictions] PRIMARY KEY ([DietaryRestrictionsId])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260128144431_InitialCreate'
)
BEGIN
    CREATE TABLE [Ingredients] (
        [IngredientId] int NOT NULL IDENTITY,
        [IngredientName] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_Ingredients] PRIMARY KEY ([IngredientId])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260128144431_InitialCreate'
)
BEGIN
    CREATE TABLE [Recipes] (
        [RecipeId] int NOT NULL IDENTITY,
        [Name] nvarchar(max) NOT NULL,
        [Description] nvarchar(max) NOT NULL,
        [CookTime] int NOT NULL,
        [Difficulty] nvarchar(max) NOT NULL,
        [CreatedAt] datetime2 NOT NULL DEFAULT (GETDATE()),
        [Img] nvarchar(max) NULL,
        [InstructionsId] int NOT NULL,
        CONSTRAINT [PK_Recipes] PRIMARY KEY ([RecipeId])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260128144431_InitialCreate'
)
BEGIN
    CREATE TABLE [Users] (
        [UserId] int NOT NULL IDENTITY,
        [Username] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_Users] PRIMARY KEY ([UserId])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260128144431_InitialCreate'
)
BEGIN
    CREATE TABLE [Instructions] (
        [InstructionsId] int NOT NULL IDENTITY,
        [Instruction] nvarchar(max) NOT NULL,
        [RecipeId] int NOT NULL,
        CONSTRAINT [PK_Instructions] PRIMARY KEY ([InstructionsId]),
        CONSTRAINT [FK_Instructions_Recipes_RecipeId] FOREIGN KEY ([RecipeId]) REFERENCES [Recipes] ([RecipeId]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260128144431_InitialCreate'
)
BEGIN
    CREATE TABLE [RecipeDietaryRestrictions] (
        [DietaryRestrictionsId] int NOT NULL,
        [RecipeId] int NOT NULL,
        CONSTRAINT [PK_RecipeDietaryRestrictions] PRIMARY KEY ([DietaryRestrictionsId], [RecipeId]),
        CONSTRAINT [FK_RecipeDietaryRestrictions_DietaryRestrictions_DietaryRestrictionsId] FOREIGN KEY ([DietaryRestrictionsId]) REFERENCES [DietaryRestrictions] ([DietaryRestrictionsId]) ON DELETE CASCADE,
        CONSTRAINT [FK_RecipeDietaryRestrictions_Recipes_RecipeId] FOREIGN KEY ([RecipeId]) REFERENCES [Recipes] ([RecipeId]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260128144431_InitialCreate'
)
BEGIN
    CREATE TABLE [RecipeIngredients] (
        [IngredientId] int NOT NULL,
        [RecipeId] int NOT NULL,
        CONSTRAINT [PK_RecipeIngredients] PRIMARY KEY ([IngredientId], [RecipeId]),
        CONSTRAINT [FK_RecipeIngredients_Ingredients_IngredientId] FOREIGN KEY ([IngredientId]) REFERENCES [Ingredients] ([IngredientId]) ON DELETE CASCADE,
        CONSTRAINT [FK_RecipeIngredients_Recipes_RecipeId] FOREIGN KEY ([RecipeId]) REFERENCES [Recipes] ([RecipeId]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260128144431_InitialCreate'
)
BEGIN
    CREATE TABLE [UserRecipes] (
        [RecipeId] int NOT NULL,
        [UserId] int NOT NULL,
        CONSTRAINT [PK_UserRecipes] PRIMARY KEY ([RecipeId], [UserId]),
        CONSTRAINT [FK_UserRecipes_Recipes_RecipeId] FOREIGN KEY ([RecipeId]) REFERENCES [Recipes] ([RecipeId]) ON DELETE CASCADE,
        CONSTRAINT [FK_UserRecipes_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([UserId]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260128144431_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Instructions_RecipeId] ON [Instructions] ([RecipeId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260128144431_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_RecipeDietaryRestrictions_RecipeId] ON [RecipeDietaryRestrictions] ([RecipeId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260128144431_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_RecipeIngredients_RecipeId] ON [RecipeIngredients] ([RecipeId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260128144431_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_UserRecipes_UserId] ON [UserRecipes] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260128144431_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260128144431_InitialCreate', N'8.0.23');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260129135722_AddUserRecipesCompositeKey'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260129135722_AddUserRecipesCompositeKey', N'8.0.23');
END;
GO

COMMIT;
GO

