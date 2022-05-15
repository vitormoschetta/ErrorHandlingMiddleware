using System;
using System.Net;
using System.Text.Json;
using GlobalExceptionsTest.Middlewares;
using Microsoft.AspNetCore.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseResponseCaching();

app.UseAuthorization();

app.MapControllers();

app.Map("/map1", HandleMapTest1);

app.Map("/map2", HandleMapTest2);

app.MapWhen(context => context.Request.Query.ContainsKey("branch"), HandleBranch);

// app.UseExceptionHandler(options =>
// {
//     options.Run(async context =>
//     {
//         context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
//         context.Response.ContentType = "application/json";
//         var exceptionObject = context.Features.Get<IExceptionHandlerFeature>();
//         string errorMessage = null;
//         if (null != exceptionObject)
//         {
//             errorMessage = exceptionObject.Error.Message;
//         }
//         await context.Response.WriteAsync(JsonSerializer.Serialize(
//             new
//             {
//                 success = false,
//                 message = errorMessage ?? "Internal Server Error"
//             }));
//     });
// });

//app.UseMiddleware<ErrorHandlingMiddleware>();
//app.UseErrorHandling();

// Encadeie vários delegados de solicitação junto com o Use. O parâmetro next representa o próximo delegado no pipeline.
app.Use(async (context, next) =>
{
    Console.WriteLine("Init Middleware 1");

    try
    {
        await next.Invoke();
    }
    catch (System.Exception ex)
    {
        var result = JsonSerializer.Serialize(new { success = false, message = ex.Message });
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        await context.Response.WriteAsync(result);
    }

    Console.WriteLine("End Middleware 1");
});

app.Use(async (context, next) =>
{
    Console.WriteLine("Init Middleware 2");
    await next.Invoke();
    Console.WriteLine("End Middleware 2");
});

app.Run();

// A ordem em que os componentes do middleware são adicionados aqui define a ordem na qual os componentes do middleware são invocados 


static void HandleMapTest1(IApplicationBuilder app)
{
    app.Run(async context =>
    {
        await context.Response.WriteAsync("Map Test 1");
    });
}

static void HandleMapTest2(IApplicationBuilder app)
{
    app.Run(async context =>
    {
        await context.Response.WriteAsync("Map Test 2");
    });
}

static void HandleBranch(IApplicationBuilder app)
{
    app.Run(async context =>
    {
        var branchVer = context.Request.Query["branch"];
        await context.Response.WriteAsync($"Branch used = {branchVer}");
    });
}