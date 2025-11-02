
using MqttGateway.Server.Hubs;
using MqttGateway.Server.Services;
using MqttGateway.Server.Services.Contracts;

namespace MqttGateway.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddSignalR();

            // outros services
            builder.Services.AddSingleton<ISessionContextStore, SessionContextStore>();
            builder.Services.AddSingleton<IMqttBrokerConnectionHandler, MqttBrokerConnectionHandler>();
            builder.Services.AddSingleton<ISessionManager, SessionManagerService>();
            builder.Services.AddSingleton<IMqttEventDispatcher, SignalRMessageRelay>(); // <- depois do AddSignalR()


            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();


            var handler = app.Services.GetRequiredService<IMqttBrokerConnectionHandler>();
            var dispatcher = app.Services.GetRequiredService<IMqttEventDispatcher>();

            handler.SetDispatcher(dispatcher);

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();
            app.MapHub<UserHub>("/hub");

            app.Run();
        }
    }
}
