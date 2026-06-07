using QPhising.Domain.Templates.Aggregates;
using QPhising.Domain.Templates.Enums;
using QPhising.Domain.Templates.ValueObjects;
using Xunit;

namespace QPhising.Api.Tests;

public sealed class TemplateDomainUnitTests
{
    [Fact]
    public void Constructor_ShouldCreateInDraftWithVersion1()
    {
        var template = CreateTemplate();

        Assert.Equal(TemplateLifecycleState.Draft, template.LifecycleState);
        Assert.Equal(TemplateAggregate.InitialVersion, template.Version);
    }

    [Fact]
    public void Constructor_ShouldSetProperties()
    {
        var id = Guid.NewGuid();
        var name = new TemplateName("Test Template");
        var content = new TemplateContent("<html>Hello</html>");
        var metadata = new TemplateMetadata("Description", new[] { "tag1" });

        var template = new TemplateAggregate(id, name, content, metadata);

        Assert.Equal(id, template.Id);
        Assert.Equal("Test Template", template.Name.Value);
        Assert.Equal("<html>Hello</html>", template.Content.HtmlContent);
        Assert.Equal("Description", template.Metadata.Description);
        Assert.Equal(TemplateLifecycleState.Draft, template.LifecycleState);
        Assert.Equal(1, template.Version);
    }

