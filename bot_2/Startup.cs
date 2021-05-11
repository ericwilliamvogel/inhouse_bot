using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using db;
using Microsoft.EntityFrameworkCore;

namespace bot_2
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<Context>(options => {
                options.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=Context;Trusted_Connection=true;MultipleActiveResultSets=true",
                    x => x.MigrationsAssembly("Migrations"));
            });
            var serviceProvider = services.BuildServiceProvider();
            var bot = new Bot(serviceProvider);
            services.AddSingleton(bot);
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {

        }
    }
}
