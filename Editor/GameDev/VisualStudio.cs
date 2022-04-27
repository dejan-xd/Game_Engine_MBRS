﻿using Editor.GameProject.ViewModel;
using Editor.Utilities;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace Editor.GameDev
{
    internal static class VisualStudio
    {
        public static bool BuildSucceeded { get; private set; } = true;
        public static bool BuildDone { get; private set; } = true;

        private static EnvDTE80.DTE2 _vsInstance = null;
        private static readonly string _progID = "VisualStudio.DTE.16.0";   //  supporting only Visual Studio 2019

        [DllImport("Ole32.dll")]
        private static extern int CreateBindCtx(uint reserved, out IBindCtx ppbc);

        [DllImport("Ole32.dll")]
        private static extern int GetRunningObjectTable(uint reserved, out IRunningObjectTable pprot);

        public static void OpenVisualStudio(string solutionPath)
        {
            IRunningObjectTable rot = null;
            IEnumMoniker monikerTable = null;
            IBindCtx bindCtx = null;
            try
            {
                if (_vsInstance == null)
                {
                    // find and open visual studio
                    int hResult = GetRunningObjectTable(0, out rot);
                    if (hResult < 0 || rot == null)
                    {
                        Logger.Log(MessageType.Error, $"GetRunningObjectTable() returned HRESULT: {hResult:X8}");
                        throw new COMException($"GetRunningObjectTable() returned HRESULT: {hResult:X8}");
                    }

                    rot.EnumRunning(out monikerTable);
                    monikerTable.Reset();

                    hResult = CreateBindCtx(0, out bindCtx);
                    if (hResult < 0 || bindCtx == null)
                    {
                        Logger.Log(MessageType.Error, $"CreateBindCtx() returned HRESULT: {hResult:X8}");
                        throw new COMException($"CreateBindCtx() returned HRESULT: {hResult:X8}");
                    }

                    IMoniker[] currentMoniker = new IMoniker[1];
                    while (monikerTable.Next(1, currentMoniker, IntPtr.Zero) == 0)
                    {
                        string name = string.Empty;
                        currentMoniker[0]?.GetDisplayName(bindCtx, null, out name);
                        if (name.Contains(_progID))
                        {
                            hResult = rot.GetObject(currentMoniker[0], out object obj);
                            if (hResult < 0 || obj == null)
                            {
                                Logger.Log(MessageType.Error, $"Running object table's GetObject() returned HRESULT: {hResult:X8}");
                                throw new COMException($"Running object table's GetObject() returned HRESULT: {hResult:X8}");
                            }

                            EnvDTE80.DTE2 dte = obj as EnvDTE80.DTE2;
                            string solutionName = dte.Solution.FullName;
                            if (solutionName == solutionPath)
                            {
                                _vsInstance = dte;
                                break;
                            }
                        }
                    }

                    if (_vsInstance == null)
                    {
                        Type visualStudioType = Type.GetTypeFromProgID(_progID, true);
                        _vsInstance = Activator.CreateInstance(visualStudioType) as EnvDTE80.DTE2;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Logger.Log(MessageType.Error, "Failed to open Visual Studio");
                throw;
            }
            finally
            {
                if (monikerTable != null) Marshal.ReleaseComObject(monikerTable);
                if (rot != null) Marshal.ReleaseComObject(rot);
                if (bindCtx != null) Marshal.ReleaseComObject(bindCtx);
            }
        }

        public static void CloseVisualStudio()
        {
            if (_vsInstance?.Solution.IsOpen == true)
            {
                _vsInstance.ExecuteCommand("File.SaveAll");
                _vsInstance.Solution.Close(true);
            }
            _vsInstance?.Quit();
        }

        public static bool AddFilesToSolution(string solution, string projectName, string[] files, int tries)
        {
            Debug.Assert(files?.Length > 0);
            OpenVisualStudio(solution);
            try
            {
                if (_vsInstance != null)
                {
                    if (!_vsInstance.Solution.IsOpen) _vsInstance.Solution.Open(solution);
                    else _vsInstance.ExecuteCommand("File.SaveAll");

                    foreach (EnvDTE.Project project in _vsInstance.Solution.Projects)
                    {
                        if (project.UniqueName.Contains(projectName))
                        {
                            foreach (string file in files)
                            {
                                project.ProjectItems.AddFromFile(file);
                            }
                        }
                    }

                    string cpp = files.FirstOrDefault(x => Path.GetExtension(x) == ".cpp");
                    if (!string.IsNullOrEmpty(cpp))
                    {
                        _vsInstance.ItemOperations.OpenFile(cpp, EnvDTE.Constants.vsViewKindTextView).Visible = true;
                    }
                    _vsInstance.MainWindow.Activate();
                    _vsInstance.MainWindow.Visible = true;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Debug.WriteLine("Failed to add files to Visual Studio project");
                Logger.Log(MessageType.Error, $"Failed to add files to {projectName} project. ({tries}/3)");
                return false;
                throw;
            }
            return true;
        }

        private static void OnBuildSolutionBegin(string project, string projectConfig, string platform, string solutionConfig)
        {
            Logger.Log(MessageType.Info, $"Building {project}, {projectConfig}, {platform}, {solutionConfig}");
        }

        private static void OnBuildSolutionDone(string project, string projectConfig, string platform, string solutionConfig, bool success)
        {
            if (BuildDone) return;
            if (success) Logger.Log(MessageType.Info, $"Building {projectConfig} configuration succeeded");
            else Logger.Log(MessageType.Error, $"Building {projectConfig} configuration failed");

            BuildDone = true;
            BuildSucceeded = success;
        }

        public static bool IsDebugging()
        {
            bool result = false;
            for (int i = 0; i < 3; ++i)
            {
                try
                {
                    result = _vsInstance != null && (_vsInstance.Debugger.CurrentProgram != null || _vsInstance.Debugger.CurrentMode == EnvDTE.dbgDebugMode.dbgRunMode);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    if (!result) System.Threading.Thread.Sleep(1000);
                }
            }
            return result;
        }

        public static void BuildSolution(Project project, string configName, bool showWindow = true)
        {
            if (IsDebugging())
            {
                Logger.Log(MessageType.Error, "Visual Studio is currently running a process.");
                return;
            }

            OpenVisualStudio(project.Solution);
            BuildDone = BuildSucceeded = false;

            for (int i = 0; i < 3; ++i)
            {
                try
                {
                    if (BuildDone) break;
                    if (!_vsInstance.Solution.IsOpen) _vsInstance.Solution.Open(project.Solution);
                    _vsInstance.MainWindow.Visible = showWindow;

                    _vsInstance.Events.BuildEvents.OnBuildProjConfigBegin += OnBuildSolutionBegin;
                    _vsInstance.Events.BuildEvents.OnBuildProjConfigDone += OnBuildSolutionDone;

                    try
                    {
                        foreach (string pdbFile in Directory.GetFiles(Path.Combine($"{project.Path}", $@"x64{configName}"), "*.pdb"))
                        {
                            File.Delete(pdbFile);
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                    }

                    _vsInstance.Solution.SolutionBuild.SolutionConfigurations.Item(configName).Activate();
                    _vsInstance.ExecuteCommand("Build.BuildSolution");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    Debug.WriteLine($"Attempt {i}: failed to build {project.Name}");
                    System.Threading.Thread.Sleep(1000);
                }
            }
        }
    }
}