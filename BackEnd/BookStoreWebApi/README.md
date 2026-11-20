# 📚 BookStore Data Access & Migrations Guide

This repository demonstrates the implementation of data access layers using **Entity Framework Core**, **Dapper**, and **ADO.NET**. 

This guide serves as a technical reference for managing the database schema, generating models, and safely deploying changes across different environments.

---

## 🏗️ Code-First Approach (Generating DB from C#)

When using the Code-First approach, your C# entities dictate the database schema. You can execute these commands using either the **Package Manager Console (PMC)** in Visual Studio, or the cross-platform **.NET CLI** (Terminal/Command Prompt).

> **💡 Context Note:** In this solution, `BookStoreWebApi` is the startup project, and `BookStore.DataAccess` contains the DbContext. The CLI commands below explicitly target these projects.

### 1. Create a New Migration
Compares your current Entity models with the last database snapshot and creates a new C# migration file.

* **PMC:** `Add-Migration InitialMigration`
* **CLI:** `dotnet ef migrations add InitialMigration --project BookStore.DataAccess --startup-project BookStoreWebApi`

### 2. Apply Changes to the Database
Applies all pending migrations to the physical database.

* **PMC:** `Update-Database`
* **CLI:** `dotnet ef database update --project BookStore.DataAccess --startup-project BookStoreWebApi`

### 3. Revert Database to a Previous State
If you made a mistake and applied it to the DB, you must roll the database back **before** deleting the migration files.

* **PMC:** `Update-Database <NameOfPreviousMigration>`
* **CLI:** `dotnet ef database update <NameOfPreviousMigration> --project BookStore.DataAccess --startup-project BookStoreWebApi`

### 4. Remove the Last Migration
Deletes the latest migration files. **Only use this if the migration has NOT been applied to the database** (or if you just rolled it back using the command above).

* **PMC:** `Remove-Migration`
* **CLI:** `dotnet ef migrations remove --project BookStore.DataAccess --startup-project BookStoreWebApi`

### 5. List All Migrations
Shows a list of all migrations and whether they have been applied to the database.

* **PMC:** `Get-Migration`
* **CLI:** `dotnet ef migrations list --project BookStore.DataAccess --startup-project BookStoreWebApi`

---

## 👥 Managing Migrations in a Multi-Developer Team

Working with EF Core migrations in a team can lead to `ModelSnapshot.cs` merge conflicts, especially if developers share a single central development database.

### The Best Practice: Isolated Local Databases
**Do not use a shared centralized database for active development.** Every developer should have their own local database (using SQL Server LocalDB, Docker containers, or SQLite). 
1. Dev A and Dev B generate migrations locally and test them in isolation.
2. They open Pull Requests to the `main` branch.
3. Only the CI/CD pipeline runs the updates against the shared Staging/Development servers.

### The Survival Guide (If you MUST use a shared Dev DB)
If your infrastructure forces you to share a single dev database, follow the **"Pull, Update, Add, Push"** rule:
1. `git pull` the latest `main` branch to get your teammates' migrations.
2. Run `Update-Database` so your local snapshot matches the shared server.
3. Run `Add-Migration <YourFeature>` to generate your changes.
4. Immediately commit and push so others have your snapshot.

### How to Resolve `ModelSnapshot.cs` Merge Conflicts
**Never try to manually resolve a `ModelSnapshot.cs` Git conflict line-by-line.** You will corrupt the tracking graph.
1. Dev A merges their code to `main`.
2. Dev B pulls `main` and gets a conflict.
3. Dev B **deletes** their newly generated (unmerged) migration files from their `Migrations` folder.
4. Dev B resolves the Git conflict by explicitly choosing **Dev A's version** of the `ModelSnapshot.cs`.
5. Dev B runs `Add-Migration <TheirFeature>` *again*. EF Core will cleanly calculate the diff and generate a fresh migration.

---

## 🚀 Deployment & Environment Management (Production)

**CRITICAL RULE:** Developers should not have direct access to run migrations against the Production database from their local machines. Do this via automated processes.

### Strategy 1: Idempotent SQL Scripts (DBA Friendly)
Ideal for environments where a Database Administrator must review all SQL before it is executed, or if you use standard SQL deployment tools.

```bash
dotnet ef migrations script --idempotent --output migratedb.sql --project BookStore.DataAccess --startup-project BookStoreWebApi
```

Explanation: The --idempotent flag wraps every SQL command in an IF NOT EXISTS block. This ensures the script is safe to run on any environment, as it checks the __EFMigrationsHistory table and only applies missing changes. Your CI/CD pipeline runs this script during deployment.

### Strategy 2: EF Core Migration Bundles (Automated CI/CD)
Ideal for fully automated DevOps pipelines (like GitHub Actions or Azure DevOps). This compiles your migrations into a standalone executable that doesn't require the .NET SDK to run on the server.

1. Build bundle
```bash
dotnet ef migrations bundle --project BookStore.DataAccess --startup-project BookStoreWebApi --output efbundle.exe
```
2. Execute it during the CD Release Phase:
```bash
./efbundle.exe --connection "Server=prod-server;Database=BookStoreDb;User Id=DeployUser;Password=***;
```
Explanation: The bundle automatically connects to the production database, checks the history table, calculates which migrations are missing, and applies them sequentially.

## ⚠️ Anti-Patterns to Avoid in Code-First
### 1. Running context.Database.Migrate() on Application Startup:
Why it's bad: If your application scales horizontally (multiple instances spinning up simultaneously), they will all attempt to migrate the database at the same time. This causes table locks, race conditions, and catastrophic schema corruption. Always apply migrations via CI/CD pipelines.

### 2. Modifying an existing Migration File after applying it:
Why it's bad: Once a migration is applied to a database (or merged to main), it is written in stone. If you alter the C# `Up()` or `Down()` methods, EF Core will not know, because it only tracks the migration filename in the `__EFMigrationsHistory` table. Always create a new migration to fix previous mistakes.

### 3. Ignoring the Down() method:
Why it's bad: If you write raw SQL in a migration's `Up()` method, always write the exact reverse operation in the `Down()` method. Otherwise, rolling back a deployment during an outage becomes impossible.

## 🔄 Database-First Approach (Generating C# from DB)
If you have an existing database and want to automatically generate C# POCO entities and the DbContext (Reverse Engineering):
```bash
Scaffold-DbContext -Connection "Name=ConnectionStrings:DefaultConnection" Microsoft.EntityFrameworkCore.SqlServer -OutputDir ../BookStore.Model/EntityModels -Namespace "BookStore.Model.EntityModels" -ContextDir DbContexts -Context ApplicationDbContext -ContextNamespace "BookStore.DataAccess"
```

## 🛠️ Troubleshooting & VS Package Manager Tips
1. Startup Project Check: Always ensure `BookStoreWebApi` is set as your Startup Project in `Visual Studio (Right-click -> Set as Startup Project)` so EF Core can find the connection strings in appsettings.json.
2. Default Project Check: Ensure `BookStore.DataAccess` is selected in the Default project dropdown menu located at the top of the Package Manager Console window.
