SET XACT_ABORT ON;
BEGIN TRANSACTION;
IF OBJECT_ID(N'dbo.video_watch_sessions',N'U') IS NULL
CREATE TABLE dbo.video_watch_sessions (
 id uniqueidentifier NOT NULL PRIMARY KEY,
 enrollment_id bigint NOT NULL REFERENCES dbo.enrollments(id),
 lesson_id bigint NOT NULL REFERENCES dbo.lessons(id),
 position_sec int NOT NULL,
 last_seen_utc datetime2 NOT NULL
);
IF OBJECT_ID(N'dbo.video_watch_segments',N'U') IS NULL
CREATE TABLE dbo.video_watch_segments (
 enrollment_id bigint NOT NULL REFERENCES dbo.enrollments(id),
 lesson_id bigint NOT NULL REFERENCES dbo.lessons(id),
 start_sec int NOT NULL,
 end_sec int NOT NULL,
 CONSTRAINT PK_video_watch_segments PRIMARY KEY(enrollment_id,lesson_id,start_sec),
 CONSTRAINT CK_video_watch_segments_range CHECK(start_sec>=0 AND end_sec>start_sec)
);
IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name=N'IX_orders_pending_coupon' AND object_id=OBJECT_ID(N'dbo.orders'))
CREATE INDEX IX_orders_pending_coupon ON dbo.orders(coupon_id,status,created_at);
IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name=N'IX_payments_order' AND object_id=OBJECT_ID(N'dbo.payments'))
CREATE INDEX IX_payments_order ON dbo.payments(order_id);
IF COL_LENGTH('dbo.payout_requests','bank_name_snapshot') IS NULL
ALTER TABLE dbo.payout_requests ADD bank_name_snapshot nvarchar(100) NULL, bank_number_snapshot nvarchar(50) NULL, bank_owner_snapshot nvarchar(100) NULL;
IF COL_LENGTH('dbo.payments','payment_url') IS NULL
ALTER TABLE dbo.payments ADD payment_url nvarchar(max) NULL;
COMMIT;
