USE [Workflow_Management_DB]
GO

-- ============================================================
-- Phase 5: Workflow Engine (Instances & Tasks)
-- Tables and Stored Procedures for Workflow Execution
-- ============================================================

-- 1. Table: workflow.WorkflowInstances
IF OBJECT_ID(N'[workflow].[WorkflowInstances]', N'U') IS NULL
BEGIN
    CREATE TABLE [workflow].[WorkflowInstances]
    (
        [WorkflowInstanceId] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
        [WorkflowId] UNIQUEIDENTIFIER NOT NULL,
        [InitiatedByUserId] UNIQUEIDENTIFIER NOT NULL,
        [Title] NVARCHAR(200) NOT NULL,
        [CurrentStepOrder] INT NOT NULL DEFAULT 1,
        [Status] NVARCHAR(50) NOT NULL DEFAULT 'Pending',
        [CreatedOn] DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
        [CompletedOn] DATETIME2(7) NULL,

        CONSTRAINT [PK_WorkflowInstances] PRIMARY KEY CLUSTERED ([WorkflowInstanceId] ASC),
        CONSTRAINT [FK_WorkflowInstances_Workflows] FOREIGN KEY ([WorkflowId]) REFERENCES [workflow].[Workflows] ([WorkflowId]),
        CONSTRAINT [FK_WorkflowInstances_Users] FOREIGN KEY ([InitiatedByUserId]) REFERENCES [auth].[Users] ([UserId])
    );
END
GO

-- Non-clustered FK indexes on WorkflowInstances
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_WorkflowInstances_WorkflowId' AND object_id = OBJECT_ID('[workflow].[WorkflowInstances]'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_WorkflowInstances_WorkflowId ON [workflow].[WorkflowInstances]([WorkflowId]);
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_WorkflowInstances_InitiatedByUserId' AND object_id = OBJECT_ID('[workflow].[WorkflowInstances]'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_WorkflowInstances_InitiatedByUserId ON [workflow].[WorkflowInstances]([InitiatedByUserId]);
END
GO


-- 2. Table: workflow.WorkflowTasks
IF OBJECT_ID(N'[workflow].[WorkflowTasks]', N'U') IS NULL
BEGIN
    CREATE TABLE [workflow].[WorkflowTasks]
    (
        [WorkflowTaskId] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
        [WorkflowInstanceId] UNIQUEIDENTIFIER NOT NULL,
        [WorkflowStepId] UNIQUEIDENTIFIER NOT NULL,
        [AssignedToRoleId] UNIQUEIDENTIFIER NOT NULL,
        [AssignedToUserId] UNIQUEIDENTIFIER NULL,
        [Status] NVARCHAR(50) NOT NULL DEFAULT 'Pending',
        [Comments] NVARCHAR(500) NULL,
        [AssignedOn] DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
        [ActionedOn] DATETIME2(7) NULL,
        [ActionedByUserId] UNIQUEIDENTIFIER NULL,

        CONSTRAINT [PK_WorkflowTasks] PRIMARY KEY CLUSTERED ([WorkflowTaskId] ASC),
        CONSTRAINT [FK_WorkflowTasks_WorkflowInstances] FOREIGN KEY ([WorkflowInstanceId]) REFERENCES [workflow].[WorkflowInstances] ([WorkflowInstanceId]),
        CONSTRAINT [FK_WorkflowTasks_WorkflowSteps] FOREIGN KEY ([WorkflowStepId]) REFERENCES [workflow].[WorkflowSteps] ([WorkflowStepId]),
        CONSTRAINT [FK_WorkflowTasks_Roles] FOREIGN KEY ([AssignedToRoleId]) REFERENCES [auth].[Roles] ([RoleId]),
        CONSTRAINT [FK_WorkflowTasks_AssignedUsers] FOREIGN KEY ([AssignedToUserId]) REFERENCES [auth].[Users] ([UserId]),
        CONSTRAINT [FK_WorkflowTasks_ActionedUsers] FOREIGN KEY ([ActionedByUserId]) REFERENCES [auth].[Users] ([UserId])
    );
END
GO

