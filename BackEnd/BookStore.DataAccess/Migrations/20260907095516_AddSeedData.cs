using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace BookStore.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddSeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Authors",
                columns: new[] { "Id", "Biography", "Email", "Expertise", "FirstName", "LastName", "WebsiteUrl" },
                values: new object[,]
                {
                    { 1, "A veteran systems engineer and cloud software  architect    with   over 20 years of experience designing fault- tolerant    distributed networks,    microservices infrastructure,   and enterprise  data pipelines.", "jeff.smith@example.com", "Software Architecture & Cloud Computing", "Jeff", "Smith", "https://jeffsmith.dev" },
                    { 2, "Famed institutional investor, macroeconomic theorist,  and      founder of Bridgewater Associates. He specializes in    financial  market    cycles, systemic debt crises, and algorithmic     corporate    management     frameworks.", "ray.dalio@example.com", "Macroeconomics, Global Investing & Corporate   Principles", "Ray", "Dalio", "https://www.principles.com" },
                    { 3, "A trained ethnographer, motivational speaker, and      leadership   strategist. Best known for discovering the concept of  ' The Golden Circle'     and helping organizations foster    psychological    safety, trust, and long- term vision.", "simon@example.com", "Organizational Leadership & Behavioral Culture", "Simon", "Sinek", "https://simonsinek.com" },
                    { 4, "An investigative journalist, staff writer for The New      Yorker,  and host of the Revisionist History podcast. He    synthesizes     complex  academic research in sociology and    psychology into compelling  narratives     about human behavior.", "malcolm@example.com", "Social Psychology, Sociology & Pop Culture Dynamics", "Malcolm", "Gladwell", "https://www.gladwellbooks.com" },
                    { 5, "A research professor at the University of Houston who      spent    decades studying the nuances of empathy, shame,        vulnerability, and     resilience, translating those insights  into  actionable frameworks for   modern executive leaders.", "brene@example.com", "Emotional Intelligence, Vulnerability & Courageous         Leadership", "Brené", "Brown", "https://brenebrown.com" }
                });

            migrationBuilder.InsertData(
                table: "Publishers",
                columns: new[] { "Id", "Description", "EstablishedYear", "Headquarters", "Name", "WebsiteUrl" },
                values: new object[,]
                {
                    { 1, "The world's largest general interest paperback       publisher,    globally renowned for distributing mainstream     commercial    fiction,  technical literature, and award-winning     non-fiction across   multiple  historic imprints.", 2013, "New York City, New York, USA", "Penguin Random House", "https://www.penguinrandomhouse.com" },
                    { 2, "One of the world's largest publishing companies and  part     of  the 'Big Five' English-language publishing houses,  specializing in  a     massive catalog spanning academic works,  reference materials,  children's    books, and global fiction.", 1989, "New York City, New York, USA", "Harper Collins", "https://www.harpercollins.com" }
                });

            migrationBuilder.InsertData(
                table: "Books",
                columns: new[] { "Id", "CreatedBy", "CustomCoverColor", "DeletedOn", "Description", "ISBN", "IsDeleted", "Price", "PromoCodeId", "PublisherId", "SearchVector", "TenantId", "Title" },
                values: new object[,]
                {
                    { 1, null, null, null, "A comprehensive handbook on software design patterns,    architectural boundaries, and building decoupled, testable systems     for modern cloud applications.", "978-0134685991", false, 45.99m, null, 1, null, new Guid("226dc44f-5a83-4d39-901e-87f7a790197b"), "Clean Architecture: A Craftsman's Guide to Software    Structure and Design" },
                    { 2, null, null, null, "An essential guide for software engineers detailing  best practices for writing readable, maintainable code,  eliminating technical debt, and implementing effective   refactoring.", "978-0132350884", false, 42.50m, null, 1, null, new Guid("226dc44f-5a83-4d39-901e-87f7a790197b"), "Clean Code: A Handbook of Agile Software Craftsmanship" },
                    { 3, null, null, null, "A revolutionary framework for personal   development,      behavioral psychology, and daily routines   designed to achieve   microscopic,   compounding self-improvement     goals.", "978-0593157824", false, 16.99m, null, 2, null, new Guid("226dc44f-5a83-4d39-901e-87f7a790197b"), "Atomic Habits: An Easy & Proven Way to Build Good  Habits &     Break   Bad Ones" },
                    { 4, null, null, null, "An expansive historical and anthropological study        exploring  how cognitive evolution, agricultural developments,  and     shared myths     shaped modern global civilizations.", "978-0062316097", false, 18.00m, null, 1, null, new Guid("226dc44f-5a83-4d39-901e-87f7a790197b"), "Sapiens: A Brief History of Humankind" },
                    { 5, null, null, null, "An epic high-fantasy masterpiece documenting a   perilous      mythical quest across Middle-earth to destroy a     powerful ancient    artifact    and defeat an dark dark lord.", "978-0544003415", false, 25.00m, null, 2, null, new Guid("226dc44f-5a83-4d39-901e-87f7a790197b"), "The Lord of the Rings: The Fellowship of the Ring" },
                    { 6, null, null, null, "A satirical science fiction comedy adventure     featuring   an    ordinary human navigating space travel, alien     lifeforms,    bureaucracy, and  cosmic absurdities after Earth's    demolition.", "978-0345391803", false, 15.99m, null, 1, null, new Guid("226dc44f-5a83-4d39-901e-87f7a790197b"), "The Hitchhiker's Guide to the Galaxy" },
                    { 7, null, null, null, "A deep technical analysis exploring the core     principles  of   storage engines, distributed database systems,     scalability, data    replication, and transaction processing.", "978-1492056300", false, 49.99m, null, 2, null, new Guid("864d14e2-615c-4d8f-85b6-24b521dd4bd9"), "Designing Data-Intensive Applications" },
                    { 8, null, null, null, "An advanced developer manual offering    authoritative      architectural paradigms, memory optimization    techniques, and best       practices for robust object-oriented    software engineering.", "978-0321356680", false, 55.00m, null, 1, null, new Guid("864d14e2-615c-4d8f-85b6-24b521dd4bd9"), "Effective Java: Programming Language Guide" },
                    { 9, null, null, null, "A practical introduction to data-driven  business         intelligence, machine learning algorithms,  predictive analytics,    and    extracting commercial value from     enterprise big data.", "978-1119363026", false, 35.50m, null, 2, null, new Guid("864d14e2-615c-4d8f-85b6-24b521dd4bd9"), "Data Science for Business: What You Need to Know" },
                    { 10, null, null, null, "The seminal software engineering text defining   standard      creational, structural, and behavioral design   patterns to solve     common   object-oriented software   architecture problems.", "978-0201633610", false, 64.95m, null, 1, null, new Guid("864d14e2-615c-4d8f-85b6-24b521dd4bd9"), "Design Patterns: Elements of Reusable Object-Oriented  Software" },
                    { 11, null, null, null, "A hands-on DevOps guide to cloud-native cluster      management,  container orchestration, microservices deployment,     automation scales, and    modern system reliability engineering.", "978-1492037255", false, 58.00m, null, 2, null, new Guid("864d14e2-615c-4d8f-85b6-24b521dd4bd9"), "Kubernetes: Up and Running: Dive into the Future of        Infrastructure" },
                    { 12, null, null, null, "A collection of timeless advice, coding  philosophies,        personal discipline tips, and career guidance   for becoming a highly     efficient, professional software    development practitioner.", "978-0135957059", false, 40.00m, null, 1, null, new Guid("864d14e2-615c-4d8f-85b6-24b521dd4bd9"), "The Pragmatic Programmer: Your Journey to Mastery" }
                });

            migrationBuilder.InsertData(
                table: "BookAuthorMaps",
                columns: new[] { "AuthorId", "BookId", "AuthorOrder", "ContributionRole", "Description" },
                values: new object[,]
                {
                    { 1, 1, 1, "Lead Author", null },
                    { 3, 1, 2, "Foreword", null },
                    { 1, 2, 1, "Primary Author", null },
                    { 5, 3, 1, "Lead Author", null },
                    { 4, 4, 1, "Lead Author", null },
                    { 2, 5, 1, "Primary Author", null },
                    { 4, 6, 1, "Primary Author", null },
                    { 1, 7, 1, "Lead Author", null },
                    { 2, 7, 2, "Co-Author", null },
                    { 1, 8, 1, "Lead Author", null },
                    { 3, 8, 2, "Co-Author", null },
                    { 2, 9, 1, "Lead Author", null },
                    { 4, 9, 2, "Co-Author", null },
                    { 1, 10, 1, "Lead Author", null },
                    { 3, 10, 2, "Co-Author", null },
                    { 5, 10, 3, "Technical Reviewer", null },
                    { 1, 11, 1, "Lead Author", null },
                    { 2, 11, 2, "Co-Author", null },
                    { 4, 11, 3, "Editor", null },
                    { 1, 12, 1, "Lead Author", null },
                    { 3, 12, 2, "Co-Author", null },
                    { 5, 12, 3, "Foreword", null }
                });

            migrationBuilder.InsertData(
                table: "BookDetails",
                columns: new[] { "BookId", "Format", "ItemWeightInGrams", "Language", "NumberOfChapters", "NumberOfPages" },
                values: new object[,]
                {
                    { 1, "Paperback", 610, "English", 34, 432 },
                    { 2, "Paperback", 794, "English", 17, 464 },
                    { 3, "Hardcover", 380, "English", 20, 320 },
                    { 4, "Paperback", 340, "English", 20, 464 },
                    { 5, "Mass Market Paperback", 220, "English", 22, 432 },
                    { 6, "Paperback", 150, "English", 35, 224 },
                    { 7, "Paperback", 930, "English", 12, 616 },
                    { 8, "Paperback", 650, "English", 12, 416 },
                    { 9, "Paperback", 700, "English", 14, 414 },
                    { 10, "Hardcover", 810, "English", 6, 395 },
                    { 11, "Paperback", 450, "English", 16, 278 },
                    { 12, "Hardcover", 600, "English", 9, 352 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Books_TenantId",
                table: "Books",
                column: "TenantId",
                filter: "[IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Books_TenantId",
                table: "Books");

            migrationBuilder.DeleteData(
                table: "BookAuthorMaps",
                keyColumns: new[] { "AuthorId", "BookId" },
                keyValues: new object[] { 1, 1 });

            migrationBuilder.DeleteData(
                table: "BookAuthorMaps",
                keyColumns: new[] { "AuthorId", "BookId" },
                keyValues: new object[] { 3, 1 });

            migrationBuilder.DeleteData(
                table: "BookAuthorMaps",
                keyColumns: new[] { "AuthorId", "BookId" },
                keyValues: new object[] { 1, 2 });

            migrationBuilder.DeleteData(
                table: "BookAuthorMaps",
                keyColumns: new[] { "AuthorId", "BookId" },
                keyValues: new object[] { 5, 3 });

            migrationBuilder.DeleteData(
                table: "BookAuthorMaps",
                keyColumns: new[] { "AuthorId", "BookId" },
                keyValues: new object[] { 4, 4 });

            migrationBuilder.DeleteData(
                table: "BookAuthorMaps",
                keyColumns: new[] { "AuthorId", "BookId" },
                keyValues: new object[] { 2, 5 });

            migrationBuilder.DeleteData(
                table: "BookAuthorMaps",
                keyColumns: new[] { "AuthorId", "BookId" },
                keyValues: new object[] { 4, 6 });

            migrationBuilder.DeleteData(
                table: "BookAuthorMaps",
                keyColumns: new[] { "AuthorId", "BookId" },
                keyValues: new object[] { 1, 7 });

            migrationBuilder.DeleteData(
                table: "BookAuthorMaps",
                keyColumns: new[] { "AuthorId", "BookId" },
                keyValues: new object[] { 2, 7 });

            migrationBuilder.DeleteData(
                table: "BookAuthorMaps",
                keyColumns: new[] { "AuthorId", "BookId" },
                keyValues: new object[] { 1, 8 });

            migrationBuilder.DeleteData(
                table: "BookAuthorMaps",
                keyColumns: new[] { "AuthorId", "BookId" },
                keyValues: new object[] { 3, 8 });

            migrationBuilder.DeleteData(
                table: "BookAuthorMaps",
                keyColumns: new[] { "AuthorId", "BookId" },
                keyValues: new object[] { 2, 9 });

            migrationBuilder.DeleteData(
                table: "BookAuthorMaps",
                keyColumns: new[] { "AuthorId", "BookId" },
                keyValues: new object[] { 4, 9 });

            migrationBuilder.DeleteData(
                table: "BookAuthorMaps",
                keyColumns: new[] { "AuthorId", "BookId" },
                keyValues: new object[] { 1, 10 });

            migrationBuilder.DeleteData(
                table: "BookAuthorMaps",
                keyColumns: new[] { "AuthorId", "BookId" },
                keyValues: new object[] { 3, 10 });

            migrationBuilder.DeleteData(
                table: "BookAuthorMaps",
                keyColumns: new[] { "AuthorId", "BookId" },
                keyValues: new object[] { 5, 10 });

            migrationBuilder.DeleteData(
                table: "BookAuthorMaps",
                keyColumns: new[] { "AuthorId", "BookId" },
                keyValues: new object[] { 1, 11 });

            migrationBuilder.DeleteData(
                table: "BookAuthorMaps",
                keyColumns: new[] { "AuthorId", "BookId" },
                keyValues: new object[] { 2, 11 });

            migrationBuilder.DeleteData(
                table: "BookAuthorMaps",
                keyColumns: new[] { "AuthorId", "BookId" },
                keyValues: new object[] { 4, 11 });

            migrationBuilder.DeleteData(
                table: "BookAuthorMaps",
                keyColumns: new[] { "AuthorId", "BookId" },
                keyValues: new object[] { 1, 12 });

            migrationBuilder.DeleteData(
                table: "BookAuthorMaps",
                keyColumns: new[] { "AuthorId", "BookId" },
                keyValues: new object[] { 3, 12 });

            migrationBuilder.DeleteData(
                table: "BookAuthorMaps",
                keyColumns: new[] { "AuthorId", "BookId" },
                keyValues: new object[] { 5, 12 });

            migrationBuilder.DeleteData(
                table: "BookDetails",
                keyColumn: "BookId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "BookDetails",
                keyColumn: "BookId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "BookDetails",
                keyColumn: "BookId",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "BookDetails",
                keyColumn: "BookId",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "BookDetails",
                keyColumn: "BookId",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "BookDetails",
                keyColumn: "BookId",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "BookDetails",
                keyColumn: "BookId",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "BookDetails",
                keyColumn: "BookId",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "BookDetails",
                keyColumn: "BookId",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "BookDetails",
                keyColumn: "BookId",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "BookDetails",
                keyColumn: "BookId",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "BookDetails",
                keyColumn: "BookId",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "Authors",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Authors",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Authors",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Authors",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Authors",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "Publishers",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Publishers",
                keyColumn: "Id",
                keyValue: 2);
        }
    }
}
