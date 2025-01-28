using Newtonsoft.Json;
using PeliculasAPI.DTOs;
using PeliculasAPI.Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeliculaAPITests.PruebasDeIntegracion
{
    [TestClass]
    public class ReviewControllerTest:BasePruebas
    {
        private static readonly string url = "/api/peliculas/1/reviews";

        [TestMethod]
        public async Task ObtenerReviewPeliculaInexistente()
        {
            var nombreDb = Guid.NewGuid().ToString();
            var factory = ConstruirWebApplicationFactory(nombreDb);
            var context = ConstruirContext(nombreDb);

            var cliente = factory.CreateClient();
            var respuesta = await cliente.GetAsync(url);

            Assert.AreEqual(404, (int)respuesta.StatusCode);
        }

        [TestMethod]
        public async Task ObtenerReviewDevuelveListadoVacio()
        {
            var nombreDb = Guid.NewGuid().ToString();
            var factory = ConstruirWebApplicationFactory(nombreDb);
            var context = ConstruirContext(nombreDb);
            context.Pelicula.Add(new Pelicula() { Titulo = "pelicula 1" });
            await context.SaveChangesAsync();

            var cliente = factory.CreateClient();
            var respuesta = await cliente.GetAsync(url);

            respuesta.EnsureSuccessStatusCode();
            var reviews = JsonConvert.DeserializeObject<List<ReviewDTO>>(await respuesta.Content.ReadAsStringAsync());  

            Assert.AreEqual(0, reviews.Count);
        }

    }
}
