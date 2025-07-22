namespace Legerity.Features.Generators.Web;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Infrastructure.IO;
using Legerity.Features.Generators;
using Legerity.Features.Generators.Models;
using Legerity.Infrastructure.Extensions;
using MADE.Collections.Compare;
using MADE.Data.Validation.Extensions;
using Scriban;
using Serilog;

internal class VuePageObjectGenerator : IPageObjectGenerator
{
    private const string BaseElementType = "WebElement";

    private static readonly GenericEqualityComparer<string> SimpleStringComparer = new(s => s.ToLower());

    public static IEnumerable<string> SupportedCoreWebElements => new List<string>
    {
        "Button",
        "CheckBox",
        "Select",
        "TextInput",
        "FileInput",
        "Image",
        "List",
        "Table",
        "TableRow",
        "Form",
        "Option",
        "TextArea",
        "RadioButton",
        "RangeInput",
        "NumberInput",
        "DateInput"
    };

    public async Task GenerateAsync(string ns, string inputPath, string outputPath)
    {
        IEnumerable<string>? filePaths = GetVueFilePaths(inputPath)?.ToList();

        if (filePaths == null || !filePaths.Any())
        {
            Log.Warning("No Vue files found in {InputPath}", inputPath);
            return;
        }

        foreach (string filePath in filePaths)
        {
            Log.Information($"Processing {filePath}...");

            try
            {
                string vueContent = await File.ReadAllTextAsync(filePath);
                var htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(vueContent);

                var templateNode = htmlDoc.DocumentNode.SelectSingleNode("//template");
                if (templateNode == null)
                {
                    Log.Warning($"Skipping {filePath} as no <template> section was found");
                    continue;
                }

                var templateData =
                    new GeneratorTemplateData(ns, Path.GetFileNameWithoutExtension(filePath), BaseElementType);

                Log.Information($"Generating template for {templateData}...");

                IEnumerable<HtmlNode> elements = this.FlattenElements(templateNode.ChildNodes);
                foreach (HtmlNode element in elements)
                {
                    if (element.NodeType != HtmlNodeType.Element)
                        continue;

                    string? id = element.GetAttributeValue("id", null);
                    string? dataTestId = element.GetAttributeValue("data-testid", null);
                    string? vTestId = element.GetAttributeValue("v-test-id", null);
                    string? classAttr = element.GetAttributeValue("class", null);

                    string? byLocatorType = GetByLocatorType(id, dataTestId, vTestId, classAttr);

                    if (byLocatorType == null || byLocatorType.IsNullOrWhiteSpace())
                    {
                        continue;
                    }

                    string? byQueryValue = GetByQueryValue(byLocatorType, id, dataTestId, vTestId, classAttr);

                    if (byQueryValue == null || byQueryValue.IsNullOrWhiteSpace())
                    {
                        continue;
                    }

                    string propertyName = GenerateUniquePropertyName(byQueryValue, templateData.Elements);
                    string elementType = GetElementWrapperType(element.Name, element.GetAttributeValue("type", null));

                    var uiElement = new UiElement(
                        elementType,
                        propertyName,
                        byLocatorType,
                        byQueryValue);

                    Log.Information($"Element found on page - {uiElement}");

                    if (templateData.Trait == null)
                    {
                        templateData.Trait = uiElement;
                    }
                    templateData.Elements.Add(uiElement);
                }

                await GeneratePageObjectClassFileAsync(templateData, outputPath);
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Failed to parse {filePath}");
                continue;
            }
        }
    }

    private static async Task GeneratePageObjectClassFileAsync(
        GeneratorTemplateData templateData,
        string outputFolder)
    {
        var pageObjectTemplate = Template.Parse(await EmbeddedResourceLoader.ReadAsync("Legerity.Templates.WebPageObject.template"));

        string outputFile = $"{templateData.Page}.cs";

        Log.Information($"Generating {outputFile} page object file...");
        string result = await pageObjectTemplate.RenderAsync(templateData);

        FileStream output = File.Create(Path.Combine(outputFolder, outputFile));
        var outputWriter = new StreamWriter(output, Encoding.UTF8);

        await using (outputWriter)
        {
            await outputWriter.WriteAsync(result);
        }
    }

