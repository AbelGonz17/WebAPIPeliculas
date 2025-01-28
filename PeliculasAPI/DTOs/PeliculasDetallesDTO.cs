namespace PeliculasAPI.DTOs
{
    public class PeliculasDetallesDTO: PeliculaDTO
    {
        public  List<GeneroDTO> Generos { get; set; }
        public  List<ActorPeliculaDetalleDTO> Actores { get; set; }
    }
}
