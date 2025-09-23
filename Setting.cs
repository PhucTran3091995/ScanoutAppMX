Microsoft.Extensions.Configuration
Microsoft.Extensions.Configuration.Json
Microsoft.Extensions.Configuration.Binder
Microsoft.Extensions.Configuration.EnvironmentVariables
Microsoft.EntityFrameworkCore
Microsoft.EntityFrameworkCore.Design
Microsoft.Extensions.Hosting
Pomelo.EntityFrameworkCore.MySql

dotnet ef dbcontext scaffold "server=127.0.0.1;user=root;password=root;database=mex_mes;" Pomelo.EntityFrameworkCore.MySql --output-dir Models --context-dir Data --context MMesDbContext -f

dotnet ef dbcontext scaffold "server=10.7.10.6;user=root;password=ivihaengsung@1;database=ars;" Pomelo.EntityFrameworkCore.MySql --output-dir Models --context-dir Data --context ArsDbContext -f



