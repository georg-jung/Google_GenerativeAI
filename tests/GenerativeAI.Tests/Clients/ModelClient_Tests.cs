﻿using GenerativeAI.Clients;
using Shouldly;
using Xunit.Abstractions;

namespace GenerativeAI.Tests.Clients
{
    public class ModelClient_Tests:TestBase
    {
        public ModelClient_Tests(ITestOutputHelper helper):base(helper)
        {
            
        }
        [Fact]
        public async Task ShouldGetListOfModels()
        {
            var client = new ModelClient(GetTestGooglePlatform());

            var response = await client.ListModelsAsync();
            
            var models = response.Models;
            models.ShouldNotBeNull();
            models.Count.ShouldBeGreaterThan(0);
            foreach (var modelInfo in models)
            {
                modelInfo.Name.ShouldNotBeNullOrEmpty();
                modelInfo.Description.ShouldNotBeNullOrEmpty();
                modelInfo.DisplayName.ShouldNotBeNullOrEmpty();
                modelInfo.InputTokenLimit.ShouldBeGreaterThan(0);
                modelInfo.OutputTokenLimit.ShouldBeGreaterThan(0);
                //modelInfo.Temperature.ShouldBeGreaterThan(0);
                //modelInfo.TopK.ShouldBeGreaterThan(0);
                //modelInfo.TopP.ShouldBeGreaterThan(0);
                modelInfo.Version.ShouldNotBeNullOrEmpty();
                modelInfo.SupportedGenerationMethods.ShouldNotBeNull();
                //modelInfo.BaseModelId.ShouldNotBeNullOrEmpty();
                modelInfo.SupportedGenerationMethods.Count.ShouldBeGreaterThan(0);
                Console.WriteLine(modelInfo.Name);
                Console.WriteLine(modelInfo.BaseModelId);
                Console.WriteLine(modelInfo.DisplayName);
                Console.WriteLine(modelInfo.Description);
                Console.WriteLine("");

            }
        }

        [Fact]
        public async Task GetModelInfo()
        {
            var client = new ModelClient(GetTestGooglePlatform());

            var modelInfo = await client.GetModelAsync("gemini-flash-1.0");
            modelInfo.Name.ShouldNotBeNullOrEmpty();
            modelInfo.Description.ShouldNotBeNullOrEmpty();
            modelInfo.DisplayName.ShouldNotBeNullOrEmpty();
            modelInfo.InputTokenLimit.ShouldBeGreaterThan(0);
            modelInfo.OutputTokenLimit.ShouldBeGreaterThan(0);
            //modelInfo.BaseModelId.ShouldNotBeNullOrEmpty();
            //modelInfo.Temperature.ShouldBeGreaterThan(0);
            //modelInfo.TopK.ShouldBeGreaterThan(0);
            //modelInfo.TopP.ShouldBeGreaterThan(0);
            modelInfo.Version.ShouldNotBeNullOrEmpty();
            modelInfo.SupportedGenerationMethods.ShouldNotBeNull();
            modelInfo.SupportedGenerationMethods.Count.ShouldBeGreaterThan(0);
            Console.WriteLine(modelInfo.Name);
            Console.WriteLine(modelInfo.BaseModelId);
            Console.WriteLine(modelInfo.DisplayName);
            Console.WriteLine(modelInfo.Description);
            Console.WriteLine("");
        }
    }
}
