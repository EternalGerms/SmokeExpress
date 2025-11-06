// Projeto Smoke Express - Autores: Bruno Bueno e Matheus Esposto
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Diagnostics.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;
using SmokeExpress.Web.Data;
using SmokeExpress.Web.Models;
using SmokeExpress.Web.Security;
using SmokeExpress.Web.Services;
using SmokeExpress.Web.Routing;

var builder = WebApplication.CreateBuilder(args);

// Configuração da conexão com o SQL Server
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("A string de conexão 'DefaultConnection' não foi configurada.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services
    .AddDefaultIdentity<ApplicationUser>(options =>
    {
        // Regras de segurança básicas para senhas e contas
        options.SignIn.RequireConfirmedAccount = false;
        options.User.RequireUniqueEmail = true;
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = false;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredLength = 8;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddScoped<IPasswordHasher<ApplicationUser>, BcryptPasswordHasher>();

builder.Services.ConfigureApplicationCookie(options =>
{
    // Garante cookies seguros e HttpOnly de acordo com o requisito
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.SlidingExpiration = true;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.LoginPath = "/account/login";
    options.LogoutPath = "/account/logout";
    options.AccessDeniedPath = "/account/login";
    options.ReturnUrlParameter = "returnUrl";
});

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddMudServices();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddHttpClient();

// Serviços de domínio
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IImageUploadService, ImageUploadService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IAddressService, AddressService>();

var app = builder.Build();

// Seed inicial: criar roles e usuário administrador
try
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var logger = services.GetRequiredService<ILogger<Program>>();

        // Criar roles se não existirem
        const string adminRole = "Admin";
        const string userRole = "User";

        if (!await roleManager.RoleExistsAsync(adminRole))
        {
            await roleManager.CreateAsync(new IdentityRole(adminRole));
            logger.LogInformation("Role '{Role}' criada com sucesso.", adminRole);
        }

        if (!await roleManager.RoleExistsAsync(userRole))
        {
            await roleManager.CreateAsync(new IdentityRole(userRole));
            logger.LogInformation("Role '{Role}' criada com sucesso.", userRole);
        }

        // Criar usuário administrador se não existir
        const string adminEmail = "admin@smokeexpress.com";
        const string adminPassword = "Admin@123";

        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                NomeCompleto = "Administrador Smoke Express",
                DocumentoFiscal = "00000000000", // CPF genérico para admin
                TipoCliente = "PF",
                DataNascimento = DateTime.UtcNow.AddYears(-30),
                Rua = "Rua Administrativa",
                Numero = "1",
                Cidade = "São Paulo",
                Bairro = "Centro",
                ConsentiuMarketing = false,
                TermosAceitosEm = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(adminUser, adminPassword);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, adminRole);
                await userManager.AddToRoleAsync(adminUser, userRole);
                logger.LogInformation("Usuário administrador '{Email}' criado com sucesso.", adminEmail);
            }
            else
            {
                logger.LogError("Erro ao criar usuário administrador: {Errors}",
                    string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
        else
        {
            logger.LogInformation("Usuário administrador '{Email}' já existe no sistema.", adminEmail);
        }

        // Seed de categorias: criar categorias principais e subcategorias
        var context = services.GetRequiredService<ApplicationDbContext>();
        
        var categoriasParaCriar = new[]
        {
            // Categorias principais
            new Category { Nome = "Bong", Descricao = "Bongs em diversos tamanhos e estilos para uma experiência premium." },
            new Category { Nome = "Pipe", Descricao = "Pipes de alta qualidade em diferentes materiais e designs." },
            new Category { Nome = "Seda", Descricao = "Sedas profissionais em vários tamanhos e sabores." },
            new Category { Nome = "Piteira", Descricao = "Piteiras reutilizáveis e descartáveis para maior conforto." },
            new Category { Nome = "Dichavador", Descricao = "Dichavadores práticos e eficientes de diversos materiais." },
            new Category { Nome = "Isqueiro e Cinzeiro", Descricao = "Isqueiros e cinzeiros para completar sua experiência." },
            new Category { Nome = "Acessórios", Descricao = "Acessórios essenciais para conservação e uso de produtos." },
            new Category { Nome = "Caixa", Descricao = "Caixas e kits selecionados com produtos especiais." },
            new Category { Nome = "Marca", Descricao = "Produtos das principais marcas do mercado." },
            new Category { Nome = "Promoções", Descricao = "Ofertas especiais e produtos em promoção." },
            
            // Subcategorias - Bong
            new Category { Nome = "Percolator", Descricao = "Bongs com sistema de percolação para filtragem aprimorada." },
            new Category { Nome = "Mini Bong", Descricao = "Bongs compactos e portáteis ideais para viagens." },
            new Category { Nome = "Bong de Silicone", Descricao = "Bongs flexíveis e resistentes de silicone." },
            
            // Subcategorias - Pipe
            new Category { Nome = "Pipes de Metal", Descricao = "Pipes duráveis e resistentes de metal." },
            new Category { Nome = "Pipes de Vidro", Descricao = "Pipes de vidro transparente com designs únicos." },
            new Category { Nome = "Pipes de Madeira", Descricao = "Pipes clássicos de madeira com acabamento artesanal." },
            
            // Subcategorias - Seda
            new Category { Nome = "Seda King Size", Descricao = "Sedas no formato King Size para baseados maiores." },
            new Category { Nome = "Seda Slim", Descricao = "Sedas Slim para baseados mais finos e elegantes." },
            new Category { Nome = "Seda Flavor", Descricao = "Sedas saborizadas para uma experiência aromática." },
            
            // Subcategorias - Piteira
            new Category { Nome = "Piteira de Vidro", Descricao = "Piteiras reutilizáveis de vidro com filtro integrado." },
            new Category { Nome = "Piteira de Papel", Descricao = "Piteiras descartáveis de papel biodegradável." },
            new Category { Nome = "Piteira Reutilizável", Descricao = "Piteiras ecológicas reutilizáveis de diversos materiais." },
            
            // Subcategorias - Dichavador
            new Category { Nome = "Dichavador de Metal", Descricao = "Dichavadores robustos de metal com dentes afiados." },
            new Category { Nome = "Dichavador Acrílico", Descricao = "Dichavadores leves e coloridos de acrílico." },
            new Category { Nome = "Dichavador 4 Partes", Descricao = "Dichavadores com compartimento para coleta de kief." },
            
            // Subcategorias - Isqueiro e Cinzeiro
            new Category { Nome = "Isqueiros Recarregáveis", Descricao = "Isqueiros econômicos e ecológicos recarregáveis." },
            new Category { Nome = "Isqueiros Jet", Descricao = "Isqueiros à prova de vento para uso externo." },
            new Category { Nome = "Cinzeiros Portáteis", Descricao = "Cinzeiros compactos e práticos para uso em qualquer lugar." },
            
            // Subcategorias - Acessórios
            new Category { Nome = "Caixas Herméticas", Descricao = "Caixas herméticas para conservação de produtos." },
            new Category { Nome = "Filtros", Descricao = "Filtros para pipes e bongs para uma experiência mais suave." },
            new Category { Nome = "Bolsas e Cases", Descricao = "Bolsas e cases para transporte seguro de seus produtos." },
            
            // Subcategorias - Caixa
            new Category { Nome = "Club Mensal", Descricao = "Caixas do clube mensal com produtos selecionados." },
            new Category { Nome = "Kits Selecionados", Descricao = "Kits especiais com combinações de produtos." },
            new Category { Nome = "Presentes", Descricao = "Kits presentes perfeitos para presentear." },
            
            // Subcategorias - Marca
            new Category { Nome = "Smoke Express", Descricao = "Produtos exclusivos da marca Smoke Express." },
            new Category { Nome = "Raw", Descricao = "Produtos da marca Raw, líder mundial em sedas." },
            new Category { Nome = "Lion Rolling Circus", Descricao = "Produtos da marca Lion Rolling Circus." }
        };

        foreach (var categoria in categoriasParaCriar)
        {
            var existe = await context.Categories
                .AnyAsync(c => c.Nome.ToLower() == categoria.Nome.ToLower());
            
            if (!existe)
            {
                context.Categories.Add(categoria);
                logger.LogInformation("Categoria '{Nome}' criada com sucesso.", categoria.Nome);
            }
        }

        await context.SaveChangesAsync();
        logger.LogInformation("Seed de categorias concluído.");
    }
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "Erro ao executar seed inicial do banco de dados.");
    // Não lança a exceção para não impedir a inicialização da aplicação
}

app.MapAccountEndpoints();
app.MapProductEndpoints();
app.MapAddressEndpoints();
app.MapOrderEndpoints();

// Pipeline de requisições HTTP
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.MapStaticAssets();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
