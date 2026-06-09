using Microsoft.Extensions.Options;
using Turnos.Api.Common.Contracts;
using Turnos.Api.Common.Services;
using Turnos.Api.Common.Settings;

namespace Turnos.Api.Common.Extensions;

public static class MercadoPagoExtensions
{
    public static IServiceCollection AddMercadoPagoServices(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<MercadoPagoSettings>(configuration.GetSection("MercadoPago"));
        services.AddHttpClient<IMercadoPagoService, MercadoPagoService>(client =>
        {
            client.BaseAddress = new Uri("https://api.mercadopago.com/");
        });

        return services;
    }
}