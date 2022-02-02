namespace Groundsman.Models;

public enum PropertyType
{
    String, Integer, Float, Boolean
}

public class DisplayProperty
{
    public string Key { get; set; }
    public PropertyType Type { get; set; }
    public object Value { get; set; }
    public int DisplayType { get => (int)Type; set => Type = (PropertyType)value; }

    public DisplayProperty(string key, object value, PropertyType type = PropertyType.String)
    {
        Key = key;
        Value = value;
        Type = type;
    }
    public static DisplayProperty FromObject(string name, object value)
    {
        Type propertyType = value.GetType();

        if (propertyType == typeof(bool))
        {
            return new DisplayProperty(name, Convert.ToBoolean(value), PropertyType.Boolean);
        }
        else if (propertyType == typeof(float) || propertyType == typeof(double))
        {
            return new DisplayProperty(name, Convert.ToSingle(value), PropertyType.Float);
        }
        else if (propertyType == typeof(long) || propertyType == typeof(int) || propertyType == typeof(short))
        {
            return new DisplayProperty(name, Convert.ToInt32(value), PropertyType.Integer);
        }
        if (propertyType == typeof(string))
        {
            return new DisplayProperty(name, value.ToString(), PropertyType.String);
        }
        else
        {
            return new DisplayProperty(name, "Unknown Type", PropertyType.String);
        }
    }

    public object ToValueObject()
    {
        try
        {
            return Type switch
            {
                PropertyType.String => Value,
                PropertyType.Integer => Convert.ToInt16(Value),
                PropertyType.Float => Convert.ToSingle(Value),
                PropertyType.Boolean => Convert.ToBoolean(Value),
                _ => throw new ArgumentException($"Property type '{Type}' not supported."),
            };
        }
        catch (Exception ex)
        {
            throw new ArgumentException($"Could not save property '{Key}'. {ex.Message}");
        }
    }
}
