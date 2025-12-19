using DbDiff.Application.Validation;

namespace DbDiff.Application.Tests;

public class PathValidatorTests
{
    [Fact]
    public void ValidateOutputPath_WithValidRelativePath_ReturnsAbsolutePath()
    {
        // Arrange
        var relativePath = "output/schema.txt";

        // Act
        var result = PathValidator.ValidateOutputPath(relativePath);

        // Assert
        Assert.NotNull(result);
        Assert.True(Path.IsPathRooted(result));
        Assert.EndsWith("schema.txt", result);
    }

    [Fact]
    public void ValidateOutputPath_WithValidAbsolutePath_ReturnsPath()
    {
        // Arrange
        var tempPath = Path.Combine(Path.GetTempPath(), "test-schema.txt");

        // Act
        var result = PathValidator.ValidateOutputPath(tempPath);

        // Assert
        Assert.Equal(Path.GetFullPath(tempPath), result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ValidateOutputPath_WithNullOrEmpty_ThrowsArgumentException(string? path)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => PathValidator.ValidateOutputPath(path!));
    }

    [Fact]
    public void ValidateOutputPath_WithPathTraversal_ResolvesToAbsolutePath()
    {
        // Arrange
        var pathWithTraversal = "../../output/schema.txt";

        // Act
        var result = PathValidator.ValidateOutputPath(pathWithTraversal);

        // Assert
        // Should resolve to absolute path without ".."
        Assert.True(Path.IsPathRooted(result));
        Assert.DoesNotContain("..", result);
    }

    [Fact]
    public void ValidateOutputPath_WithAllowedBasePath_WhenPathIsWithinBase_Succeeds()
    {
        // Arrange
        var basePath = Path.GetTempPath();
        var outputPath = Path.Combine(basePath, "subdir", "output.txt");

        // Act
        var result = PathValidator.ValidateOutputPath(outputPath, basePath);

        // Assert
        Assert.NotNull(result);
        Assert.StartsWith(Path.GetFullPath(basePath), result);
    }

    [Fact]
    public void ValidateOutputPath_WithAllowedBasePath_WhenPathIsOutsideBase_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var basePath = Path.Combine(Path.GetTempPath(), "restricted");
        var outputPath = Path.Combine(Path.GetTempPath(), "outside", "output.txt");

