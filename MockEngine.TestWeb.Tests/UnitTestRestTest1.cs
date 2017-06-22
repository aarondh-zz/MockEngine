using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MockEngine.TestWeb.Controllers;
using MockEngine.TestWeb.Models;
using System.Diagnostics;
using MockEngine.Utilities;
using System.Runtime.Serialization;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Net;
using System.Dynamic;

namespace MockEngine.TestWeb.Tests
{
    [TestClass]
    public class UnitTestRestTest1
    {
        static HttpClient httpClient = new HttpClient();
        static UnitTestRestTest1()
        {
            // New code:
            httpClient.BaseAddress = new Uri("http://localhost:11848/");
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));
        }
        static async Task<HttpResponseMessage> SendTestMessageAsync(string requestUri, int numberParamter1, TestRequestMessage testRequestMessage, string scenario, bool boolParameter2, string textParameter3)
        {
            return await httpClient.PostAsXmlAsync<TestRequestMessage>($"{requestUri}?numberParameter1={numberParamter1}&scenario={scenario}&booleanParameter2={boolParameter2}&textParameter3={textParameter3}", testRequestMessage);
        }
        static async Task<TestResponseMessage> SendTestMessageAndReadAsync(string requestUri, int numberParamter1, TestRequestMessage testRequestMessage, string scenario, bool boolParameter2, string textParameter3, HttpStatusCode expectedStatusCode = HttpStatusCode.OK)
        {
            TestResponseMessage testResponseMessage;
            HttpResponseMessage response = await SendTestMessageAsync(requestUri, numberParamter1, testRequestMessage, scenario, boolParameter2, textParameter3);
            if ( response.IsSuccessStatusCode )
            {
                testResponseMessage = await response.Content.ReadAsAsync<TestResponseMessage>();
            }
            else
            {
                var text = await response.Content.ReadAsStringAsync();
                Trace.WriteLine(text);
                testResponseMessage = null;
            }
            Assert.AreEqual(expectedStatusCode, response.StatusCode, $"Expected status code {expectedStatusCode} but got {response.StatusCode}");


            return testResponseMessage;
        }
        static async Task<DynamicXml> SendTestMessageAndReadDynamicAsync(string requestUri, int numberParamter1, TestRequestMessage testRequestMessage, string scenario, bool boolParameter2, string textParameter3, HttpStatusCode expectedStatusCode = HttpStatusCode.OK)
        {
            DynamicXml graph;
            HttpResponseMessage response = await SendTestMessageAsync(requestUri, numberParamter1, testRequestMessage, scenario, boolParameter2, textParameter3);
            if ( response.IsSuccessStatusCode)
            {
                var text = await response.Content.ReadAsStringAsync();
                graph = DynamicXml.Parse(text);
            }
            else
            {
                var text = await response.Content.ReadAsStringAsync();
                Trace.WriteLine(text);
                graph = null;
            }
            Assert.AreEqual(expectedStatusCode, response.StatusCode, $"Expected status code {expectedStatusCode} but got {response.StatusCode}");

            return graph;
        }
        [TestMethod]
        public void TestReady()
        {
            var requestMessage = new TestRequestMessage();

            var task = httpClient.GetAsync("/api/MockTest/Ready");
            task.Wait();
            HttpResponseMessage response = task.Result;
            response.EnsureSuccessStatusCode();
            Trace.WriteLine($"response message:\n{response.ToYamlString()}");
        }
        [TestMethod]
        public void TestEngineReady()
        {
            var requestMessage = new TestRequestMessage();

            var task = httpClient.GetAsync("/api/MockTest/EngineReady");
            task.Wait();
            HttpResponseMessage response = task.Result;
            response.EnsureSuccessStatusCode();
            Trace.WriteLine($"response message:\n{response.ToYamlString()}");
        }
        [TestMethod]
        public void TestMatchingFirstAction()
        {
            var requestMessage = new TestRequestMessage();

            var response = SendTestMessageAndReadAsync("/api/MockTest/TestMethod1", 1, requestMessage, "test1", false, "My text parameter3");
            response.Wait();
            Trace.WriteLine($"response message:\n{response.Result.ToYamlString()}");
        }
        [TestMethod]
        public void TestMatchingSecondAction()
        {
            var requestMessage = new TestRequestMessage();

            var response = SendTestMessageAndReadAsync("/api/MockTest/TestMethod1", 2, requestMessage, "test1", true, "My text parameter3");
            response.Wait();

            Trace.WriteLine($"response message:\n{response.Result.ToYamlString()}");
        }
        [TestMethod]
        public void TestMatchingThirdAction()
        {
            var requestMessage = new TestRequestMessage();

            var response = SendTestMessageAsync("/api/mocktest/testMethod1", 3, requestMessage, "test1", true, "My text parameter3");
            response.Wait();
            Assert.AreEqual(HttpStatusCode.InternalServerError, response.Result.StatusCode, "Should have returned an internal server error");
        }
        [TestMethod]
        public void TestMatchingForthAction()
        {
            var requestMessage = new TestRequestMessage();

            var response = SendTestMessageAndReadAsync("/api/mocktest/testMethod1", 4, requestMessage, "test1", true, "My text parameter3");
            response.Wait();

            Trace.WriteLine($"response message:\n{response.Result.ToYamlString()}");
        }
        [TestMethod]
        public void TestMatchingFifthAction()
        {
            var requestMessage = new TestRequestMessage();

            var response = SendTestMessageAndReadDynamicAsync($"/api/mocktest/testMethod2", 5, requestMessage, "test1", false, "My text parameter3");
            response.Wait();

            dynamic result = response.Result;
            Trace.WriteLine($"response message:\n{response.Result.ToYamlString()}");
            Assert.AreEqual("101", result.storeNumber);
        }
        [TestMethod]
        public void TestMatchingSixthAction()
        {
            var requestMessage = new TestRequestMessage();

            var response = SendTestMessageAndReadAsync("/api/mocktest/testMethod1", 6, requestMessage, "test1", false, "My text parameter3");
            response.Wait();

            Trace.WriteLine($"response message:\n{response.Result.ToYamlString()}");
        }
        [TestMethod]
        public void TestMatchingSevenAction()
        {
            var requestMessage = new TestRequestMessage();

            var response = SendTestMessageAndReadAsync("/api/mocktest/testMethod1", 7, requestMessage, "test1", false, "My text parameter3");
            response.Wait();

            Trace.WriteLine($"response message:\n{response.Result.ToYamlString()}");
        }
        [TestMethod]
        public void TestResponseSerialization()
        {
            var responseMessage = new TestResponseMessage()
            {
                ADateTime = DateTime.UtcNow,
                AGuid = Guid.NewGuid(),
                AnEnum = ResponseEnumValues.One,
                ANumber = 42,
                Message = "A message",
                Success = true
            };

            Trace.WriteLine($"response message:\n{responseMessage.ToYamlString()}");
        }
        [TestMethod]
        public void TestResponseXml()
        {
            var response = new TestResponseMessage()
            {
                ADateTime = DateTime.UtcNow,
                AnEnum = ResponseEnumValues.Two,
                ANumber = 42,
                Success = true,
                Message = "This is a test message"
            };
            Trace.WriteLine($"response message xml:\n{response.ToXmlString()}");
        }
        [TestMethod]
        public void TestDynamicResponse()
        {
            var requestMessage = new TestRequestMessage();

            var response = SendTestMessageAndReadDynamicAsync("/api/mocktest/testMethod2", 5, requestMessage, "test1", false, "My text parameter3");
            response.Wait();
            dynamic result = response.Result;
            Trace.WriteLine($"response message:\n{response.Result.ToYamlString()}");
            Assert.AreEqual("101", result.storeNumber);
        }
        [TestMethod]
        public void TestMessageHandlerResponse()
        {
            var requestMessage = new TestRequestMessage();

            var response = SendTestMessageAndReadDynamicAsync("/api/mocktest/testMethod3", 5, requestMessage, "test1", false, "My text parameter3");
            response.Wait();

            Trace.WriteLine($"response message:\n{response.Result.ToYamlString()}");

            Assert.Fail("The result is not serializing to yaml"); // TBD:  need to fix this
        }
        [TestMethod]
        public void TestMatching101Action()
        {
            var requestMessage = new TestRequestMessage();

            try
            {
                var response = SendTestMessageAndReadAsync("/api/mocktest/testMethod1", 101, requestMessage, "test1", true, "My text parameter3");
                response.Wait();

                Trace.WriteLine($"response message:\n{response.Result.ToYamlString()}");
            }
            catch( System.AggregateException e)
            {
                throw e.InnerException;
            }
        }
        [TestMethod]
        public void TestThrowingAnHttpResponseException()
        {

            var requestMessage = new TestRequestMessage();
            try
            {
                var response = SendTestMessageAsync("/api/mocktest/testMethod1", 1234, requestMessage, "test1", true, "My text parameter3");
                response.Wait();

                var responseMessage = response.Result;

                Trace.WriteLine($"response message:\n{responseMessage.ToYamlString()}");
                Assert.AreEqual(HttpStatusCode.NotFound, response.Result.StatusCode, "Should have returned a 404 \"Not found\"");
            }
            catch ( System.AggregateException e)
            {
                Trace.WriteLine(e);
            }
        }
    }
}
