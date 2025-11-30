-- Migration: ChangeStudentQuizPrimaryKey
-- This script changes the StudentQuizzes table primary key from composite (StudentId, QuizId) to single Id column

BEGIN TRANSACTION;

-- Step 1: Find and drop ANY existing primary key on StudentQuizzes table
DECLARE @PrimaryKeyName NVARCHAR(200);
SELECT @PrimaryKeyName = name 
FROM sys.key_constraints 
WHERE type = 'PK' 
  AND parent_object_id = OBJECT_ID('StudentQuizzes');

IF @PrimaryKeyName IS NOT NULL
BEGIN
    DECLARE @DropPKSQL NVARCHAR(MAX);
    SET @DropPKSQL = 'ALTER TABLE [StudentQuizzes] DROP CONSTRAINT [' + @PrimaryKeyName + ']';
    EXEC sp_executesql @DropPKSQL;
    PRINT 'Dropped existing primary key: ' + @PrimaryKeyName;
END

-- Step 2: Add the new Id column ONLY if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('StudentQuizzes') AND name = 'Id')
BEGIN
    ALTER TABLE [StudentQuizzes] ADD [Id] nvarchar(450) NOT NULL DEFAULT NEWID();
    PRINT 'Added Id column';
END
ELSE
BEGIN
    -- If Id column already exists, just make sure it has unique values
    UPDATE [StudentQuizzes] SET [Id] = NEWID() WHERE [Id] IS NULL OR [Id] = '';
    PRINT 'Updated existing Id column with unique values';
END

-- Step 3: Set the new primary key
ALTER TABLE [StudentQuizzes] ADD CONSTRAINT [PK_StudentQuizzes] PRIMARY KEY ([Id]);
PRINT 'Created new primary key on Id column';

-- Step 4: Create an index on StudentId and QuizId for performance (if it doesn't exist)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_StudentQuizzes_StudentId_QuizId' AND object_id = OBJECT_ID('StudentQuizzes'))
BEGIN
    CREATE INDEX [IX_StudentQuizzes_StudentId_QuizId] ON [StudentQuizzes] ([StudentId], [QuizId]);
    PRINT 'Created index on StudentId and QuizId';
END

COMMIT TRANSACTION;
PRINT 'Migration completed successfully!';
