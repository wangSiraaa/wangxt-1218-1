-- 迁移：调阅用途标签
-- 日期：2026-06-21
-- 说明：为 RetrievalLogs 表添加 PurposeTag 字段，用于按用途分类筛选调阅记录

-- =============================================
-- SQL Server 版本
-- =============================================
USE JudicialEvidence;
GO

-- 添加 PurposeTag 字段
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.RetrievalLogs') AND name = N'PurposeTag')
BEGIN
    ALTER TABLE dbo.RetrievalLogs
    ADD PurposeTag INT NOT NULL
    CONSTRAINT DF_RetrievalLogs_PurposeTag DEFAULT 0;
END
GO

-- 添加 PurposeTag 索引
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_RetrievalLogs_PurposeTag')
CREATE INDEX IX_RetrievalLogs_PurposeTag ON dbo.RetrievalLogs(PurposeTag);
GO

-- =============================================
-- SQLite 版本
-- =============================================
-- 执行以下语句到 SQLite 数据库（judicial.db）：
--
-- ALTER TABLE RetrievalLogs ADD COLUMN PurposeTag INTEGER NOT NULL DEFAULT 0;
-- CREATE INDEX IF NOT EXISTS IX_RetrievalLogs_PurposeTag ON RetrievalLogs(PurposeTag);
--
-- 说明：SQLite 的 ALTER TABLE ADD COLUMN 不支持 IF NOT EXISTS，
-- 如字段已存在会报错，可忽略错误或先检查 PRAGMA table_info(RetrievalLogs);
