using Microsoft.AspNetCore.Mvc.ApplicationModels;
using System.Text.RegularExpressions;

namespace THtracker.API.Routing;

/// <summary>
/// Convención para aplicar versionamiento automático basado en la estructura de carpetas (namespaces).
/// Si un controlador está en el namespace THtracker.API.Controllers.v1, 
/// la ruta resultante será api/v1/[controller].
/// </summary>
public class ApiVersioningConvention : IControllerModelConvention
{
    public void Apply(ControllerModel controller)
    {
        var controllerNamespace = controller.ControllerType.Namespace;
        if (string.IsNullOrEmpty(controllerNamespace)) return;

        // Buscamos el patrón .vX. en el namespace
        var match = Regex.Match(controllerNamespace, @"\.Controllers\.(v\d+)", RegexOptions.IgnoreCase);

        if (match.Success)
        {
            var version = match.Groups[1].Value.ToLower();

            foreach (var selector in controller.Selectors)
            {
                if (selector.AttributeRouteModel != null)
                {
                    // Si ya tiene una ruta definida, la ajustamos para que incluya la versión si no la tiene
                    // O mejor, reemplazamos el api/v1/ por api/[version]/ de forma dinámica si el usuario quiere "v1"
                    // Sin embargo, para que sea automático basado en carpeta, lo ideal es que el 
                    // [Route] del controlador sea simplemente "[controller]" o "api/[controller]"
                    
                    var template = selector.AttributeRouteModel.Template;
                    
                    if (template != null)
                    {
                        // Limpiamos prefijos de versión manuales para estandarizar
                        template = Regex.Replace(template, @"^api/v\d+/", "", RegexOptions.IgnoreCase);
                        template = Regex.Replace(template, @"^v\d+/", "", RegexOptions.IgnoreCase);
                        
                        // Prefijamos con la versión detectada por la carpeta
                        selector.AttributeRouteModel.Template = $"api/{version}/{template.TrimStart('/')}";
                    }
                }
                else
                {
                    // Si no tiene atributo Route, asignamos uno por defecto
                    selector.AttributeRouteModel = new AttributeRouteModel
                    {
                        Template = $"api/{version}/{controller.ControllerName}"
                    };
                }
            }
        }
    }
}
