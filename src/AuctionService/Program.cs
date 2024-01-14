using AuctionService.Cnsumers;
using AuctionService.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddDbContext<AuctionDbContext>(opt =>
{
	opt.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddMassTransit(x =>
{
	x.AddEntityFrameworkOutbox<AuctionDbContext>(o =>
	{
		o.QueryDelay = TimeSpan.FromSeconds(30);
		o.UsePostgres();
		o.UseBusOutbox();
	});
	
	x.AddConsumersFromNamespaceContaining<AuctionCreatedFaultConsumer>();
	x.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("auction", includeNamespace: false));
	
	x.UsingRabbitMq((context, cfg) => 
	{
		cfg.ConfigureEndpoints(context);
	});
});

var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

try
{
	DbInitializer.InitDb(app);
}
catch (Exception ex)
{
	Console.WriteLine(ex);
}

app.Run();
