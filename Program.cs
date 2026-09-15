using CratisApp.Expenses.Receipts;

var builder = WebApplication.CreateBuilder(args);
builder.AddCratis(
    configureChronicleBuilder: chronicleBuilder => chronicleBuilder.WithCamelCaseNamingPolicy(),
    configureArcBuilder: arcBuilder => arcBuilder.WithMongoDB(configureMongoDB: builder => builder.WithCamelCaseNamingPolicy()));
    
builder.Services.AddSingleton<CratisApp.Expenses.Payout.IPaymentGateway, CratisApp.Expenses.Payout.MockPaymentGateway>();
builder.Services.AddSingleton(_ => Google.Cloud.Storage.V1.StorageClient.Create());
builder.Services.AddControllers();
builder.Services.AddMvc();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options => options.AddConcepts());

var app = builder.Build();

app.UseRouting();

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseWebSockets();
app.MapControllers();
app.MapReceiptEndpoints();
app.UseCratis();
 
app.UseSwagger();
app.UseSwaggerUI();
app.MapFallbackToFile("/index.html");

await app.RunAsync();
