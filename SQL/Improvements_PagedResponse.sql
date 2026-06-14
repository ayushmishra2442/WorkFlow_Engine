USE [Workflow_Management_DB]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- ============================================================
-- SQL Updates for PagedResponse Support
-- Adds COUNT(*) OVER() AS TotalCount to paginated SPs
-- ============================================================

-- ============================================================
-- Update: workflow.sp_GetWorkflows
-- Description: Retrieve paginated workflows with total count
-- Table columns: WorkflowId, OrganizationId, Name, Description, IsActive, CreatedOn, DeleteFlag
-- ============================================================
CREATE OR ALTER PROCEDURE [workflow].[sp_GetWorkflows]
(
    @PageNumber INT = 1,
    @PageSize   INT = 10
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        COUNT(*) OVER()  AS TotalCount, -- Added for pagination support
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

-- ============================================================
-- Update: auth.sp_GetUsers
-- Description: Retrieve paginated users with total count
-- Table columns: UserId, OrganizationId, DisplayName, Email, AzureObjectId,
--                IsActive, CreatedOn, CreatedBy, ModifiedOn, ModifiedBy, DeleteFlag
-- ============================================================
CREATE OR ALTER PROCEDURE [auth].[sp_GetUsers]
(
    @PageNumber INT = 1,
    @PageSize   INT = 10
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        COUNT(*) OVER()  AS TotalCount, -- Added for pagination support
        UserId,
        OrganizationId,
        DisplayName,
        Email,
        IsActive,
        CreatedOn
    FROM [auth].[Users]
    WHERE DeleteFlag = 0
    ORDER BY DisplayName ASC
    OFFSET (@PageNumber - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END;
GO
