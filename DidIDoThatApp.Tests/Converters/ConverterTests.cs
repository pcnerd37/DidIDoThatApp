using System.Globalization;
using DidIDoThatApp.Converters;

namespace DidIDoThatApp.Tests.Converters;

public class InvertedBoolConverterTests
{
    private readonly InvertedBoolConverter _sut = new();
    private readonly CultureInfo _culture = CultureInfo.InvariantCulture;

    #region Convert Tests

    [Fact]
    public void Convert_WhenTrue_ReturnsFalse()
    {
        // Arrange
        var value = true;

        // Act
        var result = _sut.Convert(value, typeof(bool), null, _culture);

        // Assert
        result.Should().Be(false);
    }

    [Fact]
    public void Convert_WhenFalse_ReturnsTrue()
    {
        // Arrange
        var value = false;

        // Act
        var result = _sut.Convert(value, typeof(bool), null, _culture);

        // Assert
        result.Should().Be(true);
    }

    [Fact]
    public void Convert_WhenNull_ReturnsFalse()
    {
        // Arrange
        object? value = null;

        // Act
        var result = _sut.Convert(value, typeof(bool), null, _culture);

        // Assert
        result.Should().Be(false);
    }

    [Fact]
    public void Convert_WhenNonBoolValue_ReturnsFalse()
    {
        // Arrange
        var value = "not a bool";

        // Act
        var result = _sut.Convert(value, typeof(bool), null, _culture);

        // Assert
        result.Should().Be(false);
    }

    [Fact]
    public void Convert_WhenIntValue_ReturnsFalse()
    {
        // Arrange
        var value = 42;

        // Act
        var result = _sut.Convert(value, typeof(bool), null, _culture);

        // Assert
        result.Should().Be(false);
    }

    #endregion

    #region ConvertBack Tests

    [Fact]
    public void ConvertBack_WhenTrue_ReturnsFalse()
    {
        // Arrange
        var value = true;

        // Act
        var result = _sut.ConvertBack(value, typeof(bool), null, _culture);

        // Assert
        result.Should().Be(false);
    }

    [Fact]
    public void ConvertBack_WhenFalse_ReturnsTrue()
    {
        // Arrange
        var value = false;

        // Act
        var result = _sut.ConvertBack(value, typeof(bool), null, _culture);

        // Assert
        result.Should().Be(true);
    }

    [Fact]
    public void ConvertBack_WhenNull_ReturnsFalse()
    {
        // Arrange
        object? value = null;

        // Act
        var result = _sut.ConvertBack(value, typeof(bool), null, _culture);

        // Assert
        result.Should().Be(false);
    }

    #endregion
}

public class IsNotNullConverterTests
{
    private readonly IsNotNullConverter _sut = new();
    private readonly CultureInfo _culture = CultureInfo.InvariantCulture;

    [Fact]
    public void Convert_WhenValueIsNull_ReturnsFalse()
    {
        // Arrange
        object? value = null;

        // Act
        var result = _sut.Convert(value, typeof(bool), null, _culture);

        // Assert
        result.Should().Be(false);
    }

    [Fact]
    public void Convert_WhenValueIsString_ReturnsTrue()
    {
        // Arrange
        var value = "test";

        // Act
        var result = _sut.Convert(value, typeof(bool), null, _culture);

        // Assert
        result.Should().Be(true);
    }

    [Fact]
    public void Convert_WhenValueIsEmptyString_ReturnsTrue()
    {
        // Arrange - empty string is NOT null
        var value = "";

        // Act
        var result = _sut.Convert(value, typeof(bool), null, _culture);

        // Assert
        result.Should().Be(true);
    }

    [Fact]
    public void Convert_WhenValueIsInt_ReturnsTrue()
    {
        // Arrange
        var value = 42;

        // Act
        var result = _sut.Convert(value, typeof(bool), null, _culture);

        // Assert
        result.Should().Be(true);
    }

