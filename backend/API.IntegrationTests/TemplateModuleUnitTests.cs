using AutoMapper;
using Microsoft.Extensions.Logging.Abstractions;
using QPhising.Application.Common;
using QPhising.Application.Common.Abstractions;
using QPhising.Application.Features.Templates.ArchiveTemplate;
using QPhising.Application.Features.Templates.CreateTemplate;
using QPhising.Application.Features.Templates.GetTemplateById;
using QPhising.Application.Features.Templates.ListTemplates;
using QPhising.Application.Features.Templates.PublishTemplate;
using QPhising.Application.Features.Templates.UpdateTemplate;
using QPhising.Domain.Abstractions;
using QPhising.Domain.Templates;
using Xunit;

namespace QPhising.API.IntegrationTests;

public sealed class TemplateModuleUnitTests
{
    private readonly IMapper _mapper;
    private readonly TemplateHtmlSanitizer _templateHtmlSanitizer = new();
    private readonly ITemplateVariableSubstitutionEngine _templateVariableSubstitutionEngine = new TemplateVariableSubstitutionEngine();

    public TemplateModuleUnitTests()
    {
        MapperConfiguration configuration = new(cfg =>
        {
            cfg.AddProfile<CreateTemplateMappingProfile>();
            cfg.AddProfile<UpdateTemplateMappingProfile>();
            cfg.AddProfile<TemplateReadMappingProfile>();
            cfg.AddProfile<PublishTemplateMappingProfile>();
            cfg.AddProfile<ArchiveTemplateMappingProfile>();
        }, NullLoggerFactory.Instance);

        _mapper = configuration.CreateMapper();
    }

