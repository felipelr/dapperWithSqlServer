using System;
using System.Linq;
using Microsoft.Data.SqlClient;
using Dapper;
using AdoNetDataAccess.Models;
using System.Data;
using System.Collections.Generic;

namespace AdoNetDataAccess
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Clear();
            const string connectionString = "Server=localhost,1433;Database=balta;User ID=sa;Password=FelipeLima123";

            using (var connection = new SqlConnection(connectionString))
            {
                Console.WriteLine("Conectado");

                //CreateManyCategories(connection);
                //ListCategories(connection);
                //CreateCategory(connection);
                //UpdateCategory(connection);
                //ExecuteProcedure(connection);
                //ExecuteReadProcedure(connection);
                //ExecuteScalar(connection);
                //OneToOne(connection);
                //OneToMany(connection);
                //QueryMultiple(connection);
                //SelectIn(connection);
                //Like(connection);
                Transaction(connection);
            }

        }

        static void ListCategories(SqlConnection connection)
        {
            var categories = connection.Query<Category>("SELECT [Id], [Title] FROM Category ORDER BY [Order]");
            foreach (var item in categories)
            {
                Console.WriteLine($"{item.Id} = {item.Title}");
            }
        }
        static void CreateCategory(SqlConnection connection)
        {
            Category category = new Category();
            category.Id = Guid.NewGuid();
            category.Title = "Amazon AWS";
            category.Url = "amazon";
            category.Summary = "Amazon Cloud";
            category.Description = "Categoria destinada a serviços do AWS";
            category.Order = 8;
            category.Featured = false;

            var insertSql = @"INSERT INTO [Category] VALUES (@Id, @Title, @Url, @Summary, @Order, @Description, @Featured)";

            var rows = connection.Execute(insertSql, new
            {
                category.Id,
                category.Title,
                category.Url,
                category.Summary,
                category.Order,
                category.Description,
                category.Featured,
            });

            Console.WriteLine($"{rows} - Linhas inseridas");
        }
        static void UpdateCategory(SqlConnection connection)
        {
            var updateQuery = @"UPDATE Category SET [Title] = @Title WHERE Id = @Id";

            var rows = connection.Execute(updateQuery, new
            {
                Id = new Guid("af3407aa-11ae-4621-a2ef-2028b85507c4"),
                Title = "Frontend 2021",
            });

            Console.WriteLine($"{rows} - registros atualizados");
        }
        static void CreateManyCategories(SqlConnection connection)
        {
            Category category = new Category();
            category.Id = Guid.NewGuid();
            category.Title = "Amazon AWS";
            category.Url = "amazon";
            category.Summary = "Amazon Cloud";
            category.Description = "Categoria destinada a serviços do AWS";
            category.Order = 8;
            category.Featured = false;

            Category category2 = new Category();
            category2.Id = Guid.NewGuid();
            category2.Title = "Amazon AWS 2";
            category2.Url = "amazon-2";
            category2.Summary = "Amazon Cloud 2";
            category2.Description = "Categoria destinada a serviços do AWS 2";
            category2.Order = 9;
            category2.Featured = true;

            var insertSql = @"INSERT INTO [Category] VALUES (@Id, @Title, @Url, @Summary, @Order, @Description, @Featured)";

            var rows = connection.Execute(insertSql, new[]
            {
                    new
                    {
                        category.Id,
                        category.Title,
                        category.Url,
                        category.Summary,
                        category.Order,
                        category.Description,
                        category.Featured,
                    },
                    new
                    {
                        category2.Id,
                        category2.Title,
                        category2.Url,
                        category2.Summary,
                        category2.Order,
                        category2.Description,
                        category2.Featured,
                    }
                });

            Console.WriteLine($"{rows} - Linhas inseridas");
        }
        static void ExecuteProcedure(SqlConnection connection)
        {
            var produre = "[spDeleteStudent]";
            var parms = new { StudentId = "1aacd544-722c-4fda-89fa-85570bd90bab" };

            var rows = connection.Execute(produre, parms, commandType: CommandType.StoredProcedure);

            Console.WriteLine($"{rows} - linhas afetadas");
        }
        static void ExecuteReadProcedure(SqlConnection connection)
        {
            var produre = "[spGetCoursesByCategory]";
            var parms = new { CategoryId = "af3407aa-11ae-4621-a2ef-2028b85507c4" };

            var courses = connection.Query(produre, parms, commandType: CommandType.StoredProcedure);

            foreach (var item in courses)
            {
                Console.WriteLine($"{item.Id} = {item.Title}");
            }
        }
        static void ExecuteScalar(SqlConnection connection)
        {
            Category category = new Category();
            category.Title = "Amazon AWS";
            category.Url = "amazon";
            category.Summary = "Amazon Cloud";
            category.Description = "Categoria destinada a serviços do AWS";
            category.Order = 8;
            category.Featured = false;

            var insertSql = @"INSERT INTO [Category] OUTPUT inserted.[Id] VALUES (NEWID(), @Title, @Url, @Summary, @Order, @Description, @Featured)";

            var id = connection.ExecuteScalar<Guid>(insertSql, new
            {
                category.Title,
                category.Url,
                category.Summary,
                category.Order,
                category.Description,
                category.Featured,
            });

            Console.WriteLine($"Categoria inserida {id}");
        }
        static void OneToOne(SqlConnection connection)
        {
            var sql = @"SELECT * FROM [CareerItem] INNER JOIN [Course] ON [CareerItem].[CourseId] = [Course].[Id]";

            var items = connection.Query<CareerItem, Course, CareerItem>(sql, (careerItem, course) =>
            {
                careerItem.Course = course;
                return careerItem;
            },
            splitOn: "Id");

            foreach (var item in items)
            {
                Console.WriteLine($"{item.Title} - {item.Course.Title}");
            }
        }
        static void OneToMany(SqlConnection connection)
        {
            var sql = @"SELECT 
                [Career].[Id],
                [Career].[Title],
                [CareerItem].[CareerId],
                [CareerItem].[Title]
            FROM 
                [Career] 
            INNER JOIN 
                [CareerItem] ON [CareerItem].[CareerId] = [Career].[Id]
            ORDER BY
                [Career].[Title]";

            var careers = new List<Career>();
            var items = connection.Query<Career, CareerItem, Career>(sql, (career, careerItem) =>
            {
                var car = careers.Where(x => x.Id == career.Id).FirstOrDefault();
                if (car == null)
                {
                    car = new Career();
                    car.Id = career.Id;
                    car.Title = career.Title;
                    car.Items.Add(careerItem);
                    careers.Add(car);
                }
                else
                {
                    car.Items.Add(careerItem);
                }
                return car;
            },
            splitOn: "CareerId");

            foreach (var career in careers)
            {
                Console.WriteLine($"{career.Title}");

                foreach (var item in career.Items)
                {
                    Console.WriteLine($" - {item.Title}");
                }
            }
        }
        static void QueryMultiple(SqlConnection connection)
        {
            var query = @"SELECT * FROM [Category]; SELECT * FROM [Course];";

            using (var multi = connection.QueryMultiple(query))
            {
                var categories = multi.Read<Category>();
                var courses = multi.Read<Course>();

                foreach (var item in categories)
                {
                    Console.WriteLine(item.Title);
                }

                foreach (var item in courses)
                {
                    Console.WriteLine(item.Title);
                }
            }
        }
        static void SelectIn(SqlConnection connection)
        {
            var query = @"SELECT * FROM [Career] WHERE [Id] IN @Id";

            var items = connection.Query<Career>(query, new
            {
                Id = new[] { "4327ac7e-963b-4893-9f31-9a3b28a4e72b", "4327ac7e-963b-4893-9f31-9a3b28a4e72b" }
            });

            foreach (var item in items)
            {
                Console.WriteLine(item.Title);
            }
        }
        static void Like(SqlConnection connection)
        {
            var term = "api";
            var query = @"SELECT * FROM [Course] WHERE [Title] LIKE @exp";

            var items = connection.Query<Course>(query, new
            {
                exp = $"%{term}%"
            });

            foreach (var item in items)
            {
                Console.WriteLine(item.Title);
            }
        }
        static void Transaction(SqlConnection connection)
        {
            Category category = new Category();
            category.Id = Guid.NewGuid();
            category.Title = "Amazon AWS";
            category.Url = "amazon";
            category.Summary = "Amazon Cloud";
            category.Description = "Nao salvar";
            category.Order = 8;
            category.Featured = false;

            var insertSql = @"INSERT INTO [Category] VALUES (@Id, @Title, @Url, @Summary, @Order, @Description, @Featured)";

            connection.Open();

            using (SqlTransaction transaction = connection.BeginTransaction())
            {
                var rows = connection.Execute(insertSql, new
                {
                    category.Id,
                    category.Title,
                    category.Url,
                    category.Summary,
                    category.Order,
                    category.Description,
                    category.Featured,
                },
                transaction);

                transaction.Commit();
                //transaction.Rollback();

                Console.WriteLine($"{rows} - Linhas inseridas");
            }
        }
    }
}
