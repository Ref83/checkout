using Interview.PaymentExecutor.Host.Composition;
using Interview.PaymentExecutor.Host.Composition.Subscriptions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services
    .ConfigureHttpServices(builder.Configuration)
    .ConfigureKafka(builder.Configuration)
    .AddApplicationServices(builder.Configuration);

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.Run();