﻿namespace PeliculasAPI.DTOs
{
    public class FiltroPeliculaDTO
    {
        public int pagina { get; set; } = 1;
        public int RecordPorPagina { get; set; } = 10;
        public PaginacionDTO Paginacion
        {
            get { return new PaginacionDTO() { Pagina = pagina, RecordsPorPagina = RecordPorPagina }; }
        }

        public string Titulo { get; set; }
        public int GeneroId { get; set; }
        public bool EnCines { get; set; }
        public bool ProximosEstrenos { get; set; }
        public string CampoOrdenar { get; set; }
        public bool OrdenAscendente { get; set; } = true;
    }
}
