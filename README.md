# TravelGuideDbTools

EF Core migrations tooling of [TravelGuide](https://github.com/merabza/TravelGuide): the migrations assembly for `TravelGuideDbContext` (defined in [TravelGuideDbPart](https://github.com/merabza/TravelGuideDbPart)) and the fake web host that `dotnet ef` uses as its startup project.

| Project | Purpose |
|---|---|
| `TravelGuideDbTools.DbMigration` | Migrations assembly: the `Migrations` folder plus `AssemblyReference` |
| `TravelGuideDbTools.FakeHost` | Minimal web host used only as the `dotnet ef` startup project; `TravelGuideDesignTimeDbContextFactory` creates the context at design time |

The design-time factory reads the connection string from the FakeHost project's User Secrets (`ConnectionString` key; the `UserSecretsId` is in `TravelGuideDbTools.FakeHost.csproj`):

```powershell
dotnet user-secrets set ConnectionString "<connection string>" --project TravelGuideDbTools.FakeHost
```

## Migrations

```powershell
dotnet ef migrations add <Name> --project TravelGuideDbTools.DbMigration --startup-project TravelGuideDbTools.FakeHost
dotnet ef database update --project TravelGuideDbTools.DbMigration --startup-project TravelGuideDbTools.FakeHost
```

## Repository layout — sibling repos are required

Projects reference sibling clones by relative path (`..\..\TravelGuideDbPart\...`, `..\..\TravelGuideCore\...`, `..\..\SystemTools\...`), so the repositories must be cloned next to each other:

```
<root>\
├── TravelGuideDbTools\      this repository (TravelGuideDbTools.slnx lives here)
├── TravelGuideDbPart\       TravelGuideDbContext and entity configurations (merabza/TravelGuideDbPart)
├── TravelGuideCore\         domain entities and abstractions (merabza/TravelGuideCore)
└── SystemTools\             shared libraries (merabza/SystemTools)
```

## Build

```powershell
dotnet build TravelGuideDbTools.slnx
```

## License

[MIT](LICENSE)
