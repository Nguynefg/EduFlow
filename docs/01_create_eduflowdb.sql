
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO
IF DB_ID(N'EduFlowDB') IS NULL
    CREATE DATABASE EduFlowDB COLLATE Vietnamese_CI_AS;
GO
USE EduFlowDB;
GO
IF OBJECT_ID(N'users') IS NOT NULL
BEGIN
    RAISERROR(N'EduFlowDB đã có bảng. Script dừng để không ghi đè dữ liệu.', 16, 1);
    SET NOEXEC ON;
END
GO

/* ===================== A. NGƯỜI DÙNG & PHÂN QUYỀN ===================== */
CREATE TABLE users (
    id                 BIGINT IDENTITY(1,1) CONSTRAINT PK_users PRIMARY KEY,
    email              NVARCHAR(255) NOT NULL CONSTRAINT UQ_users_email UNIQUE,
    password_hash      NVARCHAR(255) NULL,
    full_name          NVARCHAR(150) NOT NULL,
    avatar_url         NVARCHAR(500) NULL,
    phone_number       NVARCHAR(20)  NULL,
    bio                NVARCHAR(MAX) NULL,
    social_links       NVARCHAR(1000) NULL,
    is_active          BIT NOT NULL CONSTRAINT DF_users_is_active DEFAULT 1,
    email_verified_at  DATETIME2 NULL,
    last_login_at      DATETIME2 NULL,
    created_at         DATETIME2 NOT NULL CONSTRAINT DF_users_created_at DEFAULT GETDATE(),
    updated_at         DATETIME2 NULL,
    deleted_at         DATETIME2 NULL
);

CREATE TABLE roles (
    id            INT IDENTITY(1,1) CONSTRAINT PK_roles PRIMARY KEY,
    code          NVARCHAR(50)  NOT NULL CONSTRAINT UQ_roles_code UNIQUE,
    display_name  NVARCHAR(100) NOT NULL
);

CREATE TABLE user_roles (
    user_id      BIGINT NOT NULL CONSTRAINT FK_user_roles_users REFERENCES users(id) ON DELETE CASCADE,
    role_id      INT    NOT NULL CONSTRAINT FK_user_roles_roles REFERENCES roles(id) ON DELETE CASCADE,
    assigned_at  DATETIME2 NOT NULL CONSTRAINT DF_user_roles_assigned_at DEFAULT GETDATE(),
    CONSTRAINT PK_user_roles PRIMARY KEY (user_id, role_id)
);

CREATE TABLE instructor_profiles (
    user_id              BIGINT NOT NULL CONSTRAINT PK_instructor_profiles PRIMARY KEY
                         CONSTRAINT FK_instructor_profiles_users REFERENCES users(id) ON DELETE CASCADE,
    headline             NVARCHAR(255) NULL,
    identity_card        NVARCHAR(50)  NULL,
    tax_id               NVARCHAR(50)  NULL,
    kyc_status           NVARCHAR(20)  NOT NULL CONSTRAINT DF_instructor_profiles_kyc_status DEFAULT N'pending'
                         CONSTRAINT CK_instructor_profiles_kyc_status CHECK (kyc_status IN (N'pending', N'approved', N'rejected')),
    kyc_reject_reason    NVARCHAR(500) NULL,
    commission_rate      DECIMAL(5,2)  NOT NULL CONSTRAINT DF_instructor_profiles_commission DEFAULT 70,
    bank_name            NVARCHAR(100) NULL,
    bank_account_number  NVARCHAR(50)  NULL,
    bank_account_name    NVARCHAR(100) NULL,
    balance              DECIMAL(15,2) NOT NULL CONSTRAINT DF_instructor_profiles_balance DEFAULT 0,
    submitted_at         DATETIME2 NULL
);

/* ===================== B. KHÓA HỌC & NỘI DUNG ===================== */
CREATE TABLE categories (
    id         INT IDENTITY(1,1) CONSTRAINT PK_categories PRIMARY KEY,
    parent_id  INT NULL CONSTRAINT FK_categories_parent REFERENCES categories(id),
    name       NVARCHAR(255) NOT NULL,
    slug       NVARCHAR(255) NOT NULL CONSTRAINT UQ_categories_slug UNIQUE,
    is_active  BIT NOT NULL CONSTRAINT DF_categories_is_active DEFAULT 1
);

