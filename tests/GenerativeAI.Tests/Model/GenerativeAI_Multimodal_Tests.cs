﻿using GenerativeAI.Types;
using Shouldly;
using Xunit.Abstractions;

namespace GenerativeAI.Tests.Model
{
    public class GenerativeAI_Multimodal_Tests : TestBase
    {
        private ITestOutputHelper Console;
        private const string TestModel = GoogleAIModels.Gemini15Flash;

        public GenerativeAI_Multimodal_Tests(ITestOutputHelper helper)
        {
            this.Console = helper;
        }

        [Fact]
        public async Task ShouldIdentifyObjectInImage()
        {
            //Arrange
            var model = new GenerativeModel(GetTestGooglePlatform(), TestModel);
            var request = new GenerateContentRequest();
            request.AddInlineFile("image.png", false);
            request.AddText("Identify objects in the image?");

            //Act
            var result = await model.GenerateContentAsync(request);

            //Assert
            var text = result.Text();
            text.ShouldNotBeNull();
            text.ShouldContain("Flower", Case.Insensitive);
            Console.WriteLine(result.Text());
        }

        [Fact]
        public async Task ShouldIdentifyImageWithFilePath()
        {
            //Arrange
            var model = new GenerativeModel(GetTestGooglePlatform(), TestModel);

            string prompt = "Identify objects in the image?";

            //Act
            var result = await model.GenerateContentAsync(prompt, "image.png");

            //Assert
            result.ShouldNotBeNull();
            var text = result.Text();
            text.ShouldNotBeNull();
            text.ShouldContain("Flower", Case.Insensitive);
            Console.WriteLine(result.Text());
        }

        [Fact]
        public async Task ShouldProcessVideoWithFilePath()
        {
            //Arrange
            var model = new GenerativeModel(GetTestGooglePlatform(), TestModel);

            string prompt = "Describe this video?";

            //Act
            var result = await model.GenerateContentAsync(prompt, "TestData/testvideo.mp4");

            //Assert
            result.ShouldNotBeNull();
            var text = result.Text();
            text.ShouldNotBeNull();
            text.ShouldContain("meeting", Case.Insensitive);
            Console.WriteLine(result.Text());
        }

        [Fact]
        public async Task ShouldProcessAudioWithFilePath()
        {
            //Arrange
            var model = new GenerativeModel(GetTestGooglePlatform(), TestModel);

            string prompt = "Describe this audio?";

            //Act
            var result = await model.GenerateContentAsync(prompt, "TestData/testaudio.mp3");

            //Assert
            result.ShouldNotBeNull();
            var text = result.Text();
            text.ShouldNotBeNull();
            text.ShouldContain("theological", Case.Insensitive);
            Console.WriteLine(result.Text());
        }

        [Fact]
        public async Task ShouldIdentifyImageWithWithStreaming()
        {
            var imageFile = "image.png";

            string prompt = "Identify objects in the image?";

            var apiKey = Environment.GetEnvironmentVariable("Gemini_API_Key", EnvironmentVariableTarget.User);

            var model = new GenerativeModel(GetTestGooglePlatform(), TestModel);

            var responses = new List<string>();
            await foreach (var response in model.StreamContentAsync(prompt, imageFile))
            {
                response.ShouldNotBeNull();
                responses.Add(response.Text() ?? string.Empty);
                Console.WriteLine($"Chunk: {response.Text()}");
            }

            responses.Count.ShouldBeGreaterThan(0);
        }

        [Fact]
        public async Task ShouldIdentifyImageWithChatAndStreaming()
        {
            var imageFile = "image.png";

            string prompt = "Identify objects in the image?";

            var apiKey = Environment.GetEnvironmentVariable("Gemini_API_Key", EnvironmentVariableTarget.User);

            var model = new GenerativeModel(GetTestGooglePlatform(), TestModel);

            var chatSession = model.StartChat();
            var responses = new List<string>();
            
            await foreach (var response in chatSession.StreamContentAsync(prompt, imageFile))
            {
                response.ShouldNotBeNull();
                responses.Add(response.Text() ?? string.Empty);
                Console.WriteLine($"Chunk: {response.Text()}");
            }
            var fullString = String.Join("", responses);
            
            responses.Count.ShouldBeGreaterThan(0);
            
            fullString.ShouldContain("blueberry", Case.Insensitive);
            
            var response2 = await chatSession.GenerateContentAsync("can you estimate number of blueberries?");
            response2.Text().ShouldContain("blueberries", Case.Insensitive);
            Console.WriteLine(response2.Text());
        }
    }
}