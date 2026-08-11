/* =====================================================================
   Phase 6: Password Algorithm Migration
   ===================================================================== */

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.Users') AND name = 'PasswordAlgorithm'
)
BEGIN
    ALTER TABLE dbo.Users
    ADD PasswordAlgorithm NVARCHAR(20) NOT NULL DEFAULT ('SHA256');
END
GO

UPDATE dbo.Users
SET PasswordAlgorithm = 'SHA256'
WHERE PasswordAlgorithm IS NULL;
GO

/* ROLLBACK (only if needed, run manually):
   ALTER TABLE dbo.Users DROP COLUMN PasswordAlgorithm;
*/