    [Fact]
    public void Constructor_ShouldThrowForNullName()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new TemplateAggregate(Guid.NewGuid(), null!, new TemplateContent("<html></html>"), CreateMetadata()));
    }

    [Fact]
    public void Constructor_ShouldThrowForNullContent()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new TemplateAggregate(Guid.NewGuid(), new TemplateName("Name"), null!, CreateMetadata()));
    }

    [Fact]
    public void Constructor_ShouldThrowForNullMetadata()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new TemplateAggregate(Guid.NewGuid(), new TemplateName("Name"), new TemplateContent("<html></html>"), null!));
    }

    [Fact]
    public void Publish_ShouldTransitionDraftToPublished()
    {
        var template = CreateTemplate();

        template.Publish();

        Assert.Equal(TemplateLifecycleState.Published, template.LifecycleState);
    }

    [Fact]
    public void Publish_ShouldWorkFromPublished()
    {
        var template = CreateTemplate();
        template.Publish();

        template.Publish();

        Assert.Equal(TemplateLifecycleState.Published, template.LifecycleState);
    }

    [Fact]
    public void Publish_ShouldThrowFromArchived()
    {
        var template = CreateTemplate();
        template.Archive();

        Assert.Throws<InvalidOperationException>(() => template.Publish());
    }

    [Fact]
    public void Archive_ShouldTransitionDraftToArchived()
    {
        var template = CreateTemplate();

        template.Archive();

        Assert.Equal(TemplateLifecycleState.Archived, template.LifecycleState);
    }

    [Fact]
    public void Archive_ShouldTransitionPublishedToArchived()
    {
        var template = CreateTemplate();
        template.Publish();

        template.Archive();

        Assert.Equal(TemplateLifecycleState.Archived, template.LifecycleState);
    }

    [Fact]
    public void Archive_ShouldWorkFromArchived()
    {
        var template = CreateTemplate();
        template.Archive();

        template.Archive();

        Assert.Equal(TemplateLifecycleState.Archived, template.LifecycleState);
    }

    [Fact]
    public void Update_ShouldChangeNameContentMetadataAndIncrementVersion()
    {
        var template = CreateTemplate();
        var newName = new TemplateName("Updated Name");
        var newContent = new TemplateContent("<html>Updated</html>");
        var newMetadata = new TemplateMetadata("New description", new[] { "updated-tag" });

        template.Update(newName, newContent, newMetadata);

        Assert.Equal("Updated Name", template.Name.Value);
        Assert.Equal("<html>Updated</html>", template.Content.HtmlContent);
        Assert.Equal("New description", template.Metadata.Description);
        Assert.Equal(2, template.Version);
    }

    [Fact]
    public void Update_ShouldNotIncrementVersionWhenNoChanges()
    {
        var template = CreateTemplate();
        var sameName = template.Name;
        var sameContent = template.Content;
        var sameMetadata = template.Metadata;

        template.Update(sameName, sameContent, sameMetadata);

        Assert.Equal(1, template.Version);
    }

    [Fact]
    public void Update_ShouldThrowForNullName()
    {
        var template = CreateTemplate();

        Assert.Throws<ArgumentNullException>(() =>
            template.Update(null!, template.Content, template.Metadata));
    }

    [Fact]
    public void Update_ShouldThrowForNullContent()
    {
        var template = CreateTemplate();

        Assert.Throws<ArgumentNullException>(() =>
            template.Update(template.Name, null!, template.Metadata));
    }

    [Fact]
    public void Update_ShouldThrowForNullMetadata()
    {
        var template = CreateTemplate();

        Assert.Throws<ArgumentNullException>(() =>
            template.Update(template.Name, template.Content, null!));
    }

    [Fact]
    public void Update_ShouldThrowWhenArchived()
    {
        var template = CreateTemplate();
        template.Archive();

        Assert.Throws<InvalidOperationException>(() =>
            template.Update(
                new TemplateName("Should Fail"),
                new TemplateContent("<html>Fail</html>"),
                CreateMetadata()));
    }

    [Fact]
    public void Update_ShouldThrowWhenDeleted()
    {
        var template = CreateTemplate();
        template.MarkDeleted();

        Assert.Throws<InvalidOperationException>(() =>
            template.Update(
                new TemplateName("Should Fail"),
                new TemplateContent("<html>Fail</html>"),
                CreateMetadata()));
    }

    [Fact]
    public void EnsureMutable_ShouldBlockOperationsAfterArchive()
    {
        var template = CreateTemplate();
        template.Archive();

        Assert.Throws<InvalidOperationException>(() => template.Publish());
        Assert.Throws<InvalidOperationException>(() =>
            template.Update(
                new TemplateName("Fail"),
                new TemplateContent("<html>Fail</html>"),
                CreateMetadata()));
    }

    [Fact]
    public void EnsureMutable_ShouldBlockOperationsAfterDelete()
    {
        var template = CreateTemplate();
        template.MarkDeleted();

        Assert.Throws<InvalidOperationException>(() =>
            template.Update(
                new TemplateName("Fail"),
                new TemplateContent("<html>Fail</html>"),
                CreateMetadata()));
    }

    [Fact]
    public void Version_ShouldNotExceedMaxVersion()
    {
        var template = TemplateAggregate.Rehydrate(
            Guid.NewGuid(),
            new TemplateName("Max Version"),
            new TemplateContent("<html>Test</html>"),
            CreateMetadata(),
            TemplateLifecycleState.Draft,
            TemplateAggregate.MaxVersion,
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow);

        Assert.Throws<InvalidOperationException>(() =>
            template.Update(
                new TemplateName("Bump"),
                new TemplateContent("<html>Bump</html>"),
                CreateMetadata()));
    }

    [Fact]
    public void Rehydrate_ShouldCreateWithGivenState()
    {
        var id = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;

        var template = TemplateAggregate.Rehydrate(
            id,
            new TemplateName("Rehydrated"),
            new TemplateContent("<html>Rehydrated</html>"),
            new TemplateMetadata("Desc", new[] { "tag" }),
            TemplateLifecycleState.Published,
            5,
            now,
            now);

        Assert.Equal(id, template.Id);
        Assert.Equal(TemplateLifecycleState.Published, template.LifecycleState);
        Assert.Equal(5, template.Version);
        Assert.Equal("Rehydrated", template.Name.Value);
    }

    [Fact]
    public void TemplateName_ShouldNormalizeWhitespace()
    {
        var name = new TemplateName("  My Template  ");

        Assert.Equal("My Template", name.Value);
    }

    [Fact]
    public void TemplateName_ShouldRejectNull()
    {
        Assert.Throws<ArgumentException>(() => new TemplateName(null!));
    }

    [Fact]
    public void TemplateName_ShouldRejectEmpty()
    {
        Assert.Throws<ArgumentException>(() => new TemplateName(""));
    }

    [Fact]
    public void TemplateName_ShouldRejectWhitespaceOnly()
    {
        Assert.Throws<ArgumentException>(() => new TemplateName("   "));
    }

    [Fact]
    public void TemplateName_ShouldRejectExceedingMaxLength()
    {
        var longName = new string('x', TemplateName.MaxLength + 1);

        Assert.Throws<ArgumentException>(() => new TemplateName(longName));
    }

    [Fact]
    public void TemplateName_ShouldAcceptAtMaxLength()
    {
        var validName = new string('x', TemplateName.MaxLength);

        var name = new TemplateName(validName);

        Assert.Equal(validName, name.Value);
    }

    [Fact]
    public void TemplateContent_ShouldNormalizeWhitespace()
    {
        var content = new TemplateContent("  <html>Hello</html>  ");

        Assert.Equal("<html>Hello</html>", content.HtmlContent);
    }

    [Fact]
    public void TemplateContent_ShouldRejectNull()
    {
        Assert.Throws<ArgumentException>(() => new TemplateContent(null!));
    }

    [Fact]
    public void TemplateContent_ShouldRejectEmpty()
    {
        Assert.Throws<ArgumentException>(() => new TemplateContent(""));
    }

    [Fact]
    public void TemplateContent_ShouldRejectExceedingMaxHtmlLength()
    {
        var longHtml = new string('x', TemplateContent.MaxHtmlLength + 1);

        Assert.Throws<ArgumentException>(() => new TemplateContent(longHtml));
    }

    [Fact]
    public void TemplateContent_ShouldAcceptAtMaxLength()
    {
        var validHtml = new string('x', TemplateContent.MaxHtmlLength);

        var content = new TemplateContent(validHtml);

        Assert.Equal(validHtml, content.HtmlContent);
    }

    [Fact]
    public void TemplateMetadata_ShouldStoreDescription()
    {
        var metadata = new TemplateMetadata("Test description", null);

        Assert.Equal("Test description", metadata.Description);
    }

    [Fact]
    public void TemplateMetadata_ShouldNormalizeNullDescriptionToNull()
    {
        var metadata = new TemplateMetadata(null, null);

        Assert.Null(metadata.Description);
    }

    [Fact]
    public void TemplateMetadata_ShouldNormalizeWhitespaceDescriptionToNull()
    {
        var metadata = new TemplateMetadata("   ", null);

        Assert.Null(metadata.Description);
    }

    [Fact]
    public void TemplateMetadata_ShouldRejectDescriptionExceedingMaxLength()
    {
        var longDesc = new string('x', TemplateMetadata.MaxDescriptionLength + 1);

        Assert.Throws<ArgumentException>(() => new TemplateMetadata(longDesc, null));
    }

    [Fact]
    public void TemplateMetadata_ShouldAcceptDescriptionAtMaxLength()
    {
        var validDesc = new string('x', TemplateMetadata.MaxDescriptionLength);

        var metadata = new TemplateMetadata(validDesc, null);

        Assert.Equal(validDesc, metadata.Description);
    }

    [Fact]
    public void TemplateMetadata_ShouldAcceptNullTags()
    {
        var metadata = new TemplateMetadata(null, null);

        Assert.Empty(metadata.Tags);
    }

    [Fact]
    public void TemplateMetadata_ShouldStoreTags()
    {
        var metadata = new TemplateMetadata(null, new[] { "tag1", "tag2" });

        Assert.Equal(2, metadata.Tags.Count);
        Assert.Contains("tag1", metadata.Tags);
        Assert.Contains("tag2", metadata.Tags);
    }

    [Fact]
    public void TemplateMetadata_ShouldTrimTags()
    {
        var metadata = new TemplateMetadata(null, new[] { "  tag1  ", "  tag2  " });

        Assert.Contains("tag1", metadata.Tags);
        Assert.Contains("tag2", metadata.Tags);
    }

    [Fact]
    public void TemplateMetadata_ShouldFilterEmptyOrWhitespaceTags()
    {
        var metadata = new TemplateMetadata(null, new[] { "tag1", "", "  ", "tag2" });

        Assert.Equal(2, metadata.Tags.Count);
    }

    [Fact]
    public void TemplateMetadata_ShouldDeduplicateTagsCaseInsensitively()
    {
        var metadata = new TemplateMetadata(null, new[] { "Tag1", "tag1", "TAG1", "tag2" });

        Assert.Equal(2, metadata.Tags.Count);
    }

    [Fact]
    public void TemplateMetadata_ShouldRejectExceedingMaxTagCount()
    {
        var tags = Enumerable.Range(0, TemplateMetadata.MaxTagCount + 1).Select(i => $"tag{i}").ToArray();

        Assert.Throws<ArgumentException>(() => new TemplateMetadata(null, tags));
    }

    [Fact]
    public void TemplateMetadata_ShouldRejectTagExceedingMaxLength()
    {
        var longTag = new string('x', TemplateMetadata.MaxTagLength + 1);

        Assert.Throws<ArgumentException>(() => new TemplateMetadata(null, new[] { longTag }));
    }

    [Fact]
    public void TemplateMetadata_Equality_ShouldBeBasedOnDescriptionAndTags()
    {
        var m1 = new TemplateMetadata("Desc", new[] { "a", "b" });
        var m2 = new TemplateMetadata("Desc", new[] { "a", "b" });
        var m3 = new TemplateMetadata("Other", new[] { "a", "b" });

        Assert.Equal(m1, m2);
        Assert.NotEqual(m1, m3);
        Assert.Equal(m1.GetHashCode(), m2.GetHashCode());
    }

    [Fact]
    public void TemplateName_Equality_ShouldBeBasedOnValue()
    {
        var n1 = new TemplateName("Alpha");
        var n2 = new TemplateName("Alpha");
        var n3 = new TemplateName("Beta");

        Assert.Equal(n1, n2);
        Assert.NotEqual(n1, n3);
    }

    [Fact]
    public void TemplateContent_Equality_ShouldBeBasedOnHtmlContent()
    {
        var c1 = new TemplateContent("<html>A</html>");
        var c2 = new TemplateContent("<html>A</html>");
        var c3 = new TemplateContent("<html>B</html>");

        Assert.Equal(c1, c2);
        Assert.NotEqual(c1, c3);
    }

    private static TemplateAggregate CreateTemplate()
        => new(
            Guid.NewGuid(),
            new TemplateName("Default Template"),
            new TemplateContent("<html>Default</html>"),
            CreateMetadata());

    private static TemplateMetadata CreateMetadata()
        => new("Default description", new[] { "default-tag" });
}
