public class StringEventChannel : GenericEventChannel<StringEvent> {}

[System.Serializable]
public struct StringEvent
{
    public string Value;

    public StringEvent(string value)
    {
        Value = value;
    }
}
