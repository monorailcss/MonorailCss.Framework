using MonorailCss.DataTypes;
using Shouldly;

namespace MonorailCss.Tests.DataTypes;

public class DataTypeInferenceTests
{
    [Theory]
    [InlineData("#123456", true)]
    [InlineData("#abc", true)]
    [InlineData("#12345678", true)]
    [InlineData("rgb(255, 0, 0)", true)]
    [InlineData("rgba(255, 0, 0, 0.5)", true)]
    [InlineData("hsl(120, 100%, 50%)", true)]
    [InlineData("hsla(120, 100%, 50%, 0.8)", true)]
    [InlineData("red", true)]
    [InlineData("transparent", true)]
    [InlineData("currentColor", true)]
    [InlineData("oklch(0.7 0.15 200)", true)]
    [InlineData("color-mix(in oklch, red, blue)", true)]
    [InlineData("10px", false)]
    [InlineData("not-a-color", false)]
    [InlineData("#gg1234", false)]
    [InlineData("#12345", false)]
    public void IsColor_ShouldValidateCorrectly(string value, bool expected)
    {
        var result = DataTypeInference.IsColor(value);
        result.ShouldBe(expected);
    }

    [Theory]
    [InlineData("10px", true)]
    [InlineData("1rem", true)]
    [InlineData("0.5em", true)]
    [InlineData("100vh", true)]
    [InlineData("0", true)]
    [InlineData("calc(100% - 20px)", true)]
    [InlineData("min(10px, 1rem)", true)]
    [InlineData("red", false)]
    [InlineData("10", false)]
    [InlineData("#123456", false)]
    public void IsLength_ShouldValidateCorrectly(string value, bool expected)
    {
        var result = DataTypeInference.IsLength(value);
        result.ShouldBe(expected);
    }

    [Theory]
    [InlineData("50%", true)]
    [InlineData("100%", true)]
    [InlineData("0%", true)]
    [InlineData("-50%", true)]
    [InlineData("calc(50% + 10%)", true)]
    [InlineData("10px", false)]
    [InlineData("50", false)]
    public void IsPercentage_ShouldValidateCorrectly(string value, bool expected)
    {
        var result = DataTypeInference.IsPercentage(value);
        result.ShouldBe(expected);
    }

    [Theory]
    [InlineData("10", true)]
    [InlineData("3.14", true)]
    [InlineData("-5", true)]
    [InlineData("1e3", true)]
    [InlineData("calc(10 + 5)", true)]
    [InlineData("10px", false)]
    [InlineData("red", false)]
    public void IsNumber_ShouldValidateCorrectly(string value, bool expected)
    {
        var result = DataTypeInference.IsNumber(value);
        result.ShouldBe(expected);
    }

    [Theory]
    [InlineData("1/2", true)]
    [InlineData("3/4", true)]
    [InlineData("16/9", true)]
    [InlineData("calc(1/2)", true)]
    [InlineData("10px", false)]
    [InlineData("50%", false)]
    public void IsFraction_ShouldValidateCorrectly(string value, bool expected)
    {
        var result = DataTypeInference.IsFraction(value);
        result.ShouldBe(expected);
    }

    [Theory]
    [InlineData("45deg", true)]
    [InlineData("1rad", true)]
    [InlineData("0.5turn", true)]
    [InlineData("200grad", true)]
    [InlineData("calc(45deg + 10deg)", true)]
    [InlineData("45", false)]
    [InlineData("10px", false)]
    public void IsAngle_ShouldValidateCorrectly(string value, bool expected)
    {
        var result = DataTypeInference.IsAngle(value);
        result.ShouldBe(expected);
    }

    [Theory]
    [InlineData("url(image.jpg)", true)]
    [InlineData("url('image.jpg')", true)]
    [InlineData("url(\"image.jpg\")", true)]
    [InlineData("image.jpg", false)]
    [InlineData("red", false)]
    public void IsUrl_ShouldValidateCorrectly(string value, bool expected)
    {
        var result = DataTypeInference.IsUrl(value);
        result.ShouldBe(expected);
    }

    [Theory]
    [InlineData("calc(100% - 20px)", true)]
    [InlineData("min(10px, 1rem)", true)]
    [InlineData("max(50%, 300px)", true)]
    [InlineData("clamp(200px, 50%, 500px)", true)]
    [InlineData("sin(45deg)", true)]
    [InlineData("cos(1rad)", true)]
    [InlineData("10px", false)]
    [InlineData("red", false)]
    public void HasMathFunction_ShouldDetectCorrectly(string value, bool expected)
    {
        var result = DataTypeInference.HasMathFunction(value);
        result.ShouldBe(expected);
    }

    [Theory]
    [InlineData("center", true)]
    [InlineData("top left", true)]
    [InlineData("50% 50%", true)]
    [InlineData("10px center", true)]
    [InlineData("left 20px", true)]
    [InlineData("invalid position", false)]
    public void IsBackgroundPosition_ShouldValidateCorrectly(string value, bool expected)
    {
        var result = DataTypeInference.IsBackgroundPosition(value);
        result.ShouldBe(expected);
    }

    [Theory]
    [InlineData("cover", true)]
    [InlineData("contain", true)]
    [InlineData("100px 200px", true)]
    [InlineData("50% auto", true)]
    [InlineData("auto", true)]
    [InlineData("invalid size", false)]
    public void IsBackgroundSize_ShouldValidateCorrectly(string value, bool expected)
    {
        var result = DataTypeInference.IsBackgroundSize(value);
        result.ShouldBe(expected);
    }

    [Theory]
    [InlineData("var(--my-color)", null)]
    [InlineData("#123456", DataType.Color)]
    [InlineData("10px", DataType.Length)]
    [InlineData("50%", DataType.Percentage)]
    internal void InferDataType_ShouldReturnCorrectType(string value, DataType? expected)
    {
        var result = DataTypeInference.InferDataType(value, new[] { DataType.Color, DataType.Length, DataType.Percentage });
        result.ShouldBe(expected);
    }
}