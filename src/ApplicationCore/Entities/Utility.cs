using System;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.AI.ImageGeneration;

namespace Microsoft.eShopWeb.ApplicationCore.Entities;

public class Utility {
    public IKernel? _kernel { get; set; }

    public Utility(){
        var (useAzureOpenAI, model, azureEndpoint, apiKey, orgId, bingApiKey, openAIApiKey) = Settings.LoadFromFile();

        if(_kernel == null){
            _kernel = new KernelBuilder()
            .WithAzureTextCompletionService(model, azureEndpoint, apiKey)
            .WithOpenAIImageGenerationService(openAIApiKey, orgId)
            .Build();
        }
    }

    public async Task<string> GenerateAiPictureUriAsync(string description)
    {
        var dallE = _kernel.GetService<IImageGeneration>();

        var genImgDescription = _kernel.CreateSemanticFunction(@"
        {{$input}}

        Given the above keywords think about a related image and describe the image with one detailed sentence. 
        The description cannot contain numbers.", maxTokens: 256, temperature: 1);

        var imageDescription = await genImgDescription.InvokeAsync(description);
        string imageDescriptionString = imageDescription.ToString();

        var imageUrl = await dallE.GenerateImageAsync(imageDescriptionString, 256, 256);
        
        // Log imageUrl to console
        Console.WriteLine("--- Image URL ---");
        Console.WriteLine(imageUrl);
        Console.WriteLine("--- Image URL ---");
        
        return imageUrl;
    }
}