    [Fact]
    public async Task CreateTemplateCommandHandler_Should_Persist_Draft_Template()
    {
        var repository = new InMemoryTemplateRepository();
        var unitOfWork = new RecordingUnitOfWork();
        var handler = new CreateTemplateCommandHandler(repository, unitOfWork, _templateHtmlSanitizer, _mapper);

        var result = await handler.Handle(new CreateTemplateCommand(
            "Security Awareness",
            TemplateType.Email,
            "<h1>Hello {{first_name}}</h1>",
            ["first_name"]), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(TemplateStatus.Draft, result.Value!.Status);
        Assert.Contains("first_name", result.Value.Variables);
        Assert.Equal(1, repository.TemplateCount);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
        Assert.Equal("<h1>Hello {{first_name}}</h1>", result.Value.HtmlContent);
    }

    [Fact]
    public async Task CreateTemplateCommandHandler_Should_Reject_Unsafe_Html_Markup()
    {
        var repository = new InMemoryTemplateRepository();
        var unitOfWork = new RecordingUnitOfWork();
        var handler = new CreateTemplateCommandHandler(repository, unitOfWork, _templateHtmlSanitizer, _mapper);

        var result = await handler.Handle(new CreateTemplateCommand(
            "Unsafe Template",
            TemplateType.Email,
            "<div>Welcome</div><script>alert('xss')</script>"), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.True(result.Errors.Single().Contains("unsafe markup", StringComparison.OrdinalIgnoreCase));
        Assert.Equal(0, repository.TemplateCount);
        Assert.Equal(0, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task UpdateTemplateCommandHandler_Should_Return_Failure_When_Template_Not_Found()
    {
        var repository = new InMemoryTemplateRepository();
        var unitOfWork = new RecordingUnitOfWork();
        var handler = new UpdateTemplateCommandHandler(repository, unitOfWork, _templateHtmlSanitizer, _mapper);

        var result = await handler.Handle(new UpdateTemplateCommand(
            Guid.NewGuid(),
            "Updated",
            TemplateType.Email,
            "<p>Updated</p>",
            ["first_name"]), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("was not found", result.Errors.Single());
        Assert.Equal(0, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task PublishTemplateCommandHandler_Should_Transition_Draft_To_Published()
    {
        var template = Template.Create("Publish Me", TemplateType.Email, "<p>{{first_name}}</p>", ["first_name"]);
        var repository = new InMemoryTemplateRepository(template);
        var unitOfWork = new RecordingUnitOfWork();
        var handler = new PublishTemplateCommandHandler(repository, unitOfWork, _mapper);

        var result = await handler.Handle(new PublishTemplateCommand(template.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(TemplateStatus.Published, result.Value!.Status);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task ArchiveTemplateCommandHandler_Should_Return_Failure_For_Invalid_Transition()
    {
        var template = Template.Create("Archive Me", TemplateType.Email, "<p>{{first_name}}</p>", ["first_name"]);
        template.Archive();

        var repository = new InMemoryTemplateRepository(template);
        var unitOfWork = new RecordingUnitOfWork();
        var handler = new ArchiveTemplateCommandHandler(repository, unitOfWork, _mapper);

        var result = await handler.Handle(new ArchiveTemplateCommand(template.Id), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("is not allowed", result.Errors.Single());
        Assert.Equal(0, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task GetTemplateByIdQueryHandler_Should_Return_Failure_When_Not_Found()
    {
        var repository = new InMemoryTemplateRepository();
        var handler = new GetTemplateByIdQueryHandler(repository, _mapper);

        var result = await handler.Handle(new GetTemplateByIdQuery(Guid.NewGuid()), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("was not found", result.Errors.Single());
    }

    [Fact]
    public async Task ListTemplatesQueryHandler_Should_Map_Read_Response()
    {
        var template = Template.Create("List Me", TemplateType.LandingPage, "<p>{{company_name}}</p>", ["company_name"]);
        var repository = new InMemoryTemplateRepository(template);
        var handler = new ListTemplatesQueryHandler(repository, _mapper);

        var result = await handler.Handle(new ListTemplatesQuery(Type: TemplateType.LandingPage), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value!.Items);
        Assert.Equal("List Me", result.Value.Items.Single().Name);
        Assert.Contains("company_name", result.Value.Items.Single().Variables);
    }

    [Fact]
    public void CreateTemplateCommandValidator_Should_Reject_Empty_Name()
    {
        var validator = new CreateTemplateCommandValidator();

        var validation = validator.Validate(new CreateTemplateCommand(
            string.Empty,
            TemplateType.Email,
            "<h1>{{first_name}}</h1>",
            ["first_name"]));

        Assert.False(validation.IsValid);
        Assert.Contains(validation.Errors, error => error.PropertyName == nameof(CreateTemplateCommand.Name));
    }

    [Fact]
    public void ListTemplatesQueryValidator_Should_Reject_Invalid_Pagination()
    {
        var validator = new ListTemplatesQueryValidator();

        var validation = validator.Validate(new ListTemplatesQuery(PageNumber: 0, PageSize: 101));

        Assert.False(validation.IsValid);
        Assert.Contains(validation.Errors, error => error.PropertyName == nameof(ListTemplatesQuery.PageNumber));
        Assert.Contains(validation.Errors, error => error.PropertyName == nameof(ListTemplatesQuery.PageSize));
    }

    [Fact]
    public void TemplateHtmlSanitizer_Should_Remove_Disallowed_Attributes_And_Tags()
    {
        var sanitizer = new TemplateHtmlSanitizer();
        var input = "<div onclick=\"hack()\"><a href=\"https://example.com\" style=\"color:red\">link</a><video src=\"/sample.mp4\"></video></div>";

        var result = sanitizer.Sanitize(input);

        Assert.True(result.IsSuccess);
        Assert.Equal("<div><a href=\"https://example.com\">link</a></div>", result.Value);
    }

    [Fact]
    public void TemplateVariableSubstitutionEngine_Should_Render_Html_With_Encoded_Values()
    {
        var result = _templateVariableSubstitutionEngine.Render(
            "<h1>Hello {{first_name}}</h1><p>{{company_name}}</p>",
            ["first_name", "company_name"],
            new Dictionary<string, string>
            {
                ["first_name"] = "Alice <Admin>",
                ["company_name"] = "QPhising"
            });

        Assert.True(result.IsSuccess);
        Assert.Equal("<h1>Hello Alice &lt;Admin&gt;</h1><p>QPhising</p>", result.Value);
    }

    [Fact]
    public void TemplateVariableSubstitutionEngine_Should_Fail_When_Placeholder_Value_Is_Missing()
    {
        var result = _templateVariableSubstitutionEngine.Render(
            "<h1>Hello {{first_name}}</h1><p>{{company_name}}</p>",
            ["first_name", "company_name"],
            new Dictionary<string, string>
            {
                ["first_name"] = "Alice"
            });

        Assert.False(result.IsSuccess);
        Assert.Contains("Missing values for placeholders: company_name.", result.Errors.Single());
    }

    [Fact]
    public void TemplateVariableSubstitutionEngine_Should_Fail_When_Template_Contains_Undeclared_Placeholder()
    {
        var result = _templateVariableSubstitutionEngine.Render(
            "<h1>Hello {{first_name}}</h1><p>{{department}}</p>",
            ["first_name", "company_name"],
            new Dictionary<string, string>
            {
                ["first_name"] = "Alice",
                ["company_name"] = "QPhising"
            });

        Assert.False(result.IsSuccess);
        Assert.Contains("undeclared placeholder", result.Errors.Single(), StringComparison.OrdinalIgnoreCase);
    }

    private sealed class RecordingUnitOfWork : IUnitOfWork
    {
        public int SaveChangesCallCount { get; private set; }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SaveChangesCallCount++;
            return Task.FromResult(1);
        }
    }

    private sealed class InMemoryTemplateRepository : ITemplateRepository
    {
        private readonly Dictionary<Guid, Template> _templates;

        public InMemoryTemplateRepository(params Template[] templates)
        {
            _templates = templates.ToDictionary(template => template.Id, template => template);
        }

        public int TemplateCount => _templates.Count;

        public Task<Template?> GetByIdAsync(Guid templateId, CancellationToken cancellationToken = default)
        {
            _templates.TryGetValue(templateId, out Template? template);
            return Task.FromResult(template);
        }

        public Task<IReadOnlyCollection<Template>> ListAsync(TemplateReadCriteria criteria, CancellationToken cancellationToken = default)
        {
            IEnumerable<Template> query = _templates.Values;

            if (criteria.Status.HasValue)
            {
                query = query.Where(template => template.Status == criteria.Status.Value);
            }

            if (criteria.Type.HasValue)
            {
                query = query.Where(template => template.Type == criteria.Type.Value);
            }

            if (!string.IsNullOrWhiteSpace(criteria.SearchTerm))
            {
                string search = criteria.SearchTerm.Trim();
                query = query.Where(template => template.Name.Contains(search, StringComparison.OrdinalIgnoreCase));
            }

            IReadOnlyCollection<Template> result = query
                .Skip((criteria.PageNumber - 1) * criteria.PageSize)
                .Take(criteria.PageSize)
                .ToArray();

            return Task.FromResult(result);
        }

        public Task<Template?> GetPublishedByNameAsync(string templateName, CancellationToken cancellationToken = default)
        {
            Template? template = _templates.Values.SingleOrDefault(current =>
                string.Equals(current.Name, templateName, StringComparison.OrdinalIgnoreCase) &&
                current.Status == TemplateStatus.Published);

            return Task.FromResult(template);
        }

        public Task AddAsync(Template template, CancellationToken cancellationToken = default)
        {
            _templates[template.Id] = template;
            return Task.CompletedTask;
        }

        public void Update(Template template)
        {
            _templates[template.Id] = template;
        }
    }
}
