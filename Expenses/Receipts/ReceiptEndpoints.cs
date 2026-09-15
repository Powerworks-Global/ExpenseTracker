namespace CratisApp.Expenses.Receipts;

using Google.Cloud.Storage.V1;

// Minimal-API endpoints, deliberately not an MVC [ApiController]: Cratis's proxy generator scans
// controller-based endpoints too and its reflection (IFormFile/IActionResult) crashes on this
// shape. A receipt photo is a pre-submission artefact referenced by URL from ExpenseSubmitted,
// not an event-sourced fact (see Expense Tracker.md's "Receipt handling" design note) - it was
// never meant to go through the generated command/query proxy surface anyway.
public static class ReceiptEndpoints
{
    static readonly string[] AllowedContentTypes = ["image/jpeg", "image/png", "image/webp", "image/heic"];
    const long MaxSizeBytes = 10 * 1024 * 1024;

    public static void MapReceiptEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/receipts");

        group.MapPost("/", async (IFormFile file, StorageClient storage, IConfiguration configuration) =>
        {
            if (file.Length == 0)
            {
                return Results.BadRequest("No file uploaded");
            }

            if (file.Length > MaxSizeBytes)
            {
                return Results.BadRequest($"File exceeds the {MaxSizeBytes / (1024 * 1024)}MB limit");
            }

            if (!AllowedContentTypes.Contains(file.ContentType))
            {
                return Results.BadRequest($"Unsupported content type '{file.ContentType}'. Allowed: {string.Join(", ", AllowedContentTypes)}");
            }

            var bucket = configuration["Receipts:BucketName"]
                ?? throw new InvalidOperationException("Receipts:BucketName is not configured");

            var extension = file.ContentType switch
            {
                "image/jpeg" => "jpg",
                "image/png" => "png",
                "image/webp" => "webp",
                "image/heic" => "heic",
                _ => "bin"
            };
            var objectName = $"{DateTime.UtcNow:yyyy/MM/dd}/{Guid.NewGuid()}.{extension}";

            await using var stream = file.OpenReadStream();
            await storage.UploadObjectAsync(bucket, objectName, file.ContentType, stream);

            // Deliberately not a public/signed GCS URL: receipts can carry real financial detail,
            // so the bucket stays private and reads go through this API, same trust boundary as
            // every other read in the app.
            return Results.Ok(new { url = $"/api/receipts/{objectName}" });
        }).DisableAntiforgery();

        group.MapGet("/{*objectName}", async (string objectName, StorageClient storage, IConfiguration configuration) =>
        {
            var bucket = configuration["Receipts:BucketName"]
                ?? throw new InvalidOperationException("Receipts:BucketName is not configured");

            var stream = new MemoryStream();
            var obj = await storage.DownloadObjectAsync(bucket, objectName, stream);
            stream.Position = 0;

            return Results.File(stream, obj.ContentType ?? "application/octet-stream");
        });
    }
}