CREATE TABLE courses (
    id              BIGINT IDENTITY(1,1) CONSTRAINT PK_courses PRIMARY KEY,
    instructor_id   BIGINT NOT NULL CONSTRAINT FK_courses_users REFERENCES users(id),
    category_id     INT    NOT NULL CONSTRAINT FK_courses_categories REFERENCES categories(id),
    title           NVARCHAR(255) NOT NULL,
    slug            NVARCHAR(255) NOT NULL CONSTRAINT UQ_courses_slug UNIQUE,
    short_desc      NVARCHAR(500) NULL,
    description     NVARCHAR(MAX) NULL,
    objectives      NVARCHAR(MAX) NULL,
    requirements    NVARCHAR(MAX) NULL,
    thumbnail_url   NVARCHAR(500) NULL,
    trailer_url     NVARCHAR(500) NULL,
    level           NVARCHAR(20)  NOT NULL CONSTRAINT DF_courses_level DEFAULT N'beginner'
                    CONSTRAINT CK_courses_level CHECK (level IN (N'beginner', N'intermediate', N'advanced')),
    price           DECIMAL(15,2) NOT NULL CONSTRAINT DF_courses_price DEFAULT 0,
    discount_price  DECIMAL(15,2) NULL,
    discount_start  DATETIME2 NULL,
    discount_end    DATETIME2 NULL,
    status          NVARCHAR(20)  NOT NULL CONSTRAINT DF_courses_status DEFAULT N'draft'
                    CONSTRAINT CK_courses_status CHECK (status IN (N'draft', N'pending', N'published', N'rejected', N'hidden')),
    reject_reason   NVARCHAR(500) NULL,
    avg_rating      DECIMAL(3,2)  NOT NULL CONSTRAINT DF_courses_avg_rating DEFAULT 0,
    published_at    DATETIME2 NULL,
    created_at      DATETIME2 NOT NULL CONSTRAINT DF_courses_created_at DEFAULT GETDATE(),
    updated_at      DATETIME2 NULL,
    deleted_at      DATETIME2 NULL
);

CREATE TABLE sections (
    id           BIGINT IDENTITY(1,1) CONSTRAINT PK_sections PRIMARY KEY,
    course_id    BIGINT NOT NULL CONSTRAINT FK_sections_courses REFERENCES courses(id) ON DELETE CASCADE,
    title        NVARCHAR(255) NOT NULL,
    order_index  INT NOT NULL
);

CREATE TABLE lessons (
    id               BIGINT IDENTITY(1,1) CONSTRAINT PK_lessons PRIMARY KEY,
    section_id       BIGINT NOT NULL CONSTRAINT FK_lessons_sections REFERENCES sections(id) ON DELETE CASCADE,
    title            NVARCHAR(255) NOT NULL,
    lesson_type      NVARCHAR(20)  NOT NULL CONSTRAINT CK_lessons_type CHECK (lesson_type IN (N'video', N'article', N'quiz')),
    video_url        NVARCHAR(500) NULL,
    duration_sec     INT NOT NULL CONSTRAINT DF_lessons_duration DEFAULT 0,
    content          NVARCHAR(MAX) NULL,
    pass_score       INT NULL CONSTRAINT CK_lessons_pass_score CHECK (pass_score BETWEEN 0 AND 100),
    is_free_preview  BIT NOT NULL CONSTRAINT DF_lessons_free_preview DEFAULT 0,
    order_index      INT NOT NULL
);

CREATE TABLE lesson_resources (
    id         BIGINT IDENTITY(1,1) CONSTRAINT PK_lesson_resources PRIMARY KEY,
    lesson_id  BIGINT NOT NULL CONSTRAINT FK_lesson_resources_lessons REFERENCES lessons(id) ON DELETE CASCADE,
    file_name  NVARCHAR(255) NOT NULL,
    file_url   NVARCHAR(500) NOT NULL,
    file_size  BIGINT NOT NULL
);

CREATE TABLE quiz_questions (
    id             BIGINT IDENTITY(1,1) CONSTRAINT PK_quiz_questions PRIMARY KEY,
    lesson_id      BIGINT NOT NULL CONSTRAINT FK_quiz_questions_lessons REFERENCES lessons(id) ON DELETE CASCADE,
    question_text  NVARCHAR(MAX) NOT NULL,
    question_type  NVARCHAR(20)  NOT NULL CONSTRAINT DF_quiz_questions_type DEFAULT N'single'
                   CONSTRAINT CK_quiz_questions_type CHECK (question_type IN (N'single', N'fill')),
    order_index    INT NOT NULL
);

CREATE TABLE quiz_options (
    id           BIGINT IDENTITY(1,1) CONSTRAINT PK_quiz_options PRIMARY KEY,
    question_id  BIGINT NOT NULL CONSTRAINT FK_quiz_options_questions REFERENCES quiz_questions(id) ON DELETE CASCADE,
    option_text  NVARCHAR(500) NOT NULL,
    is_correct   BIT NOT NULL CONSTRAINT DF_quiz_options_is_correct DEFAULT 0
);

