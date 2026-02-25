# ARCHITECTURE.md — モジュール設計

---

## モジュール一覧

| # | モジュール | クラス名 | 責務 |
|---|---|---|---|
| 1 | Logger | `TCOLogger` | ログ出力の一元管理 |
| 2 | Constants | `Constants` | 定数定義 |
| 3 | UV Island Detector | `UVIslandDetector` | UV島の検出・AABB算出 |
| 4 | UV Rect Calculator | `UVRectCalculator` | 島のAABB和集合→UsedRect確定 |
| 5 | Power Of Two Calculator | `PowerOfTwoCalculator` | 必要最小2のべき乗サイズの算出・閾値判定 |
| 6 | Shader Property Resolver | `ShaderPropertyResolver` | シェーダーごとのプロパティ・UVチャンネル解決 |
| 7 | Texture Readable Handler | `TextureReadableHandler` | Read/Writeの一時有効化・復元 |
| 8 | Texture Rebuilder | `TextureRebuilder` | テクスチャ複製・UsedRect領域のピクセルコピー再構成 |
| 9 | Mesh Remapper | `MeshRemapper` | メッシュ複製・UV0のリマップ |
| 10 | Material Rebuilder | `MaterialRebuilder` | マテリアル複製・テクスチャプロパティ差し替え |
| 11 | Renderer Collector | `RendererCollector` | Avatar配下のRenderer・Mesh・Materialを収集 |
| 12 | Texture Group Builder | `TextureGroupBuilder` | テクスチャ単位でMesh・Materialをグループ化 |
| 13 | Optimization Pipeline | `OptimizationPipeline` | 全モジュールを順序通り呼び出すメインフロー |
| 14 | Settings Component | `TextureCropSettings` | Unityコンポーネント（設定データ保持） |
| 15 | Settings Editor | `TextureCropSettingsEditor` | インスペクターUI・検知ボタン |
| 16 | NDMF Plugin | `TextureCropOptimizerPlugin` | NDMFへの登録・フェーズ制御 |

---

## 依存関係図

依存は下方向のみ（上のモジュールは下のモジュールを呼ぶ）。  
横断依存は禁止。

```
[16 NDMF Plugin]
        |
[13 Optimization Pipeline]
        |
   ┌────┴──────────────────────────┐
   |                               |
[11 Renderer Collector]   [12 Texture Group Builder]
                                   |
        ┌──────────────────────────┤
        |              |           |
[3 UV Island   [6 Shader    [5 Power Of Two
  Detector]     Property     Calculator]
        |        Resolver]
[4 UV Rect
  Calculator]
        |
   ┌──────────┬────────────┐
   |          |            |
[8 Texture [9 Mesh    [7 Texture
  Rebuilder] Remapper]   Readable
   |                     Handler]
[10 Material              (未使用)
  Rebuilder]

[1 Logger]    ← 全モジュールから参照可
[2 Constants] ← 全モジュールから参照可
[14 Settings Component] ← OptimizationPipeline・SettingsEditorから参照
[15 Settings Editor]    ← Unityエディタのみ
```

---

## 各モジュール詳細

### 1. TCOLogger
```csharp
namespace TextureCropOptimizer

public static class TCOLogger
{
    public static void Info(string category, string message, string target = null);
    public static void Warning(string category, string message, string target = null, string detail = null);
    public static void Error(string category, string message, string target = null, string detail = null, string fix = null);
    // Errorは例外をスローしてビルドを中断する
}
```
依存：なし

---

### 2. Constants
```csharp
namespace TextureCropOptimizer

public static class Constants
{
    public const string MenuPath = "GameObject/TextureCropOptimizer/TextureCropSettings";
    public const string LogPrefix = "[TextureCropOptimizer]";
    public const float DefaultTiling = 1.0f;
    public const float DefaultOffset = 0.0f;
    public const int MinTextureSize = 4;
    // など
}
```
依存：なし

---

