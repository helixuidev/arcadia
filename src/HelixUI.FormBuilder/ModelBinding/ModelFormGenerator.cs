using System.ComponentModel.DataAnnotations;
using System.Reflection;
using HelixUI.FormBuilder.Schema;

namespace HelixUI.FormBuilder.ModelBinding;

/// <summary>
/// Generates a <see cref="FormSchema"/> from a C# class using reflection.
/// Respects DataAnnotations, Display attributes, and HelixUI-specific attributes.
/// </summary>
public static class ModelFormGenerator
{
    /// <summary>
    /// Generates a FormSchema from a model type.
    /// </summary>
    /// <typeparam name="T">The model type to reflect.</typeparam>
    /// <param name="title">Optional form title. Defaults to the type name.</param>
    public static FormSchema Generate<T>(string? title = null)
    {
        return Generate(typeof(T), title);
    }

    /// <summary>
    /// Generates a FormSchema from a model type.
    /// </summary>
    /// <param name="modelType">The model type to reflect.</param>
    /// <param name="title">Optional form title.</param>
    public static FormSchema Generate(Type modelType, string? title = null)
    {
        var schema = new FormSchema
        {
            Title = title ?? Humanize(modelType.Name)
        };

        var properties = modelType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead && p.CanWrite)
            .Where(p => p.GetCustomAttribute<HelixIgnoreAttribute>() is null)
            .ToList();

        // Group by section
        var sectionGroups = new Dictionary<string, (HelixSectionAttribute Attr, List<(PropertyInfo Prop, int Order)> Props)>();
        var unsectioned = new List<(PropertyInfo Prop, int Order)>();

        foreach (var prop in properties)
        {
            var sectionAttr = prop.GetCustomAttribute<HelixSectionAttribute>();
            var fieldAttr = prop.GetCustomAttribute<HelixFieldAttribute>();
            var order = fieldAttr?.Order ?? int.MaxValue;

            if (sectionAttr is not null)
            {
                if (!sectionGroups.ContainsKey(sectionAttr.Title))
                {
                    sectionGroups[sectionAttr.Title] = (sectionAttr, new List<(PropertyInfo, int)>());
                }
                sectionGroups[sectionAttr.Title].Props.Add((prop, order));
            }
            else
            {
                unsectioned.Add((prop, order));
            }
        }

        // Build unsectioned fields
        foreach (var (prop, _) in unsectioned.OrderBy(x => x.Order))
        {
            schema.Fields.Add(BuildFieldSchema(prop));
        }

        // Build sections
        foreach (var (sectionTitle, (attr, props)) in sectionGroups.OrderBy(g => g.Value.Attr.Order))
        {
            var section = new FormSectionSchema
            {
                Title = sectionTitle,
                Description = attr.Description,
                Collapsible = attr.Collapsible
            };

            foreach (var (prop, _) in props.OrderBy(x => x.Order))
            {
                section.Fields.Add(BuildFieldSchema(prop));
            }

            schema.Sections.Add(section);
        }

