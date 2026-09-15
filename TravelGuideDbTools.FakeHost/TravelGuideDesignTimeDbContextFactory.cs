using Microsoft.EntityFrameworkCore;
using SystemTools.DatabaseToolsShared;
using TravelGuideDbPart.Db;
using TravelGuideDbTools.DbMigration;

namespace TravelGuideDbTools.FakeHost;

//ეს კლასი საჭიროა იმისათვის, რომ შესაძლებელი გახდეს მიგრაციასთან მუშაობა.
//ანუ დეველოპერ ბაზის წაშლა და ახლიდან დაგენერირება, ან მიგრაციაში ცვლილებების გაკეთება
// ReSharper disable once UnusedType.Global
public sealed class TravelGuideDesignTimeDbContextFactory : SqlServerDesignTimeDbContextFactory<TravelGuideDbContext>
{
    //ConnectionString, როგორც დაცული ინფორმაცია, მოდის FakeHost-ის User Secrets-იდან (UserSecretsId წერია csproj-ში).
    //კონსტრუქტორი აუცილებლად უპარამეტრო უნდა იყოს, რადგან dotnet ef ამ კლასს თვითონ ქმნის რეფლექსიით
    // ReSharper disable once ConvertToPrimaryConstructor
    public TravelGuideDesignTimeDbContextFactory() : base(AssemblyReference.Assembly.GetName().Name!,
        "ConnectionString", true)
    {
    }

    protected override TravelGuideDbContext CreateDbContext(DbContextOptions<TravelGuideDbContext> options)
    {
        // ReSharper disable once DisposableConstructor
        return new TravelGuideDbContext(options);
    }
}
