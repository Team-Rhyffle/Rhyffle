using NUnit.Framework;

public class ChartLoaderTest {
    [Test]
    public void LoadFromJson_ParsesNormalNotes() {
        string json = @"{
            ""NormalNotes"": [
                {""position"": 48, ""line"": 0, ""length"": 1},
                {""position"": 96, ""line"": 5, ""length"": 2}
            ],
            ""HoldNotes"": [],
            ""SlideNotes"": [],
            ""FlickNotes"": []
        }";

        ChartData chart = ChartLoader.LoadFromJson(json);

        Assert.AreEqual(2, chart.NormalNotes.Count);
        Assert.AreEqual(48, chart.NormalNotes[0].position);
        Assert.AreEqual(0,  chart.NormalNotes[0].line);
        Assert.AreEqual(1,  chart.NormalNotes[0].length);
        Assert.AreEqual(96, chart.NormalNotes[1].position);
        Assert.AreEqual(5,  chart.NormalNotes[1].line);
        Assert.AreEqual(2,  chart.NormalNotes[1].length);
    }

    [Test]
    public void LoadFromJson_ParsesFlickDirection() {
        string json = @"{
            ""NormalNotes"": [],
            ""HoldNotes"": [],
            ""SlideNotes"": [],
            ""FlickNotes"": [
                {""position"": 48, ""line"": 10, ""length"": 1, ""direction"": 1}
            ]
        }";

        ChartData chart = ChartLoader.LoadFromJson(json);

        Assert.AreEqual(1, chart.FlickNotes.Count);
        Assert.AreEqual(1, chart.FlickNotes[0].direction);  // Right
    }

    [Test]
    public void LoadFromJson_EmptyChart_ReturnsZeroNotes() {
        string json = @"{""NormalNotes"":[],""HoldNotes"":[],""SlideNotes"":[],""FlickNotes"":[]}";

        ChartData chart = ChartLoader.LoadFromJson(json);

        Assert.AreEqual(0, chart.NormalNotes.Count);
        Assert.AreEqual(0, chart.HoldNotes.Count);
        Assert.AreEqual(0, chart.SlideNotes.Count);
        Assert.AreEqual(0, chart.FlickNotes.Count);
    }
}