### 3. UVIslandDetector
```csharp
namespace TextureCropOptimizer

public static class UVIslandDetector
{
    /// <summary>
    /// メッシュのUV0からUV島を検出し、各島のAABBリストを返す。
    /// 0–1範囲外の頂点が存在する場合はnullを返す。
    /// </summary>
    public static List<Rect>? DetectIslandBounds(Mesh mesh);
}
```
依存：TCOLogger, Constants

---

### 4. UVRectCalculator
```csharp
namespace TextureCropOptimizer

public static class UVRectCalculator
{
    /// <summary>
    /// 複数のAABBリストを受け取り、全体の和集合Rectを返す。
    /// </summary>
    public static Rect CalculateUsedRect(IEnumerable<Rect> islandBounds);
}
```
依存：なし

---

### 5. PowerOfTwoCalculator
```csharp
namespace TextureCropOptimizer

public static class PowerOfTwoCalculator
{
    /// <summary>
    /// UsedRectとオリジナルサイズから必要最小の2のべき乗サイズを算出する。
    /// </summary>
    public static int Calculate(Rect usedRect, int originalSize);

    /// <summary>
    /// 最適化が有効かどうかを判定する（1段階以上小さくなるか）。
    /// </summary>
    public static bool IsWorthOptimizing(int originalSize, int optimizedSize);
}
```
依存：なし

---

### 6. ShaderPropertyResolver
```csharp
namespace TextureCropOptimizer

public static class ShaderPropertyResolver
{
    /// <summary>
    /// マテリアルからUV0を使用しているテクスチャプロパティ名のリストを返す。
    /// tiling/offsetがデフォルト以外のプロパティは除外する。
    /// </summary>
    public static List<string> GetUV0TextureProperties(Material material);
}
```
依存：TCOLogger, Constants

---

### 7. TextureReadableHandler
```csharp
namespace TextureCropOptimizer

public class TextureReadableHandler : IDisposable
{
    /// <summary>
    /// コンストラクタでRead/Writeを一時有効化し、Disposeで元に戻す。
    /// </summary>
    public TextureReadableHandler(Texture2D texture);
    public void Dispose();
}
```
依存：TCOLogger

---

### 8. TextureRebuilder
```csharp
namespace TextureCropOptimizer

public static class TextureRebuilder
{
    /// <summary>
    /// UsedRectに対応するピクセル領域を新しいテクスチャとして再構成して返す。
    /// </summary>
    public static Texture2D Rebuild(Texture2D source, Rect usedRect, int targetSize);
}
```
依存：TCOLogger（Graphics.Blit使用のためTextureReadableHandler不要）

---

### 9. MeshRemapper
```csharp
namespace TextureCropOptimizer

public static class MeshRemapper
{
    /// <summary>
    /// メッシュを複製し、UV0をUsedRectに合わせてリマップした複製を返す。
    /// </summary>
    public static Mesh Remap(Mesh source, Rect usedRect);
}
```
依存：TCOLogger

---

### 10. MaterialRebuilder
```csharp
namespace TextureCropOptimizer

public static class MaterialRebuilder
{
    /// <summary>
    /// マテリアルを複製し、指定プロパティのテクスチャを差し替えた複製を返す。
    /// </summary>
    public static Material Rebuild(Material source, Dictionary<string, Texture2D> textureMap);
}
```
依存：TCOLogger

---

### 11. RendererCollector
```csharp
namespace TextureCropOptimizer

public static class RendererCollector
{
    /// <summary>
    /// Avatar配下の全RendererからRenderer・Mesh・Materialの組を収集して返す。
    /// </summary>
    public static List<RendererEntry> Collect(GameObject avatarRoot);
}

public readonly struct RendererEntry
{
    public Renderer Renderer { get; }
    public Mesh Mesh { get; }
    public Material[] Materials { get; }
}
```
依存：TCOLogger

---

