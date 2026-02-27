using System;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using TextureCropOptimizer;

namespace TextureCropOptimizer.Tests
{
    [TestFixture]
    public class TCOLoggerTests
    {
        [Test]
        public void Info_LogsWithCorrectFormat()
        {
            // SPEC: [TextureCropOptimizer] INFO | {Category} | {Message}
            LogAssert.Expect(LogType.Log,
                "[TextureCropOptimizer] INFO | TestCategory | Test message");
            TCOLogger.Info("TestCategory", "Test message");
        }

        [Test]
        public void Info_WithTarget_LogsTargetLine()
        {
            LogAssert.Expect(LogType.Log,
                "[TextureCropOptimizer] INFO | TestCategory | Test message\n" +
                "  Target  : MyTexture.png");
            TCOLogger.Info("TestCategory", "Test message", "MyTexture.png");
        }

        [Test]
        public void Warning_LogsWithCorrectFormat()
        {
            LogAssert.Expect(LogType.Warning,
                "[TextureCropOptimizer] WARNING | TestCategory | Warning message");
            TCOLogger.Warning("TestCategory", "Warning message");
        }

        [Test]
        public void Warning_WithTargetAndDetail_LogsAllLines()
        {
            LogAssert.Expect(LogType.Warning,
                "[TextureCropOptimizer] WARNING | TestCategory | Warning message\n" +
                "  Detail  : Some detail\n" +
                "  Target  : MyMesh");
            TCOLogger.Warning("TestCategory", "Warning message", "MyMesh", "Some detail");
        }

        [Test]
        public void Error_ThrowsException()
        {
            LogAssert.Expect(LogType.Error,
                "[TextureCropOptimizer] ERROR | TestCategory | Error message\n" +
                "  Action  : Build Aborted");
            Assert.Throws<Exception>(() =>
                TCOLogger.Error("TestCategory", "Error message"));
        }

        [Test]
        public void Error_LogsBeforeThrowing()
        {
            LogAssert.Expect(LogType.Error,
                "[TextureCropOptimizer] ERROR | TestCategory | Error message\n" +
                "  Action  : Build Aborted");

            Assert.Throws<Exception>(() =>
                TCOLogger.Error("TestCategory", "Error message"));
        }

        [Test]
        public void Error_WithAllParameters_LogsFullFormat()
        {
            LogAssert.Expect(LogType.Error,
                "[TextureCropOptimizer] ERROR | TestCategory | Error message\n" +
                "  Detail  : Error detail\n" +
                "  Target  : BrokenAsset\n" +
                "  Action  : Build Aborted\n" +
                "  Fix     : Check the asset settings");

            Assert.Throws<Exception>(() =>
                TCOLogger.Error("TestCategory", "Error message", "BrokenAsset", "Error detail", "Check the asset settings"));
        }

        [Test]
        public void FormatBytes_Bytes_ReturnsB()
        {
            Assert.AreEqual("512 B", TCOLogger.FormatBytes(512));
        }

        [Test]
        public void FormatBytes_Kilobytes_ReturnsKB()
        {
            Assert.AreEqual("1.0 KB", TCOLogger.FormatBytes(1024));
        }

        [Test]
        public void FormatBytes_Megabytes_ReturnsMB()
        {
            Assert.AreEqual("1.0 MB", TCOLogger.FormatBytes(1024 * 1024));
        }

        [Test]
        public void FormatBytes_LargeMB_ReturnsFormattedMB()
        {
            // 64MB = 67108864 bytes
            Assert.AreEqual("64.0 MB", TCOLogger.FormatBytes(64L * 1024 * 1024));
        }

        [Test]
        public void FormatBytes_Zero_ReturnsZeroB()
        {
            Assert.AreEqual("0 B", TCOLogger.FormatBytes(0));
        }
    }
}
