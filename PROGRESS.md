# PROGRESS.md — 作業進捗

作業再開時は**このファイルを最初に読む**こと。
作業終了・中断時は必ずこのファイルを更新すること。

---

## 現在地

```
ステータス  : 全モジュール実装完了・品質改善・テスト強化・パフォーマンス改善完了
現在のモジュール : -
現在のフェーズ   : Unity結合テスト・実機テスト
最終更新    : 2026-02-25
```

---

## モジュール別進捗

| # | モジュール | ステータス | TESTファイル | 実装ファイル | 備考 |
|---|---|---|---|---|---|
| 1 | Constants | 🟢 完了 | Tests/EditMode/ConstantsTests.cs | Runtime/Constants.cs | |
| 2 | TCOLogger | 🟢 完了 | Tests/EditMode/TCOLoggerTests.cs | Runtime/TCOLogger.cs | Error時に例外スロー |
| 3 | UVIslandDetector | 🟢 完了 | Tests/EditMode/UVIslandDetectorTests.cs | Runtime/UVIslandDetector.cs | Union-Find。エッジケースTST追加 |
| 4 | UVRectCalculator | 🟢 完了 | Tests/EditMode/UVRectCalculatorTests.cs | Runtime/UVRectCalculator.cs | 空入力ガード追加 |
| 5 | PowerOfTwoCalculator | 🟢 完了 | Tests/EditMode/PowerOfTwoCalculatorTests.cs | Runtime/PowerOfTwoCalculator.cs | エッジケースTST追加 |
| 6 | ShaderPropertyResolver | 🟢 完了 | Tests/EditMode/ShaderPropertyResolverTests.cs | Runtime/ShaderPropertyResolver.cs | Poiyomi/Liltoon UVチャンネル判定実装済み |
| 7 | TextureReadableHandler | 🟡 未使用 | Tests/EditMode/TextureReadableHandlerTests.cs | Runtime/TextureReadableHandler.cs | GPU Blit移行により不要。残存 |
| 8 | TextureRebuilder | 🟢 完了 | Tests/EditMode/TextureRebuilderTests.cs | Runtime/TextureRebuilder.cs | GPU Blit。圧縮テクスチャ対応。try-finally |
| 9 | MeshRemapper | 🟢 完了 | Tests/EditMode/MeshRemapperTests.cs | Runtime/MeshRemapper.cs | 法線・頂点保持TST追加。名前引継ぎ。ゼロ除算ガード |
| 10 | MaterialRebuilder | 🟢 完了 | Tests/EditMode/MaterialRebuilderTests.cs | Runtime/MaterialRebuilder.cs | 名前引継ぎ。複数テクスチャTST追加 |
| 11 | RendererCollector | 🟢 完了 | Tests/EditMode/RendererCollectorTests.cs | Runtime/RendererCollector.cs | 非アクティブ・複数MAT TST追加 |
| 12 | TextureGroupBuilder | 🟢 完了 | Tests/EditMode/TextureGroupBuilderTests.cs | Runtime/TextureGroupBuilder.cs | tiling/offset/null統合テスト追加 |
| 13 | TextureCropSettings | 🟢 完了 | Tests/EditMode/TextureCropSettingsTests.cs | Runtime/TextureCropSettings.cs | MonoBehaviour |
| 14 | OptimizationPipeline | 🟢 完了 | Tests/EditMode/OptimizationPipelineTests.cs | Runtime/OptimizationPipeline.cs | 2フェーズ。materialMap共有。サマリーログ。ReadableHandler不要化 |
| 15 | TextureCropSettingsEditor | 🟢 完了 | - | Editor/TextureCropSettingsEditor.cs | エディタUI。サイズ・VRAM削減サマリー |
| 16 | TextureCropOptimizerPlugin | 🟢 完了 | - | Runtime/TextureCropOptimizerPlugin.cs | NDMF。AAO/TTT/MA順序依存。nullガード |

---

## 次のタスク

```
1. Unity上での結合テスト（NDMFパッケージ導入後）
2. Poiyomi/Liltoon シェーダーでの実機テスト
3. TextureReadableHandler: 削除検討（GPU Blit移行により不要）
```

---

## 既知の問題・懸念事項