    [Fact]
    public void Convert_WhenValueIsZero_ReturnsTrue()
    {
        // Arrange - zero is NOT null
        var value = 0;

        // Act
        var result = _sut.Convert(value, typeof(bool), null, _culture);

        // Assert
        result.Should().Be(true);
    }

    [Fact]
    public void Convert_WhenValueIsObject_ReturnsTrue()
    {
        // Arrange
        var value = new object();

        // Act
        var result = _sut.Convert(value, typeof(bool), null, _culture);

        // Assert
        result.Should().Be(true);
    }

    [Fact]
    public void Convert_WhenValueIsGuid_ReturnsTrue()
    {
        // Arrange
        var value = Guid.NewGuid();

        // Act
        var result = _sut.Convert(value, typeof(bool), null, _culture);

        // Assert
        result.Should().Be(true);
    }

    [Fact]
    public void Convert_WhenValueIsEmptyGuid_ReturnsTrue()
    {
        // Arrange - Guid.Empty is NOT null
        var value = Guid.Empty;

        // Act
        var result = _sut.Convert(value, typeof(bool), null, _culture);

        // Assert
        result.Should().Be(true);
    }
}

public class IsNotNullOrEmptyConverterTests
{
    private readonly IsNotNullOrEmptyConverter _sut = new();
    private readonly CultureInfo _culture = CultureInfo.InvariantCulture;

    [Fact]
    public void Convert_WhenNull_ReturnsFalse()
    {
        // Arrange
        object? value = null;

        // Act
        var result = _sut.Convert(value, typeof(bool), null, _culture);

        // Assert
        result.Should().Be(false);
    }

    [Fact]
    public void Convert_WhenEmptyString_ReturnsFalse()
    {
        // Arrange
        var value = "";

        // Act
        var result = _sut.Convert(value, typeof(bool), null, _culture);

        // Assert
        result.Should().Be(false);
    }

    [Fact]
    public void Convert_WhenWhitespaceString_ReturnsFalse()
    {
        // Arrange
        var value = "   ";

        // Act
        var result = _sut.Convert(value, typeof(bool), null, _culture);

        // Assert
        result.Should().Be(false);
    }

    [Fact]
    public void Convert_WhenTabAndNewlineString_ReturnsFalse()
    {
        // Arrange
        var value = "\t\n\r";

        // Act
        var result = _sut.Convert(value, typeof(bool), null, _culture);

        // Assert
        result.Should().Be(false);
    }

    [Fact]
    public void Convert_WhenValidString_ReturnsTrue()
    {
        // Arrange
        var value = "Hello World";

        // Act
        var result = _sut.Convert(value, typeof(bool), null, _culture);

        // Assert
        result.Should().Be(true);
    }

    [Fact]
    public void Convert_WhenStringWithLeadingTrailingWhitespace_ReturnsTrue()
    {
        // Arrange
        var value = "  valid  ";

        // Act
        var result = _sut.Convert(value, typeof(bool), null, _culture);

        // Assert
        result.Should().Be(true);
    }

    [Fact]
    public void Convert_WhenNonStringObject_ReturnsTrue()
    {
        // Arrange - non-string objects just check for null
        var value = new object();

        // Act
        var result = _sut.Convert(value, typeof(bool), null, _culture);

        // Assert
        result.Should().Be(true);
    }

    [Fact]
    public void Convert_WhenInt_ReturnsTrue()
    {
        // Arrange - non-string values just check for null
        var value = 42;

        // Act
        var result = _sut.Convert(value, typeof(bool), null, _culture);

        // Assert
        result.Should().Be(true);
    }

    [Fact]
    public void Convert_WhenSingleCharacter_ReturnsTrue()
    {
        // Arrange
        var value = "a";

        // Act
        var result = _sut.Convert(value, typeof(bool), null, _culture);

        // Assert
        result.Should().Be(true);
    }
}
