# 📚 BookMart Data Access & Migrations Guide

## 🔄 Database-First Approach (Generating C# from DB)
If you have an existing database and want to automatically generate C# POCO entities and the DbContext (Reverse Engineering):
```bash
Scaffold-DbContext -Connection "Name=ConnectionStrings:DefaultConnection" Microsoft.EntityFrameworkCore.SqlServer -OutputDir ../BookMart.Model/EntityModels -Namespace "BookMart.Model.EntityModels" -ContextDir DbContexts -Context ApplicationDbContext -ContextNamespace "BookMart.DataAccess"
```

## 🛠️ Troubleshooting & VS Package Manager Tips
1. Startup Project Check: Always ensure `BookMartApi` is set as your Startup Project in `Visual Studio (Right-click -> Set as Startup Project)` so EF Core can find the connection strings in appsettings.json.
2. Default Project Check: Ensure `BookMart.DataAccess` is selected in the Default project dropdown menu located at the top of the Package Manager Console window.
