using Application.Parley.Workflow.Nodes.CatFacts;
using Parley.Configuration;

var builder = WebApplication.CreateBuilder(args);

ParleyConfiguration.ConfigureParley(builder.Services,
                                    builder.Configuration,
                                    useDefaultMongoDb: true);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

await ParleyConfiguration.PreloadNodes(app, typeof(CatFactsNode).Assembly);

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseCors("AllowAll");
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();