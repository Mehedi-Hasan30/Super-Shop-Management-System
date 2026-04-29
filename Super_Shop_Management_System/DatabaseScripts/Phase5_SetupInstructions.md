# Phase 5 Setup Instructions (Production)

## 1. Run database setup (required)
1. Open SQL Server Management Studio and connect to the database referenced by `App.config` (`SuperShopConnection`).
2. Execute:
   - `DatabaseScripts/Phase5_ProductionSetup.sql`

## 2. Backup/Restore permissions
- Ensure the SQL login used by `SuperShopConnection` has permission to run `BACKUP DATABASE` and `RESTORE DATABASE`.
- Admin users in the app can access **Backup & Restore** from the sidebar.

## 3. Backup reminder behavior
- The app stores the last successful backup timestamp locally under:
  - `%APPDATA%\SuperShopManagementSystem\last_backup_utc.txt`
- `NotificationService` uses that timestamp to generate backup reminders.

## 4. Dark mode preference
- Theme selection is persisted locally under:
  - `%APPDATA%\SuperShopManagementSystem\theme.txt`
- Use **Settings** → **Enable Dark Mode** to toggle.

## 5. Charts/export dependencies
- Reports uses `System.Windows.Forms.DataVisualization` (Charting) which is added to the project references.

