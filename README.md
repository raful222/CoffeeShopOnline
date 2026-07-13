# GTR Coffee Shop Online

An ASP.NET MVC 5 application for cafe table selection, menu ordering, inventory, loyalty rewards, staff ordering, and administration.

## What the application provides

- Responsive Hebrew-first menu with search, live cart updates, and stock feedback
- Table selection for customer and barista-assisted orders
- Transactional checkout that revalidates the table and inventory
- Role-protected admin and barista areas
- ASP.NET Identity accounts with loyalty stars and age-restricted products
- Optional PayPal sandbox and Google sign-in integrations

## Requirements

- Visual Studio 2022 with **ASP.NET and web development**
- .NET Framework 4.7.2 Developer Pack
- SQL Server LocalDB

## Run locally

1. Clone the repository and open `CoffeeShopOnline.sln`.
2. Restore NuGet packages.
3. Build the solution in Visual Studio.
4. Run with IIS Express. Entity Framework migrations initialize the LocalDB database used by `DefaultConnection`.

Command-line build from a Visual Studio Developer PowerShell:

```powershell
msbuild CoffeeShopOnline.sln /t:Restore /p:RestorePackagesConfig=true
msbuild CoffeeShopOnline.sln /p:Configuration=Debug
```

## Optional integrations

Credentials are never stored in source control. Set these environment variables for the integrations you use:

| Variable | Purpose |
| --- | --- |
| `COFFEESHOP_PAYPAL_CLIENT_ID` | PayPal sandbox client ID |
| `COFFEESHOP_PAYPAL_CLIENT_SECRET` | PayPal sandbox client secret |
| `COFFEESHOP_GOOGLE_CLIENT_ID` | Google OAuth client ID |
| `COFFEESHOP_GOOGLE_CLIENT_SECRET` | Google OAuth client secret |

Restart Visual Studio/IIS Express after changing environment variables.

> The credentials previously committed to this repository must be revoked and replaced in the provider dashboards; removing them from the latest commit does not remove them from Git history.

## Security notes

- New passwords use ASP.NET Identity's salted PBKDF2 hasher. Existing legacy MD5 accounts are upgraded automatically after a successful login.
- Login lockout is enabled after five failed attempts.
- State-changing ordering actions validate anti-forgery tokens.
- Never commit `.mdf`, publish profiles, or API credentials.

## Continuous integration

The GitHub Actions workflow restores packages and builds the solution on Windows for every push and pull request to `master`.
