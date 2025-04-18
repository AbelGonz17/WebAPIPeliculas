﻿using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PeliculasAPI;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PeliculaAPITests
{
    [TestClass]
    public class LocalDbDataBaseInitializer
    {
        private static readonly string _dbName = "PruebasDeIntegracion";

        [AssemblyInitialize]
        public static void Initialize(TestContext testContext)
        {
            DeleteDB();
            CreateDB();
        }

        [AssemblyCleanup]
        public static void End()
        {
            DeleteDB();
        }
        

        public static ApplicationDbContext GetDbContextLocalDb(bool beginTransaction = true)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer($"Server=DESKTOP-4A6QGUJ;Database=PeliculasAPI;Integrated Security = true; TrustServerCertificate = True",
                x => x.UseNetTopologySuite())
                .Options;
            var context = new ApplicationDbContext(options);
            if (beginTransaction)
            {
                context.Database.BeginTransaction();
            }
            return context;
        }

        private static void CreateDB()
        {
            ExecuteCommand(Master, $@"
                CREATE DATABASE [{_dbName}]
                ON (NAME = '{_dbName}',
                FILENAME = '{Filename}')");

            using (var context = GetDbContextLocalDb(beginTransaction: false))
            {
                context.Database.Migrate();
                // Buen lugar para colocar data de prueba
                context.SaveChanges();
            }
        }

        static void DeleteDB()
        {
            var fileNames = GetDbFiles(Master, $@"
                SELECT [physical_name] FROM [sys].[master_files]
                WHERE [database_id] = DB_ID('{_dbName}')");

            if (fileNames.Any())
            {
                ExecuteCommand(Master, $@"
                    ALTER DATABASE [{_dbName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                    EXEC sp_detach_db '{_dbName}'");

                foreach (var filename in fileNames)
                {
                    File.Delete(filename);
                }
            }
        }

        static void ExecuteCommand(string connectionString, string query)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                using (var cmd = new SqlCommand(query, connection))
                {
                    connection.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        static IEnumerable<string> GetDbFiles(
           string connectionString,
           string query)
        {
            IEnumerable<string> files;
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (var cmd = new SqlCommand(query, connection))
                {
                    using (var da = new SqlDataAdapter(cmd))
                    {
                        var dt = new DataTable();
                        da.Fill(dt);
                        files = from DataRow myRow in dt.Rows
                                select (string)myRow["physical_name"];
                    }
                }
            }
            return files;
        }

        static string Master =>
           new SqlConnectionStringBuilder
           {
               DataSource = @"(LocalDB)\MSSQLLocalDB",
               InitialCatalog = "master",
               IntegratedSecurity = true
           }.ConnectionString;

        static string Filename => Path.Combine(
           Path.GetDirectoryName(
               typeof(LocalDbDataBaseInitializer).GetTypeInfo().Assembly.Location),
           $"{_dbName}.mdf");
    }
}

