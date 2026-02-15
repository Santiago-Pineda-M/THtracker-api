namespace THtracker.Application;

/// <summary>
/// Interfaz de marcador (Marker Interface) utilizada para identificar el ensamblado de la capa de Application.
/// Esto permite el escaneo automático de componentes como Validadores (FluentValidation), 
/// Handlers (MediatR) o Mappings sin depender de una clase de negocio específica.
/// Cumple con Clean Code al evitar dependencias accidentales y hacer el código auto-descriptivo.
/// </summary>
public interface IApplicationAssemblyMarker
{
}
