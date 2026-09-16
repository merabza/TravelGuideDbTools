using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TravelGuideDbTools.DbMigration.Migrations
{
    /// <inheritdoc />
    public partial class MoveStateToUrls : Migration
    {
        //სტატუსის (State) გადატანა Places-იდან Urls-ში მონაცემების დაკარგვის გარეშე: სტატუსი მისამართის ქროულინგის
        //მდგომარეობაა და ადგილს კი არა, მის მისამართს ეკუთვნის; უმისამართო (ხელით შეყვანილ) ადგილს სტატუსი აღარ აქვს.
        //ავტომატურად დაგენერირებული ვარიანტი Places.State-ს ჯერ შლიდა და Urls.State-ს მერე ამატებდა (მნიშვნელობები
        //დაიკარგებოდა), ამიტომ Up/Down ხელით არის დაწერილი: ჯერ ახალი სვეტი ემატება, მერე მნიშვნელობები გადადის და
        //ძველი სვეტი მხოლოდ ბოლოს იშლება
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "State",
                table: "Urls",
                type: "int",
                nullable: false,
                defaultValue: 0);

            //ერთ მისამართზე რამდენიმე ადგილს აპლიკაცია არ ქმნის (მისამართის უნიკალურობა შენახვამდე მოწმდება) —
            //ასეთი, სხვადასხვა სტატუსიანი ადგილების არსებობისას მისამართს ერთ-ერთის სტატუსი შემთხვევით შეხვდებოდა
            //და დანარჩენი დაიკარგებოდა, ამიტომ მიგრაცია ჩერდება: ჯერ ხელით უნდა გაირკვეს.
            //EXEC-ით, რომ `migrations script`-ით მიღებულ ერთბატჩიან სკრიპტშიც იმუშაოს — ახლად დამატებულ სვეტს
            //იმავე ბატჩის ჩვეულებრივი UPDATE კომპილაციისას ვერ ხედავს
            migrationBuilder.Sql("""
                IF EXISTS (SELECT 1
                           FROM dbo.Places p1
                               JOIN dbo.Places p2 ON p2.UrlId = p1.UrlId AND p2.PlaceId <> p1.PlaceId
                           WHERE p1.State <> p2.State)
                    THROW 50000, N'Places sharing a Url with different States exist - resolve them first', 1;
                EXEC(N'UPDATE u SET u.State = p.State FROM dbo.Urls u JOIN dbo.Places p ON p.UrlId = u.UrlId;');
                """);

            //ძველი სვეტი მხოლოდ ბოლოს, მნიშვნელობების გადატანის შემდეგ იშლება
            migrationBuilder.DropColumn(
                name: "State",
                table: "Places");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "State",
                table: "Places",
                type: "int",
                nullable: false,
                defaultValue: 0);

            //სტატუსი ადგილებს უბრუნდება; უმისამართო ადგილს ძველ სქემაში საკუთარი სტატუსი ჰქონდა, რომელიც ახლა არსად
            //ინახება — ის Analysed (4) ხდება, რაც ხელით შექმნილი ადგილის ნაგულისხმევი სტატუსი იყო (ქროულერი
            //უმისამართოს სტატუსის მიუხედავად არ ეხება)
            migrationBuilder.Sql("""
                EXEC(N'UPDATE p SET p.State = u.State FROM dbo.Places p JOIN dbo.Urls u ON u.UrlId = p.UrlId;
                       UPDATE dbo.Places SET State = 4 WHERE UrlId IS NULL;');
                """);

            migrationBuilder.DropColumn(
                name: "State",
                table: "Urls");
        }
    }
}
