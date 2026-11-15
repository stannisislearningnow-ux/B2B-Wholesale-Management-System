/*
    B2B Wholesale Management System
    Database: SQL Server
    Assignment: Software Engineering Lab Assignment 2

    Run this script in SQL Server Management Studio (SSMS).
    It creates the database and the five required tables, then inserts
    15-30 sample records into each table.

    IMPORTANT:
    - This script is intended for a NEW database. It does not drop existing data.
    - Users.PasswordHash stores sample placeholder hashes for demonstration.
      Use ASP.NET Core Identity (or another secure password hasher) in the real app.
    - AgentID in Users is an extra nullable FK so an Agent login can be linked
      to the corresponding Agent record. Admin accounts leave it NULL.
*/

IF DB_ID(N'B2BWholesaleDB') IS NULL
BEGIN
    CREATE DATABASE B2BWholesaleDB;
END;
GO

USE B2BWholesaleDB;
GO

/* 1. ITEM */
CREATE TABLE dbo.Item
(
    ItemID          INT IDENTITY(1,1) CONSTRAINT PK_Item PRIMARY KEY,
    ItemName        NVARCHAR(120) NOT NULL,
    Size            NVARCHAR(50) NOT NULL,
    Price           DECIMAL(12,2) NOT NULL,
    StockQuantity   INT NOT NULL,
    CONSTRAINT CK_Item_Price CHECK (Price >= 0),
    CONSTRAINT CK_Item_Stock CHECK (StockQuantity >= 0)
);
GO

/* 2. AGENT */
CREATE TABLE dbo.Agent
(
    AgentID     INT IDENTITY(1,1) CONSTRAINT PK_Agent PRIMARY KEY,
    AgentName   NVARCHAR(120) NOT NULL,
    Address     NVARCHAR(250) NOT NULL,
    Phone       VARCHAR(20) NOT NULL,
    CONSTRAINT UQ_Agent_Phone UNIQUE (Phone)
);
GO

/* 3. USERS
   AgentID is an optional extension to connect an Agent-role login to Agent.
   [Lock] is bracketed because LOCK can be a keyword in some SQL contexts.
*/
CREATE TABLE dbo.Users
(
    UserID       INT IDENTITY(1,1) CONSTRAINT PK_Users PRIMARY KEY,
    UserName     NVARCHAR(60) NOT NULL,
    Email        NVARCHAR(150) NOT NULL,
    PasswordHash NVARCHAR(255) NOT NULL,
    [Lock]       BIT NOT NULL CONSTRAINT DF_Users_Lock DEFAULT (0),
    Role         NVARCHAR(20) NOT NULL,
    AgentID      INT NULL,
    CONSTRAINT UQ_Users_UserName UNIQUE (UserName),
    CONSTRAINT UQ_Users_Email UNIQUE (Email),
    CONSTRAINT CK_Users_Role CHECK (Role IN (N'Admin', N'Agent')),
    CONSTRAINT FK_Users_Agent FOREIGN KEY (AgentID)
        REFERENCES dbo.Agent(AgentID)
        ON UPDATE CASCADE
        ON DELETE NO ACTION,
    CONSTRAINT CK_Users_AgentRole CHECK
        ((Role = N'Admin' AND AgentID IS NULL)
         OR (Role = N'Agent' AND AgentID IS NOT NULL))
);
GO

/* 4. ORDER */
CREATE TABLE dbo.[Order]
(
    OrderID     INT IDENTITY(1,1) CONSTRAINT PK_Order PRIMARY KEY,
    OrderDate   DATETIME2(0) NOT NULL CONSTRAINT DF_Order_OrderDate DEFAULT (SYSDATETIME()),
    AgentID     INT NOT NULL,
    OrderStatus NVARCHAR(20) NOT NULL CONSTRAINT DF_Order_Status DEFAULT (N'Pending'),
    CONSTRAINT CK_Order_Status CHECK
        (OrderStatus IN (N'Pending', N'Confirmed', N'Shipped', N'Cancelled')),
    CONSTRAINT FK_Order_Agent FOREIGN KEY (AgentID)
        REFERENCES dbo.Agent(AgentID)
        ON UPDATE CASCADE
        ON DELETE NO ACTION
);
GO

