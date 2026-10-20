# First-run administrator setup
The first administrator chooses their own password in the browser. The app NEVER supplies a default administrator password.

1. Generate and apply reviewed EF Core migrations. This repository does not yet contain a generated initial migration. Do not enable startup migrations before generating and reviewing it.
2. Configure `INITIAL_SETUP_ENABLED=true` and a cryptographically random `INITIAL_SETUP_TOKEN` (32+ characters) in private deployment secrets. Do not commit the token or share it in screenshots.
3. Initialize the Admin, Supervisor and Employee roles via DatabaseInitializationService (or a controlled migration/setup command). Role initialization currently runs when Database__ApplyMigrationsOnStartup is enabled.
4. Restrict setup access to a trusted network, preferably HTTPS, and open `/setup`. Enter your name, email, a strong password of your choice, confirmation and setup token.
5. After the first Admin is created, `/setup` returns 404. Immediately disable `INITIAL_SETUP_ENABLED`, remove the setup token, and restart the app.
6. Subsequent sign-ins use `/Account/Login`. The first administrator can then manage user roles.

Security note: avoid running multiple app replicas during first-time setup. There is currently no database-level one-time setup lock. A public deployment requires HTTPS termination and secure-cookie settings. Do not expose the setup page on the open internet.

Remaining engineering: migrations, verified compilation, database-backed integration tests, employee onboarding, complete CRUD, and production deployment hardening.