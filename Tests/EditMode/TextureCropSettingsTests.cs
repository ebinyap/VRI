using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using TextureCropOptimizer;

namespace TextureCropOptimizer.Tests
{
    [TestFixture]
    public class TextureCropSettingsTests
    {
        private GameObject _go;

        [TearDown]
        public void TearDown()
        {
            if (_go != null)
                Object.DestroyImmediate(_go);
        }

        [Test]
        public void CanAddComponentToGameObject()
        {
            _go = new GameObject("Avatar");
            var settings = _go.AddComponent<TextureCropSettings>();
            Assert.IsNotNull(settings);
        }

        [Test]
        public void Entries_DefaultIsEmpty()
        {
            _go = new GameObject("Avatar");
            var settings = _go.AddComponent<TextureCropSettings>();
            Assert.IsNotNull(settings.Entries);
            Assert.AreEqual(0, settings.Entries.Count);
        }

        [Test]
        public void Entries_CanAddMaterialEntry()
        {
            _go = new GameObject("Avatar");
            var settings = _go.AddComponent<TextureCropSettings>();
            var mat = new Material(Shader.Find("Standard"));

            settings.Entries.Add(new MaterialEntry
            {
                Material = mat,
                Excluded = false,
                UVRotation = UVRotationMode.Normal
            });

            Assert.AreEqual(1, settings.Entries.Count);
            Assert.AreSame(mat, settings.Entries[0].Material);
            Assert.IsFalse(settings.Entries[0].Excluded);
            Assert.AreEqual(UVRotationMode.Normal, settings.Entries[0].UVRotation);

            Object.DestroyImmediate(mat);
        }

        [Test]
        public void MaterialEntry_ExcludedFlag_Works()
        {
            var entry = new MaterialEntry
            {
                Material = null,
                Excluded = true,
                UVRotation = UVRotationMode.EmissionFixed
            };

            Assert.IsTrue(entry.Excluded);
            Assert.AreEqual(UVRotationMode.EmissionFixed, entry.UVRotation);
        }

        [Test]
        public void UVRotationMode_HasExpectedValues()
        {
            Assert.AreEqual(0, (int)UVRotationMode.Normal);
            Assert.AreEqual(1, (int)UVRotationMode.EmissionFixed);
        }

        [Test]
        public void Entries_MultipleEntries_CanBeModifiedIndependently()
        {
            _go = new GameObject("Avatar");
            var settings = _go.AddComponent<TextureCropSettings>();
            var mat1 = new Material(Shader.Find("Standard"));
            var mat2 = new Material(Shader.Find("Standard"));

            settings.Entries.Add(new MaterialEntry { Material = mat1, Excluded = false });
            settings.Entries.Add(new MaterialEntry { Material = mat2, Excluded = true });

            Assert.AreEqual(2, settings.Entries.Count);
            Assert.IsFalse(settings.Entries[0].Excluded);
            Assert.IsTrue(settings.Entries[1].Excluded);

            // 1つ目を除外に変更しても2つ目には影響しない
            settings.Entries[0].Excluded = true;
            Assert.IsTrue(settings.Entries[0].Excluded);
            Assert.IsTrue(settings.Entries[1].Excluded);

            Object.DestroyImmediate(mat1);
            Object.DestroyImmediate(mat2);
        }

        [Test]
        public void Entries_CanBeReplacedWithNewList()
        {
            _go = new GameObject("Avatar");
            var settings = _go.AddComponent<TextureCropSettings>();
            var mat = new Material(Shader.Find("Standard"));

            settings.Entries.Add(new MaterialEntry { Material = mat });

            // エントリリストを丸ごと差し替え（DetectTextures相当の動作）
            settings.Entries = new List<MaterialEntry>();
            Assert.AreEqual(0, settings.Entries.Count);

            Object.DestroyImmediate(mat);
        }

        [Test]
        public void DisallowMultipleComponent_AttributePresent()
        {
            var attributes = typeof(TextureCropSettings).GetCustomAttributes(
                typeof(DisallowMultipleComponent), true);
            Assert.AreEqual(1, attributes.Length);
        }
    }
}
