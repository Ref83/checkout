using Interview.PaymentGateway.Host.Composition;
using Interview.PaymentGateway.Host.Middlewares;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services
    .ConfigureKafka(builder.Configuration)
    .ConfigurePostgresServices(builder.Configuration)
    .ConfigureApplicationServices(builder.Configuration)
    .ConfigureSubscriptions(builder.Configuration);

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();

app.Run();

