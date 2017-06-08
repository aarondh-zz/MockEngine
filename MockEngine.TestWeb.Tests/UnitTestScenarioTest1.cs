using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MockEngine.TestWeb.Controllers;
using MockEngine.TestWeb.Models;
using System.Diagnostics;
using MockEngine.Utilities;
using System.Runtime.Serialization;
using System.IO;

namespace MockEngine.TestWeb.Tests
{
    [TestClass]
    public class UnitTestScenarioTest1
    {
        [TestMethod]
        public void TestMatchingFirstAction()
        {
            var mockTestControler = new MockTestController();
            var requestMessage = new TestRequestMessage();

            var response = mockTestControler.TestMethod1(1, requestMessage, "test1", false, "My text parameter3");
            response.Wait();
            Trace.WriteLine($"response message:\n{response.Result.ToYamlString()}");
        }
        [TestMethod]
        public void TestMatchingSecondAction()
        {
            var mockTestControler = new MockTestController();
            var requestMessage = new TestRequestMessage();

            var response = mockTestControler.TestMethod1(2, requestMessage, "test1", true, "My text parameter3");
            response.Wait();
            Trace.WriteLine($"response message:\n{response.Result.ToYamlString()}");
        }
        [TestMethod]
        public void TestMatchingThirdAction()
        {
            var mockTestControler = new MockTestController();
            var requestMessage = new TestRequestMessage();

            try
            {
                var response = mockTestControler.TestMethod1(3, requestMessage, "test1", true, "My text parameter3");
                response.Wait();
                Assert.Fail("Did not throw an exception");
            }
            catch (System.AggregateException e)
            {
                if ("MockEngine.MockEngineException" != e.InnerException.GetType().FullName)
                {
                    Trace.WriteLine(e);
                }
                Assert.AreEqual("MockEngine.MockEngineException", e.InnerException.GetType().FullName, "Should have returned an HttpResponseException");
            }
        }
        [TestMethod]
        public void TestMatchingForthAction()
        {
            var mockTestControler = new MockTestController();
            var requestMessage = new TestRequestMessage();

            var response = mockTestControler.TestMethod1(4, requestMessage, "test1", true, "My text parameter3");
            response.Wait();
            Trace.WriteLine($"response message:\n{response.Result.ToYamlString()}");
        }
        [TestMethod]
        public void TestMatchingFifthAction()
        {
            var mockTestControler = new MockTestController();
            var requestMessage = new TestRequestMessage();

            var response = mockTestControler.TestMethod2(5, requestMessage, "test1", true, "My text parameter3");
            response.Wait();
            Trace.WriteLine($"response message:\n{response.Result.ToYamlString()}");
        }
        [TestMethod]
        public void TestMatchingSixthAction()
        {
            var mockTestControler = new MockTestController();
            var requestMessage = new TestRequestMessage();

            var response = mockTestControler.TestMethod1(6, requestMessage, "test1", true, "My text parameter3");
            response.Wait();
            Trace.WriteLine($"response message:\n{response.Result.ToYamlString()}");
        }
        [TestMethod]
        public void TestMatchingSevenAction()
        {
            var mockTestControler = new MockTestController();
            var requestMessage = new TestRequestMessage();

            var response = mockTestControler.TestMethod2(7, requestMessage, "test1", true, "My text parameter3");
            response.Wait();
            Trace.WriteLine($"response message:\n{response.Result.ToYamlString()}");
        }
        [TestMethod]
        public void TestResponseSerialization()
        {
            var mockTestControler = new MockTestController();
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
            var serializer = new DataContractSerializer(typeof(TestResponseMessage));
            var stream = new MemoryStream();
            serializer.WriteObject(stream, response);
            stream.Seek(0, SeekOrigin.Begin);
            var reader = new StreamReader(stream);
            var result = reader.ReadToEnd();
            Trace.WriteLine($"response message xml:\n{result}");
        }
        [TestMethod]
        public void TestDynamicResponse()
        {
            var mockTestControler = new MockTestController();
            var requestMessage = new TestRequestMessage();
            var response = mockTestControler.TestMethod2(5, requestMessage, "test1", true, "My text parameter3");
            response.Wait();
            Trace.WriteLine($"response message:\n{response.Result.ToYamlString()}");
        }
        [TestMethod]
        public void TestMatching101Action()
        {
            var mockTestControler = new MockTestController();
            var requestMessage = new TestRequestMessage();
            try
            {
                var response = mockTestControler.TestMethod1(101, requestMessage, "test1", true, "My text parameter3");
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
            var mockTestControler = new MockTestController();
            var requestMessage = new TestRequestMessage();
            try
            {
                var response = mockTestControler.TestMethod1(1234, requestMessage, "test1", true, "My text parameter3");
                response.Wait();
                var responseMessage = response.Result;
                Assert.Fail("Did not throw an exception");
            }
            catch( System.AggregateException e)
            {
                if ("System.Web.Http.HttpResponseException" != e.InnerException.GetType().FullName)
                {
                    Trace.WriteLine(e);
                }
                Assert.AreEqual("System.Web.Http.HttpResponseException", e.InnerException.GetType().FullName, "Should have returned an HttpResponseException");
            }
        }
    }
}