    private static string? GetByLocatorType(string? id, string? dataTestId, string? vTestId, string? classAttr)
    {
        if (!string.IsNullOrWhiteSpace(id))
        {
            return "Id";
        }

        if (!string.IsNullOrWhiteSpace(dataTestId) || !string.IsNullOrWhiteSpace(vTestId))
        {
            return "CssSelector";
        }

        return !string.IsNullOrWhiteSpace(classAttr) ? "ClassName" : null;
    }

    private static string? GetByQueryValue(string byLocatorType, string? id, string? dataTestId, string? vTestId, string? classAttr)
    {
        return byLocatorType switch
        {
            "Id" => id,
            "CssSelector" => !string.IsNullOrWhiteSpace(dataTestId) 
                ? $"[data-testid='{dataTestId}']" 
                : $"[v-test-id='{vTestId}']",
            "ClassName" => classAttr?.Split(' ').FirstOrDefault()?.Trim(),
            _ => null
        };
    }

    private static string GenerateUniquePropertyName(string baseValue, List<UiElement> existingElements)
    {
        string propertyName = NormalizePropertyName(baseValue);
        string originalName = propertyName;
        int suffix = 1;

        while (existingElements.Any(e => e.Name == propertyName))
        {
            propertyName = $"{originalName}{suffix}";
            suffix++;
        }

        return propertyName;
    }

    private static string NormalizePropertyName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "Element";

        // Remove CSS selector brackets and quotes if present
        if (value.StartsWith("[") && value.EndsWith("]"))
        {
            // Extract value from [attr='value'] format
            var match = System.Text.RegularExpressions.Regex.Match(value, @"\['?([^'=\]]+)'?\]");
            if (match.Success)
            {
                value = match.Groups[1].Value;
            }
            else
            {
                // Extract from [attr='value'] format
                match = System.Text.RegularExpressions.Regex.Match(value, @"='([^']+)'");
                if (match.Success)
                {
                    value = match.Groups[1].Value;
                }
            }
        }

        // Remove non-alphanumeric characters and capitalize
        var cleaned = System.Text.RegularExpressions.Regex.Replace(value, @"[^a-zA-Z0-9]", "");
        
        if (string.IsNullOrEmpty(cleaned))
            return "Element";

        // Ensure it starts with a letter for valid C# identifier
        if (char.IsDigit(cleaned[0]))
            cleaned = "Element" + cleaned;

        return cleaned.Capitalize();
    }

    private static IEnumerable<string>? GetVueFilePaths(string searchFolder)
    {
        string[]? filePaths = default;

        try
        {
            filePaths = Directory.GetFiles(searchFolder, "*.vue", SearchOption.AllDirectories);
        }
        catch (UnauthorizedAccessException)
        {
            Log.Error("An error occurred while retrieving Vue files for processing");
        }

        return filePaths;
    }

    private static string GetElementWrapperType(string tagName, string? inputType)
    {
        string elementName;

        if (tagName.Equals("input", StringComparison.OrdinalIgnoreCase))
        {
            elementName = GetInputElementType(inputType);
        }
        else
        {
            elementName = GetStandardElementType(tagName);
        }

        return SupportedCoreWebElements.Contains(elementName, SimpleStringComparer) ? elementName : BaseElementType;
    }

    private static string GetInputElementType(string? typeAttr)
    {
        if (string.IsNullOrEmpty(typeAttr))
            return "TextInput";

        return typeAttr.ToLower() switch
        {
            "checkbox" => "CheckBox",
            "radio" => "RadioButton",
            "file" => "FileInput",
            "range" => "RangeInput",
            "number" => "NumberInput",
            "date" or "datetime-local" or "time" => "DateInput",
            "button" or "submit" or "reset" => "Button",
            "password" => "TextInput",
            _ => "TextInput"
        };
    }

    private static string GetStandardElementType(string tagName)
    {
        return tagName.ToLower() switch
        {
            "button" => "Button",
            "select" => "Select",
            "option" => "Option",
            "textarea" => "TextArea",
            "img" => "Image",
            "form" => "Form",
            "table" => "Table",
            "tr" => "TableRow",
            "ul" or "ol" => "List",
            _ => tagName.Capitalize()
        };
    }

    private IEnumerable<HtmlNode> FlattenElements(HtmlNodeCollection nodes)
    {
        return nodes.SelectMany(child => this.FlattenElements(child.ChildNodes)).Concat(nodes);
    }
}
