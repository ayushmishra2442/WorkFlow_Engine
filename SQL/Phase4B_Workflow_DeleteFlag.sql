USE [Workflow_Management_DB]
GO

-- ============================================================
-- Phase 4B: Workflow Soft Delete Standardisation
-- Adds DeleteFlag to workflows table and updates stored procedures
-- ============================================================

-- 1. Alter Table to add DeleteFlag column
IF NOT EXISTS (
    SELECT 1 
    FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'[workflow].[Workflows]') 
      AND name = N'DeleteFlag'
)
BEGIN
    ALTER TABLE [workflow].[Workflows] 
    ADD [DeleteFlag] BIT NOT NULL DEFAULT 0;
END
GO

-- 2. Update sp_DeleteWorkflow to set DeleteFlag = 1 and IsActive = 0
CREATE OR ALTER PROCEDURE [workflow].[sp_DeleteWorkflow]
(
    @WorkflowId UNIQUEIDENTIFIER
)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE [workflow].[Workflows]
    SET [DeleteFlag] = 1,
        [IsActive] = 0
    WHERE [WorkflowId] = @WorkflowId;
END;
GO

-- 3. Update sp_GetWorkflows to filter by DeleteFlag = 0 and include DeleteFlag in select list
CREATE OR ALTER PROCEDURE [workflow].[sp_GetWorkflows]
(
    @PageNumber INT = 1,
    @PageSize   INT = 10
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        COUNT(*) OVER()  AS TotalCount,
        WorkflowId,
        OrganizationId,
        Name,
        Description,
        IsActive,
        CreatedOn,
        DeleteFlag
    FROM [workflow].[Workflows]
    WHERE [DeleteFlag] = 0
    ORDER BY CreatedOn DESC
    OFFSET (@PageNumber - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END;
GO

-- 4. Update sp_GetWorkflowById to filter by DeleteFlag = 0 and include DeleteFlag in select list
CREATE OR ALTER PROCEDURE [workflow].[sp_GetWorkflowById]
(
    @WorkflowId UNIQUEIDENTIFIER
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        [WorkflowId],
        [OrganizationId],
        [Name],
        [Description],
        [IsActive],
        [CreatedOn],
        [DeleteFlag]
    FROM [workflow].[Workflows]
    WHERE [WorkflowId] = @WorkflowId
      AND [DeleteFlag] = 0;
END;
GO

-- 5. Update sp_UpdateWorkflow to prevent updates to soft-deleted records
CREATE OR ALTER PROCEDURE [workflow].[sp_UpdateWorkflow]
(
    @WorkflowId  UNIQUEIDENTIFIER,
    @Name        NVARCHAR(200),
    @Description NVARCHAR(500),
    @IsActive    BIT
)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE [workflow].[Workflows]
    SET [Name]        = @Name,
        [Description] = @Description,
        [IsActive]    = @IsActive
    WHERE [WorkflowId]  = @WorkflowId
      AND [DeleteFlag]   = 0;
END;
GO
