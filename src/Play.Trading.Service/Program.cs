using System;
using System.Reflection;
using System.Text.Json.Serialization;
using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Play.Common.Identity;
using Play.Common.MassTransit;
using Play.Common.MongoDB;
using Play.Common.Settings;
using Play.Trading.Service.Entities;
using Play.Trading.Service.StateMachine;
using GreenPipes;
using Play.Trading.Service.Exceptions;
using Play.Trading.Service.Settings;
using Play.Inventory.Contracts;
using Play.Identity.Contracts;
using Microsoft.AspNetCore.SignalR;
using Play.Trading.Service.SignalR;
using Play.Common.HealthChecks;

var builder = WebApplication.CreateBuilder(args);
var Configuration = builder.Configuration;
var services = builder.Services;

const string AllowedOriginSetting = "AllowedOrigin";

builder.Services.AddMongo()
                .AddMongoRepository<CatalogItem>("catalogitems")
                .AddMongoRepository<InventoryItem>("inventoryitems")
                .AddMongoRepository<ApplicationUser>("users")
                .AddJwtBearerAuthentication();

AddMassTransit(Configuration,services);

builder.Services.AddControllers(options =>
{
    options.SuppressAsyncSuffixInActionNames = false;
}).AddJsonOptions(options => options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull);


builder.Services.AddRouting(options => options.LowercaseUrls = true);
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Play.Trading.Service", Version = "v1" });
});

builder.Services.AddSingleton<IUserIdProvider, UserIdProvider>()
                .AddSingleton<MessageHub>()
                .AddSignalR();

builder.Services.AddHealthChecks().AddMongoDb();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Play.Catalog.Service v1");
        c.RoutePrefix = string.Empty;
    });

    app.UseCors(builder =>
    {
        builder.WithOrigins(Configuration[AllowedOriginSetting])
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials();
    });
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<MessageHub>("/messageHub");
app.MapPlayEconomyHealthChecks();
app.Run();

void AddMassTransit(IConfiguration configuration,IServiceCollection services)
{
    services.AddMassTransit(configure =>
    {
        configure.UsingPlayEconomyMessageBroker(configuration,retryConfigurator =>
        {
            retryConfigurator.Interval(3, TimeSpan.FromSeconds(5));
            retryConfigurator.Ignore(typeof(UnknownItemException));
        });
        configure.AddConsumers(Assembly.GetEntryAssembly());
        configure.AddSagaStateMachine<PurchaseStateMachine, PurchaseState>(sagaConfigurator =>
        {
            sagaConfigurator.UseInMemoryOutbox();
        })
                    .MongoDbRepository(r =>
                    {
                        var serviceSettings = Configuration.GetSection(nameof(ServiceSettings)).Get<ServiceSettings>();
                        var mongoSettings = Configuration.GetSection(nameof(MongoDbSettings)).Get<MongoDbSettings>();

                        r.Connection = mongoSettings.ConnectionString;
                        r.DatabaseName = serviceSettings.ServiceName;
                    });
    });
    
    var queueSettings = Configuration.GetSection(nameof(QueueSettings)).Get<QueueSettings>();

    EndpointConvention.Map<GrantItems>(new Uri(queueSettings.GrantItemsQueueAddress));

    EndpointConvention.Map<DebitGil>(new Uri(queueSettings.DebitGilQueueAddress));

    EndpointConvention.Map<SubtractItems>(new Uri(queueSettings.SubtractItemsQueueAddress));

    services.AddMassTransitHostedService();

    services.AddGenericRequestClient();
}