using Castle.Core.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;
using Microsoft.Extensions.Logging;
using Moq;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Noding;
using PeliculasAPI.Controllers;
using PeliculasAPI.DTOs;
using PeliculasAPI.Entidades;
using PeliculasAPI.Migrations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeliculaAPITests.PruebasUnitarias
{
    [TestClass]
    public class PeliculasControllerTests: BasePruebas
    {
        [TestMethod]
        public async Task FiltrarPorTitulo()
        {
            var nombredb = CrearDataPrueba();
            var mapper = ConstruirAutoMapper();
            var contexto = ConstruirContext(nombredb);

            var controller = new PeliculasController(contexto, mapper, null,null);
            controller.ControllerContext.HttpContext = new DefaultHttpContext();

            var tituloPelicula = "Pelicula 1";

            var filtroDto = new FiltroPeliculaDTO()
            {
                Titulo = tituloPelicula,
                RecordPorPagina = 10
            };

            var respuesta = await controller.Filtrar(filtroDto);
            var peliculas = respuesta.Value;
            Assert.AreEqual(1, peliculas.Count);
            Assert.AreEqual(tituloPelicula, peliculas[0].Titulo);
        }

        [TestMethod]
        public async Task EnCines()
        {
            var nombredb = CrearDataPrueba();
            var mapper = ConstruirAutoMapper();
            var contexto = ConstruirContext(nombredb);

            var controller = new PeliculasController(contexto, mapper, null, null);
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
           
            var filtroDto = new FiltroPeliculaDTO()
            {               
                EnCines = true                
            };

            var respuesta = await controller.Filtrar(filtroDto);
            var peliculas = respuesta.Value;
            Assert.AreEqual(1, peliculas.Count);
            Assert.AreEqual("Película en Cines", peliculas[0].Titulo);
        }
        [TestMethod]
        public async Task FiltrarProximosEstrenos()
        {
            var nombreBD = CrearDataPrueba();
            var mapper = ConstruirAutoMapper();
            var contexto = ConstruirContext(nombreBD);

            var controller = new PeliculasController(contexto, mapper, null, null);
            controller.ControllerContext.HttpContext = new DefaultHttpContext();

            var filtroDTO = new FiltroPeliculaDTO()
            {
                ProximosEstrenos = true
            };

            var respuesta = await controller.Filtrar(filtroDTO);
            var peliculas = respuesta.Value;
            Assert.AreEqual(1, peliculas.Count);
            Assert.AreEqual("No estrenada", peliculas[0].Titulo);
        }

        [TestMethod]
        public async Task FiltrarPorGenero()
        {
            var nombreBD = CrearDataPrueba();
            var mapper = ConstruirAutoMapper();
            var contexto = ConstruirContext(nombreBD);

            var controller = new PeliculasController(contexto, mapper, null, null);
            controller.ControllerContext.HttpContext = new DefaultHttpContext();

            var generoid = contexto.generos.Select(x => x.Id).First();

            var filtroDTO = new FiltroPeliculaDTO()
            {
                GeneroId = generoid
            };

            var respuesta = await controller.Filtrar(filtroDTO);
            var peliculas = respuesta.Value;

            Assert.AreEqual(1, peliculas.Count);
            Assert.AreEqual("Película con Género", peliculas[0].Titulo);

        }

        [TestMethod]
        public async Task FiltrarOrdenaTituloAscedente()
        {
            var nombreBD = CrearDataPrueba();
            var mapper = ConstruirAutoMapper();
            var contexto = ConstruirContext(nombreBD);

            var controller = new PeliculasController(contexto, mapper, null, null);
            controller.ControllerContext.HttpContext = new DefaultHttpContext();

            var filtroDTO = new FiltroPeliculaDTO()
            {
                CampoOrdenar = "titulo",
                OrdenAscendente = true
            };

            var respuesta = await controller.Filtrar(filtroDTO);
            var peliculas = respuesta.Value;

            var contexto2 = ConstruirContext(nombreBD);
            var peliculasDB = contexto2.Pelicula.OrderBy(x => x.Titulo).ToList();

            Assert.AreEqual(peliculasDB.Count,peliculas.Count);

            for (int i = 0; i< peliculasDB.Count; i++)
            {
                var peliculaDelControlador = peliculas[i];
                var peliculaDB = peliculasDB[i];

                Assert.AreEqual(peliculaDB.Id, peliculaDelControlador.Id);
            }
        }

        [TestMethod]
        public async Task FiltrarOrdenTituloDescendente()
        {
            var nombreBD = CrearDataPrueba();
            var mapper = ConstruirAutoMapper();
            var contexto = ConstruirContext(nombreBD);

            var controller = new PeliculasController(contexto, mapper, null, null);
            controller.ControllerContext.HttpContext = new DefaultHttpContext();

            var filtroDTO = new FiltroPeliculaDTO()
            {
                CampoOrdenar = "titulo",
                OrdenAscendente = false
            };

            var respuesta = await controller.Filtrar(filtroDTO);
            var peliculas = respuesta.Value;

            var contexto2 = ConstruirContext(nombreBD);
            var peliculasDB = contexto2.Pelicula.OrderByDescending(x => x.Titulo).ToList();

            Assert.AreEqual(peliculasDB.Count, peliculas.Count);

            for (int i = 0; i < peliculasDB.Count; i++)
            {
                var peliculaDelControlador = peliculas[i];
                var peliculaDB = peliculasDB[i];

                Assert.AreEqual(peliculaDB.Id, peliculaDelControlador.Id);
            }
        }

        [TestMethod]
        public async Task FiltrarCampoIncorrectoDevuelvePeliculas()
        {
            var nombreBD = CrearDataPrueba();
            var mapper = ConstruirAutoMapper();
            var contexto = ConstruirContext(nombreBD);

            var mock = new Mock<ILogger<PeliculasController>>();
           
            var controller = new PeliculasController(contexto, mapper, null, mock.Object);
            controller.ControllerContext.HttpContext = new DefaultHttpContext();

            var filtroDTO = new FiltroPeliculaDTO()
            {
                CampoOrdenar = "abc",
                OrdenAscendente = true
            };

            var respuesta = await controller.Filtrar(filtroDTO);
            var peliculas = respuesta.Value;

            var contexto2 = ConstruirContext(nombreBD);
            var peliculasDB = contexto2.Pelicula.ToList();
            Assert.AreEqual(peliculasDB.Count,peliculas.Count);
            Assert.AreEqual(1,mock.Invocations.Count);
        }

        private string CrearDataPrueba()
        {
            var databaseName = Guid.NewGuid().ToString();
            var context = ConstruirContext(databaseName);
            var genero = new Genero() { Nombre = "genre 1" };

            var peliculas = new List<Pelicula>()
            {
                new Pelicula(){Titulo = "Pelicula 1", FechaEstreno = new DateTime(2010, 1,1), EnCines = false},
                new Pelicula(){Titulo = "No estrenada", FechaEstreno = DateTime.Today.AddDays(1), EnCines = false},
                new Pelicula(){Titulo = "Película en Cines", FechaEstreno = DateTime.Today.AddDays(-1), EnCines = true}
            };

            var peliculaConGenero = new Pelicula()
            {
                Titulo = "Película con Género",
                FechaEstreno = new DateTime(2010, 1, 1),
                EnCines = false
            };
            peliculas.Add(peliculaConGenero);

            context.Add(genero);
            context.AddRange(peliculas);
            context.SaveChanges();

            var peliculaGenero = new PeliculaGeneros() { GeneroId = genero.Id, PeliculaId = peliculaConGenero.Id };
            context.Add(peliculaGenero);
            context.SaveChanges();

            return databaseName;

        }
    }
}
