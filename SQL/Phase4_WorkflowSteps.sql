-- ============================================================
-- Phase 4: WorkflowSteps Module
-- Schema: workflow
-- Run AFTER Phase3A_Roles.sql
-- ============================================================

-- ============================================================
-- TABLE: workflow.WorkflowSteps
-- ============================================================
CREATE TABLE workflow.WorkflowSteps
(
    WorkflowStepId  UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    WorkflowId      UNIQUEIDENTIFIER NOT NULL,
    RoleId          UNIQUEIDENTIFIER NOT NULL,
    StepName        NVARCHAR(200)    NOT NULL,
    StepOrder       INT              NOT NULL,
    Description     NVARCHAR(500)    NULL,
    IsActive        BIT              NOT NULL DEFAULT 1,
    CreatedOn       DATETIME2        NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy       UNIQUEIDENTIFIER NULL,
    ModifiedOn      DATETIME2        NULL,
    ModifiedBy      UNIQUEIDENTIFIER NULL,
    DeleteFlag      BIT              NOT NULL DEFAULT 0,

    CONSTRAINT PK_WorkflowSteps
        PRIMARY KEY (WorkflowStepId),

    -- Enforce ordering uniqueness at DB level
    CONSTRAINT UQ_WorkflowSteps_Order
        UNIQUE (WorkflowId, StepOrder),

    CONSTRAINT FK_WorkflowSteps_Workflows
        FOREIGN KEY (WorkflowId)
        REFERENCES workflow.Workflows(WorkflowId),

    CONSTRAINT FK_WorkflowSteps_Roles
        FOREIGN KEY (RoleId)
        REFERENCES auth.Roles(RoleId)
);
GO

-- ============================================================
-- SP: workflow.sp_GetWorkflowSteps
-- All steps for a workflow, ordered by StepOrder
-- JOINs auth.Roles to return RoleName
-- ============================================================
CREATE OR ALTER PROCEDURE workflow.sp_GetWorkflowSteps
    @WorkflowId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        ws.WorkflowStepId,
        ws.WorkflowId,
        ws.RoleId,
        r.Name      AS RoleName,
        ws.StepName,
        ws.StepOrder,
        ws.Description,
        ws.IsActive,
        ws.CreatedOn
    FROM workflow.WorkflowSteps ws
    INNER JOIN auth.Roles r
        ON r.RoleId      = ws.RoleId
        AND r.DeleteFlag  = 0
    WHERE ws.WorkflowId  = @WorkflowId
      AND ws.DeleteFlag   = 0
    ORDER BY ws.StepOrder ASC;
END;
GO

-- ============================================================
-- SP: workflow.sp_GetWorkflowStepById
-- Single step with full audit fields
-- ============================================================
CREATE OR ALTER PROCEDURE workflow.sp_GetWorkflowStepById
    @WorkflowStepId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        ws.WorkflowStepId,
        ws.WorkflowId,
        ws.RoleId,
        r.Name      AS RoleName,
        ws.StepName,
        ws.StepOrder,
        ws.Description,
        ws.IsActive,
        ws.CreatedOn,
        ws.CreatedBy,
        ws.ModifiedOn,
        ws.ModifiedBy,
        ws.DeleteFlag
    FROM workflow.WorkflowSteps ws
    INNER JOIN auth.Roles r
        ON r.RoleId          = ws.RoleId
        AND r.DeleteFlag      = 0
    WHERE ws.WorkflowStepId  = @WorkflowStepId
      AND ws.DeleteFlag       = 0;
END;
GO

-- ============================================================
-- SP: workflow.sp_CreateWorkflowStep
-- The UNIQUE constraint (WorkflowId, StepOrder) will raise
-- an error if a duplicate StepOrder is attempted.
-- ============================================================
CREATE OR ALTER PROCEDURE workflow.sp_CreateWorkflowStep
    @WorkflowId  UNIQUEIDENTIFIER,
    @RoleId      UNIQUEIDENTIFIER,
    @StepName    NVARCHAR(200),
    @StepOrder   INT,
    @Description NVARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO workflow.WorkflowSteps
        (WorkflowId, RoleId, StepName, StepOrder, Description)
    VALUES
        (@WorkflowId, @RoleId, @StepName, @StepOrder, @Description);
END;
GO

-- ============================================================
-- SP: workflow.sp_UpdateWorkflowStep
-- Updates step details including StepOrder and RoleId.
-- The UNIQUE constraint will error if StepOrder conflicts.
-- ============================================================
CREATE OR ALTER PROCEDURE workflow.sp_UpdateWorkflowStep
    @WorkflowStepId UNIQUEIDENTIFIER,
    @RoleId         UNIQUEIDENTIFIER,
    @StepName       NVARCHAR(200),
    @StepOrder      INT,
    @Description    NVARCHAR(500) = NULL,
    @IsActive       BIT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE workflow.WorkflowSteps
    SET
        RoleId      = @RoleId,
        StepName    = @StepName,
        StepOrder   = @StepOrder,
        Description = @Description,
        IsActive    = @IsActive,
        ModifiedOn  = GETUTCDATE()
    WHERE WorkflowStepId = @WorkflowStepId
      AND DeleteFlag      = 0;
END;
GO

-- ============================================================
-- SP: workflow.sp_DeleteWorkflowStep
-- Soft delete
-- ============================================================
CREATE OR ALTER PROCEDURE workflow.sp_DeleteWorkflowStep
    @WorkflowStepId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE workflow.WorkflowSteps
    SET
        DeleteFlag = 1,
        IsActive   = 0,
        ModifiedOn = GETUTCDATE()
    WHERE WorkflowStepId = @WorkflowStepId;
END;
GO
