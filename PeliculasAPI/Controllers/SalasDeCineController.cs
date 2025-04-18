﻿using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using PeliculasAPI.DTOs;
using PeliculasAPI.Entidades;

namespace PeliculasAPI.Controllers
{
    [ApiController]
    [Route("api/salaDeCine")]
    public class SalasDeCineController : CustomBaseController
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;
        private readonly GeometryFactory geometryFactory;

        public SalasDeCineController(ApplicationDbContext context, IMapper mapper, GeometryFactory geometryFactory) : base(context, mapper)
        {
            this.context = context;
            this.mapper = mapper;
            this.geometryFactory = geometryFactory;
        }

        [HttpGet]
        public async Task<ActionResult<List<SalaDeCineDTO>>> Get()
        {
            return await Get<SalaDeCine, SalaDeCineDTO>();
        }

        [HttpGet("{id:int}", Name = "obtenerSalaDeCine")]
        public async Task<ActionResult<SalaDeCineDTO>> Get (int id)
        {
            return await Get<SalaDeCine,SalaDeCineDTO>(id);
        }

        [HttpGet("cercanos")]
        public async Task<ActionResult<List<SalaDeCineCercanoDTO>>> Cercanos(
            [FromQuery] SalaDeCineCercanoFiltroDTO Filtro)
        {
            var ubicacionUsuario = geometryFactory.CreatePoint(new Coordinate(Filtro.Longitud, Filtro.Latitud));

            var salasDeCine = await context.salaDeCines
                .OrderBy(x => x.Ubicacion.Distance(ubicacionUsuario))
                .Where(x => x.Ubicacion.IsWithinDistance(ubicacionUsuario, Filtro.DistanciaEnKms * 1000))
                .Select(x => new SalaDeCineCercanoDTO
                {
                    Id = x.Id,
                    Nombre = x.Nombre,
                    Latitud = x.Ubicacion.Y,
                    Longitud = x.Ubicacion.X,
                    DistanciaEnMetros = Math.Round(x.Ubicacion.Distance(ubicacionUsuario))
                })
                .ToListAsync();
            return salasDeCine;
        }

        [HttpPost]
        public async Task<ActionResult> Post(CrearSalaDeCineDTO crearSalaDeCineDTO)
        {
            return await Post<CrearSalaDeCineDTO, SalaDeCine, SalaDeCineDTO>(crearSalaDeCineDTO, "obtenerSalaDeCine");
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult> Put(int id, CrearSalaDeCineDTO crearSalaDeCineDTO)
        {
            return await Put<CrearSalaDeCineDTO,SalaDeCine>(id, crearSalaDeCineDTO);
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete (int id)
        {
            return await Delete<SalaDeCine>(id);
        }
    }
}
