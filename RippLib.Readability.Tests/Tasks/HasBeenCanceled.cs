using FluentAssertions;
using RippLib.Util;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Tasks;
public class HasBeenCanceled
{
    public class Null
    {
        [Fact]
        public void Should_Return_False()
        {
            Task nullTask = null;

            var result = nullTask.HasBeenCanceled();

            result.Should().BeFalse();
        }
    }

    public class Canceled
    {
        [Fact]
        public void Should_Return_True()
        {
            var canceledTask = Task.FromCanceled(new CancellationToken(true));
            var result = canceledTask.HasBeenCanceled();
            result.Should().BeTrue();
        }
    }

    public class Active
    {
        [Fact]
        public void Should_Return_False()
        {
            var activeTask = Task.CompletedTask;
            var result = activeTask.HasBeenCanceled();
            result.Should().BeFalse();
        }
    }
}
