using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NextStep.RabbitMQ.Interfaces;
using NextStep.RabbitMQ.Models;
using NextStep.RabbitMQ.Services;


namespace Producer.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRabbitMQProducer(this IServiceCollection services, IConfiguration configuration)
        {
            var settings = new RabbitMqSettings();
            configuration.GetSection("RabbitMQ").Bind(settings);

            services.AddSingleton(settings);
            services.AddSingleton<IRabbitMqProducer, RabbitMqProducer>();

            return services;
        }
    }
}
