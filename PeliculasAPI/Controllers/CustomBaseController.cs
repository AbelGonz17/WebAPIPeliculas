using AutoMapper;
using Azure;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PeliculasAPI.DTOs;
using PeliculasAPI.Entidades;
using PeliculasAPI.Utilidades;
using System.Security.Cryptography;

namespace PeliculasAPI.Controllers
{
    public class CustomBaseController:ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;

        public CustomBaseController(ApplicationDbContext context, IMapper mapper)
        {
            this.context = context;
            this.mapper = mapper;
        }

        protected async Task<List<TDTO>> Get <TEntidad, TDTO>() where TEntidad : class
        {
            var entidades = await context.Set<TEntidad>().AsNoTracking().ToListAsync();
            var dtos = mapper.Map<List<TDTO>>(entidades);
            return dtos;
        }

        protected async Task<List<TDTO>> Get <TEntidad,TDTO>(PaginacionDTO paginacionDTO) where TEntidad : class 
        {
            var queryable = context.Set<TEntidad>().AsQueryable();
            return await Get<TEntidad, TDTO>(paginacionDTO, queryable);
        }

        protected async Task<List<TDTO>> Get<TEntidad, TDTO>(PaginacionDTO paginacionDTO, IQueryable<TEntidad> queryable) where TEntidad : class
        {
            
            await HttpContext.InsertarParametrosDePaginacionEnCabecera(queryable, paginacionDTO.RecordsPorPagina);
            var entidades = await queryable.paginar(paginacionDTO).ToListAsync();
            return mapper.Map<List<TDTO>>(entidades);
        }

        protected async Task<ActionResult<TDTO>> Get<TEntidad, TDTO>(int id) where TEntidad : class, IId
        {
            var entidad = await context.Set<TEntidad>().AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);

            if(entidad == null)
            {
                return NotFound();
            }

            var dtos = mapper.Map<TDTO>(entidad);

            return dtos;
        }

        protected async Task<ActionResult> Post <TCreacion, TEntidad, TLectura>
            (TCreacion CrearGenero, string nombreRuta) where TEntidad : class , IId 
        {       
            var entidad = mapper.Map<TEntidad>(CrearGenero);

            context.Add(entidad);
            await context.SaveChangesAsync();
            var dtoLectura = mapper.Map<TLectura>(entidad);

            return CreatedAtRoute(nombreRuta, new { id = entidad.Id }, dtoLectura);

        }

        protected async Task<ActionResult> Put <TCreacion, TEntidad>
            (int id, TCreacion CreacionDto) where TEntidad : class, IId
        {
            var entidad = mapper.Map<TEntidad>(CreacionDto);
            entidad.Id = id;

            context.Entry(entidad).State = EntityState.Modified;
            await context.SaveChangesAsync();
            return NoContent();

        }

        protected async Task<ActionResult> Patch<TEntidad, TDTO>(int id, JsonPatchDocument<TDTO> patchDocument)
            where TDTO : class
            where TEntidad : class, IId
        {
            if (patchDocument == null)
            {
                return BadRequest();
            }

            var entidad = await context.Set<TEntidad>().FirstOrDefaultAsync(x => x.Id == id);

            if (entidad == null)
            {
                return NotFound();
            }

            var entidadDTO = mapper.Map<TDTO>(entidad);

            patchDocument.ApplyTo(entidadDTO, ModelState);

            var esValido = TryValidateModel(entidadDTO);

            if (!esValido)
            {
                return BadRequest(ModelState);
            }

            mapper.Map(entidadDTO, entidad);

            await context.SaveChangesAsync();
            return NoContent();

        }

        protected async Task<ActionResult> Delete <TEntidad>(int id) where TEntidad : class, IId
        {
            var entidad = await context.Set<TEntidad>().FirstOrDefaultAsync(x => x.Id == id);
            if (entidad == null)
            {
                return NotFound();
            }

            context.Remove(entidad);
            await context.SaveChangesAsync();
            return NoContent();
        }


    }
}
