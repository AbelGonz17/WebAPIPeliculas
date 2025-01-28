using AutoMapper;
using AutoMapper.Configuration.Annotations;
using Azure;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using PeliculasAPI.DTOs;
using PeliculasAPI.Entidades;
using PeliculasAPI.Migrations;
using PeliculasAPI.Servicios;
using PeliculasAPI.Utilidades;
using System.Linq.Dynamic.Core;

namespace PeliculasAPI.Controllers
{
    [ApiController]
    [Route("api/peliculas")]
    public class PeliculasController : CustomBaseController
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;
        private readonly IAlmacenadorArchivos almacenadorArchivos;
        private readonly ILogger<PeliculasController> logger;
        private readonly string contenedor = "Peliculas";


        public PeliculasController(ApplicationDbContext context, IMapper mapper, IAlmacenadorArchivos almacenadorArchivos, ILogger<PeliculasController> logger) : base(context,mapper)
        {
            this.context = context;
            this.mapper = mapper;
            this.almacenadorArchivos = almacenadorArchivos;
            this.logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<PeliculasIndexDTO>> Get()
        {
            var top = 5;
            var hoy = new DateTime(2020,02,29);

            var fechaEstreno = await context.Pelicula
                .Where(x => x.FechaEstreno > hoy)
                .OrderBy(x => x.FechaEstreno)
                .Take(top)
                .ToListAsync();

            var enCines = await context.Pelicula
                .Where(x => x.EnCines)
                .Take(top)
                .ToListAsync();

            var resultado = new PeliculasIndexDTO();
            resultado.FuturosEstrenos = mapper.Map<List<PeliculaDTO>>(fechaEstreno);
            resultado.Encines = mapper.Map<List<PeliculaDTO>>(enCines);

            return resultado; 
        }

        [HttpGet("filtro")]
        public async Task<ActionResult<List<PeliculaDTO>>> Filtrar([FromQuery] FiltroPeliculaDTO filtroPeliculaDTO)
        {
            var peliculaQueryable = context.Pelicula.AsQueryable();

            if(!string.IsNullOrEmpty(filtroPeliculaDTO.Titulo))
            {
                peliculaQueryable = peliculaQueryable
                    .Where(x => x.Titulo
                    .Contains(filtroPeliculaDTO.Titulo));
            }

            if (filtroPeliculaDTO.EnCines)
            {
                peliculaQueryable = peliculaQueryable
                    .Where(x => x.EnCines);
            }

            if (filtroPeliculaDTO.ProximosEstrenos)
            {
                var hoy = DateTime.Now;
                peliculaQueryable = peliculaQueryable.Where(x => x.FechaEstreno > hoy);
            }

            if (filtroPeliculaDTO.GeneroId != 0)
            {
                peliculaQueryable = peliculaQueryable
                    .Where(x => x.PeliculaGeneros
                    .Select(y => y.GeneroId)
                    .Contains(filtroPeliculaDTO.GeneroId));
            }

            if(!string.IsNullOrEmpty(filtroPeliculaDTO.CampoOrdenar))
            {
                var tipoOrden = filtroPeliculaDTO.OrdenAscendente ? "ascending" : "descending";

                try
                {
                    peliculaQueryable = peliculaQueryable.OrderBy($" {filtroPeliculaDTO.CampoOrdenar} {tipoOrden}");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex.Message, ex);
                }                              
            }
            await HttpContext.InsertarParametrosDePaginacionEnCabecera(peliculaQueryable
                , filtroPeliculaDTO.Paginacion.RecordsPorPagina);

            var peliculas = await peliculaQueryable.paginar(filtroPeliculaDTO.Paginacion).ToListAsync();

            return mapper.Map<List<PeliculaDTO>>(peliculas);
        }

        [HttpGet("{id:int}", Name = "obtenerPelicula")]
        public async Task<ActionResult<PeliculasDetallesDTO>> Get(int id)
        {
            var pelicula = await context.Pelicula
                .Include(x => x.PeliculasActores).ThenInclude(x => x.Actor)
                .Include(x => x.PeliculaGeneros).ThenInclude(x => x.Genero)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (pelicula == null)
            {
                return NotFound();
            }

            pelicula.PeliculasActores = pelicula.PeliculasActores.OrderBy(x => x.Orden).ToList();

            var peliculaDTO = mapper.Map<PeliculasDetallesDTO>(pelicula);
            return peliculaDTO;
        }

        [HttpPost]
        public async Task<ActionResult> Post(CrearPeliculaDTO crearPeliculaDTO)
        {
            var existePelicula = await context.Pelicula.AnyAsync(x => x.Titulo == crearPeliculaDTO.Titulo);

            if (existePelicula)
            {
                return BadRequest("Esta pelicula ya esta en el sistema");
            }

            var pelicula = mapper.Map<Pelicula>(crearPeliculaDTO);

            if (crearPeliculaDTO.Poster != null)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await crearPeliculaDTO.Poster.CopyToAsync(memoryStream);
                    var contenido = memoryStream.ToArray();
                    var extension = Path.GetExtension(crearPeliculaDTO.Poster.FileName);
                    pelicula.Poster = await almacenadorArchivos.GuardarArchivo(contenido, extension, contenedor, crearPeliculaDTO.Poster.ContentType);
                }
            }

            OrdenActor(pelicula);
            context.Add(pelicula);
            await context.SaveChangesAsync();
            var peliculaDTO = mapper.Map<PeliculaDTO>(pelicula);

            return CreatedAtRoute("obtenerPelicula", new { id = pelicula.Id }, peliculaDTO);

        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult> Put(int id, CrearPeliculaDTO crearPeliculaDTO)
        {
            var pelicula = await context.Pelicula
                .Include(x => x.PeliculasActores)
                .Include(x => x.PeliculaGeneros)
                .FirstOrDefaultAsync(x => x.Id == id);
            if (pelicula == null)
            {
                return NotFound();
            }

            var peliculaDb = mapper.Map(crearPeliculaDTO, pelicula);

            if (crearPeliculaDTO.Poster != null)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await crearPeliculaDTO.Poster.CopyToAsync(memoryStream);
                    var contenido = memoryStream.ToArray();
                    var extension = Path.GetExtension(crearPeliculaDTO.Poster.FileName);
                    pelicula.Poster = await almacenadorArchivos.GuardarArchivo(contenido, extension, contenedor, crearPeliculaDTO.Poster.ContentType);
                }
            }

            OrdenActor(pelicula);
            await context.SaveChangesAsync();
            return NoContent();    
        }

        [HttpPatch("{id:int}")]
        public async Task<ActionResult> Patch (int id , JsonPatchDocument<PeliculaPatchDTO> patchDocument )
        {
            return await Patch<Pelicula, PeliculaPatchDTO>(id,patchDocument);
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            return await Delete<Pelicula>(id);
        }

        private void OrdenActor(Pelicula peliculas)
        {
            if (peliculas.PeliculasActores != null)
            {
                for (int i = 0; i < peliculas.PeliculasActores.Count; i++)
                {
                    peliculas.PeliculasActores[i].Orden = i;
                }
            }
        }
    }
}
