USE [Workflow_Management_DB]
GO

-- ============================================================
-- Phase 5B: Manager Hierarchy & Dynamic Routing
-- Run AFTER Phase5_Workflow_Engine.sql
-- ============================================================

-- 1. Alter auth.Users: Add ManagerUserId
IF NOT EXISTS (
    SELECT * FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'[auth].[Users]') 
      AND name = N'ManagerUserId'
)
BEGIN
    ALTER TABLE [auth].[Users]
    ADD [ManagerUserId] UNIQUEIDENTIFIER NULL;

    ALTER TABLE [auth].[Users]
    ADD CONSTRAINT [FK_Users_Users_Manager] 
    FOREIGN KEY ([ManagerUserId]) REFERENCES [auth].[Users] ([UserId]);
END
GO

-- 2. Alter workflow.WorkflowSteps: Add RoutingType Column
IF NOT EXISTS (
    SELECT * FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'[workflow].[WorkflowSteps]') 
      AND name = N'RoutingType'
)
BEGIN
    ALTER TABLE [workflow].[WorkflowSteps]
    ADD [RoutingType] NVARCHAR(50) NOT NULL DEFAULT 'Role';
END
GO

-- 3. Alter workflow.WorkflowSteps: Add CHECK Constraint & Alter RoleId Nullability
-- Separated into a new batch so 'RoutingType' column exists in database catalog during compilation
IF EXISTS (
    SELECT * FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'[workflow].[WorkflowSteps]') 
      AND name = N'RoutingType'
)
BEGIN
    -- Drop constraint if it already exists to avoid errors on rerun
    IF EXISTS (SELECT * FROM sys.check_constraints WHERE name = 'CK_WorkflowSteps_RoutingType')
    BEGIN
        ALTER TABLE [workflow].[WorkflowSteps] DROP CONSTRAINT [CK_WorkflowSteps_RoutingType];
    END

    ALTER TABLE [workflow].[WorkflowSteps]
    ADD CONSTRAINT [CK_WorkflowSteps_RoutingType]
    CHECK ([RoutingType] IN ('Role', 'DirectManager'));

    ALTER TABLE [workflow].[WorkflowSteps]
    ALTER COLUMN [RoleId] UNIQUEIDENTIFIER NULL;
END
GO

-- 4. Alter workflow.WorkflowTasks: Make AssignedToRoleId Nullable
ALTER TABLE [workflow].[WorkflowTasks]
ALTER COLUMN [AssignedToRoleId] UNIQUEIDENTIFIER NULL;
GO

-- 5. Re-create user stored procedures with ManagerUserId support

CREATE OR ALTER PROCEDURE auth.sp_CreateUser
    @OrganizationId UNIQUEIDENTIFIER,
    @DisplayName    NVARCHAR(200),
    @Email          NVARCHAR(320),
    @ManagerUserId  UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO auth.Users
        (OrganizationId, DisplayName, Email, ManagerUserId)
    VALUES
        (@OrganizationId, @DisplayName, @Email, @ManagerUserId);
END;
GO

CREATE OR ALTER PROCEDURE auth.sp_UpdateUser
    @UserId         UNIQUEIDENTIFIER,
    @DisplayName    NVARCHAR(200),
    @Email          NVARCHAR(320),
    @IsActive       BIT,
    @ManagerUserId  UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE auth.Users
    SET
        DisplayName = @DisplayName,
        Email       = @Email,
        IsActive    = @IsActive,
        ManagerUserId = @ManagerUserId,
        ModifiedOn  = GETUTCDATE()
    WHERE UserId    = @UserId
      AND DeleteFlag = 0;
END;
GO

