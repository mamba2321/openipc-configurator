using System;
using System.Collections.Generic;

namespace OpenIPC_Config.Parsers;

// Base class for key-pair config parsers
//
// Usage:
// Reading a config
//string fileContents = File.ReadAllText("vdec.conf");
// var config = VdecConfParser.ReadConfig(fileContents);
//
// Modifying properties
// config.Port = 6000;
//
// Generating a new config string
// string newConfig = config.GenerateConfig();
// File.WriteAllText("vdec.conf", newConfig);
//
public abstract class ConfigParserBase<T> where T : ConfigParserBase<T>, new()
{
    protected static readonly string CommentPrefix = "###";
    protected static readonly string KeyValueSeparator = "=";

    // Method to parse the config from a string
    public static T ReadConfig(string fileContents)
    {
        if (string.IsNullOrWhiteSpace(fileContents))
            throw new ArgumentException("Input configuration content is empty.");

        var config = new T();
        var lines = fileContents.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();

            // Skip comments and empty lines
            if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith(CommentPrefix) || trimmedLine.StartsWith("#"))
                continue;

            // Split key-value pairs
            var parts = trimmedLine.Split(new[] { KeyValueSeparator }, 2, StringSplitOptions.TrimEntries);
            if (parts.Length != 2)
                continue;

            var key = parts[0].Trim();
            var value = parts[1].Trim();

            // Map the key-value pair to the config properties
            config.MapKeyToProperty(key, value);
        }

        return config;
    }

    // Method to generate the config string
    public string GenerateConfig()
    {
        var lines = new List<string>();

        foreach (var (key, value, comment) in GetConfigProperties())
        {
            if (!string.IsNullOrEmpty(comment))
            {
                lines.Add($"{CommentPrefix} {comment}");
            }
            lines.Add($"{key}{KeyValueSeparator}{value}");
            lines.Add(""); // Add an empty line for readability
        }

        return string.Join("\n", lines);
    }

    // Abstract methods to implement in derived classes
    protected abstract void MapKeyToProperty(string key, string value);
    protected abstract IEnumerable<(string Key, string Value, string Comment)> GetConfigProperties();
}