using AutoMapper;
using Azure;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PeliculasAPI.DTOs;
using PeliculasAPI.Entidades;
using PeliculasAPI.Servicios;
using PeliculasAPI.Utilidades;

namespace PeliculasAPI.Controllers
{
    [ApiController]
    [Route("api/actores")]
    public class ActoresController : CustomBaseController
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;
        private readonly IAlmacenadorArchivos almacenadorArchivos;
        private readonly string contenedor = "Actores";

        public ActoresController(ApplicationDbContext context, IMapper mapper, IAlmacenadorArchivos almacenadorArchivos): base (context,mapper)
        {
            this.context = context;
            this.mapper = mapper;
            this.almacenadorArchivos = almacenadorArchivos;
        }

        [HttpGet]
        public async Task<ActionResult<List<ActorDTO>>> Get([FromQuery] PaginacionDTO paginacionDTO)
        {
            return await Get<Actor, ActorDTO>(paginacionDTO);
        }

        [HttpGet("{id:int}", Name = "obtenerActor")]
        public async Task<ActionResult<ActorDTO>> Get(int id)
        {
            return await Get<Actor, ActorDTO>(id);
        }

        [HttpPost]
        public async Task<ActionResult> Post([FromForm] CrearActorDTO crearActorDTO)
        {
            var existeActorConElMismoNombre = await context.Actores.AnyAsync(x => x.Nombre == crearActorDTO.Nombre);

            if (existeActorConElMismoNombre)
            {
                return BadRequest($"ya existe un actor con el nombre {crearActorDTO.Nombre}");
            }

            var actor = mapper.Map<Actor>(crearActorDTO);

            if (crearActorDTO.Foto != null)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await crearActorDTO.Foto.CopyToAsync(memoryStream);
                    var contenido = memoryStream.ToArray();
                    var extension = Path.GetExtension(crearActorDTO.Foto.FileName);
                    actor.Foto = await almacenadorArchivos.GuardarArchivo(contenido, extension, contenedor, crearActorDTO.Foto.ContentType);
                }
            }
            context.Add(actor);
            await context.SaveChangesAsync();
            var actorDTO = mapper.Map<ActorDTO>(actor);

            return CreatedAtRoute("obtenerActor", new { id = actor.Id }, actorDTO);
        }

       

        [HttpPut("{id:int}")]
        public async Task<ActionResult> Put(int id, CrearActorDTO crearActorDTO)
        {
            var actorDb = await context.Actores.FirstOrDefaultAsync(x => x.Id == id);

            if(actorDb == null) { return NotFound(); }

            actorDb = mapper.Map(crearActorDTO, actorDb);

            if (crearActorDTO.Foto != null)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await crearActorDTO.Foto.CopyToAsync(memoryStream);
                    var contenido = memoryStream.ToArray();
                    var extension = Path.GetExtension(crearActorDTO.Foto.FileName);
                    actorDb.Foto = await almacenadorArchivos.EditarArchivo(contenido, extension, contenedor,actorDb.Foto,crearActorDTO.Foto.ContentType);
                }
            }
            await context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPatch("{id:int}")]
        public async Task<ActionResult> Patch(int id, JsonPatchDocument<ActorPatchDTO> patchDocument)
        {
           return await Patch<Actor,ActorPatchDTO>(id, patchDocument);
        }



        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
           return await Delete<Actor>(id);  
        }
    }
}
