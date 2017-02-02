using Moq;
using ScriptCs.Contracts;
using Should;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using Xunit;

namespace ScriptCs.ComponentModel.Composition.Test
{
    public class ScriptCsCatalogFacts
    {
        public class InitializeScriptsFiles
        {
            [Fact]
            public void ShouldWorkWithSimpleScript()
            {
                // arrange
                var scriptName = @"c:\workingdirectory\SimpleScript.csx";
                var scriptCsCatalog = new ScriptCsCatalog(new[] { scriptName },
                    GetOptions(fileSystem: GetMockFileSystem(scriptName, Scripts.SimpleScript).Object));

                // act
                var mefHost = GetComposedMefHost(scriptCsCatalog);

                // assert
                mefHost.Plugins.ShouldNotBeNull();
                mefHost.Plugins.Count.ShouldEqual(1);
                mefHost.Plugins[0].DoSomething().ShouldEqual("Simple");
            }

            [Fact]
            public void ShouldWorkWithSimpleScriptAndReference()
            {
                // arrange
                var scriptName = @"c:\workingdirectory\SimpleScript.csx";
                var scriptCsCatalog = new ScriptCsCatalog(new[] { scriptName },
                    GetOptions(
                        fileSystem: GetMockFileSystem(scriptName, Scripts.SimpleScriptWithoutReference).Object,
                        references: new[] { typeof(IDoSomething) }));

                // act
                var mefHost = GetComposedMefHost(scriptCsCatalog);

                // assert
                mefHost.Plugins.ShouldNotBeNull();
                mefHost.Plugins.Count.ShouldEqual(1);
                mefHost.Plugins[0].DoSomething().ShouldEqual("Simple");
            }

            [Fact]
            public void ShouldWorkWithMultipleScripts()
            {
                // arrange
                var scriptName = @"c:\workingdirectory\SimpleScript.csx";
                var scriptName2 = @"c:\workingdirectory\DoubleScript.csx";
                var scriptCsCatalog = new ScriptCsCatalog(new[] { scriptName, scriptName2 },
                    GetOptions(fileSystem: GetMockFileSystem(new[] { scriptName, scriptName2 }, new[] { Scripts.SimpleScript, Scripts.DoubleScript }).Object));

                // act
                var mefHost = GetComposedMefHost(scriptCsCatalog);

                // assert
                mefHost.Plugins.ShouldNotBeNull();
                mefHost.Plugins.Count.ShouldEqual(2);
                mefHost.Plugins[0].DoSomething().ShouldEqual("Simple");
                mefHost.Plugins[1].DoSomething().ShouldEqual("Double");
            }

            [Fact]
            public void ShouldThrowExceptionIfNullPassedForScriptsFiles()
            {
                // act
                var exception = Assert.Throws<ArgumentNullException>(() => new ScriptCsCatalog((IEnumerable<string>)null));

                // assert
                exception.ParamName.ShouldEqual("scriptFiles");
            }

            [Fact]
            public void ShouldThrowExceptionIfNoScriptsFilesArePassed()
            {
                // act
                var exception = Assert.Throws<ArgumentNullException>(() => new ScriptCsCatalog(new string[0]));

                // assert
                exception.ParamName.ShouldEqual("scriptFiles");
            }

            [Fact]
            public void ShouldThrowExceptionIfNullPassedForOptions()
            {
                // arrange
                var scriptName = @"c:\workingdirectory\SimpleScript.csx";

                // act
                var exception = Assert.Throws<ArgumentNullException>(() => new ScriptCsCatalog(new[] { scriptName }, null));

                // assert
                exception.ParamName.ShouldEqual("options");
            }

            [Fact]
            public void ShouldThrowExceptionIfScriptFileDoesntExists()
            {
                // arrange
                var scriptName = @"c:\workingdirectory\SimpleScript.csx";

                // act
                var exception = Assert.Throws<FileNotFoundException>(() => new ScriptCsCatalog(new[] { scriptName }, GetOptions(fileSystem: GetMockFileSystem(new string[0], new string[0]).Object)));

                // assert
                exception.FileName.ShouldEqual(@"c:\workingdirectory\SimpleScript.csx");
                exception.Message.ShouldEqual(@"Script: 'c:\workingdirectory\SimpleScript.csx' does not exist");
            }

