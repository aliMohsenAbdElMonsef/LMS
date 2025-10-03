
CREATE TABLE Users (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Email NVARCHAR(256) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(256) NOT NULL,
    FirstName NVARCHAR(100) NOT NULL,
    LastName NVARCHAR(100) NOT NULL,
    [Role] NVARCHAR(50) NOT NULL CHECK([Role] IN ('Student', 'Admin', 'SubAdmin', 'Instructor'))
);