/* ===================== C. GIỎ HÀNG, THANH TOÁN & TÀI CHÍNH ===================== */
CREATE TABLE cart_items (
    user_id    BIGINT NOT NULL CONSTRAINT FK_cart_items_users REFERENCES users(id),
    course_id  BIGINT NOT NULL CONSTRAINT FK_cart_items_courses REFERENCES courses(id),
    added_at   DATETIME2 NOT NULL CONSTRAINT DF_cart_items_added_at DEFAULT GETDATE(),
    CONSTRAINT PK_cart_items PRIMARY KEY (user_id, course_id)
);

CREATE TABLE coupons (
    id              INT IDENTITY(1,1) CONSTRAINT PK_coupons PRIMARY KEY,
    code            NVARCHAR(50) NOT NULL CONSTRAINT UQ_coupons_code UNIQUE,
    discount_type   NVARCHAR(20) NOT NULL CONSTRAINT CK_coupons_type CHECK (discount_type IN (N'percent', N'fixed')),
    discount_value  DECIMAL(15,2) NOT NULL,
    max_discount    DECIMAL(15,2) NULL,
    total_limit     INT NULL,
    used_count      INT NOT NULL CONSTRAINT DF_coupons_used_count DEFAULT 0,
    start_date      DATETIME2 NOT NULL,
    end_date        DATETIME2 NOT NULL,
    is_active       BIT NOT NULL CONSTRAINT DF_coupons_is_active DEFAULT 1,
    CONSTRAINT CK_coupons_dates CHECK (end_date > start_date)
);

CREATE TABLE orders (
    id               BIGINT IDENTITY(1,1) CONSTRAINT PK_orders PRIMARY KEY,
    user_id          BIGINT NOT NULL CONSTRAINT FK_orders_users REFERENCES users(id),
    coupon_id        INT NULL CONSTRAINT FK_orders_coupons REFERENCES coupons(id),
    subtotal         DECIMAL(15,2) NOT NULL,
    discount_amount  DECIMAL(15,2) NOT NULL CONSTRAINT DF_orders_discount DEFAULT 0,
    total_price      DECIMAL(15,2) NOT NULL,
    status           NVARCHAR(20) NOT NULL CONSTRAINT DF_orders_status DEFAULT N'pending'
                     CONSTRAINT CK_orders_status CHECK (status IN (N'pending', N'paid', N'failed', N'cancelled')),
    created_at       DATETIME2 NOT NULL CONSTRAINT DF_orders_created_at DEFAULT GETDATE()
);

CREATE TABLE order_items (
    id                  BIGINT IDENTITY(1,1) CONSTRAINT PK_order_items PRIMARY KEY,
    order_id            BIGINT NOT NULL CONSTRAINT FK_order_items_orders REFERENCES orders(id) ON DELETE CASCADE,
    course_id           BIGINT NOT NULL CONSTRAINT FK_order_items_courses REFERENCES courses(id),
    price               DECIMAL(15,2) NOT NULL,
    instructor_earning  DECIMAL(15,2) NOT NULL
);

CREATE TABLE payments (
    id              BIGINT IDENTITY(1,1) CONSTRAINT PK_payments PRIMARY KEY,
    order_id        BIGINT NOT NULL CONSTRAINT FK_payments_orders REFERENCES orders(id),
    gateway         NVARCHAR(50) NOT NULL,
    gateway_txn_id  NVARCHAR(255) NULL,
    amount          DECIMAL(15,2) NOT NULL,
    status          NVARCHAR(20) NOT NULL CONSTRAINT DF_payments_status DEFAULT N'pending'
                    CONSTRAINT CK_payments_status CHECK (status IN (N'pending', N'success', N'failed')),
    created_at      DATETIME2 NOT NULL CONSTRAINT DF_payments_created_at DEFAULT GETDATE()
);
-- UNIQUE nhưng cho phép nhiều dòng NULL
CREATE UNIQUE INDEX UQ_payments_gateway_txn_id ON payments(gateway_txn_id) WHERE gateway_txn_id IS NOT NULL;

CREATE TABLE payout_requests (
    id             BIGINT IDENTITY(1,1) CONSTRAINT PK_payout_requests PRIMARY KEY,
    instructor_id  BIGINT NOT NULL CONSTRAINT FK_payout_requests_users REFERENCES users(id),
    amount         DECIMAL(15,2) NOT NULL CONSTRAINT CK_payout_requests_amount CHECK (amount > 0),
    status         NVARCHAR(20) NOT NULL CONSTRAINT DF_payout_requests_status DEFAULT N'pending'
                   CONSTRAINT CK_payout_requests_status CHECK (status IN (N'pending', N'approved', N'rejected', N'paid')),
    admin_note     NVARCHAR(500) NULL,
    receipt_url    NVARCHAR(500) NULL,
    created_at     DATETIME2 NOT NULL CONSTRAINT DF_payout_requests_created_at DEFAULT GETDATE(),
    processed_at   DATETIME2 NULL
);