### 12. TextureGroupBuilder
```csharp
namespace TextureCropOptimizer

public static class TextureGroupBuilder
{
    /// <summary>
    /// RendererEntryリストをテクスチャ単位でグループ化して返す。
    /// 除外マテリアルは除く。
    /// </summary>
    public static Dictionary<Texture2D, TextureGroup> Build(
        List<RendererEntry> entries,
        HashSet<Material> excludedMaterials,
        ShaderPropertyResolver resolver);
}

public class TextureGroup
{
    public Texture2D Texture { get; }
    public List<(Mesh Mesh, string PropertyName, Material Material)> References { get; }
}
```
依存：ShaderPropertyResolver, TCOLogger

---

### 13. OptimizationPipeline
```csharp
namespace TextureCropOptimizer

public static class OptimizationPipeline
{
    /// <summary>
    /// メインの最適化フローを実行する。
    /// </summary>
    public static void Execute(GameObject avatarRoot, TextureCropSettings settings);
}
```
依存：RendererCollector, TextureGroupBuilder, UVIslandDetector, UVRectCalculator, PowerOfTwoCalculator, TextureRebuilder, MeshRemapper, MaterialRebuilder, TCOLogger

---

### 14. TextureCropSettings（Unityコンポーネント）
```csharp
namespace TextureCropOptimizer

public class TextureCropSettings : MonoBehaviour
{
    public List<MaterialEntry> Entries;
}

[Serializable]
public class MaterialEntry
{
    public Material Material;
    public bool Excluded;
    public UVRotationMode UVRotation; // Normal / EmissionFixed
}

public enum UVRotationMode { Normal, EmissionFixed }
```
依存：なし

---

### 15. TextureCropSettingsEditor（Unityエディタ）
```csharp
namespace TextureCropOptimizer.Editor

[CustomEditor(typeof(TextureCropSettings))]
public class TextureCropSettingsEditor : Editor
{
    // 「テクスチャを検知」ボタン
    // エントリリスト表示
    // 除外チェックON時にドロップダウンをグレーアウト
}
```
依存：TextureCropSettings, UVIslandDetector, UVRectCalculator, PowerOfTwoCalculator, ShaderPropertyResolver, RendererCollector, TextureGroupBuilder

---

### 16. TextureCropOptimizerPlugin（NDMFプラグイン）
```csharp
namespace TextureCropOptimizer

[assembly: ExportsPlugin(typeof(TextureCropOptimizerPlugin))]
public class TextureCropOptimizerPlugin : Plugin<TextureCropOptimizerPlugin>
{
    protected override void Configure();
    // OptimizingPhaseの末尾・AAO/TTTより後に登録
}
```
依存：OptimizationPipeline, TextureCropSettings, TCOLogger

---

## テストファイル構成

```
Tests/
├── EditMode/
│   ├── TCOLoggerTests.cs
│   ├── UVIslandDetectorTests.cs
│   ├── UVRectCalculatorTests.cs
│   ├── PowerOfTwoCalculatorTests.cs
│   ├── ShaderPropertyResolverTests.cs
│   ├── TextureReadableHandlerTests.cs
│   ├── TextureRebuilderTests.cs
│   ├── MeshRemapperTests.cs
│   ├── MaterialRebuilderTests.cs
│   ├── RendererCollectorTests.cs
│   ├── TextureGroupBuilderTests.cs
│   └── OptimizationPipelineTests.cs
```

---

## 実装順序（推奨）

依存が少ない順に実装する。

```
1. Constants
2. TCOLogger
3. UVIslandDetector
4. UVRectCalculator
5. PowerOfTwoCalculator
6. ShaderPropertyResolver
7. TextureReadableHandler
8. TextureRebuilder
9. MeshRemapper
10. MaterialRebuilder
11. RendererCollector
12. TextureGroupBuilder
13. TextureCropSettings（コンポーネント）
14. OptimizationPipeline
15. TextureCropSettingsEditor
16. TextureCropOptimizerPlugin
```
