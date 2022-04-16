using EmptyObject.Attributes;

namespace EmptyObject.Test;


[Empty]
public partial record TestRecord2(string StringValue,int IntValue,int? NullableIntValue,string[] Array);