using QPhising.Application.Contracts.Abstractions.Template;
using QPhising.Application.CQRS.Commands.Template;
using QPhising.Application.CQRS.Queries.Template;
using QPhising.Domain.Templates.Aggregates;
using QPhising.Domain.Templates.Enums;
using QPhising.Domain.Templates.ValueObjects;
using Xunit;

namespace QPhising.Api.Tests;

public sealed class TemplateApplicationUnitTests
{
    [Fact]
    public async Task CreateTemplateCommandHandler_ShouldCreateAndSaveTemplate()
    {
        var command = new CreateTemplateCommand(
            Name: "Test Template",
            HtmlContent: "<html>Hello</html>",
            Description: "A test template",
            Tags: new[] { "phishing", "awareness" });

        var repo = new FakeTemplateRepository();
        var handler = new CreateTemplateCommandHandler(repo);

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.Equal("Test Template", result.Name);
        Assert.Equal("<html>Hello</html>", result.HtmlContent);
        Assert.Equal("A test template", result.Description);
        Assert.Equal(TemplateLifecycleState.Draft, result.LifecycleState);
        Assert.Equal(1, result.Version);
        Assert.NotNull(repo.SavedTemplate);
    }

    [Fact]
    public async Task CreateTemplateCommandHandler_ShouldHandleNullDescriptionAndTags()
    {
        var command = new CreateTemplateCommand(
            Name: "Minimal Template",
            HtmlContent: "<html>Minimal</html>",
            Description: null,
            Tags: null);

        var repo = new FakeTemplateRepository();
        var handler = new CreateTemplateCommandHandler(repo);

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.Equal("Minimal Template", result.Name);
        Assert.Null(result.Description);
        Assert.Empty(result.Tags);
    }

    [Fact]
    public async Task UpdateTemplateCommandHandler_ShouldUpdateAndIncrementVersion()
    {
        var template = new TemplateAggregate(
            Guid.NewGuid(),
            new TemplateName("Original"),
            new TemplateContent("<html>Original</html>"),
            new TemplateMetadata("Original desc", new[] { "original" }));

        var repo = new FakeTemplateRepository(template);
        var handler = new UpdateTemplateCommandHandler(repo);

        var result = await handler.Handle(
            new UpdateTemplateCommand(
                template.Id,
                "Updated",
                "<html>Updated</html>",
                "Updated desc",
                new[] { "updated" }),
            CancellationToken.None);

        Assert.Equal("Updated", result.Name);
        Assert.Equal("<html>Updated</html>", result.HtmlContent);
        Assert.Equal("Updated desc", result.Description);
        Assert.Equal(2, result.Version);
        Assert.NotNull(repo.SavedTemplate);
    }

