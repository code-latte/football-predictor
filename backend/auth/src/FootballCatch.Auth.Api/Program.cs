using FootballCatch.Auth.Api.Middlewares;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();
app.UseMiddleware<ExceptionHandlerMiddleware>();
app.Run();
