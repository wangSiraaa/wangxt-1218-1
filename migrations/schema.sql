-- 司法电子证据管理系统 - SQL Server 生产库表结构
-- 数据库：JudicialEvidence
-- 说明：元数据、权限、调阅日志使用 SQL Server 存储；证据文件存本地对象目录并记录 SHA-256。

IF DB_ID(N'JudicialEvidence') IS NULL
BEGIN
    CREATE DATABASE JudicialEvidence;
END
GO
USE JudicialEvidence;
GO

IF OBJECT_ID(N'dbo.Users', N'U') IS NULL
CREATE TABLE dbo.Users (
    Id            BIGINT         IDENTITY(1,1) PRIMARY KEY,
    Username      NVARCHAR(64)   NOT NULL CONSTRAINT UQ_Users_Username UNIQUE,
    PasswordHash  NVARCHAR(256)  NOT NULL,
    FullName      NVARCHAR(64)   NOT NULL,
    Role          NVARCHAR(32)   NOT NULL CONSTRAINT DF_Users_Role DEFAULT N'Police',
    CreatedAt     DATETIME2      NOT NULL CONSTRAINT DF_Users_CreatedAt DEFAULT SYSUTCDATETIME()
);
GO

IF OBJECT_ID(N'dbo.Cases', N'U') IS NULL
CREATE TABLE dbo.Cases (
    Id          BIGINT        IDENTITY(1,1) PRIMARY KEY,
    CaseNumber  NVARCHAR(64)  NOT NULL CONSTRAINT UQ_Cases_CaseNumber UNIQUE,
    Title       NVARCHAR(128) NOT NULL,
    Stage       NVARCHAR(32)  NOT NULL CONSTRAINT DF_Cases_Stage DEFAULT N'Police',
    CreatedBy   BIGINT        NOT NULL CONSTRAINT FK_Cases_Users REFERENCES dbo.Users(Id),
    CreatedAt   DATETIME2     NOT NULL CONSTRAINT DF_Cases_CreatedAt DEFAULT SYSUTCDATETIME()
);
GO

IF OBJECT_ID(N'dbo.Evidence', N'U') IS NULL
CREATE TABLE dbo.Evidence (
    Id           BIGINT        IDENTITY(1,1) PRIMARY KEY,
    CaseId       BIGINT        NOT NULL CONSTRAINT FK_Evidence_Cases REFERENCES dbo.Cases(Id) ON DELETE CASCADE,
    Name         NVARCHAR(256) NOT NULL,
    FilePath     NVARCHAR(512) NOT NULL,
    Sha256       NVARCHAR(64)  NOT NULL,
    UploadedHash NVARCHAR(64)  NOT NULL,
    Status       NVARCHAR(32)  NOT NULL CONSTRAINT DF_Evidence_Status DEFAULT N'Pending',
    IsAdopted    BIT           NOT NULL CONSTRAINT DF_Evidence_IsAdopted DEFAULT 0,
    UploadedBy   BIGINT        NOT NULL CONSTRAINT FK_Evidence_Users REFERENCES dbo.Users(Id),
    UploadedAt   DATETIME2     NOT NULL CONSTRAINT DF_Evidence_UploadedAt DEFAULT SYSUTCDATETIME()
);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Evidence_CaseId')
CREATE INDEX IX_Evidence_CaseId ON dbo.Evidence(CaseId);
GO

IF OBJECT_ID(N'dbo.EvidenceAdoptions', N'U') IS NULL
CREATE TABLE dbo.EvidenceAdoptions (
    Id         BIGINT        IDENTITY(1,1) PRIMARY KEY,
    EvidenceId BIGINT        NOT NULL CONSTRAINT FK_Adoptions_Evidence REFERENCES dbo.Evidence(Id) ON DELETE CASCADE,
    ReviewerId BIGINT        NOT NULL CONSTRAINT FK_Adoptions_Users REFERENCES dbo.Users(Id),
    Opinion    NVARCHAR(512) NOT NULL,
    Adopted    BIT           NOT NULL,
    CreatedAt  DATETIME2     NOT NULL CONSTRAINT DF_Adoptions_CreatedAt DEFAULT SYSUTCDATETIME()
);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Adoptions_EvidenceId')
CREATE INDEX IX_Adoptions_EvidenceId ON dbo.EvidenceAdoptions(EvidenceId);
GO

IF OBJECT_ID(N'dbo.RetrievalLogs', N'U') IS NULL
CREATE TABLE dbo.RetrievalLogs (
    Id          BIGINT        IDENTITY(1,1) PRIMARY KEY,
    EvidenceId  BIGINT        NOT NULL CONSTRAINT FK_RetrievalLogs_Evidence REFERENCES dbo.Evidence(Id) ON DELETE CASCADE,
    CaseId      BIGINT        NOT NULL CONSTRAINT FK_RetrievalLogs_Cases REFERENCES dbo.Cases(Id),
    UserId      BIGINT        NOT NULL CONSTRAINT FK_RetrievalLogs_Users REFERENCES dbo.Users(Id),
    PurposeTag  INT           NOT NULL CONSTRAINT DF_RetrievalLogs_PurposeTag DEFAULT 0,
    Purpose     NVARCHAR(256) NOT NULL,
    CopyPath    NVARCHAR(512) NOT NULL,
    RetrievedAt DATETIME2     NOT NULL CONSTRAINT DF_RetrievalLogs_RetrievedAt DEFAULT SYSUTCDATETIME()
);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_RetrievalLogs_CaseId')
CREATE INDEX IX_RetrievalLogs_CaseId ON dbo.RetrievalLogs(CaseId);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_RetrievalLogs_UserId')
CREATE INDEX IX_RetrievalLogs_UserId ON dbo.RetrievalLogs(UserId);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_RetrievalLogs_PurposeTag')
CREATE INDEX IX_RetrievalLogs_PurposeTag ON dbo.RetrievalLogs(PurposeTag);
GO

-- 初始管理员账号（密码请在首次登录后由系统通过用户管理接口重置/创建）
IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Username = N'admin')
INSERT INTO dbo.Users (Username, PasswordHash, FullName, Role)
VALUES (N'admin', N'CHANGE_ME_WITH_HASHED_VALUE', N'系统管理员', N'Admin');
GO