/* 5. ORDER DETAIL */
CREATE TABLE dbo.OrderDetail
(
    ID          INT IDENTITY(1,1) CONSTRAINT PK_OrderDetail PRIMARY KEY,
    OrderID     INT NOT NULL,
    ItemID      INT NOT NULL,
    Quantity    INT NOT NULL,
    UnitAmount  DECIMAL(12,2) NOT NULL,
    CONSTRAINT CK_OrderDetail_Quantity CHECK (Quantity > 0),
    CONSTRAINT CK_OrderDetail_UnitAmount CHECK (UnitAmount >= 0),
    CONSTRAINT FK_OrderDetail_Order FOREIGN KEY (OrderID)
        REFERENCES dbo.[Order](OrderID)
        ON UPDATE CASCADE
        ON DELETE CASCADE,
    CONSTRAINT FK_OrderDetail_Item FOREIGN KEY (ItemID)
        REFERENCES dbo.Item(ItemID)
        ON UPDATE CASCADE
        ON DELETE NO ACTION,
    CONSTRAINT UQ_OrderDetail_Order_Item UNIQUE (OrderID, ItemID)
);
GO

/* Helpful indexes for common lookups */
CREATE UNIQUE INDEX UX_Users_AgentID ON dbo.Users(AgentID) WHERE AgentID IS NOT NULL;
CREATE INDEX IX_Order_AgentID ON dbo.[Order](AgentID);
CREATE INDEX IX_Order_OrderDate ON dbo.[Order](OrderDate);
CREATE INDEX IX_OrderDetail_ItemID ON dbo.OrderDetail(ItemID);
GO

/* SEED DATA: 15 ITEMS */
SET IDENTITY_INSERT dbo.Item ON;
INSERT INTO dbo.Item (ItemID, ItemName, Size, Price, StockQuantity) VALUES
(1,  N'Premium Jasmine Rice',       N'5 kg bag',       125000.00, 180),
(2,  N'White Sugar',                N'1 kg bag',        26000.00, 240),
(3,  N'Cooking Oil',                N'2 litre bottle',  78000.00, 120),
(4,  N'Fish Sauce',                 N'750 ml bottle',   42000.00, 150),
(5,  N'Instant Noodles',            N'30-pack carton', 105000.00,  90),
(6,  N'Green Tea',                  N'25 tea bags',      55000.00, 100),
(7,  N'Condensed Milk',             N'380 g can',       32000.00, 200),
(8,  N'Ground Coffee',              N'500 g bag',       89000.00,  75),
(9,  N'Dishwashing Liquid',         N'1 litre bottle',  47000.00, 110),
(10, N'Laundry Detergent',          N'3 kg bag',       135000.00,  65),
(11, N'Paper Towels',               N'6-roll pack',     68000.00,  95),
(12, N'Mineral Water',              N'24-bottle carton',72000.00, 160),
(13, N'Canned Tuna',                N'170 g can',       29000.00, 130),
(14, N'Soy Sauce',                  N'500 ml bottle',   35000.00, 140),
(15, N'Chocolate Biscuits',         N'12-pack carton',  96000.00,  55);
SET IDENTITY_INSERT dbo.Item OFF;
GO

