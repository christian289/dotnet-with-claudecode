using System;
using System.IO;
using Xunit;

// Mocking library is project-specific — pick one and use consistently:
//   using Moq;
//   using NSubstitute;
//   using FakeItEasy;
//
// Randomized / bulk data (optional):
//   using Bogus;

namespace SampleProject.Tests
{
    /// <summary>
    /// Template for a unit test class following the managing-unit-tests skill.
    ///
    /// Structure contract:
    ///   - sealed class
    ///   - IDisposable only when test holds disposable resources (temp files, streams)
    ///   - #region Happy / Boundary / Error / Helpers — omit region entirely when empty
    ///   - Method naming: {Method}_{Scenario}_{Expected}
    ///   - DisplayName: clear declarative sentence (project language policy)
    /// </summary>
    public sealed class SampleServiceTests : IDisposable
    {
        private readonly SampleService _sut;
        private readonly string _tempDirectory;

        public SampleServiceTests()
        {
            _tempDirectory = Path.Combine(Path.GetTempPath(), $"sample-{Guid.NewGuid():N}");
            Directory.CreateDirectory(_tempDirectory);

            _sut = new SampleService(_tempDirectory);
        }

        public void Dispose()
        {
            if (Directory.Exists(_tempDirectory))
            {
                Directory.Delete(_tempDirectory, recursive: true);
            }
        }

        #region Happy Path

        [Fact(DisplayName = "Valid input returns success result")]
        public void Process_ValidInput_ReturnsSuccess()
        {
            // Arrange
            var input = "valid-payload";

            // Act
            var result = _sut.Process(input);

            // Assert
            Assert.True(result.IsSuccess);
        }

        [Theory(DisplayName = "Various valid inputs all succeed")]
        [InlineData("alpha")]
        [InlineData("beta")]
        [InlineData("gamma")]
        public void Process_VariousValidInputs_AllSucceed(string input)
        {
            var result = _sut.Process(input);

            Assert.True(result.IsSuccess);
        }

        #endregion

        #region Boundary

        [Fact(DisplayName = "Empty input is treated as no-op")]
        public void Process_EmptyInput_ReturnsNoOpResult()
        {
            var result = _sut.Process(string.Empty);

            Assert.True(result.IsNoOp);
        }

        [Fact(DisplayName = "Non-existent working directory does not throw")]
        public void Process_NonExistentWorkingDirectory_DoesNotThrow()
        {
            var sut = new SampleService(Path.Combine(_tempDirectory, "missing"));

            var exception = Record.Exception(() => sut.Process("any"));

            Assert.Null(exception);
        }

        #endregion

        #region Error Path

        [Fact(DisplayName = "Null input throws ArgumentNullException")]
        public void Process_NullInput_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => _sut.Process(null!));
        }

        [Theory(DisplayName = "Whitespace-only input throws ArgumentException")]
        [InlineData(" ")]
        [InlineData("\t")]
        [InlineData("\n")]
        public void Process_WhitespaceInput_ThrowsArgumentException(string input)
        {
            Assert.Throws<ArgumentException>(() => _sut.Process(input));
        }

        #endregion

        #region Helpers

        // Test-only stubs / fakes go here. Keep them small and test-scoped.
        // For domain fakes prefer a dedicated class; reserve mocking libraries
        // for *external* dependencies (I/O, DB, network, system APIs).

        #endregion
    }

    // Dummy SUT declarations included here so this file is self-contained
    // as a reference. Delete these when copying into a real test project.
    internal sealed class SampleService
    {
        private readonly string _workingDirectory;

        public SampleService(string workingDirectory)
        {
            _workingDirectory = workingDirectory;
        }

        public ProcessResult Process(string input)
        {
            if (input is null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            if (input.Length > 0 && string.IsNullOrWhiteSpace(input))
            {
                throw new ArgumentException("Input cannot be whitespace.", nameof(input));
            }

            return input.Length == 0
                ? ProcessResult.NoOp()
                : ProcessResult.Success();
        }
    }

    internal sealed record ProcessResult(bool IsSuccess, bool IsNoOp)
    {
        public static ProcessResult Success() => new(true, false);
        public static ProcessResult NoOp() => new(false, true);
    }
}
