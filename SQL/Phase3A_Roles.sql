-- ============================================================
-- Phase 3A: Roles Module
-- Schema: auth
-- ============================================================

-- ============================================================
-- TABLE: auth.Roles
-- ============================================================
CREATE TABLE auth.Roles
(
    RoleId      UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    Name        NVARCHAR(100)    NOT NULL,
    Description NVARCHAR(500)    NULL,
    IsActive    BIT              NOT NULL DEFAULT 1,
    CreatedOn   DATETIME2        NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy   UNIQUEIDENTIFIER NULL,
    ModifiedOn  DATETIME2        NULL,
    ModifiedBy  UNIQUEIDENTIFIER NULL,
    DeleteFlag  BIT              NOT NULL DEFAULT 0,

    CONSTRAINT PK_Roles PRIMARY KEY (RoleId)
);
GO

-- ============================================================
-- SP: auth.sp_GetRoles
-- Returns all active (non-deleted) roles
-- ============================================================
CREATE OR ALTER PROCEDURE auth.sp_GetRoles
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        RoleId,
        Name,
        Description,
        IsActive,
        CreatedOn
    FROM auth.Roles
    WHERE DeleteFlag = 0
    ORDER BY Name ASC;
END;
GO

-- ============================================================
-- SP: auth.sp_GetRoleById
-- Returns a single role (including audit fields)
-- ============================================================
CREATE OR ALTER PROCEDURE auth.sp_GetRoleById
    @RoleId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        RoleId,
        Name,
        Description,
        IsActive,
        CreatedOn,
        CreatedBy,
        ModifiedOn,
        ModifiedBy,
        DeleteFlag
    FROM auth.Roles
    WHERE RoleId    = @RoleId
      AND DeleteFlag = 0;
END;
GO

-- ============================================================
-- SP: auth.sp_CreateRole
-- ============================================================
CREATE OR ALTER PROCEDURE auth.sp_CreateRole
    @Name        NVARCHAR(100),
    @Description NVARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO auth.Roles
        (Name, Description)
    VALUES
        (@Name, @Description);
END;
GO

-- ============================================================
-- SP: auth.sp_UpdateRole
-- ============================================================
CREATE OR ALTER PROCEDURE auth.sp_UpdateRole
    @RoleId      UNIQUEIDENTIFIER,
    @Name        NVARCHAR(100),
    @Description NVARCHAR(500) = NULL,
    @IsActive    BIT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE auth.Roles
    SET
        Name        = @Name,
        Description = @Description,
        IsActive    = @IsActive,
        ModifiedOn  = GETUTCDATE()
    WHERE RoleId    = @RoleId
      AND DeleteFlag = 0;
END;
GO

-- ============================================================
-- SP: auth.sp_DeleteRole
-- Soft delete
-- ============================================================
CREATE OR ALTER PROCEDURE auth.sp_DeleteRole
    @RoleId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE auth.Roles
    SET
        DeleteFlag = 1,
        IsActive   = 0,
        ModifiedOn = GETUTCDATE()
    WHERE RoleId = @RoleId;
END;
GO
