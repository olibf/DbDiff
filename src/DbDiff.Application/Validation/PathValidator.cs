namespace DbDiff.Application.Validation;

/// <summary>
/// Validates and sanitizes file paths to prevent path traversal and other security issues.
/// </summary>
public static class PathValidator
{
    private static readonly string[] WindowsRestrictedPaths = new[]
    {
        "C:\\Windows",
        "C:\\Program Files",
        "C:\\Program Files (x86)",
        "C:\\ProgramData"
    };

    private static readonly string[] UnixRestrictedPaths = new[]
    {
        "/bin",
        "/sbin",
        "/usr/bin",
        "/usr/sbin",
        "/etc",
        "/sys",
        "/proc",
        "/boot",
        "/root"
    };

    /// <summary>
    /// Validates that an output file path is safe to write to.
    /// </summary>
    /// <param name="path">The path to validate.</param>
    /// <param name="allowedBasePath">Optional base path to restrict writes to. If null, uses reasonable system restrictions.</param>
    /// <returns>The validated absolute path.</returns>
    /// <exception cref="ArgumentException">Thrown when the path is invalid or unsafe.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown when attempting to write to a restricted location.</exception>
    public static string ValidateOutputPath(string path, string? allowedBasePath = null)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("Path cannot be null or empty.", nameof(path));