            [Fact]
            public void ShouldThrowExceptionIfScriptDoesntCompile()
            {
                // arrange
                var scriptName = @"c:\workingdirectory\Script.csx";

                // act
                try
                {
                    new ScriptCsCatalog(new[] { scriptName }, GetOptions(fileSystem: GetMockFileSystem(scriptName, Scripts.CompileExceptionScript).Object));
                }
                catch (Exception exception)
                {
                    // assert
                    exception.Message.ShouldEqual(@"c:\workingdirectory\Script.csx(6,24): error CS1002: ; expected");
                }
            }

            [Fact]
            public void ShouldThrowExceptionIfScriptThrowExceptionDuringExecution()
            {
                // arrange
                var scriptName = @"c:\workingdirectory\Script.csx";

                // act
                var exception = Assert.Throws<Exception>(() => new ScriptCsCatalog(new[] { scriptName }, GetOptions(fileSystem: GetMockFileSystem(scriptName, Scripts.ExecutionExceptionScript).Object)));

                // assert
                exception.Message.ShouldEqual(@"Exception from script execution");
            }
        }

        public class InitializeFolder
        {
            [Fact]
            public void ShouldWorkWithRelativeFolder()
            {
                // arrange
                var scriptName = @"c:\workingdirectory\_plugins\SimpleScript.csx";
                var scriptName2 = @"c:\workingdirectory\_plugins\DoubleScript.csx";
                var scriptCsCatalog = new ScriptCsCatalog("_plugins",
                    GetOptions(fileSystem: GetMockFileSystem(new string[] { scriptName, scriptName2 }, new[] { Scripts.SimpleScript, Scripts.DoubleScript }).Object));

                // act
                var mefHost = GetComposedMefHost(scriptCsCatalog);

                // assert
                mefHost.Plugins.ShouldNotBeNull();
                mefHost.Plugins.Count.ShouldEqual(2);
                mefHost.Plugins[0].DoSomething().ShouldEqual("Simple");
                mefHost.Plugins[1].DoSomething().ShouldEqual("Double");
            }

            [Fact]
            public void ShouldWorkWithAbsoluteFolder()
            {
                // arrange
                var scriptName = @"c:\workingdirectory\_plugins\SimpleScript.csx";
                var scriptName2 = @"c:\workingdirectory\_plugins\DoubleScript.csx";
                var scriptCsCatalog = new ScriptCsCatalog(@"c:\workingdirectory\_plugins",
                    GetOptions(fileSystem: GetMockFileSystem(new string[] { scriptName, scriptName2 }, new[] { Scripts.SimpleScript, Scripts.DoubleScript }).Object));

                // act
                var mefHost = GetComposedMefHost(scriptCsCatalog);

                // assert
                mefHost.Plugins.ShouldNotBeNull();
                mefHost.Plugins.Count.ShouldEqual(2);
                mefHost.Plugins[0].DoSomething().ShouldEqual("Simple");
                mefHost.Plugins[1].DoSomething().ShouldEqual("Double");
            }

            [Fact]
            public void ShouldWorkWithReferences()
            {
                // arrange
                var scriptName = @"c:\workingdirectory\_plugins\SimpleScript.csx";
                var scriptCsCatalog = new ScriptCsCatalog("_plugins",
                    GetOptions(fileSystem: GetMockFileSystem(scriptName, Scripts.SimpleScriptWithoutReference).Object,
                     references: new[] { typeof(IDoSomething) }));

                // act
                var mefHost = GetComposedMefHost(scriptCsCatalog);

                // assert
                mefHost.Plugins.ShouldNotBeNull();
                mefHost.Plugins.Count.ShouldEqual(1);
                mefHost.Plugins[0].DoSomething().ShouldEqual("Simple");
            }

