using Microsoft.EntityFrameworkCore;

namespace PeliculasAPI.Utilidades
{
    public static  class HttpContextExtensions
    {
        //esto es para que la cantidad de registros total aparezca en la cabecera
        public async static Task InsertarParametrosDePaginacionEnCabecera<T>(this HttpContext httpContext
            , IQueryable<T> queryable, int cantidadDeRegistrosPorPagina)
        {
            if (httpContext == null) throw new ArgumentNullException(nameof(httpContext));

            double cantidad = await queryable.CountAsync();
            double cantidadPaginas = Math.Ceiling(cantidad / cantidadDeRegistrosPorPagina); 
            httpContext.Response.Headers.Append("CantidadTotalRegistros", cantidadPaginas.ToString());

        }
    }
}
