using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Options;
using NetTopologySuite;
using PeliculasAPI;
using PeliculasAPI.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace PeliculaAPITests
{
    public class BasePruebas
    {
        protected string usuarioPorDefectoId = "9722b56a-77ea-4e41-941d-e319b6eb3712";
        protected string usuarioPorDefectoEmail = "ejemplo@hotmail.com";

        protected ApplicationDbContext ConstruirContext(string nombreDB)
        {
            var opciones = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(nombreDB).Options;

            var dbcContext = new ApplicationDbContext(opciones);
            return dbcContext;
        }

        protected IMapper ConstruirAutoMapper()
        {
            var config = new MapperConfiguration(options =>
            {
                var geometryFactory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);
                options.AddProfile(new AutoMapperProfiles(geometryFactory));            
            });

            return config.CreateMapper();
        }

        protected ControllerContext ConstruirControllerContext()
        {
            var usuario = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
               new Claim (ClaimTypes.Name,usuarioPorDefectoEmail),
               new Claim(ClaimTypes.Email,usuarioPorDefectoEmail),
                 new Claim(ClaimTypes.NameIdentifier,usuarioPorDefectoId),
            }));

            return new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = usuario }
            };
        }

        protected WebApplicationFactory<Startup> ConstruirWebApplicationFactory(string nombreDB, bool ignorarSeguridad = true )
        {
            var factory = new WebApplicationFactory<Startup>();

            factory = factory.WithWebHostBuilder(builder =>
            {
                //Aquí puedes agregar, reemplazar o eliminar servicios que la aplicación normalmente usaría
                builder.ConfigureTestServices(services =>
                {

                    //quí buscamos si hay una configuración existente para
                    //el DbContext de la aplicación (en este caso, ApplicationDbContext).
                    var descriptorDbContext = services.SingleOrDefault(d => 
                    d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));

                    //eliminamos el servicio registrado de applicationDbContext y 
                    //creamos un nuevo servicio base de datos en memory
                    if (descriptorDbContext != null)
                    {
                        services.Remove(descriptorDbContext);
                    }

                    services.AddDbContext<ApplicationDbContext>(options => 
                    options.UseInMemoryDatabase(nombreDB)); 
                    
                    if(ignorarSeguridad)
                    {
                        services.AddSingleton<IAuthorizationHandler,AllowAnonymousHandler>();

                        services.AddControllers(options =>
                        {
                            options.Filters.Add(new UsuarioFalsoFiltro());
                        });                      
                    }
                });
            });         
            return factory;
        }
    }
}