            [Fact]
            public void ShouldWorkWithSearchPattern()
            {
                // arrange
                var scriptName = @"c:\workingdirectory\_plugins\SimpleScript.script";
                var scriptName2 = @"c:\workingdirectory\_plugins\DoubleScript.script";
                var scriptCsCatalog = new ScriptCsCatalog("_plugins",
                    "*.script",
                    GetOptions(fileSystem: GetMockFileSystem(new string[] { scriptName, scriptName2 }, new[] { Scripts.SimpleScript, Scripts.DoubleScript }).Object));

                // act
                var mefHost = GetComposedMefHost(scriptCsCatalog);

                // assert
                mefHost.Plugins.ShouldNotBeNull();
                mefHost.Plugins.Count.ShouldEqual(2);
                mefHost.Plugins[0].DoSomething().ShouldEqual("Simple");
                mefHost.Plugins[1].DoSomething().ShouldEqual("Double");
            }

            [Fact]
            public void ShouldThrowExceptionIfNullPassedForFileSystem()
            {
                // act
                var exception = Assert.Throws<ArgumentNullException>(() => new ScriptCsCatalog("_plugins", "*.csx", null));

                // assert
                exception.ParamName.ShouldEqual("options");
            }

            [Fact]
            public void ShouldThrowExceptionIfNullPassedForPath()
            {
                // act
                var exception = Assert.Throws<ArgumentNullException>(() => new ScriptCsCatalog((string)null));

                // assert
                exception.ParamName.ShouldEqual("path");
            }

            [Fact]
            public void ShouldThrowExceptionIfEmptyStringPassedForPath()
            {
                // act
                var exception = Assert.Throws<ArgumentNullException>(() => new ScriptCsCatalog(string.Empty));

                // assert
                exception.ParamName.ShouldEqual("path");
            }

            [Fact]
            public void ShouldThrowExceptionIfNullPassedForSearchPattern()
            {
                // act
                var exception = Assert.Throws<ArgumentNullException>(() => new ScriptCsCatalog("_plugins", (string)null));

                // assert
                exception.ParamName.ShouldEqual("searchPattern");
            }

            [Fact]
            public void ShouldThrowExceptionIfEmptyStringPassedForSearchPattern()
            {
                // act
                var exception = Assert.Throws<ArgumentNullException>(() => new ScriptCsCatalog("_plugins", string.Empty));

                // assert
                exception.ParamName.ShouldEqual("searchPattern");
            }

            [Fact]
            public void ShouldThrowExceptionIfFolderDoesntExists()
            {
                // act
                var exception = Assert.Throws<DirectoryNotFoundException>(() => new ScriptCsCatalog("fakeFolder", GetOptions(fileSystem: GetMockFileSystem(new string[0], new string[0]).Object)));

                // assert
                exception.Message.ShouldEqual(@"Scripts folder: 'c:\workingdirectory\fakeFolder' does not exist");
            }
        }

        public class ToStringMethod
        {
            [Fact]
            public void ShouldReturnNameAndPath_ScriptsFiles()
            {
                // arrange
                var scriptName = @"c:\workingdirectory\SimpleScript.csx";

                // act
                var scriptCsCatalog = new ScriptCsCatalog(new[] { scriptName },
                    GetOptions(fileSystem: GetMockFileSystem(scriptName, Scripts.SimpleScript).Object));

                // assert
                scriptCsCatalog.ToString().ShouldEqual("ScriptCsCatalog (Path=\"\")");
            }

            [Fact]
            public void ShouldReturnNameAndPath_ScriptsFolder()
            {
                // arrange
                var scriptName = @"c:\workingdirectory\_plugins\SimpleScript.csx";

                // act
                var scriptCsCatalog = new ScriptCsCatalog("_plugins",
                    GetOptions(fileSystem: GetMockFileSystem(scriptName, Scripts.SimpleScript).Object));

                // assert
                scriptCsCatalog.ToString().ShouldEqual("ScriptCsCatalog (Path=\"_plugins\")");
            }
        }

        public class DisplayNameProperty
        {
            [Fact]
            public void ShouldReturnNameAndPath_ScriptsFiles()
            {
                // arrange
                var scriptName = @"c:\workingdirectory\SimpleScript.csx";

                // act
                var scriptCsCatalog = new ScriptCsCatalog(new[] { scriptName },
                    GetOptions(fileSystem: GetMockFileSystem(scriptName, Scripts.SimpleScript).Object));

                // assert
                scriptCsCatalog.DisplayName.ShouldEqual("ScriptCsCatalog (Path=\"\")");
            }

