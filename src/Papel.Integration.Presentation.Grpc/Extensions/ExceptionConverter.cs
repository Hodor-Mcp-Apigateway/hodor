namespace Papel.Integration.Presentation.Grpc.Extensions;

using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

internal sealed class ExceptionConverter : JsonConverter<Exception>
{
    public override bool CanConvert(Type typeToConvert) => typeof(Exception).IsAssignableFrom(typeToConvert);

    public override Exception? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException();
        }

        //nnvar exeExceptionInfo = new JsonException();
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                return new JsonException("reader token type is EndObject");
            }

            if (reader.TokenType == JsonTokenType.PropertyName)
            {
                var propName = (reader.GetString() ?? string.Empty).ToLower(CultureInfo.InvariantCulture);
                reader.Read();

                switch (propName)
                {
                    case var _ when propName.Equals(nameof(JsonException.Message).ToLower(CultureInfo.InvariantCulture), StringComparison.Ordinal):
                        new JsonException(reader.GetString());
                        break;
                }
            }
        }
        throw new JsonException();
    }

    public override void Write(Utf8JsonWriter writer, Exception value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        writer.WriteString(nameof(value.Message), value.Message);

        writer.WriteEndObject();
    }
}