-- Non-clustered FK indexes on WorkflowTasks
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_WorkflowTasks_WorkflowInstanceId' AND object_id = OBJECT_ID('[workflow].[WorkflowTasks]'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_WorkflowTasks_WorkflowInstanceId ON [workflow].[WorkflowTasks]([WorkflowInstanceId]);
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_WorkflowTasks_AssignedToRoleId' AND object_id = OBJECT_ID('[workflow].[WorkflowTasks]'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_WorkflowTasks_AssignedToRoleId ON [workflow].[WorkflowTasks]([AssignedToRoleId]);
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_WorkflowTasks_AssignedToUserId' AND object_id = OBJECT_ID('[workflow].[WorkflowTasks]'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_WorkflowTasks_AssignedToUserId ON [workflow].[WorkflowTasks]([AssignedToUserId]);
END
GO


-- ------------------------------------------------------------
-- 3. Stored Procedures
-- ------------------------------------------------------------

-- A. sp_CreateWorkflowInstance
CREATE OR ALTER PROCEDURE [workflow].[sp_CreateWorkflowInstance]
(
    @WorkflowId UNIQUEIDENTIFIER,
    @InitiatedByUserId UNIQUEIDENTIFIER,
    @Title NVARCHAR(200)
)
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @WorkflowInstanceId UNIQUEIDENTIFIER = NEWID();

    BEGIN TRY
        BEGIN TRANSACTION;

        -- 1. Create WorkflowInstance
        INSERT INTO [workflow].[WorkflowInstances]
        (
            [WorkflowInstanceId],
            [WorkflowId],
            [InitiatedByUserId],
            [Title],
            [CurrentStepOrder],
            [Status],
            [CreatedOn]
        )
        VALUES
        (
            @WorkflowInstanceId,
            @WorkflowId,
            @InitiatedByUserId,
            @Title,
            1,
            'InProgress',
            GETUTCDATE()
        );

        -- 2. Find StepOrder = 1 details
        DECLARE @WorkflowStepId UNIQUEIDENTIFIER;
        DECLARE @AssignedToRoleId UNIQUEIDENTIFIER;

        SELECT TOP 1 
            @WorkflowStepId = [WorkflowStepId],
            @AssignedToRoleId = [RoleId]
        FROM [workflow].[WorkflowSteps]
        WHERE [WorkflowId] = @WorkflowId
          AND [StepOrder] = 1
          AND [DeleteFlag] = 0
          AND [IsActive] = 1;

        IF @WorkflowStepId IS NULL
        BEGIN
            THROW 50001, 'Cannot start workflow: No active steps are configured at Step 1.', 1;
        END;

        -- 3. Create the first WorkflowTask
        INSERT INTO [workflow].[WorkflowTasks]
        (
            [WorkflowTaskId],
            [WorkflowInstanceId],
            [WorkflowStepId],
            [AssignedToRoleId],
            [AssignedToUserId],
            [Status],
            [AssignedOn]
        )
        VALUES
        (
            NEWID(),
            @WorkflowInstanceId,
            @WorkflowStepId,
            @AssignedToRoleId,
            NULL,
            'Pending',
            GETUTCDATE()
        );

        COMMIT TRANSACTION;

        -- Return the created instance details
        SELECT 
            wi.[WorkflowInstanceId],
            wi.[WorkflowId],
            w.[Name] AS [WorkflowName],
            wi.[InitiatedByUserId],
            u.[DisplayName] AS [InitiatedByUserName],
            wi.[Title],
            wi.[CurrentStepOrder],
            wi.[Status],
            wi.[CreatedOn],
            wi.[CompletedOn]
        FROM [workflow].[WorkflowInstances] wi
        INNER JOIN [workflow].[Workflows] w ON wi.[WorkflowId] = w.[WorkflowId]
        INNER JOIN [auth].[Users] u ON wi.[InitiatedByUserId] = u.[UserId]
        WHERE wi.[WorkflowInstanceId] = @WorkflowInstanceId;

    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        THROW;
    END CATCH;
END;
GO