            [Fact]
            public void ShouldReturnNameAndPath_ScriptsFolder()
            {
                // arrange
                var scriptName = @"c:\workingdirectory\_plugins\SimpleScript.csx";

                // act
                var scriptCsCatalog = new ScriptCsCatalog("_plugins",
                    GetOptions(fileSystem: GetMockFileSystem(scriptName, Scripts.SimpleScript).Object));

                // assert
                scriptCsCatalog.DisplayName.ShouldEqual("ScriptCsCatalog (Path=\"_plugins\")");
            }
        }

        public class OriginProperty
        {
            [Fact]
            public void ShouldReturnNull()
            {
                // arrange
                var scriptName = @"c:\workingdirectory\SimpleScript.csx";

                // act
                var scriptCsCatalog = new ScriptCsCatalog(new[] { scriptName },
                    GetOptions(fileSystem: GetMockFileSystem(scriptName, Scripts.SimpleScript).Object));

                // assert
                scriptCsCatalog.Origin.ShouldBeNull();
            }
        }

        public class FullPathProperty
        {
            [Fact]
            public void ShouldReturnNullIfScriptsFilesAreProvided()
            {
                // arrange
                var scriptName = @"c:\workingdirectory\SimpleScript.csx";

                // act
                var scriptCsCatalog = new ScriptCsCatalog(new[] { scriptName },
                    GetOptions(fileSystem: GetMockFileSystem(scriptName, Scripts.SimpleScript).Object));

                // assert
                scriptCsCatalog.FullPath.ShouldBeNull();
            }

            [Fact]
            public void ShouldReturnFullPathIfFolderIsProvided()
            {
                // arrange
                var scriptName = @"c:\workingdirectory\_plugins\SimpleScript.csx";

                // act
                var scriptCsCatalog = new ScriptCsCatalog("_plugins",
                    GetOptions(fileSystem: GetMockFileSystem(scriptName, Scripts.SimpleScript).Object));

                // assert
                scriptCsCatalog.FullPath.ShouldEqual(@"c:\workingdirectory\_plugins");
            }
        }

        public class PathProperty
        {
            [Fact]
            public void ShouldReturnNullIfScriptsFilesAreProvided()
            {
                // arrange
                var scriptName = @"c:\workingdirectory\SimpleScript.csx";

                // act
                var scriptCsCatalog = new ScriptCsCatalog(new[] { scriptName },
                    GetOptions(fileSystem: GetMockFileSystem(scriptName, Scripts.SimpleScript).Object));

                // assert
                scriptCsCatalog.Path.ShouldBeNull();
            }

            [Fact]
            public void ShouldReturnPathIfFolderIsProvided()
            {
                // arrange
                var scriptName = @"c:\workingdirectory\_plugins\SimpleScript.csx";

                // act
                var scriptCsCatalog = new ScriptCsCatalog("_plugins",
                    GetOptions(fileSystem: GetMockFileSystem(scriptName, Scripts.SimpleScript).Object));

                // assert
                scriptCsCatalog.Path.ShouldEqual("_plugins");
            }
        }

        public class SearchPatternProperty
        {
            [Fact]
            public void ShouldReturnNullIfScriptsFilesAreProvided()
            {
                // arrange
                var scriptName = @"c:\workingdirectory\SimpleScript.csx";

                // act
                var scriptCsCatalog = new ScriptCsCatalog(new[] { scriptName },
                    GetOptions(fileSystem: GetMockFileSystem(scriptName, Scripts.SimpleScript).Object));

                // assert
                scriptCsCatalog.SearchPattern.ShouldBeNull();
            }

            [Fact]
            public void ShouldReturnDefaultSearchPatternIfFolderIsProvided()
            {
                // arrange
                var scriptName = @"c:\workingdirectory\_plugins\SimpleScript.csx";

                // act
                var scriptCsCatalog = new ScriptCsCatalog("_plugins",
                    GetOptions(fileSystem: GetMockFileSystem(scriptName, Scripts.SimpleScript).Object));

                // assert
                scriptCsCatalog.SearchPattern.ShouldEqual("*.csx");
            }

