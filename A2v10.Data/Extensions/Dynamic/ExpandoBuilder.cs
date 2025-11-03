// Copyright © 2025 Oleksandr Kukhtin. All rights reserved.

namespace A2v10.Data.Core.Extensions.Dynamic;

public class ExpandoBuilder
{
    private readonly IDictionary<string, object> _dict = new ExpandoObject()!;

    public ExpandoBuilder Add(string key, object value)
    {
        _dict[key] = value;
        return this;
    }

    public ExpandoObject Build() => (ExpandoObject) _dict!;
}