/* ===================== D. HỌC TẬP & CỘNG ĐỒNG ===================== */
CREATE TABLE enrollments (
    id                BIGINT IDENTITY(1,1) CONSTRAINT PK_enrollments PRIMARY KEY,
    user_id           BIGINT NOT NULL CONSTRAINT FK_enrollments_users REFERENCES users(id),
    course_id         BIGINT NOT NULL CONSTRAINT FK_enrollments_courses REFERENCES courses(id),
    progress_percent  DECIMAL(5,2) NOT NULL CONSTRAINT DF_enrollments_progress DEFAULT 0,
    status            NVARCHAR(20) NOT NULL CONSTRAINT DF_enrollments_status DEFAULT N'active'
                      CONSTRAINT CK_enrollments_status CHECK (status IN (N'active', N'revoked')),
    enrolled_at       DATETIME2 NOT NULL CONSTRAINT DF_enrollments_enrolled_at DEFAULT GETDATE(),
    completed_at      DATETIME2 NULL,
    CONSTRAINT UQ_enrollments_user_course UNIQUE (user_id, course_id)
);

CREATE TABLE lesson_progress (
    enrollment_id      BIGINT NOT NULL CONSTRAINT FK_lesson_progress_enrollments REFERENCES enrollments(id),
    lesson_id          BIGINT NOT NULL CONSTRAINT FK_lesson_progress_lessons REFERENCES lessons(id),
    last_position_sec  INT NOT NULL CONSTRAINT DF_lesson_progress_position DEFAULT 0,
    watched_sec        INT NOT NULL CONSTRAINT DF_lesson_progress_watched DEFAULT 0,
    quiz_score         INT NULL,
    is_completed       BIT NOT NULL CONSTRAINT DF_lesson_progress_completed DEFAULT 0,
    updated_at         DATETIME2 NULL,
    CONSTRAINT PK_lesson_progress PRIMARY KEY (enrollment_id, lesson_id)
);

CREATE TABLE course_reviews (
    course_id         BIGINT NOT NULL CONSTRAINT FK_course_reviews_courses REFERENCES courses(id),
    user_id           BIGINT NOT NULL CONSTRAINT FK_course_reviews_users REFERENCES users(id),
    rating            TINYINT NOT NULL CONSTRAINT CK_course_reviews_rating CHECK (rating BETWEEN 1 AND 5),
    comment           NVARCHAR(MAX) NULL,
    instructor_reply  NVARCHAR(MAX) NULL,
    is_hidden         BIT NOT NULL CONSTRAINT DF_course_reviews_hidden DEFAULT 0,
    created_at        DATETIME2 NOT NULL CONSTRAINT DF_course_reviews_created_at DEFAULT GETDATE(),
    updated_at        DATETIME2 NULL,
    CONSTRAINT PK_course_reviews PRIMARY KEY (course_id, user_id)
);

CREATE TABLE discussions (
    id                   BIGINT IDENTITY(1,1) CONSTRAINT PK_discussions PRIMARY KEY,
    lesson_id            BIGINT NOT NULL CONSTRAINT FK_discussions_lessons REFERENCES lessons(id),
    user_id              BIGINT NOT NULL CONSTRAINT FK_discussions_users REFERENCES users(id),
    parent_id            BIGINT NULL CONSTRAINT FK_discussions_parent REFERENCES discussions(id),
    title                NVARCHAR(255) NULL,
    content              NVARCHAR(MAX) NOT NULL,
    video_timestamp_sec  INT NULL,
    upvote_count         INT NOT NULL CONSTRAINT DF_discussions_upvote DEFAULT 0,
    is_pinned            BIT NOT NULL CONSTRAINT DF_discussions_pinned DEFAULT 0,
    is_resolved          BIT NOT NULL CONSTRAINT DF_discussions_resolved DEFAULT 0,
    is_hidden            BIT NOT NULL CONSTRAINT DF_discussions_hidden DEFAULT 0,
    created_at           DATETIME2 NOT NULL CONSTRAINT DF_discussions_created_at DEFAULT GETDATE()
);
GO

/* Dữ liệu bắt buộc: 3 vai trò của hệ thống (không phải dữ liệu mẫu) */
INSERT INTO roles (code, display_name) VALUES
    (N'admin',      N'Quản trị viên'),
    (N'instructor', N'Giảng viên'),
    (N'student',    N'Học viên');
GO
SET NOEXEC OFF;
GO
