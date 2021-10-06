USE [master]
GO

IF EXISTS(SELECT * FROM sys.databases WHERE [name] = 'Kanban')
    DROP DATABASE Kanban
GO

CREATE DATABASE Kanban
GO

USE Kanban
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE Tags(
    Id int IDENTITY(1,1) NOT NULL,
    [Name] nvarchar(50) NOT NULL,
    [Tasks] NVARCHAR(100) NOT NULL,
    PRIMARY KEY ([Name])
)
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE Tasks(
    Id int IDENTITY(1,1) NOT NULL,
    Title nvarchar(100) NOT NULL,
    Assigned_To NVARCHAR(100) NULL,
    [Description] nvarchar(max) NULL,
    [State] nvarchar(10) NOT NULL,
    [Tags] nvarchar(50) NOT NULL,
    PRIMARY KEY (Title)
)
GO
CREATE TABLE [Users](
    Id int IDENTITY(1,1) NOT NULL,
    [Name] NVARCHAR(100) NOT NULL,
    Email NVARCHAR(100) NOT NULL,
    [Tasks] NVARCHAR(100) NOT NULL,
    PRIMARY KEY ([Name])
)

INSERT Tags ([Name], [Tasks]) VALUES (N'Tag 1', N'Task 1, Task 2')
INSERT Tags ([Name], [Tasks]) VALUES (N'Tag 2', N'Task 1, Task 4')
INSERT Tags ([Name], [Tasks]) VALUES (N'Tag 3', N'Task 3, Task 5, Task 1')
INSERT Tags ([Name], [Tasks]) VALUES (N'Tag 4', N'Task 1, Task 6, Task 7')
INSERT Tags ([Name], [Tasks]) VALUES (N'Tag 5', N'Task 2, Task 3, Task 4')

INSERT Tasks (Title, Assigned_To, [Description], [State], [Tags]) VALUES (N'Task 1', N'Billy, Jason', N'Random description that can be any length', N'New', N'Tag 1, Tag 2, Tag 3, Tag 4')
INSERT Tasks (Title, Assigned_To, [Description], [State], [Tags]) VALUES (N'Task 2', N'Billy', NULL, N'New', N'Tag 1, Tag 5')
INSERT Tasks (Title, Assigned_To, [Description], [State], [Tags]) VALUES (N'Task 3', N'Jason', N'Another description', N'Active', N'Tag 3, Tag 5')
INSERT Tasks (Title, Assigned_To, [Description], [State], [Tags]) VALUES (N'Task 4', N'Jason', N'And another one', N'Active', N'Tag 2, Tag 5')
INSERT Tasks (Title, Assigned_To, [Description], [State], [Tags]) VALUES (N'Task 5', N'Derek', N'And another one', N'Resolved', N'Tag 3')
INSERT Tasks (Title, Assigned_To, [Description], [State], [Tags]) VALUES (N'Task 6', N'Derek', N'And another one', N'Closed', N'Tag 4')
INSERT Tasks (Title, Assigned_To, [Description], [State], [Tags]) VALUES (N'Task 7', N'Derek', NULL, N'Removed', N'Tag 4')

INSERT [Users] ([Name], Email, [Tasks]) VALUES (N'Billy', N'Billy@gmail.com', N'Task 1, Task 2')
INSERT [Users] ([Name], Email, [Tasks]) VALUES (N'Jason', N'Jason@gmail.com', N'Task 1, Task 3, Task 4')
INSERT [Users] ([Name], Email, [Tasks]) VALUES (N'Derek', N'Derek@gmail.com', N'Task 5, Task 6, Task 7')

GO