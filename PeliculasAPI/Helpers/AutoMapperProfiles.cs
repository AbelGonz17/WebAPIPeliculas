using AutoMapper;
using Microsoft.AspNetCore.Identity;
using NetTopologySuite;
using NetTopologySuite.Geometries;
using PeliculasAPI.DTOs;
using PeliculasAPI.Entidades;

namespace PeliculasAPI.Helpers
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles(GeometryFactory geometryFactory)
        {
            CreateMap<Genero, GeneroDTO>().ReverseMap();
            CreateMap<CrearGeneroDTO, Genero>().ReverseMap();

            CreateMap<Review, ReviewDTO>()
                .ForMember(x => x.NombreUsuario, x => x.MapFrom(y => y.Usuario.UserName));
            CreateMap<ReviewDTO, Review>();
            CreateMap<ReviewCreacionDTO, Review>();         

            CreateMap<Actor, ActorDTO>().ReverseMap();
            CreateMap<CrearActorDTO, Actor>().
                ForMember(x => x.Foto, options => options.Ignore());
            CreateMap<ActorPatchDTO, Actor>().ReverseMap();

            CreateMap<PeliculaPatchDTO, Pelicula>().ReverseMap();
            CreateMap<Pelicula, PeliculaDTO>().ReverseMap();
            CreateMap<CrearPeliculaDTO, Pelicula>().
                ForMember(x => x.Poster, options => options.Ignore())
                .ForMember(x => x.PeliculaGeneros, options => options.MapFrom(MapGenerosDTOPelicula))
                .ForMember(x => x.PeliculasActores, options => options.MapFrom(MapActoresDTOPelicula));

            CreateMap<Pelicula, PeliculasDetallesDTO>()
                .ForMember(x => x.Generos, options => options.MapFrom(MapGeneroDTOPelicula))
                .ForMember(x => x.Actores, options => options.MapFrom(MapActorDTOPelicula));

            CreateMap<SalaDeCine, SalaDeCineDTO>()
                .ForMember(x => x.Latitud, x => x.MapFrom(y => y.Ubicacion.Y))
                .ForMember(x => x.Longitud, x => x.MapFrom(x => x.Ubicacion.X));

            CreateMap<SalaDeCineDTO,SalaDeCine>()
                .ForMember(x => x.Ubicacion, x => x.MapFrom(y =>
                geometryFactory.CreatePoint(new Coordinate(y.Longitud, y.Latitud))));

            CreateMap<CrearSalaDeCineDTO, SalaDeCine>()
                .ForMember(x => x.Ubicacion, x => x.MapFrom(y =>
                geometryFactory.CreatePoint(new Coordinate(y.Longitud, y.Latitud))));

            CreateMap<IdentityUser, UsuarioDTO>().ReverseMap();

        }

        //sirve para mostrar relaciones transformando entidades de base de datos en DTOs que puedes enviar en la respuesta de tu API.
        private List<ActorPeliculaDetalleDTO> MapActorDTOPelicula(Pelicula pelicula, PeliculasDetallesDTO peliculasDetallesDTO)
        {
            var resultado  = new List<ActorPeliculaDetalleDTO>();
            if(pelicula.PeliculasActores == null) {  return resultado; }

            foreach( var actorPelicula in pelicula.PeliculasActores)
            {
                resultado.Add(new ActorPeliculaDetalleDTO()
                {
                   ActorId= actorPelicula.ActorId,
                   Personaje = actorPelicula.Personaje,
                   NombrePersona = actorPelicula.Actor.Nombre
                });
            }
            return resultado;
        }

        //sirve para mostrar relaciones transformando entidades de base de datos en DTOs que puedes enviar en la respuesta de tu API.
        private List<GeneroDTO> MapGeneroDTOPelicula(Pelicula pelicula, PeliculasDetallesDTO peliculasDetallesDTO)
        {
            var resultado = new List<GeneroDTO>();
            if(pelicula.PeliculaGeneros == null ) { return  resultado; }

            foreach( var generoPelicula in pelicula.PeliculaGeneros )
            {
                resultado.Add(new GeneroDTO()
                {
                    Id= generoPelicula.GeneroId,
                    Nombre = generoPelicula.Genero.Nombre
                });
            }
            return resultado;
        }

        //se enfocan en crear relaciones a partir de los datos del DTO,
        private List<PeliculasActores> MapActoresDTOPelicula(CrearPeliculaDTO crearPeliculaDTO, Pelicula pelicula)
        {
            var resultado = new List<PeliculasActores>();

            if (crearPeliculaDTO.Actores == null) { return resultado; }

            foreach (var actores in crearPeliculaDTO.Actores)
            {
                resultado.Add(new PeliculasActores()
                {
                    ActorId = actores.ActorId,
                    Personaje = actores.personaje
                  
                });
            }
            return resultado;
        }

        //se enfocan en crear relaciones a partir de los datos del DTO,
        private List<PeliculaGeneros> MapGenerosDTOPelicula(CrearPeliculaDTO crearPeliculaDTO, Pelicula pelicula)
        {
            var resultado = new List<PeliculaGeneros>();

            if (crearPeliculaDTO.GenerosId == null) { return resultado; }

            foreach (var id in crearPeliculaDTO.GenerosId)
            {
                resultado.Add(new PeliculaGeneros()
                {
                    GeneroId = id,                 
                });
            }
            return resultado;
        }
    }
}