-- B. sp_GetWorkflowInstanceById
CREATE OR ALTER PROCEDURE [workflow].[sp_GetWorkflowInstanceById]
(
    @WorkflowInstanceId UNIQUEIDENTIFIER
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        wi.[WorkflowInstanceId],
        wi.[WorkflowId],
        w.[Name] AS [WorkflowName],
        wi.[InitiatedByUserId],
        u.[DisplayName] AS [InitiatedByUserName],
        wi.[Title],
        wi.[CurrentStepOrder],
        wi.[Status],
        wi.[CreatedOn],
        wi.[CompletedOn]
    FROM [workflow].[WorkflowInstances] wi
    INNER JOIN [workflow].[Workflows] w ON wi.[WorkflowId] = w.[WorkflowId]
    INNER JOIN [auth].[Users] u ON wi.[InitiatedByUserId] = u.[UserId]
    WHERE wi.[WorkflowInstanceId] = @WorkflowInstanceId;
END;
GO

-- C. sp_GetWorkflowInstances
CREATE OR ALTER PROCEDURE [workflow].[sp_GetWorkflowInstances]
(
    @Status NVARCHAR(50) = NULL,
    @WorkflowId UNIQUEIDENTIFIER = NULL,
    @InitiatedByUserId UNIQUEIDENTIFIER = NULL,
    @PageNumber INT = 1,
    @PageSize INT = 10
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        COUNT(*) OVER() AS TotalCount,
        wi.[WorkflowInstanceId],
        wi.[WorkflowId],
        w.[Name] AS [WorkflowName],
        wi.[InitiatedByUserId],
        u.[DisplayName] AS [InitiatedByUserName],
        wi.[Title],
        wi.[CurrentStepOrder],
        wi.[Status],
        wi.[CreatedOn],
        wi.[CompletedOn]
    FROM [workflow].[WorkflowInstances] wi
    INNER JOIN [workflow].[Workflows] w ON wi.[WorkflowId] = w.[WorkflowId]
    INNER JOIN [auth].[Users] u ON wi.[InitiatedByUserId] = u.[UserId]
    WHERE (@Status IS NULL OR wi.[Status] = @Status)
      AND (@WorkflowId IS NULL OR wi.[WorkflowId] = @WorkflowId)
      AND (@InitiatedByUserId IS NULL OR wi.[InitiatedByUserId] = @InitiatedByUserId)
    ORDER BY wi.[CreatedOn] DESC
    OFFSET (@PageNumber - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END;
GO

-- D. sp_GetMyTasks
CREATE OR ALTER PROCEDURE [workflow].[sp_GetMyTasks]
(
    @UserId UNIQUEIDENTIFIER
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        wt.[WorkflowTaskId],
        wt.[WorkflowInstanceId],
        wi.[Title] AS [WorkflowInstanceTitle],
        wi.[WorkflowId],
        w.[Name] AS [WorkflowName],
        wi.[InitiatedByUserId],
        iu.[DisplayName] AS [InitiatedByUserName],
        wt.[WorkflowStepId],
        ws.[StepName],
        ws.[StepOrder],
        wt.[AssignedToRoleId],
        r.[Name] AS [AssignedToRoleName],
        wt.[AssignedToUserId],
        wt.[Status],
        wt.[AssignedOn]
    FROM [workflow].[WorkflowTasks] wt
    INNER JOIN [workflow].[WorkflowInstances] wi ON wt.[WorkflowInstanceId] = wi.[WorkflowInstanceId]
    INNER JOIN [workflow].[Workflows] w ON wi.[WorkflowId] = w.[WorkflowId]
    INNER JOIN [workflow].[WorkflowSteps] ws ON wt.[WorkflowStepId] = ws.[WorkflowStepId]
    INNER JOIN [auth].[Roles] r ON wt.[AssignedToRoleId] = r.[RoleId]
    INNER JOIN [auth].[Users] iu ON wi.[InitiatedByUserId] = iu.[UserId]
    WHERE wt.[Status] = 'Pending'
      AND (
          wt.[AssignedToUserId] = @UserId 
          OR (
              wt.[AssignedToUserId] IS NULL 
              AND wt.[AssignedToRoleId] IN (
                  SELECT [RoleId] 
                  FROM [auth].[UserRoles] 
                  WHERE [UserId] = @UserId 
                    AND [DeleteFlag] = 0 
                    AND [IsActive] = 1
              )
          )
      )
    ORDER BY wt.[AssignedOn] ASC;
END;
GO

-- E. sp_GetTasksByInstance
CREATE OR ALTER PROCEDURE [workflow].[sp_GetTasksByInstance]
(
    @WorkflowInstanceId UNIQUEIDENTIFIER
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        wt.[WorkflowTaskId],
        wt.[WorkflowInstanceId],
        wt.[WorkflowStepId],
        ws.[StepName],
        ws.[StepOrder],
        wt.[AssignedToRoleId],
        r.[Name] AS [AssignedToRoleName],
        wt.[AssignedToUserId],
        au.[DisplayName] AS [AssignedToUserName],
        wt.[Status],
        wt.[Comments],
        wt.[AssignedOn],
        wt.[ActionedOn],
        wt.[ActionedByUserId],
        actu.[DisplayName] AS [ActionedByUserName]
    FROM [workflow].[WorkflowTasks] wt
    INNER JOIN [workflow].[WorkflowSteps] ws ON wt.[WorkflowStepId] = ws.[WorkflowStepId]
    INNER JOIN [auth].[Roles] r ON wt.[AssignedToRoleId] = r.[RoleId]
    LEFT JOIN [auth].[Users] au ON wt.[AssignedToUserId] = au.[UserId]
    LEFT JOIN [auth].[Users] actu ON wt.[ActionedByUserId] = actu.[UserId]
    WHERE wt.[WorkflowInstanceId] = @WorkflowInstanceId
    ORDER BY ws.[StepOrder] ASC, wt.[AssignedOn] ASC;
END;
GO

-- F. sp_ActionTask
CREATE OR ALTER PROCEDURE [workflow].[sp_ActionTask]
(
    @WorkflowTaskId UNIQUEIDENTIFIER,
    @ActionedByUserId UNIQUEIDENTIFIER,
    @Status NVARCHAR(50), -- 'Approved' or 'Rejected'
    @Comments NVARCHAR(500) = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    IF @Status NOT IN ('Approved', 'Rejected')
    BEGIN
        THROW 50002, 'Invalid task action status. Must be "Approved" or "Rejected".', 1;
    END;

    BEGIN TRY
        BEGIN TRANSACTION;

        -- 1. Check if the task is still Pending
        DECLARE @CurrentStatus NVARCHAR(50);
        DECLARE @WorkflowInstanceId UNIQUEIDENTIFIER;
        DECLARE @WorkflowStepId UNIQUEIDENTIFIER;
        DECLARE @StepOrder INT;
        DECLARE @WorkflowId UNIQUEIDENTIFIER;

        SELECT 
            @CurrentStatus = wt.[Status],
            @WorkflowInstanceId = wt.[WorkflowInstanceId],
            @WorkflowStepId = wt.[WorkflowStepId],
            @StepOrder = ws.[StepOrder],
            @WorkflowId = wi.[WorkflowId]
        FROM [workflow].[WorkflowTasks] wt
        INNER JOIN [workflow].[WorkflowInstances] wi ON wt.[WorkflowInstanceId] = wi.[WorkflowInstanceId]
        INNER JOIN [workflow].[WorkflowSteps] ws ON wt.[WorkflowStepId] = ws.[WorkflowStepId]
        WHERE wt.[WorkflowTaskId] = @WorkflowTaskId;

        IF @CurrentStatus IS NULL
        BEGIN
            THROW 50003, 'Task not found.', 1;
        END;

        IF @CurrentStatus <> 'Pending'
        BEGIN
            THROW 50004, 'Task has already been actioned.', 1;
        END;

        -- 2. Update the task
        UPDATE [workflow].[WorkflowTasks]
        SET [Status] = @Status,
            [Comments] = @Comments,
            [ActionedOn] = GETUTCDATE(),
            [ActionedByUserId] = @ActionedByUserId
        WHERE [WorkflowTaskId] = @WorkflowTaskId;

        -- 3. Check action outcome
        IF @Status = 'Rejected'
        BEGIN
            -- If Rejected, fail the entire instance and cancel any other pending tasks
            UPDATE [workflow].[WorkflowInstances]
            SET [Status] = 'Rejected',
                [CompletedOn] = GETUTCDATE()
            WHERE [WorkflowInstanceId] = @WorkflowInstanceId;

            -- Cancel any other pending tasks for this instance
            UPDATE [workflow].[WorkflowTasks]
            SET [Status] = 'Cancelled',
                [Comments] = 'Cancelled due to workflow rejection.'
            WHERE [WorkflowInstanceId] = @WorkflowInstanceId
              AND [Status] = 'Pending';
        END;
        ELSE IF @Status = 'Approved'
        BEGIN
            -- Find next step
            DECLARE @NextWorkflowStepId UNIQUEIDENTIFIER;
            DECLARE @NextRoleId UNIQUEIDENTIFIER;
            DECLARE @NextStepOrder INT = @StepOrder + 1;

            SELECT TOP 1 
                @NextWorkflowStepId = [WorkflowStepId],
                @NextRoleId = [RoleId]
            FROM [workflow].[WorkflowSteps]
            WHERE [WorkflowId] = @WorkflowId
              AND [StepOrder] = @NextStepOrder
              AND [DeleteFlag] = 0
              AND [IsActive] = 1;

            IF @NextWorkflowStepId IS NOT NULL
            BEGIN
                -- Next step exists: update instance step order, and create next task
                UPDATE [workflow].[WorkflowInstances]
                SET [CurrentStepOrder] = @NextStepOrder
                WHERE [WorkflowInstanceId] = @WorkflowInstanceId;

                INSERT INTO [workflow].[WorkflowTasks]
                (
                    [WorkflowTaskId],
                    [WorkflowInstanceId],
                    [WorkflowStepId],
                    [AssignedToRoleId],
                    [AssignedToUserId],
                    [Status],
                    [AssignedOn]
                )
                VALUES
                (
                    NEWID(),
                    @WorkflowInstanceId,
                    @NextWorkflowStepId,
                    @NextRoleId,
                    NULL,
                    'Pending',
                    GETUTCDATE()
                );
            END;
            ELSE
            BEGIN
                -- No next step: workflow instance is fully Approved/Completed!
                UPDATE [workflow].[WorkflowInstances]
                SET [Status] = 'Approved',
                    [CompletedOn] = GETUTCDATE()
                WHERE [WorkflowInstanceId] = @WorkflowInstanceId;
            END;
        END;

        COMMIT TRANSACTION;

        -- Return the actioned task's final state
        SELECT 
            wt.[WorkflowTaskId],
            wt.[WorkflowInstanceId],
            wt.[Status] AS [TaskStatus],
            wi.[Status] AS [InstanceStatus]
        FROM [workflow].[WorkflowTasks] wt
        INNER JOIN [workflow].[WorkflowInstances] wi ON wt.[WorkflowInstanceId] = wi.[WorkflowInstanceId]
        WHERE wt.[WorkflowTaskId] = @WorkflowTaskId;

    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        THROW;
    END CATCH;
END;
GO

-- G. sp_CancelWorkflowInstance
CREATE OR ALTER PROCEDURE [workflow].[sp_CancelWorkflowInstance]
(
    @WorkflowInstanceId UNIQUEIDENTIFIER,
    @ActionedByUserId UNIQUEIDENTIFIER
)
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        DECLARE @CurrentStatus NVARCHAR(50);
        SELECT @CurrentStatus = [Status] 
        FROM [workflow].[WorkflowInstances] 
        WHERE [WorkflowInstanceId] = @WorkflowInstanceId;

        IF @CurrentStatus IS NULL
        BEGIN
            THROW 50005, 'Workflow instance not found.', 1;
        END;

        IF @CurrentStatus IN ('Approved', 'Rejected', 'Cancelled')
        BEGIN
            DECLARE @ErrMsg NVARCHAR(200) = 'Cannot cancel workflow instance. Current status is ' + @CurrentStatus + '.';
            THROW 50006, @ErrMsg, 1;
        END;

        -- Cancel the instance
        UPDATE [workflow].[WorkflowInstances]
        SET [Status] = 'Cancelled',
            [CompletedOn] = GETUTCDATE()
        WHERE [WorkflowInstanceId] = @WorkflowInstanceId;

        -- Cancel any pending tasks
        UPDATE [workflow].[WorkflowTasks]
        SET [Status] = 'Cancelled',
            [Comments] = 'Cancelled by initiator/admin.'
        WHERE [WorkflowInstanceId] = @WorkflowInstanceId
          AND [Status] = 'Pending';

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        THROW;
    END CATCH;
END;
GO

PRINT 'Phase 5 Workflow Engine database migration script compiled successfully.';
GO
