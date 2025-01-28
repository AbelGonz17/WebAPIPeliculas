using Microsoft.EntityFrameworkCore;
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
    public class GeneroControllerTest: BasePruebas
    {
        private static readonly string url = "/api/generos";

        [TestMethod]
        public async Task ObtenerTodosLosGenerosListadoVacio()
        {
            var nombreDb = Guid.NewGuid().ToString();
            var factory = ConstruirWebApplicationFactory(nombreDb);

            var cliente = factory.CreateClient();
            var respuesta = await cliente.GetAsync(url);

            respuesta.EnsureSuccessStatusCode();

            var generos = JsonConvert.DeserializeObject<List<GeneroDTO>>(await respuesta.Content.ReadAsStringAsync());

            Assert.AreEqual(0,generos.Count());
        }

        [TestMethod]
        public async Task ObtenerTodosLosGeneros()
        {
            var nombreDb = Guid.NewGuid().ToString();
            var factory = ConstruirWebApplicationFactory(nombreDb);
            var contexto = ConstruirContext(nombreDb);
            contexto.generos.Add(new Genero() { Nombre = "Genero 1" });
            contexto.generos.Add(new Genero() { Nombre = "Genero 2" });
            await contexto.SaveChangesAsync();

            var cliente = factory.CreateClient();
            var respuesta = await cliente.GetAsync(url);

            respuesta.EnsureSuccessStatusCode();

            var generos = JsonConvert.DeserializeObject<List<GeneroDTO>>(await respuesta.Content.ReadAsStringAsync());

            Assert.AreEqual(2, generos.Count());
        }

        [TestMethod]
        public async Task BorrarGenero()
        {
            var nombreDb = Guid.NewGuid().ToString();
            var factory = ConstruirWebApplicationFactory(nombreDb);
            var contexto = ConstruirContext(nombreDb);
            contexto.generos.Add(new Genero() { Nombre = "Genero 1" });
            await contexto.SaveChangesAsync();

            var cliente = factory.CreateClient();
            var respuesta = await cliente.DeleteAsync($"{url}/1");
            respuesta.EnsureSuccessStatusCode();

            var contexto2 = ConstruirContext(nombreDb);
            var existe = await contexto2.generos.AnyAsync();

            Assert.IsFalse(existe);
        }


        [TestMethod]
        public async Task BorrarGeneroRetornaUn401()
        {
            var nombreDb = Guid.NewGuid().ToString();
            var factory = ConstruirWebApplicationFactory(nombreDb,false);
                     
            var cliente = factory.CreateClient();
            var respuesta = await cliente.DeleteAsync($"{url}/1");
            Assert.AreEqual("Unauthorized", respuesta.ReasonPhrase);          
        }
    }
}
