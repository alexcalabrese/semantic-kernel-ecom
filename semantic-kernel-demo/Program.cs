using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.SemanticFunctions;
using Microsoft.SemanticKernel.Orchestration;
using Microsoft.SemanticKernel.Memory;
using Microsoft.SemanticKernel.AI.ImageGeneration;
using System.Net;
using Newtonsoft.Json;

var builder = new KernelBuilder();

var (useAzureOpenAI, model, azureEndpoint, apiKey, orgId, bingApiKey, openAIApiKey) = Settings.LoadFromFile();

Console.WriteLine(openAIApiKey);

Console.WriteLine("[INFO] Setting loaded");

if (useAzureOpenAI){
    builder.WithAzureTextCompletionService(model, azureEndpoint, apiKey);
    builder.WithAzureTextEmbeddingGenerationService("sk-ada", azureEndpoint, apiKey);
    builder.WithOpenAIImageGenerationService(openAIApiKey, orgId);
} else {
    builder.WithOpenAITextCompletionService(model, apiKey, orgId);
    builder.WithOpenAITextEmbeddingGenerationService("sk-ada", apiKey, orgId);
}

Console.WriteLine("[INFO] Kernel configured");

var kernel = builder.Build();

//
// --- DEMO ---
//

// 1. First Semantic Function
var prompt = @"{{$input}}

One line TLDR with the fewest words.";

var tldr = kernel.CreateSemanticFunction(prompt);

string text1 = @"
1st Law of Thermodynamics - Energy cannot be created or destroyed.
2nd Law of Thermodynamics - For a spontaneous process, the entropy of the universe increases.
3rd Law of Thermodynamics - A perfect crystal at zero Kelvin has zero entropy.";

Console.WriteLine("-- 1. First Semantic Function --");
// Console.WriteLine(await tldr.InvokeAsync(text1));

// Output:
//   Energy conserved, entropy increases, zero entropy at 0K.

// 2. Semantic Function with parameters
string skPrompt = """
{{$input}}

Summarize the content above in 5 words.
""";

var summarize = kernel.CreateSemanticFunction(skPrompt, maxTokens: 2000, temperature: 0.2, topP: 0.5);

var textToSummarize = @"
    1) A robot may not injure a human being or, through inaction,
    allow a human being to come to harm.

    2) A robot must obey orders given it by human beings except where
    such orders would conflict with the First Law.

    3) A robot must protect its own existence as long as such protection
    does not conflict with the First or Second Law.
";

Console.WriteLine("-- 2. Semantic Function with parameters --");
// Console.WriteLine(await summarize.InvokeAsync(textToSummarize));

// Output => Robots must not harm humans.

// 3. Run a Semantic Function with a Skill
// Load the Skills Directory
var skillsDirectory = Path.Combine(System.IO.Directory.GetCurrentDirectory(), "skills");

// Load the FunSkill from the Skills Directory
var mySkill = kernel.ImportSemanticSkillFromDirectory(skillsDirectory, "FunSkill");

var myContext = new ContextVariables(); 

// The variables are manually set when you use a ContextVariables object
myContext.Set("input", "pizza with a pineapple topping"); 
myContext.Set("audience_type", "italian people"); 

// Run the Function called Joke with the default parameter of $input
Console.WriteLine("-- 3. Run a Semantic Function with a Skill --");
// Console.WriteLine(await kernel.RunAsync(myContext, mySkill["Joke"]));

// 4. Generate an image from text
// var dallE = kernel.GetService<IImageGeneration>();

// Use DALL-E 2 to generate an image. OpenAI in this case returns a URL (though you can ask to return a base64 image)
// var imageUrl = await dallE.GenerateImageAsync("red supercar", 256, 256);

// Console.WriteLine("Image uri: " + imageUrl);

var genImgDescription = kernel.CreateSemanticFunction(@"
{{$input}}

Given the above keywords think about a related image and describe the image with one detailed sentence. 
The description cannot contain numbers.", maxTokens: 256, temperature: 1);

// var imageDescription = await genImgDescription.InvokeAsync($"T-Shirt	.NET	1	.NET Bot Black Sweatshirt	.NET Bot Black Sweatshirt");
// Console.WriteLine(imageDescription);

// string url = "https://oaidalleapiprodscus.blob.core.windows.net/private/org-2EuweKZ3vTcfs6YasrMl2dbs/user-6iDAkFWs1axtrk3BK6rInt0Y/img-YW3VABOdJU2XJYeaa3UkSinD.png?st=2023-06-21T13%3A54%3A59Z&se=2023-06-21T15%3A54%3A59Z&sp=r&sv=2021-08-06&sr=b&rscd=inline&rsct=image/png&skoid=6aaadede-4fb3-4698-a8f6-684d7786b067&sktid=a48cca56-e6da-484e-a814-9c849652bcb3&skt=2023-06-21T12%3A58%3A32Z&ske=2023-06-22T12%3A58%3A32Z&sks=b&skv=2021-08-06&sig=i1ZeqsvkghPtQXmqCL6zgId5BIiuKLYzRApfwmzk0G4%3D";
// var number = new Random().Next( int.MinValue, int.MaxValue );

// using (WebClient client = new WebClient()) 
// {
//     client.DownloadFile(new Uri(url), @"images\image" + number + ".png");
// }

// Generating some tags from given text
var generateTags = kernel.CreateSemanticFunction(@"
{{$input}}

Given the above informations think about some related tags for an eccomerce store and return as a list spearated with comma.", maxTokens: 256, temperature: 1);

var tagList = await generateTags.InvokeAsync(".NET Bot Black Sweatshirt	.NET Bot Black Sweatshirt");
Console.WriteLine(tagList);
// Convert tagList to string
// string tagListString = tagList.ToString();

// dynamic obj = JsonConvert.DeserializeObject(tagListString);

// Console.WriteLine(obj["tags"]);