- TextureReadableHandlerはGPU Blit移行により未使用。削除候補
- 同一メッシュが異なるUsedRectのテクスチャグループに参照される場合のUVリマップ整合性（V2課題）

---

## 完了したバグ修正・改善

| 日時 | 内容 |
|---|---|
| 2026-02-22 | OptimizationPipeline: materialMap/meshMapグループ横断共有バグ修正 |
| 2026-02-22 | TextureCropOptimizerPlugin: AAO/TTT/MA AfterPlugin追加 |
| 2026-02-23 | ShaderPropertyResolver: Poiyomi/Liltoon UVチャンネル判定追加 |
| 2026-02-23 | TextureRebuilder: GPU Blitベースにリファクタ（圧縮テクスチャ対応） |
| 2026-02-23 | 複製アセットに元の名前+サフィックスを設定 |
| 2026-02-23 | package.json追加（VPM対応）、asmdefにNDMF参照追加 |
| 2026-02-23 | TextureCropSettingsEditor: テクスチャサイズ・VRAM削減サマリーUI追加 |
| 2026-02-23 | DrawSummary: 除外マテリアルの正しいフィルタリング修正 |
| 2026-02-23 | OptimizationPipeline: 最適化サマリーログ出力追加 |
| 2026-02-23 | MeshRemapper: ゼロ除算ガード追加（zero-width/height UsedRect） |
| 2026-02-23 | UVRectCalculator: 空入力ガード追加（zero Rect返却） |
| 2026-02-23 | TextureRebuilder: try-finallyでRenderTextureリーク防止 |
| 2026-02-23 | TextureGroupBuilder: tiling/offset/null/multi-material統合テスト追加 |
| 2026-02-25 | TextureCropOptimizerPlugin: AvatarRootObject nullガード追加 |
| 2026-02-25 | 全モジュールのテスト強化（非正方形テクスチャ、複数テクスチャ、名前サフィックス等） |
| 2026-02-25 | VRAM計算: 非正方形テクスチャの正確な計算に修正 |
| 2026-02-25 | OptimizationPipeline: TextureReadableHandler不要化（GPU Blit移行） |
| 2026-02-25 | SPEC.md仕様変更: 非Readableテクスチャ仕様をGPU Blit前提に更新 |
| 2026-02-25 | OptimizationPipeline.AnalyzeTextureGroupをpublic化、Editorから共有 |
| 2026-02-25 | TextureCropSettingsEditor: 重複解析ロジックをPipeline共有に統合 |

---

## 中断ログ

| 日時 | 中断ポイント | 再開に必要な情報 |
|---|---|---|
| 2026-02-22 | Constants・TCOLogger完了後 | #3 UVIslandDetectorからRED開始 |
| 2026-02-22 | 全16モジュール初期実装完了 | 結合テスト・Poiyomi/Liltoon対応・NDMF統合 |
| 2026-02-22 | バグ修正・テスト強化完了 | Poiyomi/Liltoon UVチャンネル判定 |
| 2026-02-23 | 品質改善完了 | UI改善・結合テスト・実機テスト |
| 2026-02-23 | UI改善・バグ修正・テスト強化完了 | 結合テスト・実機テスト |
| 2026-02-25 | テスト強化・ReadableHandler不要化・解析ロジック共有化完了 | 結合テスト・実機テスト |

---

## メモ

- Poiyomi: `{prop}UV` (int, 0=UV0), Liltoon: `{prop}_UVMode` (int, 0=UV0)
- `TextureReadableHandler` はusingブロック前提（ただし現在未使用）
- OptimizationPipeline.AnalyzeTextureGroupはpublic。EditorとPipelineで解析ロジック共有
- NDMF AfterPlugin: AAO(com.anatawa12.avatar-optimizer), TTT(net.rs64.tex-trans-tool), MA(nadena.dev.modular-avatar)
- UVIslandDetector: Union-Find（パス圧縮+ランク付き）
- TextureRebuilder: Graphics.Blit(source, rt, scale, offset) で GPU クロップ＋リサイズ
- OptimizationPipeline: Phase1解析 → Phase2適用。materialMap/meshMapグループ横断共有
- 複製アセット命名: テクスチャ=_cropped, メッシュ=_remapped, マテリアル=_optimized
- VPMパッケージID: com.ebinyap.texture-crop-optimizer
