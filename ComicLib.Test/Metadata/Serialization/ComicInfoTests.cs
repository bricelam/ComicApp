using System.Xml.Serialization;

namespace ComicLib.Metadata.Serialization;

public class ComicInfoTests
{
    static readonly XmlSerializer _serializer = new(typeof(ComicInfo));

    [Fact]
    public void Deserialize_min()
    {
        var result = Deserialize("<ComicInfo/>");

        Assert.NotNull(result);
    }

    [Fact]
    public void Deserialize_full()
    {
        var result = Deserialize(
            """
            <ComicInfo>
              <Series>Francis, Brother of the Universe</Series>
              <Number>1</Number>
              <Year>1980</Year>
              <Writer>Roy Gasnick</Writer>
              <Publisher>Marvel</Publisher>
              <Manga>No</Manga>
              <Pages>
                <Page Image="1" Type="FrontCover" DoublePage="false" />
                <Page Image="2" DoublePage="true" />
                <Page Image="3" DoublePage="true" />
              </Pages>
            </ComicInfo>
            """);

        Assert.NotNull(result);
        Assert.Equal("Francis, Brother of the Universe", result.Series);
        Assert.Equal("1", result.Number);
        Assert.Equal(1980, result.Year);
        Assert.Equal("Roy Gasnick", result.Writer);
        Assert.Equal("Marvel", result.Publisher);
        Assert.Equal(Manga.No, result.Manga);
        Assert.NotNull(result.Pages);
        Assert.Collection(
            result.Pages,
            page =>
            {
                Assert.Equal(1, page.Image);
                Assert.Equal(ComicPageType.FrontCover, page.Type);
                Assert.False(page.DoublePage);
            },
            page =>
            {
                Assert.Equal(2, page.Image);
                Assert.Equal(ComicPageType.Story, page.Type);
                Assert.True(page.DoublePage);
            },
            page =>
            {
                Assert.Equal(3, page.Image);
            });
    }

    static ComicInfo? Deserialize(string xml)
        => (ComicInfo?)_serializer.Deserialize(new StringReader(xml));
}
