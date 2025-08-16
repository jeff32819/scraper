namespace ScraperApp2;

public class TestEnum
{
    public enum Status
    {
        [StringValue("in-progress")] InProgress,

        [StringValue("completed")] Completed
    }

    public class StringValueAttribute(string value) : Attribute
    {
        public string Value { get; } = value;
    }
}