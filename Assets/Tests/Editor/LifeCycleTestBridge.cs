using System.IO;
using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

namespace UengSystem.Tests {
	// Explicit file request only; never automatically enters Play Mode or changes the user's scene.
	[InitializeOnLoad]
	public static class LifeCycleTestBridge {
		private const string Request = "Temp/LifeCycleWork/run-tests";
		private static TestRunnerApi Runner;
		static LifeCycleTestBridge() { EditorApplication.update += CheckRequest; }
		private static void CheckRequest() {
			if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode || !File.Exists(Request)) return;
			// Avoid running an old loaded test assembly before Unity has imported edited scripts.
			System.DateTime AssemblyTime = File.GetLastWriteTimeUtc("Library/ScriptAssemblies/Assembly-CSharp-Editor.dll");
			foreach (string Source in Directory.EnumerateFiles("Assets", "*.cs", SearchOption.AllDirectories)) {
				if (File.GetLastWriteTimeUtc(Source) <= AssemblyTime) continue;
				AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
				return;
			}
			File.Delete(Request);
			Runner = ScriptableObject.CreateInstance<TestRunnerApi>();
			Runner.RegisterCallbacks(new Results());
			Runner.Execute(new ExecutionSettings(new Filter { testMode = TestMode.EditMode, testNames = new[] { "UengSystem.Tests.LifeCycleTests" } }));
		}
		[MenuItem("Tools/Tests/Run UObject Life Cycle Tests")]
		private static void RequestTests() {
			Directory.CreateDirectory("Temp/LifeCycleWork");
			File.WriteAllText(Request, "run");
		}
		private sealed class Results : ICallbacks {
			public void RunStarted(ITestAdaptor Tests) { File.WriteAllText("Temp/LifeCycleWork/test-status.txt", "Running"); }
			public void RunFinished(ITestResultAdaptor Result) {
				TestRunnerApi.SaveResultToFile(Result, "Temp/LifeCycleWork/unity-tests.xml");
				File.WriteAllText("Temp/LifeCycleWork/test-status.txt", Result.TestStatus.ToString());
			}
			public void TestStarted(ITestAdaptor Test) { }
			public void TestFinished(ITestResultAdaptor Result) { }
		}
	}
}
