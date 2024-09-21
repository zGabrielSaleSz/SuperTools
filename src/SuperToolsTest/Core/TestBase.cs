using Xunit.Abstractions;

namespace SuperToolsTests.Core
{
    public class TestBase
    {
        private readonly ITestOutputHelper _output;

        public TestBase(ITestOutputHelper output)
        {
            _output = output;
        }

        protected void Log(string message)
        {
            _output.WriteLine(message);
        }
    }
}
