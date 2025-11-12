var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build(); // app object runs the web server

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    //app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "OCR MiniApp v1");
        c.RoutePrefix = string.Empty;
    });
}

app.UseHttpsRedirection(); // redirect HTTP traffic to HTTPS for secure communication

app.UseAuthorization();

app.MapControllers();

app.Run();
