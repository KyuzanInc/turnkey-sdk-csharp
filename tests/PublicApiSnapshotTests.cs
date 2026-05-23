// PublicApiSnapshotTests.cs — Detects accidental drift in the public
// surface of KyuzanInc.Turnkey.Sdk.
//
// On first run, the test populates the baseline file
//   tests/PublicApi.expected.txt
// from the current public API. Subsequent runs compare the live API
// against that baseline; any deviation fails the test and the diff is
// shown.
//
// To intentionally bump the public surface:
//   1. delete tests/PublicApi.expected.txt
//   2. re-run the test to regenerate it
//   3. commit the new baseline with the production change

using System.IO;
using FluentAssertions;
using PublicApiGenerator;
using Xunit;

namespace Turnkey.Tests
{
    public class PublicApiSnapshotTests
    {
        [Fact]
        public void PublicApi_DoesNotDrift()
        {
            string actual = typeof(Crypto).Assembly.GeneratePublicApi(
                new ApiGeneratorOptions
                {
                    IncludeAssemblyAttributes = false,
                });

            // Resolve the baseline relative to the test project source.
            string baselinePath = ResolveBaselinePath();
            if (!File.Exists(baselinePath))
            {
                // First run; seed the baseline so CI runners without it can
                // succeed on first push. Write the file next to the test
                // assembly so test environments see it.
                File.WriteAllText(baselinePath, actual);
            }
            // Opt-in baseline regen: set TURNKEY_SDK_REGENERATE_PUBLIC_API=1
            // before running this test to overwrite the baseline file.
            if (System.Environment.GetEnvironmentVariable("TURNKEY_SDK_REGENERATE_PUBLIC_API") == "1")
            {
                File.WriteAllText(baselinePath, actual);
            }
            string expected = File.ReadAllText(baselinePath);

            // Normalize line endings for cross-platform parity.
            actual.Replace("\r\n", "\n").Should().Be(expected.Replace("\r\n", "\n"),
                "the public API has drifted. Re-run with TURNKEY_SDK_REGENERATE_PUBLIC_API=1 to update the baseline, then review the diff and commit.");
        }

        private static string ResolveBaselinePath()
        {
            // Walk up from the test bin until we find the tests/ folder.
            string dir = System.AppContext.BaseDirectory;
            for (int i = 0; i < 8; i++)
            {
                string candidate = Path.Combine(dir, "PublicApi.expected.txt");
                if (File.Exists(candidate)) return candidate;
                candidate = Path.Combine(dir, "tests", "PublicApi.expected.txt");
                if (File.Exists(candidate)) return candidate;
                dir = Path.GetDirectoryName(dir.TrimEnd(Path.DirectorySeparatorChar))!;
                if (dir == null) break;
            }
            // Fallback to next-to-binary location so the first-run write
            // succeeds without elevated permissions.
            return Path.Combine(System.AppContext.BaseDirectory, "PublicApi.expected.txt");
        }
    }
}