        // Normalize path separators
        path = path.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);

        // Get the absolute path - this resolves relative paths and normalizes the path
        string absolutePath;
        try
        {
            absolutePath = Path.GetFullPath(path);
        }
        catch (Exception ex) when (ex is ArgumentException or NotSupportedException or PathTooLongException)
        {
            throw new ArgumentException($"Invalid path format: {ex.Message}", nameof(path), ex);
        }

        // Validate file name
        var fileName = Path.GetFileName(absolutePath);
        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException("Path must include a file name.", nameof(path));

        // Check for invalid characters in filename
        var invalidChars = Path.GetInvalidFileNameChars();
        if (fileName.IndexOfAny(invalidChars) >= 0)
            throw new ArgumentException($"File name contains invalid characters: {fileName}", nameof(path));

        // If an allowed base path is specified, ensure the output path is within it
        if (!string.IsNullOrWhiteSpace(allowedBasePath))
        {
            var normalizedBasePath = Path.GetFullPath(allowedBasePath);
            if (!IsPathWithinDirectory(absolutePath, normalizedBasePath))
            {
                throw new UnauthorizedAccessException(
                    $"Output path '{absolutePath}' is outside the allowed directory '{normalizedBasePath}'.");
            }
        }

        // Check against system-restricted directories
        if (IsRestrictedSystemPath(absolutePath))
        {
            throw new UnauthorizedAccessException(
                $"Cannot write to system directory: {absolutePath}");
        }

        // Validate the directory portion exists or can be created
        var directory = Path.GetDirectoryName(absolutePath);
        if (!string.IsNullOrEmpty(directory))
        {
            ValidateDirectoryPath(directory);
        }

        return absolutePath;
    }

    /// <summary>
    /// Validates that a configuration file path is safe to read from.
    /// </summary>
    /// <param name="path">The path to validate.</param>
    /// <param name="allowedBasePath">Optional base path to restrict reads to.</param>
    /// <returns>The validated absolute path.</returns>
    public static string ValidateConfigPath(string path, string? allowedBasePath = null)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("Path cannot be null or empty.", nameof(path));

        // Normalize path separators
        path = path.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);

        // Get the absolute path
        string absolutePath;
        try
        {
            absolutePath = Path.GetFullPath(path);
        }
        catch (Exception ex) when (ex is ArgumentException or NotSupportedException or PathTooLongException)
        {
            throw new ArgumentException($"Invalid path format: {ex.Message}", nameof(path), ex);
        }

        // Check if file exists
        if (!File.Exists(absolutePath))
            throw new FileNotFoundException($"Configuration file not found: {absolutePath}");

        // If an allowed base path is specified, ensure the config path is within it
        if (!string.IsNullOrWhiteSpace(allowedBasePath))
        {
            var normalizedBasePath = Path.GetFullPath(allowedBasePath);
            if (!IsPathWithinDirectory(absolutePath, normalizedBasePath))
            {
                throw new UnauthorizedAccessException(
                    $"Configuration path '{absolutePath}' is outside the allowed directory '{normalizedBasePath}'.");
            }
        }

        // Check file extension
        var extension = Path.GetExtension(absolutePath);
        if (!string.Equals(extension, ".json", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException($"Configuration file must be a JSON file. Got: {extension}", nameof(path));
        }

        return absolutePath;
    }

    /// <summary>
    /// Validates that a log file path is safe to write to.
    /// </summary>
    public static string ValidateLogPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("Log path cannot be null or empty.", nameof(path));

        // Normalize path separators
        path = path.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);

        // Get the absolute path
        string absolutePath;
        try
        {
            absolutePath = Path.GetFullPath(path);
        }
        catch (Exception ex) when (ex is ArgumentException or NotSupportedException or PathTooLongException)
        {
            throw new ArgumentException($"Invalid log path format: {ex.Message}", nameof(path), ex);
        }

        // Check against system-restricted directories
        if (IsRestrictedSystemPath(absolutePath))
        {
            throw new UnauthorizedAccessException(
                $"Cannot write logs to system directory: {absolutePath}");
        }

        return absolutePath;
    }

    private static bool IsPathWithinDirectory(string path, string directory)
    {
        var normalizedPath = Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var normalizedDirectory = Path.GetFullPath(directory).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

        return normalizedPath.StartsWith(normalizedDirectory + Path.DirectorySeparatorChar,
            GetPathComparison()) ||
            normalizedPath.Equals(normalizedDirectory, GetPathComparison());
    }

    private static bool IsRestrictedSystemPath(string path)
    {
        var normalizedPath = Path.GetFullPath(path);

        // Check Windows restricted paths
        if (OperatingSystem.IsWindows())
        {
            foreach (var restrictedPath in WindowsRestrictedPaths)
            {
                if (normalizedPath.StartsWith(restrictedPath, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            // Check system drive root
            var systemDrive = Path.GetPathRoot(Environment.SystemDirectory);
            if (!string.IsNullOrEmpty(systemDrive) &&
                normalizedPath.Equals(systemDrive.TrimEnd(Path.DirectorySeparatorChar), StringComparison.OrdinalIgnoreCase))
                return true;
        }
        // Check Unix/Linux restricted paths
        else if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
        {
            foreach (var restrictedPath in UnixRestrictedPaths)
            {
                if (normalizedPath.StartsWith(restrictedPath, StringComparison.Ordinal))
                    return true;
            }

            // Check root directory
            if (normalizedPath == "/")
                return true;
        }

        return false;
    }

    private static void ValidateDirectoryPath(string directory)
    {
        try
        {
            // Check if directory exists
            if (Directory.Exists(directory))
            {
                // Test write access by checking attributes
                var attributes = File.GetAttributes(directory);
                if ((attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                {
                    throw new UnauthorizedAccessException($"Directory is read-only: {directory}");
                }
            }
            else
            {
                // Try to verify parent directory exists and is accessible
                var parentDir = Path.GetDirectoryName(directory);
                if (!string.IsNullOrEmpty(parentDir) && !Directory.Exists(parentDir))
                {
                    // Recursively check parent
                    ValidateDirectoryPath(parentDir);
                }
            }
        }
        catch (UnauthorizedAccessException)
        {
            throw;
        }
        catch (Exception ex) when (ex is ArgumentException or NotSupportedException or PathTooLongException)
        {
            throw new ArgumentException($"Invalid directory path: {ex.Message}", nameof(directory), ex);
        }
    }

    private static StringComparison GetPathComparison()
    {
        return OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
    }
}