            [Fact]
            public void ShouldReturnSearchPatternIfFolderAndSearchPatternAreProvided()
            {
                // arrange
                var scriptName = @"c:\workingdirectory\_plugins\SimpleScript.csx";

                // act
                var scriptCsCatalog = new ScriptCsCatalog("_plugins", "*.script",
                    GetOptions(fileSystem: GetMockFileSystem(scriptName, Scripts.SimpleScript).Object));

                // assert
                scriptCsCatalog.SearchPattern.ShouldEqual("*.script");
            }
        }

        public class LoadedFilesProperty
        {
            [Fact]
            public void ShouldReturnAllScriptsFilesIfScriptsFilesAreProvided()
            {
                // arrange
                var scriptName = @"c:\workingdirectory\SimpleScript.csx";
                var scriptName2 = @"c:\workingdirectory\DoubleScript.csx";

                // act
                var scriptCsCatalog = new ScriptCsCatalog(new[] { scriptName, scriptName2 },
                    GetOptions(fileSystem: GetMockFileSystem(new string[] { scriptName, scriptName2 }, new[] { Scripts.SimpleScript, Scripts.DoubleScript }).Object));

                // assert
                scriptCsCatalog.LoadedFiles.Count.ShouldEqual(2);
                scriptCsCatalog.LoadedFiles[0].ShouldEqual(@"c:\workingdirectory\SimpleScript.csx");
                scriptCsCatalog.LoadedFiles[1].ShouldEqual(@"c:\workingdirectory\DoubleScript.csx");
            }

            [Fact]
            public void ShouldReturnAllScriptsFilesIfFolderIsProvided()
            {
                // arrange
                var scriptName = @"c:\workingdirectory\_plugins\SimpleScript.csx";
                var scriptName2 = @"c:\workingdirectory\_plugins\DoubleScript.csx";

                // act
                var scriptCsCatalog = new ScriptCsCatalog("_plugins",
                    GetOptions(fileSystem: GetMockFileSystem(new string[] { scriptName, scriptName2 }, new[] { Scripts.SimpleScript, Scripts.DoubleScript }).Object));

                // assert
                scriptCsCatalog.LoadedFiles.Count.ShouldEqual(2);
                scriptCsCatalog.LoadedFiles[0].ShouldEqual(@"c:\workingdirectory\_plugins\SimpleScript.csx");
                scriptCsCatalog.LoadedFiles[1].ShouldEqual(@"c:\workingdirectory\_plugins\DoubleScript.csx");
            }
        }

        public class GetEnumeratorMethod
        {
            [Fact]
            public void ShouldEnumerateAllPartsIfScriptsFilesAreProvided()
            {
                // arrange
                var scriptName = @"c:\workingdirectory\SimpleScript.csx";
                var scriptName2 = @"c:\workingdirectory\DoubleScript.csx";

                // act
                var scriptCsCatalog = new ScriptCsCatalog(new[] { scriptName, scriptName2 },
                    GetOptions(fileSystem: GetMockFileSystem(new string[] { scriptName, scriptName2 }, new[] { Scripts.SimpleScript, Scripts.DoubleScript }).Object));

                // assert
                var enumerator = scriptCsCatalog.GetEnumerator();
                enumerator.ShouldNotBeNull();
                enumerator.Current.ShouldBeNull();
                enumerator.MoveNext().ShouldBeTrue();
                enumerator.Current.ShouldNotBeNull();
                enumerator.Current.ToString().ShouldEqual("Submission#0+SimpleSomething");
                enumerator.MoveNext().ShouldBeTrue();
                enumerator.Current.ShouldNotBeNull();
                enumerator.Current.ToString().ShouldEqual("Submission#0+DoubleSomething");
                enumerator.MoveNext().ShouldBeFalse();
                enumerator.Current.ShouldBeNull();
            }

