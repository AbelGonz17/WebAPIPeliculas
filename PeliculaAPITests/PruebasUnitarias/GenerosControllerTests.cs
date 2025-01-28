using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Utilities;
using PeliculasAPI.Controllers;
using PeliculasAPI.DTOs;
using PeliculasAPI.Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace PeliculaAPITests.PruebasUnitarias
{
    [TestClass]
    public class GenerosControllerTests : BasePruebas
    {
        [TestMethod]
        public async Task ObtenerTodoLosGeneros()
        {
            //preparacion
            var nombreDb = Guid.NewGuid().ToString();
            var contexto = ConstruirContext(nombreDb);
            var mapper = ConstruirAutoMapper();

            contexto.generos.Add(new Genero() { Nombre = "Genero 1" });
            contexto.generos.Add(new Genero() { Nombre = "Genero 2" });
            await contexto.SaveChangesAsync();

            var contexto2 = ConstruirContext(nombreDb);

            //prueba
            var controller = new GenerosController(contexto2, mapper);
            var respuesta = await controller.Get();

            //verificacion
            var generos = respuesta.Value;
            Assert.AreEqual(2, generos.Count);
        }

        [TestMethod]
        public async Task ObtenerGeneroPorIdNoExistente()
        {
            var nombreDb = Guid.NewGuid().ToString();
            var contexto = ConstruirContext(nombreDb);
            var mapper = ConstruirAutoMapper();

            var controller = new GenerosController(contexto, mapper);
            var respuesta = await controller.Get(1);

            var resultado = respuesta.Result as StatusCodeResult;
            Assert.AreEqual(404, resultado.StatusCode);
        }

        [TestMethod]
        public async Task ObtenerGeneroPorIdExistente()
        {
            var nombreDb = Guid.NewGuid().ToString();
            var contexto = ConstruirContext(nombreDb);
            var mapper = ConstruirAutoMapper();

            contexto.generos.Add(new Genero() { Nombre = "Genero 1" });
            contexto.generos.Add(new Genero() { Nombre = "Genero 2" });
            await contexto.SaveChangesAsync();

            var contexto2 = ConstruirContext(nombreDb);
            var controller = new GenerosController(contexto2, mapper);
            var id = 1;

            var respuesta = await controller.Get(1);
            var resultado = respuesta.Value;
            Assert.AreEqual(id, resultado.Id);
        }

        [TestMethod]
        public async Task CrearGenero()
        {
            var nombreDb = Guid.NewGuid().ToString();
            var contexto = ConstruirContext(nombreDb);
            var mapper = ConstruirAutoMapper();

            var nuevoGenero = new CrearGeneroDTO() { Nombre = "Genero" };

            var controller = new GenerosController(contexto, mapper);
            var respuesta = await controller.Post(nuevoGenero);

            var resultado = respuesta as CreatedAtRouteResult;
            Assert.IsNotNull(resultado);

            var contexto2 = ConstruirContext(nombreDb);
            var cantidad = await contexto2.generos.CountAsync();
            Assert.AreEqual(1, cantidad);
        }

        [TestMethod]
        public async Task ActualizarGenero()
        {
            var nombreDb = Guid.NewGuid().ToString();
            var contexto = ConstruirContext(nombreDb);
            var mapper = ConstruirAutoMapper();

            contexto.generos.Add(new Genero() { Nombre = "Genero 1" });
            await contexto.SaveChangesAsync();

            var contexto2 = ConstruirContext(nombreDb);
            var controller = new GenerosController(contexto2,mapper);

            var generoCreacionDTO = new CrearGeneroDTO() { Nombre = "nuevo nombre" };

            var id = 1;

            var respuesta = await controller.Put(id, generoCreacionDTO);

            var resultado = respuesta as StatusCodeResult;
            Assert.AreEqual(204, resultado.StatusCode);

            var contexto3 = ConstruirContext(nombreDb);
            var existe = await contexto3.generos.AnyAsync(x => x.Nombre == "nuevo nombre");
            Assert.IsTrue(existe);
        }

        [TestMethod]
        public async Task IntentaBorrarGeneroNoExistente()
        {
            var nombreDb = Guid.NewGuid().ToString();
            var contexto = ConstruirContext(nombreDb);
            var mapper = ConstruirAutoMapper();

            var controller = new GenerosController(contexto,mapper);    

            var id = 1;

            var respuesta = await controller.Delete(id);

            var resultado = respuesta as StatusCodeResult;
            Assert.AreEqual(404, resultado.StatusCode);
        }

        [TestMethod]
        public async Task IntentaBorrarGeneroExistente()
        {
            var nombreDb = Guid.NewGuid().ToString();
            var contexto = ConstruirContext(nombreDb);
            var mapper = ConstruirAutoMapper();

            contexto.generos.Add(new Genero() { Nombre = "Genero 1" });
            contexto.generos.Add(new Genero() { Nombre = "Genero 2" });
            await contexto.SaveChangesAsync();

            var contexto2 = ConstruirContext(nombreDb);
            var controller = new GenerosController(contexto2, mapper);
            var id = 1;

            var respuesta = await controller.Delete(id);
            var resultado = respuesta as StatusCodeResult;
            Assert.AreEqual(204, resultado.StatusCode);

            var contexto3 = ConstruirContext(nombreDb);
            var cantidad = await contexto3.generos.CountAsync();
            Assert.AreEqual(1, cantidad);
        }
    }
}
