using GrpcService;
using GrpcService.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Additional configuration is required to successfully run gRPC on macOS.
// For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682

string connStr = "Server=(localdb)\\mssqllocaldb;Database=grpcdbdb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True;";//(localdb)\\mssqllocaldb    DESKTOP-IDFUNTA\\SQLEXPRESS
builder.Services.AddDbContext<ApplicationContext>(options => options.UseSqlServer(connStr));

// Add services to the container.
builder.Services.AddGrpc();

var app = builder.Build();

app.MapGrpcService<DBManager>();
app.MapGet("/", () => "");

app.Run();