            [Fact]
            public void ShouldEnumerateAllPartsFilesIfFolderIsProvided()
            {
                // arrange
                var scriptName = @"c:\workingdirectory\_plugins\SimpleScript.csx";
                var scriptName2 = @"c:\workingdirectory\_plugins\DoubleScript.csx";

                // act
                var scriptCsCatalog = new ScriptCsCatalog("_plugins",
                    GetOptions(fileSystem: GetMockFileSystem(new string[] { scriptName, scriptName2 }, new[] { Scripts.SimpleScript, Scripts.DoubleScript }).Object));

                // assert
                var enumerator = scriptCsCatalog.GetEnumerator();
                enumerator.ShouldNotBeNull();
                enumerator.Current.ShouldBeNull();
                enumerator.MoveNext().ShouldBeTrue();
                enumerator.Current.ShouldNotBeNull();
                enumerator.Current.ToString().ShouldEqual("Submission#0+SimpleSomething");
                enumerator.MoveNext().ShouldBeTrue();
                enumerator.Current.ShouldNotBeNull();
                enumerator.Current.ToString().ShouldEqual("Submission#0+DoubleSomething");
                enumerator.MoveNext().ShouldBeFalse();
                enumerator.Current.ShouldBeNull();
            }

            [Fact]
            public void ShouldNotThrowExceptionIfFolderProvidedIsEmpty()
            {
                // act
                var scriptCsCatalog = new ScriptCsCatalog("_plugins",
                    GetOptions(fileSystem: GetMockFileSystem(new string[0], new string[0]).Object));

                // assert
                var enumerator = scriptCsCatalog.GetEnumerator();
                enumerator.ShouldNotBeNull();
                enumerator.Current.ShouldBeNull();
                enumerator.MoveNext().ShouldBeFalse();
            }
        }

        public class RefresheMethod
        {
            [Fact]
            public void ShouldThrowExceptionIfAlreadyDisposed()
            {
                // arrange
                var scriptName = @"c:\workingdirectory\_plugins\SimpleScript.csx";
                var scriptName2 = @"c:\workingdirectory\_plugins\DoubleScript.csx";
                var scriptCsCatalog = new ScriptCsCatalog("_plugins",
                    GetOptions(fileSystem: GetMockFileSystem(new[] { scriptName, scriptName2 }, new[] { Scripts.SimpleScript, Scripts.DoubleScript }).Object));
                scriptCsCatalog.Dispose();

                // act
                var exception = Assert.Throws<ObjectDisposedException>(() => scriptCsCatalog.Refresh());
                exception.ObjectName.ShouldEqual("ScriptCsCatalog");
            }

            [Fact]
            public void ShouldRefreshPartsIfScriptsAreAdded()
            {
                // arrange
                var scriptName = @"c:\workingdirectory\_plugins\SimpleScript.csx";
                var scriptName2 = @"c:\workingdirectory\_plugins\DoubleScript.csx";
                var fileSystem = GetMockFileSystem(new string[0], new string[0]);
                var scriptCsCatalog = new ScriptCsCatalog("_plugins", GetOptions(fileSystem: fileSystem.Object));

                // act
                var mefHost = GetComposedMefHost(scriptCsCatalog);

                // assert
                scriptCsCatalog.LoadedFiles.Count.ShouldEqual(0);
                mefHost.Plugins.Count.ShouldEqual(0);

                // arrange
                UpdateFileSystem(fileSystem, new[] { scriptName, scriptName2 }, new[] { Scripts.SimpleScript, Scripts.DoubleScript });

                // act
                scriptCsCatalog.Refresh();

                // assert
                scriptCsCatalog.LoadedFiles.Count.ShouldEqual(2);
                mefHost.Plugins.Count.ShouldEqual(2);
            }

