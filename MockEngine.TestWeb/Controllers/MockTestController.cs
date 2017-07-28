using MockEngine;
using MockEngine.Configuration;
using MockEngine.Interfaces;
using MockEngine.Resolvers;
using MockEngine.TestWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace MockEngine.TestWeb.Controllers
{
    public class MockTestController : ApiController
    {
        private static IMockEngine _mockEngine;
        static MockTestController() {
            var mockEngineFactory = new MockEngineFactory();
            mockEngineFactory.Initialize(MockEngineConfigurationSettings.Settings);
            _mockEngine = mockEngineFactory.CreateMockEngine("Test1");
        }
        [HttpGet]
        public HttpResponseMessage Ready()
        {
            var response = Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StringContent("The MockTestController is ready");
            return response;
        }
        [HttpGet]
        public HttpResponseMessage EngineReady()
        {
            var response = _mockEngine.Invoke("engineReady");
            return new HttpResponseMessage(response.StatusCode) { ReasonPhrase = response.ReasonPhrase };
        }
        [HttpPost]
        public async Task<TestResponseMessage> TestMethod1(long numberParameter1, [FromBody] TestRequestMessage testRequest, string scenario, bool booleanParameter2 = false, string textParameter3 = null)
        {
            var response = _mockEngine.Invoke(
                scenario,
                new
                {
                    numberParameter1 = numberParameter1,
                    testRequest = testRequest,
                    scenario = scenario,
                    booleanParameter2 = booleanParameter2,
                    textParameter3 = textParameter3
                });
            if (response.StatusCode == HttpStatusCode.OK)
            {
                return response.Content as TestResponseMessage;
            }
            else
            {
                var httpResponseMessage = new HttpResponseMessage(response.StatusCode) { ReasonPhrase = response.ReasonPhrase };
                throw new HttpResponseException(httpResponseMessage);
            }
        }
        [HttpPost]
        public async Task<object> TestMethod2(long numberParameter1, [FromBody] TestRequestMessage testRequest, string scenario, bool booleanParameter2 = false, string textParameter3 = null)
        {
                var response = _mockEngine.Invoke(scenario,
                    new
                    {
                        numberParameter1 = numberParameter1,
                        testRequest = testRequest,
                        scenario = scenario,
                        booleanParameter2 = booleanParameter2,
                        textParameter3 = textParameter3
                    });
                if ( response.Content != null)
                {
                    return response.Content;
                }
                else
                {
                    var httpResponseMessage = new HttpResponseMessage(response.StatusCode) { ReasonPhrase = response.ReasonPhrase };
                    throw new HttpResponseException(httpResponseMessage);
                }
        }
        [HttpGet]
        public HttpResponseMessage TestMethod3()
        {
            var response = Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StringContent("TestMethod3");
            return response;
        }
    }
}
