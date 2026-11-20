using BookStore.Model.ConfigModels;
using BookStore.Model.EntityModels;
using Microsoft.EntityFrameworkCore;

namespace BookStore.DataAccess.Utilities
{
    internal static class SeedDataUtility
    {
        #region Main Extension Method

        public static void SeedData(this ModelBuilder modelBuilder)
        {
            // Note for Demo: The order here usually matters if you have strict foreign key constraints. 
            // Since Books depend on Publishers and Authors, we seed the parents first.
            modelBuilder.SeedPublishers();
            modelBuilder.SeedAuthors();
            modelBuilder.SeedBooks();
        }

        #endregion

        #region Entity Seed Methods

        private static void SeedPublishers(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Publisher>().HasData(
                new Publisher
                {
                    Id = 1,
                    Name = "Penguin Random House",
                    EstablishedYear = 2013, // Formed via the merger of Penguin and     Random  House
                    Headquarters = "New York City, New York, USA",
                    WebsiteUrl = "https://www.penguinrandomhouse.com",
                    Description = "The world's largest general interest paperback       publisher,    globally renowned for distributing mainstream     commercial    fiction,  technical literature, and award-winning     non-fiction across   multiple  historic imprints."
                },
                new Publisher
                {
                    Id = 2,
                    Name = "Harper Collins",
                    EstablishedYear = 1989, // Formed via the merger of Harper & Row    and    William    Collins
                    Headquarters = "New York City, New York, USA",
                    WebsiteUrl = "https://www.harpercollins.com",
                    Description = "One of the world's largest publishing companies and  part     of  the 'Big Five' English-language publishing houses,  specializing in  a     massive catalog spanning academic works,  reference materials,  children's    books, and global fiction."
                });
        }

        private static void SeedAuthors(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Author>().HasData(
                new Author
                {
                    Id = 1,
                    FirstName = "Jeff",
                    LastName = "Smith",
                    Email = "jeff.smith@example.com",
                    Expertise = "Software Architecture & Cloud Computing",
                    Biography = "A veteran systems engineer and cloud software  architect    with   over 20 years of experience designing fault- tolerant    distributed networks,    microservices infrastructure,   and enterprise  data pipelines.",
                    WebsiteUrl = "https://jeffsmith.dev"
                },
                new Author
                {
                    Id = 2,
                    FirstName = "Ray",
                    LastName = "Dalio",
                    Email = "ray.dalio@example.com",
                    Expertise = "Macroeconomics, Global Investing & Corporate   Principles",
                    Biography = "Famed institutional investor, macroeconomic theorist,  and      founder of Bridgewater Associates. He specializes in    financial  market    cycles, systemic debt crises, and algorithmic     corporate    management     frameworks.",
                    WebsiteUrl = "https://www.principles.com"
                },
                new Author
                {
                    Id = 3,
                    FirstName = "Simon",
                    LastName = "Sinek",
                    Email = "simon@example.com",
                    Expertise = "Organizational Leadership & Behavioral Culture",
                    Biography = "A trained ethnographer, motivational speaker, and      leadership   strategist. Best known for discovering the concept of  ' The Golden Circle'     and helping organizations foster    psychological    safety, trust, and long- term vision.",
                    WebsiteUrl = "https://simonsinek.com"
                },
                new Author
                {
                    Id = 4,
                    FirstName = "Malcolm",
                    LastName = "Gladwell",
                    Email = "malcolm@example.com",
                    Expertise = "Social Psychology, Sociology & Pop Culture Dynamics",
                    Biography = "An investigative journalist, staff writer for The New      Yorker,  and host of the Revisionist History podcast. He    synthesizes     complex  academic research in sociology and    psychology into compelling  narratives     about human behavior.",
                    WebsiteUrl = "https://www.gladwellbooks.com"
                },
                new Author
                {
                    Id = 5,
                    FirstName = "Brené",
                    LastName = "Brown",
                    Email = "brene@example.com",
                    Expertise = "Emotional Intelligence, Vulnerability & Courageous         Leadership",
                    Biography = "A research professor at the University of Houston who      spent    decades studying the nuances of empathy, shame,        vulnerability, and     resilience, translating those insights  into  actionable frameworks for   modern executive leaders.",
                    WebsiteUrl = "https://brenebrown.com"
                });
        }