CREATE OR ALTER PROCEDURE auth.sp_GetUsers
    @PageNumber INT = 1,
    @PageSize   INT = 10
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        UserId,
        OrganizationId,
        DisplayName,
        Email,
        IsActive,
        CreatedOn,
        ManagerUserId,
        COUNT(*) OVER() AS TotalCount
    FROM auth.Users
    WHERE DeleteFlag = 0
    ORDER BY DisplayName ASC
    OFFSET (@PageNumber - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END;
GO

CREATE OR ALTER PROCEDURE auth.sp_GetUserById
    @UserId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        UserId,
        OrganizationId,
        DisplayName,
        Email,
        AzureObjectId,
        IsActive,
        CreatedOn,
        CreatedBy,
        ModifiedOn,
        ModifiedBy,
        DeleteFlag,
        ManagerUserId
    FROM auth.Users
    WHERE UserId    = @UserId
      AND DeleteFlag = 0;
END;
GO

-- 6. Re-create workflow step stored procedures with RoutingType support

CREATE OR ALTER PROCEDURE workflow.sp_CreateWorkflowStep
    @WorkflowId  UNIQUEIDENTIFIER,
    @RoleId      UNIQUEIDENTIFIER = NULL,
    @StepName    NVARCHAR(200),
    @StepOrder   INT,
    @Description NVARCHAR(500) = NULL,
    @RoutingType NVARCHAR(50) = 'Role'
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO workflow.WorkflowSteps
        (WorkflowId, RoleId, StepName, StepOrder, Description, RoutingType)
    VALUES
        (@WorkflowId, @RoleId, @StepName, @StepOrder, @Description, @RoutingType);
END;
GO

CREATE OR ALTER PROCEDURE workflow.sp_UpdateWorkflowStep
    @WorkflowStepId UNIQUEIDENTIFIER,
    @RoleId         UNIQUEIDENTIFIER = NULL,
    @StepName       NVARCHAR(200),
    @StepOrder      INT,
    @Description    NVARCHAR(500) = NULL,
    @IsActive       BIT,
    @RoutingType    NVARCHAR(50) = 'Role'
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
        RoutingType = @RoutingType,
        ModifiedOn  = GETUTCDATE()
    WHERE WorkflowStepId = @WorkflowStepId
      AND DeleteFlag      = 0;
END;
GO

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
        ws.CreatedOn,
        ws.RoutingType
    FROM workflow.WorkflowSteps ws
    LEFT JOIN auth.Roles r
        ON r.RoleId      = ws.RoleId
        AND r.DeleteFlag  = 0
    WHERE ws.WorkflowId  = @WorkflowId
      AND ws.DeleteFlag   = 0
    ORDER BY ws.StepOrder ASC;
END;
GO

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
        ws.DeleteFlag,
        ws.RoutingType
    FROM workflow.WorkflowSteps ws
    LEFT JOIN auth.Roles r
        ON r.RoleId          = ws.RoleId
        AND r.DeleteFlag      = 0
    WHERE ws.WorkflowStepId  = @WorkflowStepId
      AND ws.DeleteFlag       = 0;
END;
GO

-- 7. Refactor sp_CreateWorkflowInstance & sp_ActionTask for Manager routing

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
        DECLARE @RoutingType NVARCHAR(50);

        SELECT TOP 1 
            @WorkflowStepId = [WorkflowStepId],
            @AssignedToRoleId = [RoleId],
            @RoutingType = [RoutingType]
        FROM [workflow].[WorkflowSteps]
        WHERE [WorkflowId] = @WorkflowId
          AND [StepOrder] = 1
          AND [DeleteFlag] = 0
          AND [IsActive] = 1;

        IF @WorkflowStepId IS NULL
        BEGIN
            THROW 50001, 'Cannot start workflow: No active steps are configured at Step 1.', 1;
        END;

        -- Resolve manager assignment if necessary
        DECLARE @TargetUserId UNIQUEIDENTIFIER = NULL;
        DECLARE @TargetRoleId UNIQUEIDENTIFIER = @AssignedToRoleId;

        IF @RoutingType = 'DirectManager'
        BEGIN
            SELECT @TargetUserId = [ManagerUserId]
            FROM [auth].[Users]
            WHERE [UserId] = @InitiatedByUserId;

            -- If the initiator has no manager, assign it back to them (self-approval fallback)
            IF @TargetUserId IS NULL
            BEGIN
                SET @TargetUserId = @InitiatedByUserId;
            END;

            SET @TargetRoleId = NULL;
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
            @TargetRoleId,
            @TargetUserId,
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
                -- Next step exists: update instance step order
                UPDATE [workflow].[WorkflowInstances]
                SET [CurrentStepOrder] = @NextStepOrder
                WHERE [WorkflowInstanceId] = @WorkflowInstanceId;

                -- Resolve manager assignment for the next step
                DECLARE @NextRoutingType NVARCHAR(50);
                SELECT @NextRoutingType = RoutingType
                FROM [workflow].[WorkflowSteps]
                WHERE [WorkflowStepId] = @NextWorkflowStepId;

                DECLARE @NextTargetUserId UNIQUEIDENTIFIER = NULL;
                DECLARE @NextTargetRoleId UNIQUEIDENTIFIER = @NextRoleId;

                IF @NextRoutingType = 'DirectManager'
                BEGIN
                    DECLARE @InitiatorId UNIQUEIDENTIFIER;
                    SELECT @InitiatorId = InitiatedByUserId
                    FROM [workflow].[WorkflowInstances]
                    WHERE [WorkflowInstanceId] = @WorkflowInstanceId;

                    SELECT @NextTargetUserId = ManagerUserId
                    FROM auth.Users
                    WHERE UserId = @InitiatorId;

                    -- Fallback if manager is NULL (CEO case)
                    IF @NextTargetUserId IS NULL
                    BEGIN
                        SET @NextTargetUserId = @InitiatorId;
                    END;

                    SET @NextTargetRoleId = NULL;
                END;

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
                    @NextTargetRoleId,
                    @NextTargetUserId,
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

PRINT 'Phase 5B database modifications applied successfully.';
GO