            [Fact]
            public void ShouldRefreshPartsIfScriptIsModified()
            {
                // arrange
                var scriptName = @"c:\workingdirectory\_plugins\SimpleScript.csx";
                var fileSystem = GetMockFileSystem(scriptName, Scripts.SimpleScript);
                var scriptCsCatalog = new ScriptCsCatalog("_plugins", GetOptions(fileSystem: fileSystem.Object));

                // act
                var mefHost = GetComposedMefHost(scriptCsCatalog);

                // assert
                scriptCsCatalog.LoadedFiles.Count.ShouldEqual(1);
                mefHost.Plugins.Count.ShouldEqual(1);
                mefHost.Plugins[0].DoSomething().ShouldEqual("Simple");

                // arrange
                UpdateFileSystem(fileSystem, new[] { scriptName }, new[] { Scripts.DoubleScript });

                // act
                scriptCsCatalog.Refresh();

                // assert
                scriptCsCatalog.LoadedFiles.Count.ShouldEqual(1);
                mefHost.Plugins.Count.ShouldEqual(1);
                mefHost.Plugins[0].DoSomething().ShouldEqual("Double");
            }

            [Fact]
            public void ShouldRaiseOnchangedAndOnChanging()
            {
                // arrange
                bool onChangedCalled = false;
                bool onChangingCalled = false;
                ComposablePartCatalogChangeEventArgs onChangedEventArgs = null;
                ComposablePartCatalogChangeEventArgs onChangingEventArgs = null;
                var scriptName = @"c:\workingdirectory\_plugins\SimpleScript.csx";
                var fileSystem = GetMockFileSystem(scriptName, Scripts.SimpleScript);
                var scriptCsCatalog = new ScriptCsCatalog("_plugins", GetOptions(fileSystem: fileSystem.Object));
                scriptCsCatalog.Changing += (object sender, ComposablePartCatalogChangeEventArgs e) =>
                {
                    onChangingCalled = true;
                    onChangingEventArgs = e;
                };

                scriptCsCatalog.Changed += (object sender, ComposablePartCatalogChangeEventArgs e) =>
                {
                    onChangedCalled = true;
                    onChangedEventArgs = e;
                };

                // act
                var mefHost = GetComposedMefHost(scriptCsCatalog);

                // assert
                scriptCsCatalog.LoadedFiles.Count.ShouldEqual(1);
                mefHost.Plugins.Count.ShouldEqual(1);
                mefHost.Plugins[0].DoSomething().ShouldEqual("Simple");

                // arrange
                UpdateFileSystem(fileSystem, new[] { scriptName }, new[] { Scripts.DoubleScript });

                // act
                scriptCsCatalog.Refresh();

                // assert
                scriptCsCatalog.LoadedFiles.Count.ShouldEqual(1);
                mefHost.Plugins.Count.ShouldEqual(1);
                mefHost.Plugins[0].DoSomething().ShouldEqual("Double");

                onChangingCalled.ShouldBeTrue();
                onChangingEventArgs.ShouldNotBeNull();
                onChangingEventArgs.AtomicComposition.ShouldNotBeNull();
                onChangingEventArgs.AddedDefinitions.ShouldNotBeNull();
                onChangingEventArgs.AddedDefinitions.ShouldNotBeEmpty();
                onChangingEventArgs.AddedDefinitions.Count().ShouldEqual(1);
                onChangingEventArgs.AddedDefinitions.First().ToString().ShouldEqual("Submission#0+DoubleSomething");
                onChangingEventArgs.RemovedDefinitions.ShouldNotBeNull();
                onChangingEventArgs.RemovedDefinitions.ShouldNotBeEmpty();
                onChangingEventArgs.RemovedDefinitions.Count().ShouldEqual(1);
                onChangingEventArgs.RemovedDefinitions.First().ToString().ShouldEqual("Submission#0+SimpleSomething");

                onChangedCalled.ShouldBeTrue();
                onChangedEventArgs.ShouldNotBeNull();
                onChangedEventArgs.AtomicComposition.ShouldBeNull();
                onChangedEventArgs.AddedDefinitions.ShouldNotBeNull();
                onChangedEventArgs.AddedDefinitions.ShouldNotBeEmpty();
                onChangedEventArgs.AddedDefinitions.Count().ShouldEqual(1);
                onChangedEventArgs.AddedDefinitions.First().ToString().ShouldEqual("Submission#0+DoubleSomething");
                onChangedEventArgs.RemovedDefinitions.ShouldNotBeNull();
                onChangedEventArgs.RemovedDefinitions.ShouldNotBeEmpty();
                onChangedEventArgs.RemovedDefinitions.Count().ShouldEqual(1);
                onChangedEventArgs.RemovedDefinitions.First().ToString().ShouldEqual("Submission#0+SimpleSomething");
            }
        }

