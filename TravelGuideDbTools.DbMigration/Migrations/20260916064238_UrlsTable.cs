using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TravelGuideDbTools.DbMigration.Migrations
{
    /// <inheritdoc />
    public partial class UrlsTable : Migration
    {
        //მისამართების გადატანა Places-იდან ახალ Urls ცხრილში მონაცემების დაკარგვის გარეშე. ავტომატურად დაგენერირებული
        //ვარიანტი Url სვეტს შლიდა და UrlHashCode-ს UrlId-ად არქმევდა (ხეშ-კოდები იდენტიფიკატორებად ჩაითვლებოდა),
        //ამიტომ Up/Down ხელით არის დაწერილი. Urls-ის ჩანაწერს იმ ადგილის PlaceId ენიჭება UrlId-ად, რომლისგანაც
        //გადმოვიდა (IDENTITY_INSERT) — ამით UrlGraphNodes-ის FromUrlId/GotUrlId მნიშვნელობები, რომლებიც აქამდე
        //PlaceId-ები იყო, უცვლელად სწორი UrlId-ები ხდება. იგივე ლოგიკა არსებული ბაზისთვის T-SQL სკრიპტადაც არსებობს:
        //TravelGuide/SqlScripts/MigratePlaceUrlsToUrlsTable.sql
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Urls",
                columns: table => new
                {
                    UrlId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    UrlHashCode = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Urls", x => x.UrlId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Urls_UrlHashCode",
                table: "Urls",
                column: "UrlHashCode");

            //მისამართიანი ადგილების Url/UrlHashCode Urls-ში იწერება UrlId = PlaceId-ით. Urls-ში ხეშ-კოდი სავალდებულოა —
            //აპლიკაცია ორივეს ერთად წერს, უხეშო მისამართიანი ჩანაწერი მონაცემების შეცდომაა და მიგრაცია ჩერდება
            migrationBuilder.Sql("""
                IF EXISTS (SELECT 1 FROM dbo.Places WHERE Url IS NOT NULL AND UrlHashCode IS NULL)
                    THROW 50000, N'Places rows with Url but without UrlHashCode exist - fill UrlHashCode first', 1;
                SET IDENTITY_INSERT dbo.Urls ON;
                INSERT INTO dbo.Urls (UrlId, Url, UrlHashCode)
                SELECT PlaceId, Url, UrlHashCode FROM dbo.Places WHERE Url IS NOT NULL;
                SET IDENTITY_INSERT dbo.Urls OFF;
                """);

            migrationBuilder.AddColumn<int>(
                name: "UrlId",
                table: "Places",
                type: "int",
                nullable: true);

            //EXEC-ით, რომ `migrations script`-ით მიღებულ ერთბატჩიან სკრიპტშიც იმუშაოს — ახლად დამატებულ სვეტს
            //იმავე ბატჩის ჩვეულებრივი UPDATE კომპილაციისას ვერ ხედავს
            migrationBuilder.Sql("EXEC(N'UPDATE dbo.Places SET UrlId = PlaceId WHERE Url IS NOT NULL;');");

            migrationBuilder.CreateIndex(
                name: "IX_Places_UrlId",
                table: "Places",
                column: "UrlId");

            //გრაფის წიბო, რომელიც უმისამართო (ან არარსებულ) ადგილზე მიუთითებს, Urls-ზე ვერ გადავა — ასეთი წიბო
            //გრაფში უაზროა, მაგრამ მიგრაცია მას ჩუმად არ შლის: ჯერ ხელით უნდა წაიშალოს (იხ. SQL სკრიპტის მე-0 ნაბიჯი)
            migrationBuilder.Sql("""
                IF EXISTS (SELECT 1
                           FROM dbo.UrlGraphNodes ugn
                           WHERE NOT EXISTS (SELECT 1 FROM dbo.Urls u WHERE u.UrlId = ugn.FromUrlId)
                              OR NOT EXISTS (SELECT 1 FROM dbo.Urls u WHERE u.UrlId = ugn.GotUrlId))
                    THROW 50000, N'UrlGraphNodes rows referencing places without Url exist - delete them first', 1;
                """);

            migrationBuilder.DropForeignKey(
                name: "FK_UrlGraphNodes_Places_FromUrlId",
                table: "UrlGraphNodes");

            migrationBuilder.DropForeignKey(
                name: "FK_UrlGraphNodes_Places_GotUrlId",
                table: "UrlGraphNodes");

            migrationBuilder.AddForeignKey(
                name: "FK_Places_Urls_UrlId",
                table: "Places",
                column: "UrlId",
                principalTable: "Urls",
                principalColumn: "UrlId");

            migrationBuilder.AddForeignKey(
                name: "FK_UrlGraphNodes_Urls_FromUrlId",
                table: "UrlGraphNodes",
                column: "FromUrlId",
                principalTable: "Urls",
                principalColumn: "UrlId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UrlGraphNodes_Urls_GotUrlId",
                table: "UrlGraphNodes",
                column: "GotUrlId",
                principalTable: "Urls",
                principalColumn: "UrlId",
                onDelete: ReferentialAction.Restrict);

            //ძველი სვეტები მხოლოდ ბოლოს, მონაცემების გადატანისა და კავშირების გადაბმის შემდეგ იშლება
            migrationBuilder.DropIndex(
                name: "IX_Places_UrlHashCode",
                table: "Places");

            migrationBuilder.DropColumn(
                name: "Url",
                table: "Places");

            migrationBuilder.DropColumn(
                name: "UrlHashCode",
                table: "Places");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Url",
                table: "Places",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UrlHashCode",
                table: "Places",
                type: "int",
                nullable: true);

            //მისამართები ადგილებს უბრუნდება; გრაფის წიბოები Urls-ის იდენტიფიკატორებიდან ისევ PlaceId-ებზე გადადის —
            //წიბო, რომლის მისამართსაც ადგილი აღარ აქვს, ძველ სტრუქტურაში ვერ წარმოიდგინება და იშლება
            migrationBuilder.Sql("""
                UPDATE p
                SET p.Url = u.Url, p.UrlHashCode = u.UrlHashCode
                FROM dbo.Places p
                    JOIN dbo.Urls u ON u.UrlId = p.UrlId;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_Places_UrlHashCode",
                table: "Places",
                column: "UrlHashCode");

            migrationBuilder.DropForeignKey(
                name: "FK_Places_Urls_UrlId",
                table: "Places");

            migrationBuilder.DropForeignKey(
                name: "FK_UrlGraphNodes_Urls_FromUrlId",
                table: "UrlGraphNodes");

            migrationBuilder.DropForeignKey(
                name: "FK_UrlGraphNodes_Urls_GotUrlId",
                table: "UrlGraphNodes");

            migrationBuilder.Sql("""
                DELETE ugn
                FROM dbo.UrlGraphNodes ugn
                WHERE NOT EXISTS (SELECT 1 FROM dbo.Places p WHERE p.UrlId = ugn.FromUrlId)
                   OR NOT EXISTS (SELECT 1 FROM dbo.Places p WHERE p.UrlId = ugn.GotUrlId);
                UPDATE ugn
                SET ugn.FromUrlId = pf.PlaceId, ugn.GotUrlId = pg.PlaceId
                FROM dbo.UrlGraphNodes ugn
                    JOIN dbo.Places pf ON pf.UrlId = ugn.FromUrlId
                    JOIN dbo.Places pg ON pg.UrlId = ugn.GotUrlId;
                """);

            migrationBuilder.DropIndex(
                name: "IX_Places_UrlId",
                table: "Places");

            migrationBuilder.DropColumn(
                name: "UrlId",
                table: "Places");

            migrationBuilder.DropTable(
                name: "Urls");

            migrationBuilder.AddForeignKey(
                name: "FK_UrlGraphNodes_Places_FromUrlId",
                table: "UrlGraphNodes",
                column: "FromUrlId",
                principalTable: "Places",
                principalColumn: "PlaceId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UrlGraphNodes_Places_GotUrlId",
                table: "UrlGraphNodes",
                column: "GotUrlId",
                principalTable: "Places",
                principalColumn: "PlaceId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
