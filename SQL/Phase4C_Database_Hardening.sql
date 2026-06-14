USE [Workflow_Management_DB]
GO

-- ============================================================
-- Phase 4C: Database Hardening (Performance & Safety)
-- Standardises timezone formats, adds missing FK indexes, and 
-- adds transaction safety + cascade soft-deletes to procedures.
-- ============================================================

-- ------------------------------------------------------------
-- 1. Standardise workflow.Workflows.CreatedOn to UTC DATETIME2(7)
-- ------------------------------------------------------------
PRINT 'Updating workflow.Workflows.CreatedOn schema...';
GO

-- Drop existing default constraint on Workflows.CreatedOn dynamically
DECLARE @ConstraintName NVARCHAR(200);
SELECT @ConstraintName = dc.name
FROM sys.default_constraints dc
INNER JOIN sys.columns c ON dc.parent_object_id = c.object_id AND dc.parent_column_id = c.column_id
WHERE dc.parent_object_id = OBJECT_ID(N'[workflow].[Workflows]') 
  AND c.name = N'CreatedOn';

IF @ConstraintName IS NOT NULL
BEGIN
    EXEC('ALTER TABLE [workflow].[Workflows] DROP CONSTRAINT [' + @ConstraintName + ']');
END;
GO

-- Update existing NULL timestamps to UTC
UPDATE [workflow].[Workflows] 
SET [CreatedOn] = GETUTCDATE() 
WHERE [CreatedOn] IS NULL;
GO

-- Alter column to DATETIME2(7) NOT NULL
ALTER TABLE [workflow].[Workflows] 
ALTER COLUMN [CreatedOn] DATETIME2(7) NOT NULL;
GO

-- Bind new UTC default constraint
ALTER TABLE [workflow].[Workflows] 
ADD CONSTRAINT [DF_Workflows_CreatedOn] DEFAULT GETUTCDATE() FOR [CreatedOn];
GO


-- ------------------------------------------------------------
-- 2. Create Missing Indexes on Foreign Keys for JOIN optimization
-- ------------------------------------------------------------
PRINT 'Creating foreign key indexes...';
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Users_OrganizationId' AND object_id = OBJECT_ID('[auth].[Users]'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Users_OrganizationId ON [auth].[Users]([OrganizationId]);
END;
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_UserRoles_UserId' AND object_id = OBJECT_ID('[auth].[UserRoles]'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_UserRoles_UserId ON [auth].[UserRoles]([UserId]);
END;
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_UserRoles_RoleId' AND object_id = OBJECT_ID('[auth].[UserRoles]'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_UserRoles_RoleId ON [auth].[UserRoles]([RoleId]);
END;
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Workflows_OrganizationId' AND object_id = OBJECT_ID('[workflow].[Workflows]'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Workflows_OrganizationId ON [workflow].[Workflows]([OrganizationId]);
END;
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_WorkflowSteps_WorkflowId' AND object_id = OBJECT_ID('[workflow].[WorkflowSteps]'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_WorkflowSteps_WorkflowId ON [workflow].[WorkflowSteps]([WorkflowId]);
END;
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_WorkflowSteps_RoleId' AND object_id = OBJECT_ID('[workflow].[WorkflowSteps]'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_WorkflowSteps_RoleId ON [workflow].[WorkflowSteps]([RoleId]);
END;
GO


-- ------------------------------------------------------------
-- 3. Stored Procedure Refactoring (Transactions & Cascade Soft Deletes)
-- ------------------------------------------------------------
PRINT 'Refactoring stored procedures...';
GO

-- A. sp_DeleteWorkflow (Cascade soft-deletes related WorkflowSteps)
CREATE OR ALTER PROCEDURE [workflow].[sp_DeleteWorkflow]
(
    @WorkflowId UNIQUEIDENTIFIER
)
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;

        -- Soft delete the workflow
        UPDATE [workflow].[Workflows]
        SET [DeleteFlag] = 1,
            [IsActive] = 0
        WHERE [WorkflowId] = @WorkflowId;

        -- Cascade soft delete steps
        UPDATE [workflow].[WorkflowSteps]
        SET [DeleteFlag] = 1,
            [IsActive] = 0,
            [ModifiedOn] = GETUTCDATE()
        WHERE [WorkflowId] = @WorkflowId;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        THROW;
    END CATCH;
