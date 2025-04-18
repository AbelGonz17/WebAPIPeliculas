﻿using NetTopologySuite.Geometries;
using System.ComponentModel.DataAnnotations;

namespace PeliculasAPI.Entidades
{
    public class SalaDeCine : IId
    {
        public int Id {  get; set; }
        [Required]
        public string Nombre { get; set; }
        public Point Ubicacion { get; set; }
        public List<PeliculasSalasDeCine> peliculasSalasDeCines{ get; set; }
    }
}
