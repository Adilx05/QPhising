using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using QPhising.Application.Common.Abstractions.Exports;
using QPhising.Application.Features.Exports.QueueExportJob;
using QPhising.Domain.Abstractions;
using QPhising.Domain.Exports;
using QPhising.Domain.Tasks;
using Xunit;

namespace QPhising.API.IntegrationTests;

public sealed class ExportEndpointsAuthorizationTests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly ApiWebApplicationFactory _factory;

    public ExportEndpointsAuthorizationTests(ApiWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Exports_Queue_Should_Reject_Unauthenticated_Request()
    {
        using var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/exports", new
        {
            exportType = ExportType.CampaignReport,
            format = ExportFormat.Excel
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Exports_Queue_Should_Return_Created_For_Viewer_With_Owner_From_Claims()
    {
        var exportRepository = new InMemoryExportJobRepository();
        var taskRepository = new InMemoryQueuedTaskRepository();

        using var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddSingleton<IExportJobRepository>(exportRepository);
                services.AddSingleton<IQueuedTaskRepository>(taskRepository);
            });
        }).CreateClient();

        client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "Test");
        client.DefaultRequestHeaders.TryAddWithoutValidation(TestAuthHandler.RolesHeader, AuthorizationPolicies.Viewer);
        client.DefaultRequestHeaders.TryAddWithoutValidation(TestAuthHandler.UserIdHeader, "viewer-user");

        var response = await client.PostAsJsonAsync("/api/exports", new
        {
            exportType = ExportType.AnalyticsReport,
            format = ExportFormat.Pdf,
            correlationId = "corr-123"
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<ExportJobResponse>();
        Assert.NotNull(payload);
        Assert.Equal("viewer-user", payload!.OwnerUserId);
        Assert.Equal(ExportJobStatus.Queued, payload.Status);
        Assert.Equal(ExportType.AnalyticsReport, payload.ExportType);
        Assert.Equal(ExportFormat.Pdf, payload.Format);
    }

    [Fact]
    public async Task Exports_Status_Should_Reject_NonOwner_With_Forbidden()
    {
        var exportJob = ExportJob.Create("owner-user", ExportType.CampaignReport, ExportFormat.Excel);
        exportJob.Queue();

        using var client = CreateClientWithExportJob(exportJob, new InMemoryExportFileStorage());
        client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "Test");
        client.DefaultRequestHeaders.TryAddWithoutValidation(TestAuthHandler.RolesHeader, AuthorizationPolicies.Viewer);
        client.DefaultRequestHeaders.TryAddWithoutValidation(TestAuthHandler.UserIdHeader, "other-user");

        var response = await client.GetAsync($"/api/exports/{exportJob.Id}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Exports_Download_Should_Return_File_For_Owner_When_Completed()
    {
        var exportStorage = new InMemoryExportFileStorage();
        var exportJob = ExportJob.Create("owner-user", ExportType.CampaignReport, ExportFormat.Excel);
        exportJob.Queue();
        exportJob.StartProcessing();
        exportJob.Complete(
            fileName: "campaign-report.xlsx",
            storagePath: "/tmp/exports/campaign-report.xlsx",
            contentType: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            fileSizeBytes: 4,
            expiresAt: DateTimeOffset.UtcNow.AddHours(1));

        exportStorage.Files[exportJob.StoragePath!] = [1, 2, 3, 4];

        using var client = CreateClientWithExportJob(exportJob, exportStorage);
        client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "Test");
        client.DefaultRequestHeaders.TryAddWithoutValidation(TestAuthHandler.RolesHeader, AuthorizationPolicies.Viewer);
        client.DefaultRequestHeaders.TryAddWithoutValidation(TestAuthHandler.UserIdHeader, "owner-user");

        var response = await client.GetAsync($"/api/exports/{exportJob.Id}/download");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", response.Content.Headers.ContentType?.MediaType);
        Assert.Equal("attachment; filename=campaign-report.xlsx; filename*=UTF-8''campaign-report.xlsx", response.Content.Headers.ContentDisposition?.ToString());

        byte[] bytes = await response.Content.ReadAsByteArrayAsync();
        Assert.Equal(new byte[] { 1, 2, 3, 4 }, bytes);
    }

    private HttpClient CreateClientWithExportJob(ExportJob exportJob, InMemoryExportFileStorage exportFileStorage)
    {
        return _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddSingleton<IExportJobRepository>(new InMemoryExportJobRepository(exportJob));
                services.AddSingleton<IExportFileStorage>(exportFileStorage);
            });
        }).CreateClient();
    }

    private sealed record ExportJobResponse(
        Guid Id,
        string OwnerUserId,
        ExportType ExportType,
        ExportFormat Format,
        ExportJobStatus Status);

    private sealed class InMemoryExportJobRepository : IExportJobRepository
    {
        private readonly Dictionary<Guid, ExportJob> _items;

        public InMemoryExportJobRepository(params ExportJob[] items)
        {
            _items = items.ToDictionary(item => item.Id, item => item);
        }

        public Task<ExportJob?> GetByIdAsync(Guid exportJobId, CancellationToken cancellationToken = default)
        {
            _items.TryGetValue(exportJobId, out ExportJob? job);
            return Task.FromResult(job);
        }

        public Task<IReadOnlyCollection<ExportJob>> ListAsync(ExportJobReadCriteria criteria, CancellationToken cancellationToken = default)
        {
            return Task.FromResult((IReadOnlyCollection<ExportJob>)_items.Values.ToArray());
        }

        public Task AddAsync(ExportJob exportJob, CancellationToken cancellationToken = default)
        {
            _items[exportJob.Id] = exportJob;
            return Task.CompletedTask;
        }

        public void Update(ExportJob exportJob)
        {
            _items[exportJob.Id] = exportJob;
        }
    }

    private sealed class InMemoryQueuedTaskRepository : IQueuedTaskRepository
    {
        public Task AddAsync(QueuedTask task, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task<QueuedTask?> GetByIdAsync(Guid taskId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<QueuedTask?>(null);
        }

        public Task<int> RequeueExpiredClaimsAsync(DateTimeOffset? asOf = null, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(0);
        }

        public Task<QueuedTask?> ClaimNextAsync(TimeSpan leaseDuration, DateTimeOffset? asOf = null, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<QueuedTask?>(null);
        }

        public void Update(QueuedTask task)
        {
        }
    }

    private sealed class InMemoryExportFileStorage : IExportFileStorage
    {
        public Dictionary<string, byte[]> Files { get; } = [];

        public Task<StoredExportFile> SaveAsync(Guid exportJobId, ExportBinaryFile file, CancellationToken cancellationToken = default)
        {
            string path = $"/tmp/exports/{exportJobId:N}-{file.FileName}";
            Files[path] = file.Content;
            return Task.FromResult(new StoredExportFile(path, file.Content.LongLength));
        }

        public Task<ExportFileContent?> TryReadAsync(string storagePath, CancellationToken cancellationToken = default)
        {
            if (!Files.TryGetValue(storagePath, out byte[]? bytes))
            {
                return Task.FromResult<ExportFileContent?>(null);
            }

            return Task.FromResult<ExportFileContent?>(new ExportFileContent(bytes, bytes.LongLength));
        }
    }
}
