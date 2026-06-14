-- ============================================================
-- Phase 3C: UserRoles Module
-- Schema: auth
-- Run AFTER Phase3A_Roles.sql and Phase3B_Users.sql
-- ============================================================

-- ============================================================
-- TABLE: auth.UserRoles
-- ============================================================
CREATE TABLE auth.UserRoles
(
    UserRoleId  UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    UserId      UNIQUEIDENTIFIER NOT NULL,
    RoleId      UNIQUEIDENTIFIER NOT NULL,
    AssignedOn  DATETIME2        NOT NULL DEFAULT GETUTCDATE(),
    AssignedBy  UNIQUEIDENTIFIER NULL,
    IsActive    BIT              NOT NULL DEFAULT 1,
    DeleteFlag  BIT              NOT NULL DEFAULT 0,

    CONSTRAINT PK_UserRoles
        PRIMARY KEY (UserRoleId),

    -- Prevent the same role being assigned twice to same user
    CONSTRAINT UQ_UserRoles_UserRole
        UNIQUE (UserId, RoleId),

    CONSTRAINT FK_UserRoles_Users
        FOREIGN KEY (UserId)
        REFERENCES auth.Users(UserId),

    CONSTRAINT FK_UserRoles_Roles
        FOREIGN KEY (RoleId)
        REFERENCES auth.Roles(RoleId)
);
GO

-- ============================================================
-- SP: auth.sp_GetRolesForUser
-- All active role assignments for a given user
-- JOINs auth.Roles to return RoleName
-- ============================================================
CREATE OR ALTER PROCEDURE auth.sp_GetRolesForUser
    @UserId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        ur.UserRoleId,
        ur.UserId,
        ur.RoleId,
        r.Name  AS RoleName,
        ur.AssignedOn
    FROM auth.UserRoles ur
    INNER JOIN auth.Roles r
        ON r.RoleId     = ur.RoleId
        AND r.DeleteFlag = 0
    WHERE ur.UserId     = @UserId
      AND ur.DeleteFlag  = 0
    ORDER BY r.Name ASC;
END;
GO

-- ============================================================
-- SP: auth.sp_GetUsersInRole
-- All active users assigned to a given role
-- JOINs auth.Roles to return RoleName
-- ============================================================
CREATE OR ALTER PROCEDURE auth.sp_GetUsersInRole
    @RoleId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        ur.UserRoleId,
        ur.UserId,
        ur.RoleId,
        r.Name  AS RoleName,
        ur.AssignedOn
    FROM auth.UserRoles ur
    INNER JOIN auth.Roles r
        ON r.RoleId     = ur.RoleId
        AND r.DeleteFlag = 0
    WHERE ur.RoleId     = @RoleId
      AND ur.DeleteFlag  = 0
    ORDER BY ur.AssignedOn ASC;
END;
GO

-- ============================================================
-- SP: auth.sp_AssignRoleToUser
-- Inserts a new UserRole assignment.
-- The UNIQUE constraint (UserId, RoleId) prevents duplicates.
-- ============================================================
CREATE OR ALTER PROCEDURE auth.sp_AssignRoleToUser
    @UserId UNIQUEIDENTIFIER,
    @RoleId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO auth.UserRoles
        (UserId, RoleId)
    VALUES
        (@UserId, @RoleId);
END;
GO

-- ============================================================
-- SP: auth.sp_RemoveRoleFromUser
-- Soft deletes a UserRole by UserRoleId
-- ============================================================
CREATE OR ALTER PROCEDURE auth.sp_RemoveRoleFromUser
    @UserRoleId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE auth.UserRoles
    SET
        DeleteFlag = 1,
        IsActive   = 0
    WHERE UserRoleId = @UserRoleId;
END;
GO
