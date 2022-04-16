using System;
using System.Collections.Generic;
using Xunit;

namespace EmptyObject.Test;

public class EmptyObjectTest
{
    private readonly TestRecord2 _testRecord2 = new(string.Empty,0,null,Array.Empty<string>());

    [Fact]
    public void DefaultConstructorShouldEqualManuallyEmptyObject()
    {
        Assert.Equal(new TestRecord2(), _testRecord2);
    }
    [Fact]
    public void EmptyShouldEqualManuallyEmptyObject()
    {
        Assert.Equal( TestRecord2.Empty,_testRecord2);
    }
    [Fact]
    public void EmptyShouldEqualDefaultConstructor()
    {
        Assert.Equal( TestRecord2.Empty,new TestRecord2());
    }
}