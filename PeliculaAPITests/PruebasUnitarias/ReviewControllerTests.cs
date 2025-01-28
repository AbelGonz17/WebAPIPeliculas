using Microsoft.AspNetCore.Mvc.Infrastructure;
using PeliculasAPI.Controllers;
using PeliculasAPI.DTOs;
using PeliculasAPI.Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeliculaAPITests.PruebasUnitarias
{
    [TestClass]
    public class ReviewControllerTests:BasePruebas
    {
        [TestMethod]
        public async Task UsuarioNoPuedeCrearDosReviewsParaLaMismaPelicula()
        {
            var nombreDb = Guid.NewGuid().ToString();
            var contexto = ConstruirContext(nombreDb);
            CrearPeliculas(nombreDb);

            var peliculaId = contexto.Pelicula.Select(x => x.Id).First();
            var review1 = new Review() 
            { 
                PeliculaId = peliculaId, 
                UsuarioId = usuarioPorDefectoId, 
                puntuacion = 5 
            }; 

            contexto.Add(review1);
            await contexto.SaveChangesAsync();

            var contexto2 = ConstruirContext(nombreDb);
            var mapper = ConstruirAutoMapper();

            var controller = new ReviewController(contexto2, mapper);
            controller.ControllerContext = ConstruirControllerContext();

            var reviewCreacionDto = new ReviewCreacionDTO { puntuacion = 5 };
            var respuesta = await controller.Post(peliculaId, reviewCreacionDto);

            var valor = respuesta as IStatusCodeActionResult;
            Assert.AreEqual(400,valor.StatusCode.Value);    
        }

        [TestMethod]
        public async Task UsuarioPuedeCrearReviewsParaLaPelicula()
        {
            var nombreDb = Guid.NewGuid().ToString();
            var contexto = ConstruirContext(nombreDb);
            CrearPeliculas(nombreDb);

            var peliculaId = contexto.Pelicula.Select(x => x.Id).First();
            var contexto2 = ConstruirContext(nombreDb);
            var mapper = ConstruirAutoMapper();

            var controller = new ReviewController(contexto2, mapper);
            controller.ControllerContext = ConstruirControllerContext();

            var reviewCreacionDto = new ReviewCreacionDTO { puntuacion = 5 };
            var respuesta = await controller.Post(peliculaId, reviewCreacionDto);

            var valor = respuesta as IStatusCodeActionResult;
            Assert.IsNotNull(valor);
            Assert.AreEqual(204, valor.StatusCode.Value);

            var contexto3 = ConstruirContext(nombreDb);
            var reviewDb = contexto3.Reviews.First();
            Assert.AreEqual(usuarioPorDefectoId, reviewDb.UsuarioId);

        }
    
        private void CrearPeliculas(string nombreDB)
        {
            var contexto = ConstruirContext(nombreDB);

            contexto.Pelicula.Add(new Pelicula() { Titulo = "pelicula 1" });

            contexto.SaveChanges();
        }
    }
}
