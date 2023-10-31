using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using NSwag.AspNetCore;
using NSwag.Generation.Processors.Security;
using NSwag;
using OpenApiSecurityScheme = NSwag.OpenApiSecurityScheme;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.





builder.Services.AddControllers();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).
    AddJwtBearer(options =>
    {
        options.Audience = builder.Configuration["AAD:ResourceId"];
        options.Authority = $"{builder.Configuration["AAD:Instance"]}{builder.Configuration["AAD:TenantId"]}";
        options.TokenValidationParameters.ValidAudiences = new string[] { builder.Configuration["AAD:ClientId"], $"api://{builder.Configuration["AAD:ClientId"]}" };
    });

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

//builder.Services.AddSwaggerGen();
//builder.Services.AddSwaggerGen(c =>
//{
//    c.SwaggerDoc("v1", new OpenApiInfo { Title = "REST API", Version = "v1" });
//});
//builder.Services.AddOpenApiDocument();
builder.Services.AddOpenApiDocument(document =>
{
    document.Title = "WEB APIs";
    document.AddSecurity("bearer", Enumerable.Empty<string>(), new OpenApiSecurityScheme
    {
        Type = OpenApiSecuritySchemeType.OAuth2,
        Description = "Azure AAD Authentication",
        Flow = OpenApiOAuth2Flow.Implicit,
        Flows = new NSwag.OpenApiOAuthFlows()
        {
            Implicit = new NSwag.OpenApiOAuthFlow()
            {
                Scopes = new Dictionary<string, string>
                {
                    { $"api://{builder.Configuration["AAD:ClientId"]}/read", "Access Application" },
                },
                AuthorizationUrl = $"{builder.Configuration["AAD:Instance"]}{builder.Configuration["AAD:TenantId"]}/oauth2/v2.0/authorize",
                TokenUrl = $"{builder.Configuration["AAD:Instance"]}{builder.Configuration["AAD:TenantId"]}/oauth2/v2.0/token",
            },
        },
    });
    document.OperationProcessors.Add(new AspNetCoreOperationSecurityScopeProcessor("bearer"));
});

var app = builder.Build();

app.UseOpenApi(); // Serves the registered OpenAPI/Swagger documents by default on `/swagger/{documentName}/swagger.json`
//app.UseSwaggerUi3(); // Serves the Swagger UI 3 web ui to view the OpenAPI/Swagger documents by default on `/swagger`
app.UseSwaggerUi3(settings =>
{
    settings.OAuth2Client = new OAuth2ClientSettings
    {
        // Use the same client id as your application.
        // Alternatively you can register another application in the portal and use that as client id
        // Doing that you will have to create a client secret to access that application and get into space of secret management
        // This makes it easier to access the application and grab a token on behalf of user
        ClientId = builder.Configuration["AAD:ClientId"],
        AppName = "WebApiAuth-Client",
    };
});
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
     app.UseDeveloperExceptionPage();
   
    //app.UseSwagger();
    //app.UseSwaggerUI();

}
else
{
    
    //app.UseSwaggerUI(c =>
    //{
    //    c.SwaggerEndpoint("v1/swagger.json", "v1");
    //    c.RoutePrefix = string.Empty;

    //});
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