END;
GO

-- B. sp_DeleteRole (Cascade soft-deletes related UserRole assignments)
CREATE OR ALTER PROCEDURE [auth].[sp_DeleteRole]
    @RoleId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;

        -- Soft delete the role
        UPDATE [auth].[Roles]
        SET [DeleteFlag] = 1,
            [IsActive]   = 0,
            [ModifiedOn] = GETUTCDATE()
        WHERE [RoleId] = @RoleId;

        -- Cascade soft delete assignments for this role
        UPDATE [auth].[UserRoles]
        SET [DeleteFlag] = 1,
            [IsActive]   = 0
        WHERE [RoleId] = @RoleId;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        THROW;
    END CATCH;
END;
GO

-- C. sp_DeleteUser (Cascade soft-deletes UserRole assignments)
CREATE OR ALTER PROCEDURE [auth].[sp_DeleteUser]
    @UserId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;

        -- Soft delete the user
        UPDATE [auth].[Users]
        SET [DeleteFlag] = 1,
            [IsActive]   = 0,
            [ModifiedOn] = GETUTCDATE()
        WHERE [UserId] = @UserId;

        -- Cascade soft delete assignments of this user
        UPDATE [auth].[UserRoles]
        SET [DeleteFlag] = 1,
            [IsActive]   = 0
        WHERE [UserId] = @UserId;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        THROW;
    END CATCH;
END;
GO

-- D. sp_DeleteOrganization (Cascade soft-deletes users, user roles, workflows, and workflow steps)
CREATE OR ALTER PROCEDURE [auth].[sp_DeleteOrganization]
(
    @OrganizationId UNIQUEIDENTIFIER,
    @ModifiedBy UNIQUEIDENTIFIER = NULL
)
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;

        -- 1. Soft delete the organization
        UPDATE [auth].[Organizations]
        SET [DeleteFlag] = 1,
            [IsActive] = 0,
            [ModifiedOn] = GETUTCDATE(),
            [ModifiedBy] = @ModifiedBy
        WHERE [OrganizationId] = @OrganizationId;

        -- 2. Cascade soft delete user role assignments
        UPDATE ur
        SET ur.[DeleteFlag] = 1,
            ur.[IsActive] = 0
        FROM [auth].[UserRoles] ur
        INNER JOIN [auth].[Users] u ON ur.[UserId] = u.[UserId]
        WHERE u.[OrganizationId] = @OrganizationId;

        -- 3. Cascade soft delete users
        UPDATE [auth].[Users]
        SET [DeleteFlag] = 1,
            [IsActive] = 0,
            [ModifiedOn] = GETUTCDATE(),
            [ModifiedBy] = @ModifiedBy
        WHERE [OrganizationId] = @OrganizationId;

        -- 4. Cascade soft delete workflow steps
        UPDATE ws
        SET ws.[DeleteFlag] = 1,
            ws.[IsActive] = 0,
            ws.[ModifiedOn] = GETUTCDATE(),
            ws.[ModifiedBy] = @ModifiedBy
        FROM [workflow].[WorkflowSteps] ws
        INNER JOIN [workflow].[Workflows] w ON ws.[WorkflowId] = w.[WorkflowId]
        WHERE w.[OrganizationId] = @OrganizationId;

        -- 5. Cascade soft delete workflows
        UPDATE [workflow].[Workflows]
        SET [DeleteFlag] = 1,
            [IsActive] = 0
        WHERE [OrganizationId] = @OrganizationId;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        THROW;
    END CATCH;
END;
GO

PRINT 'Database hardening migration script compiled successfully.';
GO