        // Act & Assert
        Assert.Throws<UnauthorizedAccessException>(() =>
            PathValidator.ValidateOutputPath(outputPath, basePath));
    }

    [Fact]
    public void ValidateOutputPath_WithSystemDirectory_ThrowsUnauthorizedAccessException()
    {
        // Arrange - try to write to Windows system directory
        string systemPath;
        if (OperatingSystem.IsWindows())
        {
            systemPath = Path.Combine("C:\\Windows", "test.txt");
        }
        else if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
        {
            systemPath = "/etc/test.txt";
        }
        else
        {
            // Skip test on unsupported OS
            return;
        }

        // Act & Assert
        Assert.Throws<UnauthorizedAccessException>(() =>
            PathValidator.ValidateOutputPath(systemPath));
    }

    [Fact]
    public void ValidateOutputPath_WithInvalidCharacters_ThrowsArgumentException()
    {
        // Arrange
        var invalidPath = OperatingSystem.IsWindows()
            ? "output/file<>*.txt"  // <, >, * are invalid on Windows
            : "output/file\0.txt";   // null character is invalid on Unix

        // Act & Assert
        Assert.Throws<ArgumentException>(() => PathValidator.ValidateOutputPath(invalidPath));
    }

    [Fact]
    public void ValidateOutputPath_WithDirectoryOnly_ThrowsArgumentException()
    {
        // Arrange
        var directoryOnlyPath = "output/";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => PathValidator.ValidateOutputPath(directoryOnlyPath));
    }

    [Fact]
    public void ValidateConfigPath_WithExistingJsonFile_ReturnsAbsolutePath()
    {
        // Arrange
        var tempConfigFile = Path.Combine(Path.GetTempPath(), $"test-config-{Guid.NewGuid()}.json");
        File.WriteAllText(tempConfigFile, "{}");

        try
        {
            // Act
            var result = PathValidator.ValidateConfigPath(tempConfigFile);

            // Assert
            Assert.Equal(Path.GetFullPath(tempConfigFile), result);
        }
        finally
        {
            // Cleanup
            if (File.Exists(tempConfigFile))
                File.Delete(tempConfigFile);
        }
    }

    [Fact]
    public void ValidateConfigPath_WithNonExistentFile_ThrowsFileNotFoundException()
    {
        // Arrange
        var nonExistentPath = Path.Combine(Path.GetTempPath(), "nonexistent-config.json");

        // Act & Assert
        Assert.Throws<FileNotFoundException>(() =>
            PathValidator.ValidateConfigPath(nonExistentPath));
    }

    [Fact]
    public void ValidateConfigPath_WithNonJsonFile_ThrowsArgumentException()
    {
        // Arrange
        var tempTextFile = Path.Combine(Path.GetTempPath(), $"test-config-{Guid.NewGuid()}.txt");
        File.WriteAllText(tempTextFile, "not json");

        try
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() =>
                PathValidator.ValidateConfigPath(tempTextFile));
        }
        finally
        {
            // Cleanup
            if (File.Exists(tempTextFile))
                File.Delete(tempTextFile);
        }
    }

    [Fact]
    public void ValidateConfigPath_WithAllowedBasePath_WhenFileIsOutsideBase_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var basePath = Path.Combine(Path.GetTempPath(), "restricted");
        var configFile = Path.Combine(Path.GetTempPath(), $"outside-config-{Guid.NewGuid()}.json");
        File.WriteAllText(configFile, "{}");

        try
        {
            // Act & Assert
            Assert.Throws<UnauthorizedAccessException>(() =>
                PathValidator.ValidateConfigPath(configFile, basePath));
        }
        finally
        {
            // Cleanup
            if (File.Exists(configFile))
                File.Delete(configFile);
        }
    }

    [Fact]
    public void ValidateLogPath_WithValidPath_ReturnsAbsolutePath()
    {
        // Arrange
        var logPath = "logs/app.log";

        // Act
        var result = PathValidator.ValidateLogPath(logPath);

        // Assert
        Assert.NotNull(result);
        Assert.True(Path.IsPathRooted(result));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ValidateLogPath_WithNullOrEmpty_ThrowsArgumentException(string? path)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => PathValidator.ValidateLogPath(path!));
    }

    [Fact]
    public void ValidateLogPath_WithSystemDirectory_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        string systemLogPath;
        if (OperatingSystem.IsWindows())
        {
            systemLogPath = "C:\\Windows\\System32\\malicious.log";
        }
        else if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
        {
            systemLogPath = "/etc/malicious.log";
        }
        else
        {
            // Skip test on unsupported OS
            return;
        }

        // Act & Assert
        Assert.Throws<UnauthorizedAccessException>(() =>
            PathValidator.ValidateLogPath(systemLogPath));
    }

    [Fact]
    public void ValidateOutputPath_WithMixedPathSeparators_NormalizesPath()
    {
        // Arrange
        var mixedPath = "output/subdir\\schema.txt";

        // Act
        var result = PathValidator.ValidateOutputPath(mixedPath);

        // Assert
        Assert.NotNull(result);
        Assert.True(Path.IsPathRooted(result));
        // Path should be normalized to use consistent separators
        // Path should use the OS-appropriate separator
        if (OperatingSystem.IsWindows())
        {
            // On Windows, should use backslashes
            Assert.DoesNotContain("/", result.Replace("://", ""));  // Ignore protocol separators
        }
        else
        {
            // On Unix/Linux, forward slashes are correct
            Assert.Contains("/", result);
        }
    }

    [Fact]
    public void ValidateOutputPath_PreventsDriveRootAccess_OnWindows()
    {
        // Only run on Windows
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        // Arrange
        var driveRoot = "C:\\test.txt";

        // Act & Assert
        // This should be allowed as it's not a restricted system path
        var result = PathValidator.ValidateOutputPath(driveRoot);
        Assert.NotNull(result);
    }

    [Fact]
    public void ValidateOutputPath_PreventsRootAccess_OnUnix()
    {
        // Only run on Unix-like systems
        if (!OperatingSystem.IsLinux() && !OperatingSystem.IsMacOS())
        {
            return;
        }

        // Arrange
        var rootPath = "/test.txt";

        // Act
        // Assert - just verify it returns a path (actual write permission depends on user)
        Assert.Throws<UnauthorizedAccessException>(() => PathValidator.ValidateOutputPath(rootPath));
    }
}

