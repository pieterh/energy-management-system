using System;
using FluentAssertions;

using EMS.Library.Files;

namespace FileToolsUnitTest
{
    public class FileToolsTests
    {
        [Fact]
        public void FileExistsAndReadableNullAndEmptyAreFalse()
        {
            FileTools.FileExistsAndReadable(null).Should().BeFalse();
            FileTools.FileExistsAndReadable(string.Empty).Should().BeFalse();
        }

        [Fact]
        public void FileExistsAndReadableNotExistingFileIsFalse()
        {
            FileTools.FileExistsAndReadable("doesnt exist").Should().BeFalse();
        }

        [Fact]
        public void FileExistsAndReadableExistingFileIsTrue()
        {
            var f = typeof(FileToolsTests).Assembly.Location;
            FileTools.FileExistsAndReadable(f).Should().BeTrue();
        }

    }
}

