# TestRunnerPlayground
 
Goal: The purpose of this repository is for me to test and demonstrate the various functions of the Unity TestRunner.
https://docs.unity3d.com/Packages/com.unity.test-framework@1.1/manual/index.html

This project was created using Unity 2022.1.10f1 on Windows 10 (Download: https://unity3d.com/get-unity/download/archive )

The scrips being tested can be found in: ```Assets\ScriptsToBeTested```

## Edit mode tests:
Scripts located in: ```Assets\Tests\EditMode```
The edit mode tests run in the Unity Editor.
- The LinearColorSpace function checks if the Color space of the project is set to linear.
- The MeshUsesCorrectIndexForamt function checks if all meshes in the current scene use the correct index format.
  - The TestScene deliberately contains a mesh where this not correctly set.
- The ValidateAtmosphere test compares the IffnsAtmosphere class with known values

These tests can be run using the TestRunner in Edit Mode:

![image](https://user-images.githubusercontent.com/18383974/190451418-537d1d3c-34fc-41eb-868b-9c9bd208caf4.png)

When the Unity project is closed, these tests can also using the command line.
(Replace ```TestRunnerPlaygroundLocation``` with the correct path)
```"C:\Program Files\Unity\Hub\Editor\2022.1.10f1\Editor\Unity.exe" -projectPath "TestRunnerPlaygroundLocation\TestRunnerPlayground" -executeMethod EditModeTest.RunAutomatedTest -batchmode```

Note: The ```-quit``` command does not need to be added since the editor exits itself (Otherwise, the xml will output will not be generated at the moment)

The output is saved as an .xml file and added to ```Assets\Tests\EditMode\Output```

## Play mode test:
Scripts located in: ```Assets\Tests/PlayMode```
The play mode test runs in the Play mode of Unity the Unity Editor

The VelocityPredictionTest checks if the VelocityPrediction class is able to predict the velocity of a Rigidbody after a certain amount of time.

For this, 6 GameObjects with Rigidbodies are generated and fired off with different values. After a certain amount of time, the velocity of the Rigidbody is compared to the predicted value.

These tests can be run using the TestRunner in Play Mode:
![image](https://user-images.githubusercontent.com/18383974/190453277-b5514045-6540-4224-88d8-d4e8169f2b00.png)

The current version of the Play mode test does not return the xml file. The problem seems to be that the callback neededs to be re-registered after the domain reload. More about this here: https://forum.unity.com/threads/testrunnerapi-getting-callbacks-for-playmode-tests.837334/