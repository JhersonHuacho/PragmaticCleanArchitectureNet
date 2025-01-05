# Bookify

## Iniciar el proyecto

Para iniciar el proyecto debe ejecutar el siguiente comando de docker,
y desde la consola debe ubicarse en la raíz del proyecto.

```bash
docker-compose up
```

Esto creara un contenedor con la base de datos y el servidor de la aplicación.

Recordar que en la clase Program.cs cada vez que se ejecute el proyecto se creara la base de datos y se insertaran datos de prueba.
Y para no estar insertando datos de prueba cada vez que se ejecute el proyecto, se debe comentar la siguiente linea de código.

```c#
app.SeedData();
```

## Consideraciones
Si estas usando .Net 6 y usando el tipo DateOnly, y al hacer la request:
```
{{api_url}}/api/apartments?startDate=07-20-2023&endDate=07-31-2023
```
te puede salir el siguiente error:
```
Could not create an instance of type 'System.DateOnly'. Model bound complex types must not be abstract or value types and must have a parameterless constructor. Record types must have a single primary constructor. Alternatively, give the 'startDate' parameter a non-null default value.
```

El problema es que DateOnly no es compatible de forma nativa con el model binding de ASP.NET Core. 
Para solucionar esto, puedes usar DateTime en lugar de DateOnly en tu controlador y luego convertirlo a DateOnly dentro del método.

.net 6
```c#
[HttpGet]
public async Task<IActionResult> SearchsApartments(
	[FromQuery] DateTime startDate,
	[FromQuery] DateTime endDate,
	CancellationToken cancellationToken)
{
	var query = new SearchApartmentsQuery(DateOnly.FromDateTime(startDate), DateOnly.FromDateTime(endDate));

	var result = await _sender.Send(query, cancellationToken);

	return Ok(result.Value);
}
```

.net 7 en adelante
```c#
[HttpGet]
public async Task<IActionResult> SearchsApartments(
	[FromQuery] DateOnly startDate,
	[FromQuery] DateOnly endDate,
	CancellationToken cancellationToken)
{
	var query = new SearchApartmentsQuery(startDate, endDate);

	var result = await _sender.Send(query, cancellationToken);

	return Ok(result.Value);
}
```

DateOnly y TimeOnly fueron introducidos en .NET 6, pero no son compatibles de forma nativa con el model binding de ASP.NET Core en .NET 6. 
La compatibilidad nativa con el model binding para estos tipos se introdujo en .NET 7.