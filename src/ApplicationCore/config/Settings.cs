// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

// ReSharper disable InconsistentNaming

public static class Settings
{
    private const string DefaultConfigFile = "../ApplicationCore/config/settings.json";
    private const string TypeKey = "type";
    private const string ModelKey = "model";
    private const string EndpointKey = "endpoint";
    private const string SecretKey = "apikey";
    private const string OrgKey = "org";

    private const string OpenAIApiKey = "openAIApiKey";
    private const bool StoreConfigOnFile = true;

    // Load settings from file
    public static (bool useAzureOpenAI, string model, string azureEndpoint, string apiKey, string orgId, string openAIApiKey)
        LoadFromFile(string configFile = DefaultConfigFile)
    {
        if (!File.Exists(DefaultConfigFile))
        {
            Console.WriteLine("Configuration not found: " + DefaultConfigFile);
            // Console.WriteLine("\nPlease run the Setup Notebook (0-AI-settings.ipynb) to configure your AI backend first.\n");
            // throw new Exception("Configuration not found, please setup the notebooks first using notebook 0-AI-settings.pynb");
        }

        try
        {
            var config = JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(DefaultConfigFile));
            bool useAzureOpenAI = config[TypeKey] == "azure";
            string model = config[ModelKey];
            string azureEndpoint = config[EndpointKey];
            string apiKey = config[SecretKey];
            string orgId = config[OrgKey];
            if (orgId == "none") { orgId = ""; }
            string openAIApiKey = config[OpenAIApiKey];

            return (useAzureOpenAI, model, azureEndpoint, apiKey, orgId, openAIApiKey);
        }
        catch (Exception e)
        {
            Console.WriteLine("Something went wrong: " + e.Message);
            return (true, "", "", "", "", "");
        }
    }

    // Delete settings file
    public static void Reset(string configFile = DefaultConfigFile)
    {
        if (!File.Exists(configFile)) { return; }

        try
        {
            File.Delete(configFile);
            Console.WriteLine("Settings deleted. Run the notebook again to configure your AI backend.");
        }
        catch (Exception e)
        {
            Console.WriteLine("Something went wrong: " + e.Message);
        }
    }

    // Read and return settings from file
    private static (bool useAzureOpenAI, string model, string azureEndpoint, string apiKey, string orgId, string openAIApiKey)
        ReadSettings(bool _useAzureOpenAI, string configFile)
    {
        // Save the preference set in the notebook
        bool useAzureOpenAI = _useAzureOpenAI;
        string model = "";
        string azureEndpoint = "";
        string apiKey = "";
        string orgId = "";
        string openAIApiKey = "";

        try
        {
            if (File.Exists(configFile))
            {
                (useAzureOpenAI, model, azureEndpoint, apiKey, orgId, openAIApiKey) = LoadFromFile(configFile);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("Something went wrong: " + e.Message);
        }

        // If the preference in the notebook is different from the value on file, then reset
        if (useAzureOpenAI != _useAzureOpenAI)
        {
            Reset(configFile);
            useAzureOpenAI = _useAzureOpenAI;
            model = "";
            azureEndpoint = "";
            apiKey = "";
            orgId = "";
            openAIApiKey = "";
        }

        return (useAzureOpenAI, model, azureEndpoint, apiKey, orgId, openAIApiKey);
    }

    // Write settings to file
    private static void WriteSettings(
        string configFile, bool useAzureOpenAI, string model, string azureEndpoint, string apiKey, string orgId, string openAIApiKey)
    {
        try
        {
            if (StoreConfigOnFile)
            {
                var data = new Dictionary<string, string>
                {
                    { TypeKey, useAzureOpenAI ? "azure" : "openai" },
                    { ModelKey, model },
                    { EndpointKey, azureEndpoint },
                    { SecretKey, apiKey },
                    { OrgKey, orgId },
                    { OpenAIApiKey, openAIApiKey}
                };

                var options = new JsonSerializerOptions { WriteIndented = true };
                File.WriteAllText(configFile, JsonSerializer.Serialize(data, options));
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("Something went wrong: " + e.Message);
        }

        // If asked then delete the credentials stored on disk
        if (!StoreConfigOnFile && File.Exists(configFile))
        {
            try
            {
                File.Delete(configFile);
            }
            catch (Exception e)
            {
                Console.WriteLine("Something went wrong: " + e.Message);
            }
        }
    }
}