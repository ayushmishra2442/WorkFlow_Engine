-- ============================================================
-- Phase 3B: Users Module
-- Schema: auth
-- ============================================================

-- ============================================================
-- TABLE: auth.Users
-- ============================================================
CREATE TABLE auth.Users
(
    UserId          UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    OrganizationId  UNIQUEIDENTIFIER NOT NULL,
    DisplayName     NVARCHAR(200)    NOT NULL,
    Email           NVARCHAR(320)    NOT NULL,
    -- AzureObjectId: populated during Phase 6 SSO integration
    AzureObjectId   NVARCHAR(100)    NULL,
    IsActive        BIT              NOT NULL DEFAULT 1,
    CreatedOn       DATETIME2        NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy       UNIQUEIDENTIFIER NULL,
    ModifiedOn      DATETIME2        NULL,
    ModifiedBy      UNIQUEIDENTIFIER NULL,
    DeleteFlag      BIT              NOT NULL DEFAULT 0,

    CONSTRAINT PK_Users
        PRIMARY KEY (UserId),

    CONSTRAINT UQ_Users_Email
        UNIQUE (Email),

    CONSTRAINT FK_Users_Organizations
        FOREIGN KEY (OrganizationId)
        REFERENCES auth.Organizations(OrganizationId)
);
GO

-- ============================================================
-- SP: auth.sp_GetUsers
-- Paginated list of active users
-- ============================================================
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
        CreatedOn
    FROM auth.Users
    WHERE DeleteFlag = 0
    ORDER BY DisplayName ASC
    OFFSET (@PageNumber - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END;
GO

-- ============================================================
-- SP: auth.sp_GetUserById
-- Single user with full audit fields
-- ============================================================
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
        DeleteFlag
    FROM auth.Users
    WHERE UserId    = @UserId
      AND DeleteFlag = 0;
END;
GO

-- ============================================================
-- SP: auth.sp_GetUserByEmail
-- Lookup by email address (used by Phase 6 SSO)
-- ============================================================
CREATE OR ALTER PROCEDURE auth.sp_GetUserByEmail
    @Email NVARCHAR(320)
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
        CreatedOn
    FROM auth.Users
    WHERE Email     = @Email
      AND DeleteFlag = 0;
END;
GO

-- ============================================================
-- SP: auth.sp_CreateUser
-- ============================================================
CREATE OR ALTER PROCEDURE auth.sp_CreateUser
    @OrganizationId UNIQUEIDENTIFIER,
    @DisplayName    NVARCHAR(200),
    @Email          NVARCHAR(320)
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO auth.Users
        (OrganizationId, DisplayName, Email)
    VALUES
        (@OrganizationId, @DisplayName, @Email);
END;
GO

-- ============================================================
-- SP: auth.sp_UpdateUser
-- ============================================================
CREATE OR ALTER PROCEDURE auth.sp_UpdateUser
    @UserId      UNIQUEIDENTIFIER,
    @DisplayName NVARCHAR(200),
    @Email       NVARCHAR(320),
    @IsActive    BIT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE auth.Users
    SET
        DisplayName = @DisplayName,
        Email       = @Email,
        IsActive    = @IsActive,
        ModifiedOn  = GETUTCDATE()
    WHERE UserId    = @UserId
      AND DeleteFlag = 0;
END;
GO

-- ============================================================
-- SP: auth.sp_DeleteUser
-- Soft delete
-- ============================================================
CREATE OR ALTER PROCEDURE auth.sp_DeleteUser
    @UserId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE auth.Users
    SET
        DeleteFlag = 1,
        IsActive   = 0,
        ModifiedOn = GETUTCDATE()
    WHERE UserId = @UserId;
END;
GO
