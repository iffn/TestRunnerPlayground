using System.Collections;
using System.Collections.Generic;
using iffnsStuff.iffnsPhysics.PredictUnityPhysics;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.IO;
using System.Xml;
using NUnit.Framework.Interfaces;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine.TestRunner;
using UnityEditor;
using static UnityEngine.Networking.UnityWebRequest;

public class PlayModeTest
{
    static void RunAutomatedTest()
    {
        Debug.Log("Starting test");

        TestRunnerApi testRunnerApi = ScriptableObject.CreateInstance<TestRunnerApi>();

        Filter filter = new()
        {
            testMode = TestMode.PlayMode
            //testNames = new[] { "PlayModeTest.VelocityPredictionTest"}
        };

        //Register callback
        //ToDo: Re-register callback after domain reload: https://forum.unity.com/threads/testrunnerapi-getting-callbacks-for-playmode-tests.837334/
        testRunnerApi.RegisterCallbacks(new ResultSerializer());

        //Run test
        testRunnerApi.Execute(new ExecutionSettings(filter));
        Debug.Log("Test completed");

        EditorApplication.Exit(0);
    }

    public class ResultSerializer : ICallbacks
    {
        public void RunStarted(ITestAdaptor testsToRun)
        {

        }

        public void RunFinished(ITestResultAdaptor result)
        {
            string path = Path.Combine(Application.persistentDataPath, "PlayTestResults.xml");

            using XmlWriter xmlWriter = XmlWriter.Create(path, new XmlWriterSettings { Indent = true });
            result.ToXml().WriteTo(xmlWriter);


            Debug.Log($"Test results written to\n{path}");
            //System.Console.WriteLine($"Test results written to\n{path}");

            Debug.Log("Run finished");

            xmlWriter.Flush();

            EditorApplication.Exit(result.FailCount > 0 ? 1 : 0);
        }

        public void TestStarted(ITestAdaptor test)
        {

        }

        public void TestFinished(ITestResultAdaptor result)
        {

        }
    }

    [UnityTest]
    public IEnumerator VelocityPredictionTest()
    {
        Debug.Log("Velocity prediction");

        //Goal: Test the velocity prediction funciton on multiple objects

        //Define test parameters
        List<VelocityPredictionTestInstance> tetstInstances = new List<VelocityPredictionTestInstance>()
        {
            new VelocityPredictionTestInstance(drag: 0.1f, mass: 1, initialVelocity: 20),   //Normal test
            new VelocityPredictionTestInstance(drag: 0.1f, mass: 100, initialVelocity: 20), //High mass: Should not outcome due to Unity physics
            new VelocityPredictionTestInstance(drag: 0.5f, mass: 1, initialVelocity: 20),   //Higher drag
            new VelocityPredictionTestInstance(drag: 0.1f, mass: 1, initialVelocity: 10),   //Lower initial velocity
            new VelocityPredictionTestInstance(drag: 0.1f, mass: 1, initialVelocity: 0),    //No initial velocity
            new VelocityPredictionTestInstance(drag: 0, mass: 1, initialVelocity: 10),      //No drag
        };

        //Start all tests
        foreach(var instance in tetstInstances)
        {
            instance.Start();
        }

        //Wait for test time
        float testTime = 3;

        yield return new WaitForSeconds(testTime);

        //Predict and check if correct
        foreach(VelocityPredictionTestInstance instance in tetstInstances)
        {
            float predictedVelocity = VelocityPredictor.PredictVelocityDueToDragOnly(initialVelocity: instance.InitialVelocity, drag: instance.Drag, time: testTime);

            Assert.AreEqual(predictedVelocity, instance.ActualVelocity,predictedVelocity * 0.01, $"For {nameof(testTime)} = {testTime}, {instance.IdentifyString}");
        }
    }

    class VelocityPredictionTestInstance
    {
        readonly GameObject testObject;
        readonly Rigidbody testRigidbody;

        public float Mass { get; private set; }
        public float Drag { get; private set; }
        public float InitialVelocity { get; private set; }

        public string IdentifyString
        {
            get
            {
                return $"{nameof(Mass)} = {Mass}, {nameof(Drag)} = {Drag}, {nameof(InitialVelocity)} = {InitialVelocity}";
            }
        }

        public float ActualVelocity
        {
            get
            {
                return testRigidbody.velocity.z;
            }
        }

        public VelocityPredictionTestInstance(float drag, float mass, float initialVelocity)
        {
            Drag = drag;
            Mass = mass;
            InitialVelocity = initialVelocity;

            testObject = new GameObject();

            testRigidbody = testObject.AddComponent(typeof(Rigidbody)) as Rigidbody;

            testRigidbody.mass = Mass;
            testRigidbody.drag = Drag;
            testRigidbody.useGravity = false;
        }

        public void Start()
        {
            testRigidbody.velocity = InitialVelocity * Vector3.forward;
        }
    }
}
