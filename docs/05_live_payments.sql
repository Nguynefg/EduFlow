USE [EduFlowDB];
GO
SET XACT_ABORT ON;
BEGIN TRANSACTION;
IF OBJECT_ID(N'dbo.payments', N'U') IS NULL OR OBJECT_ID(N'dbo.payout_requests', N'U') IS NULL
    THROW 50001, N'Chon dung CSDL EduFlow dang dung truoc khi chay script.', 1;
IF COL_LENGTH('dbo.payments', 'is_test') IS NULL
    ALTER TABLE dbo.payments ADD is_test bit NOT NULL CONSTRAINT DF_payments_is_test DEFAULT (1) WITH VALUES;
IF COL_LENGTH('dbo.payout_requests', 'is_test') IS NULL
    ALTER TABLE dbo.payout_requests ADD is_test bit NOT NULL CONSTRAINT DF_payout_requests_is_test DEFAULT (1) WITH VALUES;
COMMIT;
-- Giu nguyen so du, don va yeu cau cu. Khong tu dong doi du lieu cu thanh tien that.
SELECT name AS DatabaseName FROM sys.databases WHERE database_id = DB_ID();