/* SEED DATA: 15 AGENTS */
SET IDENTITY_INSERT dbo.Agent ON;
INSERT INTO dbo.Agent (AgentID, AgentName, Address, Phone) VALUES
(1,  N'An Phu Trading',          N'12 Nguyen Trai, District 1, Ho Chi Minh City',       '0901000001'),
(2,  N'Binh Minh Distribution',  N'25 Le Loi, District 1, Ho Chi Minh City',            '0901000002'),
(3,  N'Central Market Supplies', N'88 Hai Ba Trung, District 3, Ho Chi Minh City',      '0901000003'),
(4,  N'Dai Viet Wholesale',      N'16 Cach Mang Thang 8, District 10, Ho Chi Minh City','0901000004'),
(5,  N'East Star Trading',       N'40 Vo Van Tan, District 3, Ho Chi Minh City',        '0901000005'),
(6,  N'Gia Han Retail Group',    N'72 Phan Van Tri, Go Vap, Ho Chi Minh City',          '0901000006'),
(7,  N'Hung Thinh Supplies',     N'9 Quang Trung, Go Vap, Ho Chi Minh City',            '0901000007'),
(8,  N'Kim Long Commerce',       N'31 Nguyen Oanh, Go Vap, Ho Chi Minh City',           '0901000008'),
(9,  N'Lan Anh Stores',           N'55 Truong Chinh, Tan Binh, Ho Chi Minh City',       '0901000009'),
(10, N'Mekong Retail Partners',  N'20 Au Co, Tan Phu, Ho Chi Minh City',                '0901000010'),
(11, N'Nam Phuong Distribution', N'14 Pham Van Dong, Thu Duc, Ho Chi Minh City',       '0901000011'),
(12, N'Phuoc Loc Trading',       N'63 Kha Van Can, Thu Duc, Ho Chi Minh City',         '0901000012'),
(13, N'Queen Mart Group',        N'101 Nguyen Thi Thap, District 7, Ho Chi Minh City', '0901000013'),
(14, N'Song Xanh Wholesale',     N'7 Huynh Tan Phat, District 7, Ho Chi Minh City',     '0901000014'),
(15, N'Thanh Cong Stores',       N'90 Ba Thang Hai, District 10, Ho Chi Minh City',    '0901000015');
SET IDENTITY_INSERT dbo.Agent OFF;
GO

/* SEED DATA: 15 USERS (3 admins + 12 agent accounts)
   Demo password for all sample accounts: ChangeMe123!
   The value below is a PLACEHOLDER, not a production-compatible password hash.
   Configure real password hashing in the application before using login.
*/
SET IDENTITY_INSERT dbo.Users ON;
INSERT INTO dbo.Users (UserID, UserName, Email, PasswordHash, [Lock], Role, AgentID) VALUES
(1,  N'admin01', N'admin01@wholesale.local', N'DEMO_HASH_CHANGE_ME', 0, N'Admin', NULL),
(2,  N'admin02', N'admin02@wholesale.local', N'DEMO_HASH_CHANGE_ME', 0, N'Admin', NULL),
(3,  N'manager', N'manager@wholesale.local', N'DEMO_HASH_CHANGE_ME', 0, N'Admin', NULL),
(4,  N'agent01', N'agent01@wholesale.local', N'DEMO_HASH_CHANGE_ME', 0, N'Agent', 1),
(5,  N'agent02', N'agent02@wholesale.local', N'DEMO_HASH_CHANGE_ME', 0, N'Agent', 2),
(6,  N'agent03', N'agent03@wholesale.local', N'DEMO_HASH_CHANGE_ME', 0, N'Agent', 3),
(7,  N'agent04', N'agent04@wholesale.local', N'DEMO_HASH_CHANGE_ME', 0, N'Agent', 4),
(8,  N'agent05', N'agent05@wholesale.local', N'DEMO_HASH_CHANGE_ME', 0, N'Agent', 5),
(9,  N'agent06', N'agent06@wholesale.local', N'DEMO_HASH_CHANGE_ME', 0, N'Agent', 6),
(10, N'agent07', N'agent07@wholesale.local', N'DEMO_HASH_CHANGE_ME', 0, N'Agent', 7),
(11, N'agent08', N'agent08@wholesale.local', N'DEMO_HASH_CHANGE_ME', 0, N'Agent', 8),
(12, N'agent09', N'agent09@wholesale.local', N'DEMO_HASH_CHANGE_ME', 0, N'Agent', 9),
(13, N'agent10', N'agent10@wholesale.local', N'DEMO_HASH_CHANGE_ME', 0, N'Agent', 10),
(14, N'agent11', N'agent11@wholesale.local', N'DEMO_HASH_CHANGE_ME', 0, N'Agent', 11),
(15, N'agent12', N'agent12@wholesale.local', N'DEMO_HASH_CHANGE_ME', 0, N'Agent', 12);
SET IDENTITY_INSERT dbo.Users OFF;
GO

