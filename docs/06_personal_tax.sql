USE [EduFlowDB];
GO
SET XACT_ABORT ON;
BEGIN TRANSACTION;
IF OBJECT_ID(N'dbo.payout_requests', N'U') IS NULL
    THROW 50001, N'Chon dung CSDL EduFlowDB, chay script Sprint 3 truoc.', 1;
IF COL_LENGTH('dbo.payout_requests', 'tax_rate') IS NULL
    ALTER TABLE dbo.payout_requests ADD tax_rate decimal(5,2) NULL;
IF COL_LENGTH('dbo.payout_requests', 'tax_amount') IS NULL
    ALTER TABLE dbo.payout_requests ADD tax_amount decimal(15,2) NULL;
IF COL_LENGTH('dbo.payout_requests', 'tax_rule') IS NULL
    ALTER TABLE dbo.payout_requests ADD tax_rule nvarchar(100) NULL;
IF COL_LENGTH('dbo.payout_requests', 'tax_id_snapshot') IS NULL
    ALTER TABLE dbo.payout_requests ADD tax_id_snapshot nvarchar(50) NULL;
IF COL_LENGTH('dbo.payout_requests', 'payee_name_snapshot') IS NULL
    ALTER TABLE dbo.payout_requests ADD payee_name_snapshot nvarchar(255) NULL;
IF COL_LENGTH('dbo.payout_requests', 'tax_declared') IS NULL
    ALTER TABLE dbo.payout_requests ADD tax_declared bit NOT NULL CONSTRAINT DF_payout_tax_declared DEFAULT(0) WITH VALUES;
IF COL_LENGTH('dbo.payout_requests', 'tax_voluntary') IS NULL
    ALTER TABLE dbo.payout_requests ADD tax_voluntary bit NOT NULL CONSTRAINT DF_payout_tax_voluntary DEFAULT(0) WITH VALUES;
IF COL_LENGTH('dbo.payout_requests', 'tax_reviewed_by') IS NULL
    ALTER TABLE dbo.payout_requests ADD tax_reviewed_by bigint NULL;
IF COL_LENGTH('dbo.payout_requests', 'tax_reviewed_at') IS NULL
    ALTER TABLE dbo.payout_requests ADD tax_reviewed_at datetime2 NULL;
IF COL_LENGTH('dbo.payout_requests', 'paid_at') IS NULL
    ALTER TABLE dbo.payout_requests ADD paid_at datetime2 NULL;
IF COL_LENGTH('dbo.payout_requests', 'tax_remitted_on') IS NULL
    ALTER TABLE dbo.payout_requests ADD tax_remitted_on datetime2 NULL;
IF COL_LENGTH('dbo.payout_requests', 'tax_remittance_ref') IS NULL
    ALTER TABLE dbo.payout_requests ADD tax_remittance_ref nvarchar(200) NULL;
IF COL_LENGTH('dbo.payout_requests', 'tax_remitted_by') IS NULL
    ALTER TABLE dbo.payout_requests ADD tax_remitted_by bigint NULL;
COMMIT;
-- Du lieu cu giu NULL: chua xac dinh thue; khong tu gan thue 0 hoac sua khoan da chi.
-- Chay 05_live_payments.sql truoc script nay. Khong tu danh dau du lieu cu la tien that.
SELECT DB_NAME() AS DatabaseName;