    [Fact]
    public async Task UpdateTemplateCommandHandler_ShouldThrowWhenNotFound()
    {
        var repo = new FakeTemplateRepository(null as TemplateAggregate);
        var handler = new UpdateTemplateCommandHandler(repo);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            handler.Handle(
                new UpdateTemplateCommand(Guid.NewGuid(), "Name", "<html></html>", null, null),
                CancellationToken.None));
    }

    [Fact]
    public async Task DeleteTemplateCommandHandler_ShouldSoftDelete()
    {
        var template = new TemplateAggregate(
            Guid.NewGuid(),
            new TemplateName("To Delete"),
            new TemplateContent("<html>Delete</html>"),
            new TemplateMetadata(null, null));

        var repo = new FakeTemplateRepository(template);
        var handler = new DeleteTemplateCommandHandler(repo);

        await handler.Handle(new DeleteTemplateCommand(template.Id), CancellationToken.None);

        Assert.NotNull(repo.DeletedTemplate);
        Assert.True(repo.DeletedTemplate!.IsDeleted);
    }

    [Fact]
    public async Task DeleteTemplateCommandHandler_ShouldThrowWhenNotFound()
    {
        var repo = new FakeTemplateRepository(null as TemplateAggregate);
        var handler = new DeleteTemplateCommandHandler(repo);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            handler.Handle(new DeleteTemplateCommand(Guid.NewGuid()), CancellationToken.None));
    }

    [Fact]
    public async Task PublishTemplateCommandHandler_ShouldPublish()
    {
        var template = new TemplateAggregate(
            Guid.NewGuid(),
            new TemplateName("To Publish"),
            new TemplateContent("<html>Publish</html>"),
            new TemplateMetadata(null, null));

        var repo = new FakeTemplateRepository(template);
        var handler = new PublishTemplateCommandHandler(repo);

        var result = await handler.Handle(new PublishTemplateCommand(template.Id), CancellationToken.None);

        Assert.Equal(TemplateLifecycleState.Published, result.LifecycleState);
        Assert.NotNull(repo.SavedTemplate);
    }

    [Fact]
    public async Task PublishTemplateCommandHandler_ShouldThrowWhenNotFound()
    {
        var repo = new FakeTemplateRepository(null as TemplateAggregate);
        var handler = new PublishTemplateCommandHandler(repo);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            handler.Handle(new PublishTemplateCommand(Guid.NewGuid()), CancellationToken.None));
    }

    [Fact]
    public async Task ArchiveTemplateCommandHandler_ShouldArchive()
    {
        var template = new TemplateAggregate(
            Guid.NewGuid(),
            new TemplateName("To Archive"),
            new TemplateContent("<html>Archive</html>"),
            new TemplateMetadata(null, null));

        var repo = new FakeTemplateRepository(template);
        var handler = new ArchiveTemplateCommandHandler(repo);

        var result = await handler.Handle(new ArchiveTemplateCommand(template.Id), CancellationToken.None);

        Assert.Equal(TemplateLifecycleState.Archived, result.LifecycleState);
        Assert.NotNull(repo.SavedTemplate);
    }

    [Fact]
    public async Task ArchiveTemplateCommandHandler_ShouldThrowWhenNotFound()
    {
        var repo = new FakeTemplateRepository(null as TemplateAggregate);
        var handler = new ArchiveTemplateCommandHandler(repo);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            handler.Handle(new ArchiveTemplateCommand(Guid.NewGuid()), CancellationToken.None));
    }

    [Fact]
    public async Task ListTemplatesQueryHandler_ShouldReturnAllTemplates()
    {
        var template = new TemplateAggregate(
            Guid.NewGuid(),
            new TemplateName("List Test"),
            new TemplateContent("<html>List</html>"),
            new TemplateMetadata(null, null));

        var repo = new FakeTemplateRepository(template);
        var handler = new ListTemplatesQueryHandler(repo);

        var results = await handler.Handle(new ListTemplatesQuery(), CancellationToken.None);

        Assert.Single(results);
        Assert.Equal("List Test", results.First().Name);
    }

    [Fact]
    public async Task GetTemplateByIdQueryHandler_ShouldReturnTemplate()
    {
        var template = new TemplateAggregate(
            Guid.NewGuid(),
            new TemplateName("Get By Id"),
            new TemplateContent("<html>Get</html>"),
            new TemplateMetadata(null, null));

        var repo = new FakeTemplateRepository(template);
        var handler = new GetTemplateByIdQueryHandler(repo);

        var result = await handler.Handle(new GetTemplateByIdQuery(template.Id), CancellationToken.None);

        Assert.Equal(template.Id, result.Id);
        Assert.Equal("Get By Id", result.Name);
    }

    [Fact]
    public async Task GetTemplateByIdQueryHandler_ShouldThrowWhenNotFound()
    {
        var repo = new FakeTemplateRepository(null as TemplateAggregate);
        var handler = new GetTemplateByIdQueryHandler(repo);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            handler.Handle(new GetTemplateByIdQuery(Guid.NewGuid()), CancellationToken.None));
    }

    [Fact]
    public void CreateTemplateCommandValidator_ShouldAcceptValidCommand()
    {
        var validator = new CreateTemplateCommandValidator();
        var command = new CreateTemplateCommand(
            Name: "Valid Template",
            HtmlContent: "<html>Valid</html>",
            Description: "A description",
            Tags: new[] { "tag1", "tag2" });

        var result = validator.Validate(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void CreateTemplateCommandValidator_ShouldRejectEmptyName()
    {
        var validator = new CreateTemplateCommandValidator();
        var command = new CreateTemplateCommand(
            Name: "",
            HtmlContent: "<html>Content</html>",
            Description: null,
            Tags: null);

        var result = validator.Validate(command);

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateTemplateCommand.Name));
    }

    [Fact]
    public void CreateTemplateCommandValidator_ShouldRejectNameExceedingMaxLength()
    {
        var validator = new CreateTemplateCommandValidator();
        var command = new CreateTemplateCommand(
            Name: new string('x', TemplateName.MaxLength + 1),
            HtmlContent: "<html>Content</html>",
            Description: null,
            Tags: null);

        var result = validator.Validate(command);

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateTemplateCommand.Name));
    }

    [Fact]
    public void CreateTemplateCommandValidator_ShouldRejectEmptyHtmlContent()
    {
        var validator = new CreateTemplateCommandValidator();
        var command = new CreateTemplateCommand(
            Name: "Template",
            HtmlContent: "",
            Description: null,
            Tags: null);

        var result = validator.Validate(command);

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateTemplateCommand.HtmlContent));
    }

    [Fact]
    public void CreateTemplateCommandValidator_ShouldRejectHtmlContentExceedingMaxLength()
    {
        var validator = new CreateTemplateCommandValidator();
        var command = new CreateTemplateCommand(
            Name: "Template",
            HtmlContent: new string('x', TemplateContent.MaxHtmlLength + 1),
            Description: null,
            Tags: null);

        var result = validator.Validate(command);

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateTemplateCommand.HtmlContent));
    }

    [Fact]
    public void CreateTemplateCommandValidator_ShouldRejectDescriptionExceedingMaxLength()
    {
        var validator = new CreateTemplateCommandValidator();
        var command = new CreateTemplateCommand(
            Name: "Template",
            HtmlContent: "<html>Content</html>",
            Description: new string('x', TemplateMetadata.MaxDescriptionLength + 1),
            Tags: null);

        var result = validator.Validate(command);

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateTemplateCommand.Description));
    }

    [Fact]
    public void CreateTemplateCommandValidator_ShouldRejectTooManyTags()
    {
        var validator = new CreateTemplateCommandValidator();
        var tags = Enumerable.Range(0, TemplateMetadata.MaxTagCount + 1).Select(i => $"tag{i}").ToArray();
        var command = new CreateTemplateCommand(
            Name: "Template",
            HtmlContent: "<html>Content</html>",
            Description: null,
            Tags: tags);

        var result = validator.Validate(command);

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateTemplateCommand.Tags));
    }

    [Fact]
    public void CreateTemplateCommandValidator_ShouldRejectTagExceedingMaxLength()
    {
        var validator = new CreateTemplateCommandValidator();
        var longTag = new string('x', TemplateMetadata.MaxTagLength + 1);
        var command = new CreateTemplateCommand(
            Name: "Template",
            HtmlContent: "<html>Content</html>",
            Description: null,
            Tags: new[] { longTag });

        var result = validator.Validate(command);

        Assert.Contains(result.Errors, e =>
            e.PropertyName.Contains(nameof(CreateTemplateCommand.Tags)));
    }

    [Fact]
    public void UpdateTemplateCommandValidator_ShouldAcceptValidCommand()
    {
        var validator = new UpdateTemplateCommandValidator();
        var command = new UpdateTemplateCommand(
            TemplateId: Guid.NewGuid(),
            Name: "Valid Template",
            HtmlContent: "<html>Valid</html>",
            Description: null,
            Tags: null);

        var result = validator.Validate(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void UpdateTemplateCommandValidator_ShouldRejectEmptyTemplateId()
    {
        var validator = new UpdateTemplateCommandValidator();
        var command = new UpdateTemplateCommand(
            TemplateId: Guid.Empty,
            Name: "Name",
            HtmlContent: "<html>Content</html>",
            Description: null,
            Tags: null);

        var result = validator.Validate(command);

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateTemplateCommand.TemplateId));
    }

    [Fact]
    public void UpdateTemplateCommandValidator_ShouldRejectEmptyName()
    {
        var validator = new UpdateTemplateCommandValidator();
        var command = new UpdateTemplateCommand(
            TemplateId: Guid.NewGuid(),
            Name: "",
            HtmlContent: "<html>Content</html>",
            Description: null,
            Tags: null);

        var result = validator.Validate(command);

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateTemplateCommand.Name));
    }

    [Fact]
    public void UpdateTemplateCommandValidator_ShouldRejectEmptyHtmlContent()
    {
        var validator = new UpdateTemplateCommandValidator();
        var command = new UpdateTemplateCommand(
            TemplateId: Guid.NewGuid(),
            Name: "Template",
            HtmlContent: "",
            Description: null,
            Tags: null);

        var result = validator.Validate(command);

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateTemplateCommand.HtmlContent));
    }

    [Fact]
    public void UpdateTemplateCommandValidator_ShouldRejectHtmlContentExceedingMaxLength()
    {
        var validator = new UpdateTemplateCommandValidator();
        var command = new UpdateTemplateCommand(
            TemplateId: Guid.NewGuid(),
            Name: "Template",
            HtmlContent: new string('x', TemplateContent.MaxHtmlLength + 1),
            Description: null,
            Tags: null);

        var result = validator.Validate(command);

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateTemplateCommand.HtmlContent));
    }

    [Fact]
    public void UpdateTemplateCommandValidator_ShouldRejectDescriptionExceedingMaxLength()
    {
        var validator = new UpdateTemplateCommandValidator();
        var command = new UpdateTemplateCommand(
            TemplateId: Guid.NewGuid(),
            Name: "Template",
            HtmlContent: "<html>Content</html>",
            Description: new string('x', TemplateMetadata.MaxDescriptionLength + 1),
            Tags: null);

        var result = validator.Validate(command);

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateTemplateCommand.Description));
    }

    [Fact]
    public void UpdateTemplateCommandValidator_ShouldRejectTooManyTags()
    {
        var validator = new UpdateTemplateCommandValidator();
        var tags = Enumerable.Range(0, TemplateMetadata.MaxTagCount + 1).Select(i => $"tag{i}").ToArray();
        var command = new UpdateTemplateCommand(
            TemplateId: Guid.NewGuid(),
            Name: "Template",
            HtmlContent: "<html>Content</html>",
            Description: null,
            Tags: tags);

        var result = validator.Validate(command);

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateTemplateCommand.Tags));
    }

    [Fact]
    public void DeleteTemplateCommandValidator_ShouldRejectEmptyTemplateId()
    {
        var validator = new DeleteTemplateCommandValidator();
        var command = new DeleteTemplateCommand(Guid.Empty);

        var result = validator.Validate(command);

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(DeleteTemplateCommand.TemplateId));
    }

    [Fact]
    public void DeleteTemplateCommandValidator_ShouldAcceptValidCommand()
    {
        var validator = new DeleteTemplateCommandValidator();
        var command = new DeleteTemplateCommand(Guid.NewGuid());

        var result = validator.Validate(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void PublishTemplateCommandValidator_ShouldRejectEmptyTemplateId()
    {
        var validator = new PublishTemplateCommandValidator();
        var command = new PublishTemplateCommand(Guid.Empty);

        var result = validator.Validate(command);

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(PublishTemplateCommand.TemplateId));
    }

    [Fact]
    public void ArchiveTemplateCommandValidator_ShouldRejectEmptyTemplateId()
    {
        var validator = new ArchiveTemplateCommandValidator();
        var command = new ArchiveTemplateCommand(Guid.Empty);

        var result = validator.Validate(command);

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(ArchiveTemplateCommand.TemplateId));
    }

    private sealed class FakeTemplateRepository : ITemplateRepository
    {
        private readonly TemplateAggregate? _template;

        public FakeTemplateRepository()
        {
        }

        public FakeTemplateRepository(TemplateAggregate? template)
        {
            _template = template;
        }

        public TemplateAggregate? SavedTemplate { get; private set; }
        public TemplateAggregate? DeletedTemplate { get; private set; }

        public Task<TemplateAggregate?> GetByIdAsync(Guid templateId, CancellationToken cancellationToken)
            => Task.FromResult(_template?.Id == templateId ? _template : null);

        public Task<IReadOnlyCollection<TemplateAggregate>> ListAsync(CancellationToken cancellationToken)
            => Task.FromResult<IReadOnlyCollection<TemplateAggregate>>(
                _template is null ? Array.Empty<TemplateAggregate>() : new[] { _template });

        public Task SaveAsync(TemplateAggregate aggregate, CancellationToken cancellationToken)
        {
            SavedTemplate = aggregate;
            return Task.CompletedTask;
        }

        public Task DeleteAsync(TemplateAggregate aggregate, CancellationToken cancellationToken)
        {
            DeletedTemplate = aggregate;
            return Task.CompletedTask;
        }
    }
}