/* SEED DATA: 15 ORDERS */
SET IDENTITY_INSERT dbo.[Order] ON;
INSERT INTO dbo.[Order] (OrderID, OrderDate, AgentID, OrderStatus) VALUES
(1,  '2026-10-01T09:15:00', 1,  N'Shipped'),
(2,  '2026-10-02T10:30:00', 2,  N'Shipped'),
(3,  '2026-10-03T11:00:00', 3,  N'Confirmed'),
(4,  '2026-10-04T13:20:00', 4,  N'Pending'),
(5,  '2026-10-05T14:10:00', 5,  N'Shipped'),
(6,  '2026-10-06T08:45:00', 6,  N'Pending'),
(7,  '2026-10-07T09:50:00', 7,  N'Confirmed'),
(8,  '2026-10-08T15:05:00', 8,  N'Shipped'),
(9,  '2026-10-09T10:15:00', 9,  N'Pending'),
(10, '2026-10-10T11:40:00', 10, N'Shipped'),
(11, '2026-10-11T13:00:00', 11, N'Confirmed'),
(12, '2026-10-12T14:25:00', 12, N'Pending'),
(13, '2026-10-13T08:30:00', 13, N'Shipped'),
(14, '2026-10-14T16:10:00', 14, N'Confirmed'),
(15, '2026-10-15T09:35:00', 15, N'Pending');
SET IDENTITY_INSERT dbo.[Order] OFF;
GO

/* SEED DATA: 30 ORDER DETAILS (2 lines per order) */
INSERT INTO dbo.OrderDetail (OrderID, ItemID, Quantity, UnitAmount) VALUES
(1,  1, 10, 125000.00), (1,  3,  5,  78000.00),
(2,  2, 20,  26000.00), (2,  5,  8, 105000.00),
(3,  4, 12,  42000.00), (3,  7, 10,  32000.00),
(4,  8,  6,  89000.00), (4,  9,  8,  47000.00),
(5, 10,  4, 135000.00), (5, 11,  6,  68000.00),
(6, 12, 15,  72000.00), (6, 13, 20,  29000.00),
(7, 14, 10,  35000.00), (7, 15,  5,  96000.00),
(8,  1,  8, 125000.00), (8,  2, 15,  26000.00),
(9,  3,  7,  78000.00), (9,  4,  9,  42000.00),
(10, 5, 12, 105000.00), (10, 6,  8,  55000.00),
(11, 7, 14,  32000.00), (11, 8,  5,  89000.00),
(12, 9, 10,  47000.00), (12, 10, 3, 135000.00),
(13, 11, 8,  68000.00), (13, 12, 10, 72000.00),
(14, 13, 15,  29000.00), (14, 14, 12, 35000.00),
(15, 15,  7,  96000.00), (15, 1,  6, 125000.00);
GO

/* QUICK VERIFICATION: each required table should have 15-30 rows */
SELECT N'Item' AS TableName, COUNT(*) AS RowCount FROM dbo.Item
UNION ALL
SELECT N'Agent', COUNT(*) FROM dbo.Agent
UNION ALL
SELECT N'Users', COUNT(*) FROM dbo.Users
UNION ALL
SELECT N'Order', COUNT(*) FROM dbo.[Order]
UNION ALL
SELECT N'OrderDetail', COUNT(*) FROM dbo.OrderDetail;
GO

/* Example query: order totals for the My Orders page / dashboard */
SELECT
    o.OrderID,
    o.OrderDate,
    a.AgentName,
    o.OrderStatus,
    SUM(od.Quantity * od.UnitAmount) AS OrderTotal
FROM dbo.[Order] AS o
INNER JOIN dbo.Agent AS a ON a.AgentID = o.AgentID
INNER JOIN dbo.OrderDetail AS od ON od.OrderID = o.OrderID
GROUP BY o.OrderID, o.OrderDate, a.AgentName, o.OrderStatus
ORDER BY o.OrderDate DESC;
GO