        private static void SeedBooks(this ModelBuilder modelBuilder)
        {
            // Existing Seed Data
            modelBuilder.Entity<Book>().HasData(
                new Book
                {
                    Id = 1,
                    ISBN = "978-0134685991",
                    Price = 45.99m,
                    Title = "Clean Architecture: A Craftsman's Guide to Software    Structure and Design",
                    Description = "A comprehensive handbook on software design patterns,    architectural boundaries, and building decoupled, testable systems     for modern cloud applications.",
                    PublisherId = 1,
                    TenantId = TenantConstants.Amazon
                },
                new Book
                {
                    Id = 2,
                    ISBN = "978-0132350884",
                    Price = 42.50m,
                    Title = "Clean Code: A Handbook of Agile Software Craftsmanship",
                    Description = "An essential guide for software engineers detailing  best practices for writing readable, maintainable code,  eliminating technical debt, and implementing effective   refactoring.",
                    PublisherId = 1,
                    TenantId = TenantConstants.Amazon
                },

                // 1 Author Books
                new Book
                {
                    Id = 3,
                    ISBN = "978-0593157824",
                    Price = 16.99m,
                    Title = "Atomic Habits: An Easy & Proven Way to Build Good  Habits &     Break   Bad Ones",
                    Description = "A revolutionary framework for personal   development,      behavioral psychology, and daily routines   designed to achieve   microscopic,   compounding self-improvement     goals.",
                    PublisherId = 2,
                    TenantId = TenantConstants.Amazon
                },
                new Book
                {
                    Id = 4,
                    ISBN = "978-0062316097",
                    Price = 18.00m,
                    Title = "Sapiens: A Brief History of Humankind",
                    Description = "An expansive historical and anthropological study        exploring  how cognitive evolution, agricultural developments,  and     shared myths     shaped modern global civilizations.",
                    PublisherId = 1,
                    TenantId = TenantConstants.Amazon
                },
                new Book
                {
                    Id = 5,
                    ISBN = "978-0544003415",
                    Price = 25.00m,
                    Title = "The Lord of the Rings: The Fellowship of the Ring",
                    Description = "An epic high-fantasy masterpiece documenting a   perilous      mythical quest across Middle-earth to destroy a     powerful ancient    artifact    and defeat an dark dark lord.",
                    PublisherId = 2,
                    TenantId = TenantConstants.Amazon
                },
                new Book
                {
                    Id = 6,
                    ISBN = "978-0345391803",
                    Price = 15.99m,
                    Title = "The Hitchhiker's Guide to the Galaxy",
                    Description = "A satirical science fiction comedy adventure     featuring   an    ordinary human navigating space travel, alien     lifeforms,    bureaucracy, and  cosmic absurdities after Earth's    demolition.",
                    PublisherId = 1,
                    TenantId = TenantConstants.Amazon
                },

                // 2 Author Books
                new Book
                {
                    Id = 7,
                    ISBN = "978-1492056300",
                    Price = 49.99m,
                    Title = "Designing Data-Intensive Applications",
                    Description = "A deep technical analysis exploring the core     principles  of   storage engines, distributed database systems,     scalability, data    replication, and transaction processing.",
                    PublisherId = 2,
                    TenantId = TenantConstants.Crossword
                },
                new Book
                {
                    Id = 8,
                    ISBN = "978-0321356680",
                    Price = 55.00m,
                    Title = "Effective Java: Programming Language Guide",
                    Description = "An advanced developer manual offering    authoritative      architectural paradigms, memory optimization    techniques, and best       practices for robust object-oriented    software engineering.",
                    PublisherId = 1,
                    TenantId = TenantConstants.Crossword
                },
                new Book
                {
                    Id = 9,
                    ISBN = "978-1119363026",
                    Price = 35.50m,
                    Title = "Data Science for Business: What You Need to Know",
                    Description = "A practical introduction to data-driven  business         intelligence, machine learning algorithms,  predictive analytics,    and    extracting commercial value from     enterprise big data.",
                    PublisherId = 2,
                    TenantId = TenantConstants.Crossword
                },

                // 3 Author Books
                new Book
                {
                    Id = 10,
                    ISBN = "978-0201633610",
                    Price = 64.95m,
                    Title = "Design Patterns: Elements of Reusable Object-Oriented  Software",
                    Description = "The seminal software engineering text defining   standard      creational, structural, and behavioral design   patterns to solve     common   object-oriented software   architecture problems.",
                    PublisherId = 1,
                    TenantId = TenantConstants.Crossword
                },
                new Book
                {
                    Id = 11,
                    ISBN = "978-1492037255",
                    Price = 58.00m,
                    Title = "Kubernetes: Up and Running: Dive into the Future of        Infrastructure",
                    Description = "A hands-on DevOps guide to cloud-native cluster      management,  container orchestration, microservices deployment,     automation scales, and    modern system reliability engineering.",
                    PublisherId = 2,
                    TenantId = TenantConstants.Crossword
                },
                new Book
                {
                    Id = 12,
                    ISBN = "978-0135957059",
                    Price = 40.00m,
                    Title = "The Pragmatic Programmer: Your Journey to Mastery",
                    Description = "A collection of timeless advice, coding  philosophies,        personal discipline tips, and career guidance   for becoming a highly     efficient, professional software    development practitioner.",
                    PublisherId = 1,
                    TenantId = TenantConstants.Crossword
                });

            // Seed 1:1 Relationship data
            modelBuilder.Entity<BookDetails>().HasData(
                // 1: Clean Architecture
                new BookDetails { BookId = 1, NumberOfChapters = 34, NumberOfPages = 432, Format = "Paperback", Language = "English", ItemWeightInGrams = 610 },

                // 2: Clean Code
                new BookDetails { BookId = 2, NumberOfChapters = 17, NumberOfPages = 464, Format = "Paperback", Language = "English", ItemWeightInGrams = 794 },

                // 3: Atomic Habits
                new BookDetails { BookId = 3, NumberOfChapters = 20, NumberOfPages = 320, Format = "Hardcover", Language = "English", ItemWeightInGrams = 380 },

                // 4: Sapiens
                new BookDetails { BookId = 4, NumberOfChapters = 20, NumberOfPages = 464, Format = "Paperback", Language = "English", ItemWeightInGrams = 340 },

                // 5: The Lord of the Rings: Fellowship of the Ring
                new BookDetails { BookId = 5, NumberOfChapters = 22, NumberOfPages = 432, Format = "Mass Market Paperback", Language = "English", ItemWeightInGrams = 220 },

                // 6: The Hitchhiker's Guide to the Galaxy
                new BookDetails { BookId = 6, NumberOfChapters = 35, NumberOfPages = 224, Format = "Paperback", Language = "English", ItemWeightInGrams = 150 },

                // 7: Designing Data-Intensive Applications
                new BookDetails { BookId = 7, NumberOfChapters = 12, NumberOfPages = 616, Format = "Paperback", Language = "English", ItemWeightInGrams = 930 },

                // 8: Effective Java
                new BookDetails { BookId = 8, NumberOfChapters = 12, NumberOfPages = 416, Format = "Paperback", Language = "English", ItemWeightInGrams = 650 },

                // 9: Data Science for Business
                new BookDetails { BookId = 9, NumberOfChapters = 14, NumberOfPages = 414, Format = "Paperback", Language = "English", ItemWeightInGrams = 700 },

                // 10: Design Patterns (GoF)
                new BookDetails { BookId = 10, NumberOfChapters = 6, NumberOfPages = 395, Format = "Hardcover", Language = "English", ItemWeightInGrams = 810 },

                // 11: Kubernetes: Up and Running
                new BookDetails { BookId = 11, NumberOfChapters = 16, NumberOfPages = 278, Format = "Paperback", Language = "English", ItemWeightInGrams = 450 },

                // 12: The Pragmatic Programmer
                new BookDetails { BookId = 12, NumberOfChapters = 9, NumberOfPages = 352, Format = "Hardcover", Language = "English", ItemWeightInGrams = 600 }
            );

            // Seed M:M Relationship data (Join Table)
            modelBuilder.Entity<BookAuthorMaps>().HasData(
                // Original mappings - Book 1: Clean Architecture (Software & Leadership)
                new BookAuthorMaps { BookId = 1, AuthorId = 1, AuthorOrder = 1, ContributionRole = "Lead Author" }, // Jeff Smith (Software)
                new BookAuthorMaps { BookId = 1, AuthorId = 3, AuthorOrder = 2, ContributionRole = "Foreword" },    // Simon Sinek (Leadership   context)

                // Book 2: Clean Code (Software - Added for completeness)
                new BookAuthorMaps { BookId = 2, AuthorId = 1, AuthorOrder = 1, ContributionRole = "Primary Author" },

                // 1 Author Books
                // Book 3: Atomic Habits (Behavioral)
                new BookAuthorMaps { BookId = 3, AuthorId = 5, AuthorOrder = 1, ContributionRole = "Lead Author" }, // Brené Brown
                                                                                                                    //    Book     4:     Sapiens   (Sociology/   History)
                new BookAuthorMaps { BookId = 4, AuthorId = 4, AuthorOrder = 1, ContributionRole = "Lead Author" }, // Malcolm Gladwell
                                                                                                                    //    Book     5: The     Lord of   the Rings     (Fantasy)
                new BookAuthorMaps { BookId = 5, AuthorId = 2, AuthorOrder = 1, ContributionRole = "Primary Author" }, // Ray Dalio (Writing     fantasy in  his spare time!)
                                                                                                                       //   Book 6:    The  Hitchhiker's  Guide   (Satire)
                new BookAuthorMaps { BookId = 6, AuthorId = 4, AuthorOrder = 1, ContributionRole = "Primary Author" }, // Malcolm Gladwell

                // 2 Author Books
                // Book 7: Designing Data-Intensive Applications (Software & Economics)
                new BookAuthorMaps { BookId = 7, AuthorId = 1, AuthorOrder = 1, ContributionRole = "Lead Author" }, // Jeff Smith
                new BookAuthorMaps { BookId = 7, AuthorId = 2, AuthorOrder = 2, ContributionRole = "Co-Author" },   // Ray Dalio (Data logic)

                // Book 8: Effective Java (Software & Structure)
                new BookAuthorMaps { BookId = 8, AuthorId = 1, AuthorOrder = 1, ContributionRole = "Lead Author" }, // Jeff Smith
                new BookAuthorMaps { BookId = 8, AuthorId = 3, AuthorOrder = 2, ContributionRole = "Co-Author" },   // Simon Sinek

                // Book 9: Data Science for Business (Data & Behavior)
                new BookAuthorMaps { BookId = 9, AuthorId = 2, AuthorOrder = 1, ContributionRole = "Lead Author" }, // Ray Dalio
                new BookAuthorMaps { BookId = 9, AuthorId = 4, AuthorOrder = 2, ContributionRole = "Co-Author" },   // Malcolm Gladwell

                // 3 Author Books
                // Book 10: Design Patterns (Software Architecture)
                new BookAuthorMaps { BookId = 10, AuthorId = 1, AuthorOrder = 1, ContributionRole = "Lead Author" },
                new BookAuthorMaps { BookId = 10, AuthorId = 3, AuthorOrder = 2, ContributionRole = "Co-Author" },
                new BookAuthorMaps { BookId = 10, AuthorId = 5, AuthorOrder = 3, ContributionRole = "Technical Reviewer" },

                // Book 11: Kubernetes: Up and Running (Cloud DevOps)
                new BookAuthorMaps { BookId = 11, AuthorId = 1, AuthorOrder = 1, ContributionRole = "Lead Author" },
                new BookAuthorMaps { BookId = 11, AuthorId = 2, AuthorOrder = 2, ContributionRole = "Co-Author" },
                new BookAuthorMaps { BookId = 11, AuthorId = 4, AuthorOrder = 3, ContributionRole = "Editor" },

                // Book 12: The Pragmatic Programmer (Career & Software)
                new BookAuthorMaps { BookId = 12, AuthorId = 1, AuthorOrder = 1, ContributionRole = "Lead Author" },
                new BookAuthorMaps { BookId = 12, AuthorId = 3, AuthorOrder = 2, ContributionRole = "Co-Author" },
                new BookAuthorMaps { BookId = 12, AuthorId = 5, AuthorOrder = 3, ContributionRole = "Foreword" });
        }

        #endregion

        #region SQL Stored Procedures (For Migrations)

        // You can use migrationBuilder.Sql() in an empty migration to create this SP in the DB.
        // DROP PROCEDURE [dbo].[sp_GetAllBooks]
        // migrationBuilder.Sql(SeedDataUtility.sp_GetAllBooks);
        // Logic to remove the procedure if the migration is rolled back:
        // migrationBuilder.Sql("DROP PROCEDURE IF EXISTS [dbo].[sp_GetAllBooks]");

        public const string sp_GetAllBooks = @"
                CREATE OR ALTER PROCEDURE [dbo].[sp_GetAllBooks]
                    @TenantId UNIQUEIDENTIFIER
                AS
                BEGIN
                SET NOCOUNT ON;

                SELECT 
                    [Id], 
                    [Title], 
                    [ISBN], 
                    [Price], 
                    [Description], 
                    [PublisherId], 
                    [CreatedOn], 
                    [UpdatedOn], 
                    [CreatedBy], 
                    [IsDeleted], 
                    [DeletedOn],
                    [TenantId] -- This MUST be selected to map to the Book entity
                FROM [Books]
                WHERE [IsDeleted] = 0 
                    AND [TenantId] = @TenantId;
                END";

        #endregion
    }
}