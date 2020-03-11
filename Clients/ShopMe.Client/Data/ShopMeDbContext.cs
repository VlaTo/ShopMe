using Microsoft.EntityFrameworkCore;
using ShopMe.Application.Models;
using System;
using System.IO;
using Xamarin.Forms;

namespace ShopMe.Client.Data
{
    internal sealed class ShopMeDbContext : DbContext
    {
        private const string databaseName = "shopme.db";

        public DbSet<Models.ShopList> ShopLists
        {
            get; 
            set;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var databasePath = String.Empty;
            //base.OnConfiguring(optionsBuilder);

            switch (Device.RuntimePlatform)
            {
                case Device.iOS:
                {
                    SQLitePCL.Batteries_V2.Init();
                    databasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "..", "Library", databaseName);

                    break;
                }

                case Device.Android:
                {
                    databasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), databaseName);

                    break;
                }

                case Device.UWP:
                {
                    databasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), databaseName);

                    break;
                }

                default:
                {
                    throw new NotImplementedException();
                }
            }

            optionsBuilder.UseSqlite($"Filename={databasePath}");
        }
    }
}