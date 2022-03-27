# Duende_IdentityServer_Test
Just a .net 6 Duende IdentityServer build based on documentation and tutorials for future reference

## ENTITY FRAMEWORK STORES:

 ### Comands to Add EntityFramework Stores (after adding propper context)

## Create Migrations for Duende Identity Server

```
> dotnet ef migrations add InitialIdentityServerMigration -c PersistedGrantDbContext

> dotnet ef migrations add InitialIdentityServerMigration -c ConfigurationDbContext
```

>Create Database

```
> dotnet ef database update -c PersistedGrantDbContext

> dotnet ef database update -c ConfigurationDbContext
```

>Seed Database [will use static config data in project]


## Create Migrations for Asp Net Core Identity

```
> dotnet ef migrations add InitialIdentityServerMigration -c ApplicationDbContext
```

>Create Database

```
> dotnet ef database update -c ApplicationDbContext
```
## Seed Both Databases

```
> dotnet run /seed
```