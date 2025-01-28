using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Diagnostics;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using PeliculasAPI.DTOs;
using PeliculasAPI.Entidades;

namespace PeliculasAPI.Controllers
{
    [ApiController]
    [Route("api/generos")]
    public class GenerosController : CustomBaseController
    {

        public GenerosController(ApplicationDbContext context, IMapper mapper) : base(context, mapper)
        {

        }

        [HttpGet]
        public async Task<ActionResult<List<GeneroDTO>>> Get()
        {
            return await Get<Genero, GeneroDTO>();
        }

        [HttpGet("{id:int}", Name = "obtenerGenero")]
        public async Task<ActionResult<GeneroDTO>> Get(int id)
        {
            return await Get<Genero, GeneroDTO>(id);
        }

        [HttpPost]
        public async Task<ActionResult> Post([FromBody]CrearGeneroDTO crearGeneroDTO)
        {
            return await Post<CrearGeneroDTO, Genero, GeneroDTO>(crearGeneroDTO, "obtenerGenero");
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult> Put (int id,CrearGeneroDTO crearGeneroDTO)
        {
            return await Put<CrearGeneroDTO, Genero>(id, crearGeneroDTO);           
        }

        [HttpDelete("{id:int}")]
        [Authorize(AuthenticationSchemes =JwtBearerDefaults.AuthenticationScheme,Roles = "Admin")]
        public async Task<ActionResult> Delete(int id)
        {
            return await Delete<Genero>(id);
        }

    }
}
