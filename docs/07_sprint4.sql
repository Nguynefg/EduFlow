USE [EduFlowDB];
GO
SET XACT_ABORT ON;
BEGIN TRANSACTION;
IF OBJECT_ID(N'dbo.courses',N'U') IS NULL OR OBJECT_ID(N'dbo.lessons',N'U') IS NULL
 THROW 50001, N'Chon dung EduFlowDB da co cac bang Sprint 1-3.',1;
IF OBJECT_ID(N'dbo.course_reviews',N'U') IS NULL
BEGIN
CREATE TABLE dbo.course_reviews (
    course_id         BIGINT NOT NULL CONSTRAINT FK_course_reviews_courses REFERENCES dbo.courses(id),
    user_id           BIGINT NOT NULL CONSTRAINT FK_course_reviews_users REFERENCES dbo.users(id),
    rating            TINYINT NOT NULL CONSTRAINT CK_course_reviews_rating CHECK (rating BETWEEN 1 AND 5),
    comment           NVARCHAR(MAX) NULL,
    instructor_reply  NVARCHAR(MAX) NULL,
    is_hidden         BIT NOT NULL CONSTRAINT DF_course_reviews_hidden DEFAULT 0,
    created_at        DATETIME2 NOT NULL CONSTRAINT DF_course_reviews_created_at DEFAULT GETDATE(),
    updated_at        DATETIME2 NULL,
    CONSTRAINT PK_course_reviews PRIMARY KEY (course_id, user_id)
);
END;
IF OBJECT_ID(N'dbo.discussions',N'U') IS NULL
BEGIN
CREATE TABLE dbo.discussions (
    id                   BIGINT IDENTITY(1,1) CONSTRAINT PK_discussions PRIMARY KEY,
    lesson_id            BIGINT NOT NULL CONSTRAINT FK_discussions_lessons REFERENCES dbo.lessons(id),
    user_id              BIGINT NOT NULL CONSTRAINT FK_discussions_users REFERENCES dbo.users(id),
    parent_id            BIGINT NULL CONSTRAINT FK_discussions_parent REFERENCES dbo.discussions(id),
    title                NVARCHAR(255) NULL,
    content              NVARCHAR(MAX) NOT NULL,
    video_timestamp_sec  INT NULL,
    upvote_count         INT NOT NULL CONSTRAINT DF_discussions_upvote DEFAULT 0,
    is_pinned            BIT NOT NULL CONSTRAINT DF_discussions_pinned DEFAULT 0,
    is_resolved          BIT NOT NULL CONSTRAINT DF_discussions_resolved DEFAULT 0,
    is_hidden            BIT NOT NULL CONSTRAINT DF_discussions_hidden DEFAULT 0,
    created_at           DATETIME2 NOT NULL CONSTRAINT DF_discussions_created_at DEFAULT GETDATE()
);
END;
IF OBJECT_ID(N'dbo.discussion_votes',N'U') IS NULL
BEGIN
 CREATE TABLE dbo.discussion_votes (
 discussion_id bigint NOT NULL,
 user_id bigint NOT NULL,
 CONSTRAINT PK_discussion_votes PRIMARY KEY(discussion_id,user_id),
 CONSTRAINT FK_discussion_votes_discussion FOREIGN KEY(discussion_id) REFERENCES dbo.discussions(id),
 CONSTRAINT FK_discussion_votes_user FOREIGN KEY(user_id) REFERENCES dbo.users(id));
END;
IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID('dbo.discussions') AND name='IX_discussions_lesson_parent')
 CREATE INDEX IX_discussions_lesson_parent ON dbo.discussions(lesson_id,parent_id,is_hidden,created_at);
COMMIT;
SELECT DB_NAME() AS DatabaseName;
-- Khong xoa du lieu cu. discussion_votes luu tung binh chon de tranh tang ao khi bam lap.