        private static MEFHost GetComposedMefHost(ScriptCsCatalog catalog)
        {
            // arrange
            var container = new CompositionContainer(catalog);
            var batch = new CompositionBatch();
            var mefHost = new MEFHost();
            batch.AddPart(mefHost);
            // act
            container.Compose(batch);

            return mefHost;
        }

        private static ScriptCsCatalogOptions GetOptions(string[] scriptArgs = null, Type[] references = null, IFileSystem fileSystem = null)
        {
            return new ScriptCsCatalogOptions
            {
                FileSystem = fileSystem,
                References = references,
                ScriptArgs = scriptArgs
            };
        }

        private static Mock<IFileSystem> GetMockFileSystem(string fileName, string fileContent)
        {
            return GetMockFileSystem(new[] { fileName }, new string[] { fileContent });
        }

        private static Mock<IFileSystem> GetMockFileSystem(string[] fileNames, string[] fileContents)
        {
            var fileSystem = new Mock<IFileSystem>();
            fileSystem.SetupGet(f => f.PackagesFile).Returns("scriptcs_packages.config");
            fileSystem.SetupGet(f => f.PackagesFolder).Returns("scriptcs_packages");
            fileSystem.SetupGet(f => f.BinFolder).Returns("scriptcs_bin");
            fileSystem.SetupGet(f => f.DllCacheFolder).Returns(".scriptcs_cache");
            fileSystem.SetupGet(f => f.NugetFile).Returns("scriptcs_nuget.config");
            fileSystem.SetupGet(f => f.GlobalFolder).Returns(@"c:\workingdirectory");
            fileSystem.SetupGet(f => f.HostBin).Returns(Environment.CurrentDirectory);
            fileSystem.SetupGet(f => f.CurrentDirectory).Returns(@"c:\workingdirectory");
            fileSystem.Setup(f => f.GetWorkingDirectory(It.IsAny<string>())).Returns(@"c:\workingdirectory");
            fileSystem.Setup(f => f.DirectoryExists(It.IsAny<string>())).Returns<string>((directory) => directory.EndsWith("_plugins"));

            fileSystem.SetupGet(f => f.NewLine).Returns(Environment.NewLine);
            fileSystem.Setup(f => f.GetFullPath(It.IsAny<string>())).Returns<string>((path) => path);
            fileSystem.Setup(f => f.SplitLines(It.IsAny<string>())).Returns<string>((file) => file.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None));

            fileSystem.Setup(f => f.EnumerateFiles(@"c:\workingdirectory\_plugins", It.IsAny<string>(), SearchOption.AllDirectories))
                .Returns<string, string, SearchOption>((directory, searchPattern, searchOption) => fileNames.Where(file => file.EndsWith(Path.GetExtension(searchPattern))));

            for (int i = 0; i < fileNames.Length; i++)
            {
                fileSystem.Setup(f => f.FileExists(fileNames[i])).Returns(true);
                fileSystem.Setup(f => f.ReadFileLines(fileNames[i])).Returns(fileContents[i].Split(new[] { "\r\n", "\n" }, StringSplitOptions.None));
            }

            return fileSystem;
        }

        private static void UpdateFileSystem(Mock<IFileSystem> fileSystem, string[] fileNames, string[] fileContents)
        {
            fileSystem.Setup(f => f.EnumerateFiles(@"c:\workingdirectory\_plugins", It.IsAny<string>(), SearchOption.AllDirectories))
                   .Returns<string, string, SearchOption>((directory, searchPattern, searchOption) => fileNames.Where(file => file.EndsWith(Path.GetExtension(searchPattern))));

            for (int i = 0; i < fileNames.Length; i++)
            {
                fileSystem.Setup(f => f.FileExists(fileNames[i])).Returns(true);
                fileSystem.Setup(f => f.ReadFileLines(fileNames[i])).Returns(fileContents[i].Split(new[] { "\r\n", "\n" }, StringSplitOptions.None));
            }
        }
    }
}
