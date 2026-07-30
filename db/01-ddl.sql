-- =============================================
-- DDL for Ticket Manager (SQL Server)
-- =============================================

CREATE TABLE [User] (
    Id          INT IDENTITY(1,1) PRIMARY KEY,
    Email       NVARCHAR(256) NOT NULL UNIQUE,
    DisplayName NVARCHAR(100) NOT NULL
);

CREATE TABLE Ticket (
    Id          INT IDENTITY(1,1) PRIMARY KEY,
    Title       NVARCHAR(120) NOT NULL,
    Description NVARCHAR(2000) NOT NULL,
    Priority    NVARCHAR(20) NOT NULL CHECK (Priority IN ('Low','Medium','High','Critical')),
    Status      NVARCHAR(20) NOT NULL CHECK (Status IN ('Open','InProgress','Resolved','Closed')),
    CreatedAt   DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt   DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedBy   INT NOT NULL,
    CONSTRAINT FK_Ticket_User FOREIGN KEY (CreatedBy) REFERENCES [User](Id)
);

CREATE TABLE Comment (
    Id          INT IDENTITY(1,1) PRIMARY KEY,
    TicketId    INT NOT NULL,
    Text        NVARCHAR(1000) NOT NULL,
    CreatedAt   DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedBy   INT NOT NULL,
    CONSTRAINT FK_Comment_Ticket FOREIGN KEY (TicketId) REFERENCES Ticket(Id) ON DELETE CASCADE,
    CONSTRAINT FK_Comment_User   FOREIGN KEY (CreatedBy) REFERENCES [User](Id)
);