        return schema;
    }

    private static FieldSchema BuildFieldSchema(PropertyInfo prop)
    {
        var fieldAttr = prop.GetCustomAttribute<HelixFieldAttribute>();
        var displayAttr = prop.GetCustomAttribute<DisplayAttribute>();
        var requiredAttr = prop.GetCustomAttribute<RequiredAttribute>();
        var stringLenAttr = prop.GetCustomAttribute<StringLengthAttribute>();
        var rangeAttr = prop.GetCustomAttribute<RangeAttribute>();
        var emailAttr = prop.GetCustomAttribute<EmailAddressAttribute>();
        var phoneAttr = prop.GetCustomAttribute<PhoneAttribute>();
        var urlAttr = prop.GetCustomAttribute<UrlAttribute>();
        var dataTypeAttr = prop.GetCustomAttribute<DataTypeAttribute>();
        var conditionAttrs = prop.GetCustomAttributes<HelixConditionAttribute>().ToList();

        var fieldType = fieldAttr?.HasExplicitType == true
            ? fieldAttr.Type
            : InferFieldType(prop.PropertyType, dataTypeAttr);

        var field = new FieldSchema
        {
            Name = prop.Name,
            Type = fieldType,
            Label = displayAttr?.GetName() ?? Humanize(prop.Name),
            Placeholder = fieldAttr?.Placeholder ?? displayAttr?.GetPrompt(),
            HelperText = fieldAttr?.HelperText ?? displayAttr?.GetDescription(),
            Required = requiredAttr is not null,
            ColumnSpan = fieldAttr?.ColumnSpan ?? 12
        };

        // Build validation rules
        var validation = new ValidationRule();
        var hasValidation = false;

        if (stringLenAttr is not null)
        {
            if (stringLenAttr.MinimumLength > 0)
            {
                validation.MinLength = stringLenAttr.MinimumLength;
                hasValidation = true;
            }
            validation.MaxLength = stringLenAttr.MaximumLength;
            hasValidation = true;
        }

        if (rangeAttr is not null)
        {
            if (rangeAttr.Minimum is IConvertible minConv)
            {
                validation.Min = Convert.ToDouble(minConv);
                hasValidation = true;
            }
            if (rangeAttr.Maximum is IConvertible maxConv)
            {
                validation.Max = Convert.ToDouble(maxConv);
                hasValidation = true;
            }
        }

        if (emailAttr is not null) { validation.Pattern = "email"; hasValidation = true; }
        if (phoneAttr is not null) { validation.Pattern = "phone"; hasValidation = true; }
        if (urlAttr is not null) { validation.Pattern = "url"; hasValidation = true; }

        if (hasValidation) field.Validation = validation;

        // Build conditions
        if (conditionAttrs.Count > 0)
        {
            field.Conditions = conditionAttrs.Select(c => c.ToRule()).ToList();
        }

        // Handle enums → Select with options
        var underlyingType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
        if (underlyingType.IsEnum)
        {
            field.Options = Enum.GetNames(underlyingType)
                .Select(name => new FieldOption
                {
                    Label = Humanize(name),
                    Value = name
                })
                .ToList();
        }

        return field;
    }

    private static FieldType InferFieldType(Type propertyType, DataTypeAttribute? dataTypeAttr)
    {
        if (dataTypeAttr is not null)
        {
            return dataTypeAttr.DataType switch
            {
                DataType.Password => FieldType.Password,
                DataType.MultilineText => FieldType.TextArea,
                DataType.EmailAddress => FieldType.Text,
                DataType.PhoneNumber => FieldType.Text,
                DataType.Url => FieldType.Text,
                DataType.Date => FieldType.Date,
                DataType.Time => FieldType.Time,
                _ => FieldType.Text
            };
        }

        var type = Nullable.GetUnderlyingType(propertyType) ?? propertyType;

        if (type == typeof(string)) return FieldType.Text;
        if (type == typeof(int) || type == typeof(long) || type == typeof(short) ||
            type == typeof(float) || type == typeof(double) || type == typeof(decimal))
            return FieldType.Number;
        if (type == typeof(bool)) return FieldType.Checkbox;
        if (type == typeof(DateTime)) return FieldType.Date;
        if (type == typeof(TimeSpan)) return FieldType.Time;
        if (type.IsEnum) return FieldType.Select;

        return FieldType.Text;
    }

    private static string Humanize(string pascalCase)
    {
        if (string.IsNullOrEmpty(pascalCase)) return pascalCase;

        var chars = new List<char> { pascalCase[0] };
        for (var i = 1; i < pascalCase.Length; i++)
        {
            if (char.IsUpper(pascalCase[i]) && !char.IsUpper(pascalCase[i - 1]))
            {
                chars.Add(' ');
            }
            chars.Add(pascalCase[i]);
        }
        return new string(chars.ToArray());
    }
}
