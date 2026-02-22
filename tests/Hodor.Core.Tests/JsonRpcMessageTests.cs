using FluentAssertions;
using Hodor.Core.Mcp;
using Xunit;

namespace Hodor.Core.Tests;

public class JsonRpcMessageTests
{
    [Fact]
    public void JsonRpcMessage_DefaultJsonrpc()
    {
        var msg = new JsonRpcMessage { Jsonrpc = "2.0", Id = "1", Method = "test" };
        msg.Jsonrpc.Should().Be("2.0");
    }

    [Fact]
    public void JsonRpcError_StoresValues()
    {
        var err = new JsonRpcError(-32603, "Internal error", null);
        err.Code.Should().Be(-32603);
        err.Message.Should().Be("Internal error");
        err.Data.Should().BeNull();
    }
}
