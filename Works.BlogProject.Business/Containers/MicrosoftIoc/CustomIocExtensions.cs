﻿using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using Works.BlogProject.Business.Concrete;
using Works.BlogProject.Business.Interfaces;
using Works.BlogProject.DataAccess.Concrete.EntityFrameworkCore.Repositories;
using Works.BlogProject.DataAccess.Interfaces;

namespace Works.BlogProject.Business.Containers.MicrosoftIoc
{
    public static class CustomIocExtensions
    {
        public static void AddDependencies(this IServiceCollection services)
        {
            services.AddScoped(typeof(IGenericDal<>), typeof(EfGenericRepository<>));
            services.AddScoped(typeof(IGenericService<>), typeof(GenericManager<>));
        }
    }
}