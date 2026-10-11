
using Microsoft.Data.SqlClient;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("TiendaSara", policy =>
        policy
            .WithOrigins(
                "http://127.0.0.1:5500",
                "http://localhost:5500",
                "http://127.0.0.1:5501",
                "http://localhost:5501")
            .AllowAnyHeader()
            .AllowAnyMethod());
});

var app = builder.Build();

app.UseCors("TiendaSara");

app.MapGet("/", () => Results.Ok(new
{
    mensaje = "API de Tienda Sara funcionando"
}));

app.MapGet("/api/productos", async (IConfiguration config) =>
{
    var cadena = config.GetConnectionString("TiendaSara");

    var productos = new List<object>();

    try
    {
        await using var conexion = new SqlConnection(cadena);
        await conexion.OpenAsync();

        const string consulta = """
            SELECT ID, DESCRIPCION, PRECIO,
                   IDCATEGORIA, IDMARCA, CANTIDAD
            FROM dbo.PRODUCTOS
            ORDER BY ID;
            """;

        await using var comando = new SqlCommand(consulta, conexion);
        await using var lector = await comando.ExecuteReaderAsync();

        while (await lector.ReadAsync())
        {
            productos.Add(new
            {
                id = lector["ID"],
                descripcion = lector["DESCRIPCION"],
                precio = lector["PRECIO"],
                idCategoria = lector["IDCATEGORIA"],
                idMarca = lector["IDMARCA"],
                cantidad = lector["CANTIDAD"]
            });
        }

        return Results.Ok(productos);
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Error al consultar los productos");

        return Results.Problem(
            title: "Error al consultar productos",
            detail: "Revisa la conexión a SQL Server y los nombres de las columnas.");
    }
});

app.Run();
