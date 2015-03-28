using Moq;
using ScriptCs.Contracts;
using Should;
using System;
using Xunit;

namespace ScriptCs.ComponentModel.Composition.Test
{
    public class ScriptCsCatalogOptionsFacts
    {
        public class OverridesNullByDefaultMethod
        {
            [Fact]
            public void ShouldNotOverridesNullReferences()
            {
                // arrange
                var options = new ScriptCsCatalogOptions { References = null };

                // act
                var result = options.OverridesNullByDefault();

                // assert
                result.References.ShouldBeNull();
            }

            [Fact]
            public void ShouldNotOverridesEmptyReferences()
            {
                // arrange
                var options = new ScriptCsCatalogOptions { References = new Type[0] };

                // act
                var result = options.OverridesNullByDefault();

                // assert
                result.References.ShouldBeEmpty();
            }

            [Fact]
            public void ShouldNotOverridesValuedReferences()
            {
                // arrange
                var options = new ScriptCsCatalogOptions { References = new[] { typeof(OverridesNullByDefaultMethod) } };

                // act
                var result = options.OverridesNullByDefault();

                // assert
                result.References.Length.ShouldEqual(1);
                result.References[0].ShouldEqual(typeof(OverridesNullByDefaultMethod));
            }

            [Fact]
            public void ShouldOverridesNullScriptArgs()
            {
                // arrange
                var options = new ScriptCsCatalogOptions { ScriptArgs = null };

                // act
                var result = options.OverridesNullByDefault();

                // assert
                result.ScriptArgs.ShouldNotBeNull();
                result.ScriptArgs.ShouldBeEmpty();
            }

            [Fact]
            public void ShouldNotOverridesEmptyScriptArgs()
            {
                // arrange
                var options = new ScriptCsCatalogOptions { ScriptArgs = new string[0] };

                // act
                var result = options.OverridesNullByDefault();

                // assert
                result.ScriptArgs.ShouldBeEmpty();
            }

            [Fact]
            public void ShouldNotOverridesValuedScriptArgs()
            {
                // arrange
                var options = new ScriptCsCatalogOptions { ScriptArgs = new[] { "arg" } };

                // act
                var result = options.OverridesNullByDefault();

                // assert
                result.ScriptArgs.Length.ShouldEqual(1);
                result.ScriptArgs[0].ShouldEqual("arg");
            }

            [Fact]
            public void ShouldOverridesNullFileSystem()
            {
                // arrange
                var options = new ScriptCsCatalogOptions { FileSystem = null };

                // act
                var result = options.OverridesNullByDefault();

                // assert
                result.FileSystem.ShouldNotBeNull();
                result.FileSystem.ShouldBeType<FileSystem>();
            }

            [Fact]
            public void ShouldNotOverridesValuedFileSystem()
            {
                // arrange
                var options = new ScriptCsCatalogOptions { FileSystem = new Mock<IFileSystem>().Object };

                // act
                var result = options.OverridesNullByDefault();

                // assert
                result.FileSystem.ShouldNotBeNull();
                result.FileSystem.GetType().Name.ShouldEqual("IFileSystemProxy");
            }
        }
    }
}