using NUnit.Framework;
using iffnsStuff.iffnsPhysics;
using UnityEngine;
using UnityEditor.TestTools.TestRunner.Api;
using System.Xml;
using System.IO;
using UnityEditor.SceneManagement;
using UnityEditor;

public class EditModeTest
{
    static void RunAutomatedTest()
    {
        Debug.Log("Starting editor test");

        EditorSceneManager.OpenScene("Assets/Scenes/TestScene.unity");

        TestRunnerApi testRunnerApi = ScriptableObject.CreateInstance<TestRunnerApi>();

        //Run all edit mode tests
        Filter filter = new()
        {
            testMode = TestMode.EditMode
        };

        //Register callback
        testRunnerApi.RegisterCallbacks(new ResultSerializer());

        testRunnerApi.Execute(new ExecutionSettings(filter));
    }

    public class ResultSerializer : ICallbacks
    {
        public void RunStarted(ITestAdaptor testsToRun)
        {

        }

        public void RunFinished(ITestResultAdaptor result)
        {
            string path = Path.Combine(Application.dataPath, "Tests/EditMode/Output/EditorTestResults.xml");

            using XmlWriter xmlWriter = XmlWriter.Create(path, new XmlWriterSettings { Indent = true });
            result.ToXml().WriteTo(xmlWriter);

            Debug.Log($"Test results written to\n{path}");

            Debug.Log("Run finished");

            xmlWriter.Flush(); //Flush since the XML writer is otherwise unable to finish in time
            EditorApplication.Exit(result.FailCount > 0 ? 1 : 0);
        }

        public void TestStarted(ITestAdaptor test)
        {

        }

        public void TestFinished(ITestResultAdaptor result)
        {
            
        }
    }

    [Test]
    public static void LinearColorSpace()
    {
        //Goal: Check if the color space is set to linear
        Assert.AreEqual(ColorSpace.Linear, PlayerSettings.colorSpace, "Color space not set to linear. Consider switching it");
    }

    [Test]
    public static void ValidateAtmpshere()
    {
        //Goal: Check the athmosphere class using known values
        //Values from: https://onlinelibrary.wiley.com/doi/pdf/10.1002/9781118534786.app1

        IffnsAtmosphere testAtmosphere = new IffnsAtmosphere(seaLevelTemperatureK: 288.15f, seaLevelAirPressurePa: 101325);

        //Check temperature
        const float temperatureTolleranceK = 0.5f;

        Assert.AreEqual(288.5f, testAtmosphere.TemperatureAtAltitude(0), temperatureTolleranceK);
        Assert.AreEqual(278.24f, testAtmosphere.TemperatureAtAltitude(1524), temperatureTolleranceK);
        Assert.AreEqual(268.34f, testAtmosphere.TemperatureAtAltitude(3048), temperatureTolleranceK);
        Assert.AreEqual(248.53f, testAtmosphere.TemperatureAtAltitude(6069f), temperatureTolleranceK);
        Assert.AreEqual(228.71f, testAtmosphere.TemperatureAtAltitude(9144f), temperatureTolleranceK);
        Assert.AreEqual(216.65f, testAtmosphere.TemperatureAtAltitude(12192f), temperatureTolleranceK);
        Assert.AreEqual(216.65f, testAtmosphere.TemperatureAtAltitude(15240f), temperatureTolleranceK);
        Assert.AreEqual(216.15f, testAtmosphere.TemperatureAtAltitude(18288f), temperatureTolleranceK);


        //Check pressure
        const float pressureTollerancePa = 20;

        Assert.AreEqual(101325f, testAtmosphere.AirPressureAtAltitude(0), pressureTollerancePa);
        Assert.AreEqual(84307f, testAtmosphere.AirPressureAtAltitude(1524), pressureTollerancePa);
        Assert.AreEqual(69682f, testAtmosphere.AirPressureAtAltitude(3048), pressureTollerancePa);
        Assert.AreEqual(46563f, testAtmosphere.AirPressureAtAltitude(6096f), pressureTollerancePa);
        Assert.AreEqual(30090f, testAtmosphere.AirPressureAtAltitude(9144f), pressureTollerancePa);
        Assert.AreEqual(18754f, testAtmosphere.AirPressureAtAltitude(12192f), pressureTollerancePa);
        Assert.AreEqual(11597f, testAtmosphere.AirPressureAtAltitude(15240f), pressureTollerancePa);
        Assert.AreEqual(7172f, testAtmosphere.AirPressureAtAltitude(18288f), pressureTollerancePa);
    }

    [Test]
    public void MeshesUseCorretIndexFormat()
    {
        //Gloal: Check all meshes in the scene to see if the index format is correctly assigned

        MeshFilter[] allMeshFilters = Object.FindObjectsOfType<MeshFilter>();

        foreach(MeshFilter meshFilter in allMeshFilters)
        {
            if (meshFilter.sharedMesh.vertexCount > 65535)
            {
                Assert.AreEqual(UnityEngine.Rendering.IndexFormat.UInt32, meshFilter.sharedMesh.indexFormat, $"The object {meshFilter.transform.name} has more than 65535 vertices while not using the 32bit index format");
            }
            else
            {
                Assert.AreEqual(UnityEngine.Rendering.IndexFormat.UInt16, meshFilter.sharedMesh.indexFormat, $"The object {meshFilter.transform.name} has less than 65535 vertices while not using the 16bit index format");
            }
        }
    }
}
