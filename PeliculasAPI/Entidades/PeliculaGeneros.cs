namespace PeliculasAPI.Entidades
{
    public class PeliculaGeneros
    {
        public  int  GeneroId { get; set; }
        public int PeliculaId   { get; set; }
        //propiedades de navegacion
        public Genero Genero { get; set; }
        public Pelicula Pelicula { get; set; }
    }
}
