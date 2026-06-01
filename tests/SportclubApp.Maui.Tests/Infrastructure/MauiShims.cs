// Test-only stand-ins for attributes that the linked MAUI source references
// at compile time but that we don't need at runtime. The real types live in
// Microsoft.Maui.Controls, which targets net10.0-android / net10.0-ios — we
// can't pull it into this plain net10.0 test project.

namespace Microsoft.Maui.Controls;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
internal sealed class QueryPropertyAttribute(string name, string queryId) : Attribute
{
    public string Name { get; } = name;
    public string QueryId { get; } = queryId;
}
