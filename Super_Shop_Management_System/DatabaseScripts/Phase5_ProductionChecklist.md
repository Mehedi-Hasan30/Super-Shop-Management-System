# Phase 5 Production Checklist

## A. Build & runtime
- [ ] Build succeeds for `net48` Release.
- [ ] Start the app and verify theme loads (Light/Dark).
- [ ] Open POS, complete a checkout with **Paid** and confirm stock deduction.
- [ ] Open POS, checkout with **Pending** and confirm no stock deduction.

## B. Reporting & analytics
- [ ] Open **Reports & Analytics** and verify filters load.
- [ ] Verify at least one report (Daily Sales) loads data.
- [ ] Verify profit/loss cards and monthly profit chart render.
- [ ] Export at least one report to CSV and PDF-compatible HTML.
- [ ] Print preview works (system print dialog appears).

## C. Notifications
- [ ] Open Dashboard; confirm Notifications panel renders.
- [ ] Open Notification Center and ensure grid loads.

## D. Backup & Restore
- [ ] Log in as Admin.
- [ ] Open **Backup & Restore**.
- [ ] Run a manual backup to a custom `.bak` path.
- [ ] Restore from a known-good backup (test in non-production if possible).

## E. Database validation
- [ ] Run `DatabaseScripts/Phase5_ProductionSetup.sql` once before deployment.
- [ ] Confirm required schema columns exist (Sales/SalesDetails/PaymentStatus fields).

