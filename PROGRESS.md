# PROGRESS.md — 作業進捗

作業再開時は**このファイルを最初に読む**こと。
作業終了・中断時は必ずこのファイルを更新すること。

---

## 現在地

```
ステータス  : 全モジュール実装完了・バグ修正・テスト強化済み
現在のモジュール : -
現在のフェーズ   : 結合テスト・Poiyomi/Liltoon対応待ち
最終更新    : 2026-02-22
```

---

## モジュール別進捗

| # | モジュール | ステータス | TESTファイル | 実装ファイル | 備考 |
|---|---|---|---|---|---|
| 1 | Constants | 🟢 REFACTOR完了 | Tests/EditMode/ConstantsTests.cs | Runtime/Constants.cs | |
| 2 | TCOLogger | 🟢 REFACTOR完了 | Tests/EditMode/TCOLoggerTests.cs | Runtime/TCOLogger.cs | Error時に例外スロー |
| 3 | UVIslandDetector | 🟢 REFACTOR完了 | Tests/EditMode/UVIslandDetectorTests.cs | Runtime/UVIslandDetector.cs | Union-Find使用。エッジケーステスト追加済み |
| 4 | UVRectCalculator | 🟢 REFACTOR完了 | Tests/EditMode/UVRectCalculatorTests.cs | Runtime/UVRectCalculator.cs | |
| 5 | PowerOfTwoCalculator | 🟢 REFACTOR完了 | Tests/EditMode/PowerOfTwoCalculatorTests.cs | Runtime/PowerOfTwoCalculator.cs | エッジケーステスト追加済み |
| 6 | ShaderPropertyResolver | 🟢 REFACTOR完了 | Tests/EditMode/ShaderPropertyResolverTests.cs | Runtime/ShaderPropertyResolver.cs | |
| 7 | TextureReadableHandler | 🟢 REFACTOR完了 | Tests/EditMode/TextureReadableHandlerTests.cs | Runtime/TextureReadableHandler.cs | IDisposable |
| 8 | TextureRebuilder | 🟢 REFACTOR完了 | Tests/EditMode/TextureRebuilderTests.cs | Runtime/TextureRebuilder.cs | |
| 9 | MeshRemapper | 🟢 REFACTOR完了 | Tests/EditMode/MeshRemapperTests.cs | Runtime/MeshRemapper.cs | 法線・頂点保持テスト追加済み |
| 10 | MaterialRebuilder | 🟢 REFACTOR完了 | Tests/EditMode/MaterialRebuilderTests.cs | Runtime/MaterialRebuilder.cs | |
| 11 | RendererCollector | 🟢 REFACTOR完了 | Tests/EditMode/RendererCollectorTests.cs | Runtime/RendererCollector.cs | 非アクティブ・複数マテリアルテスト追加済み |
| 12 | TextureGroupBuilder | 🟢 REFACTOR完了 | Tests/EditMode/TextureGroupBuilderTests.cs | Runtime/TextureGroupBuilder.cs | |
| 13 | TextureCropSettings | 🟢 REFACTOR完了 | Tests/EditMode/TextureCropSettingsTests.cs | Runtime/TextureCropSettings.cs | MonoBehaviour, MaterialEntry, UVRotationMode |
| 14 | OptimizationPipeline | 🟢 REFACTOR完了 | Tests/EditMode/OptimizationPipelineTests.cs | Runtime/OptimizationPipeline.cs | Phase1解析/Phase2適用に分離。materialMap共有バグ修正済み |
| 15 | TextureCropSettingsEditor | 🟢 REFACTOR完了 | - | Editor/TextureCropSettingsEditor.cs | エディタUI |
| 16 | TextureCropOptimizerPlugin | 🟢 REFACTOR完了 | - | Runtime/TextureCropOptimizerPlugin.cs | NDMF依存。AAO/TTT/MA実行順序依存追加済み |

ステータス凡例：
- ⬜ 未着手
- 🔴 RED（テスト作成中）
- 🟡 GREEN（実装中）
- 🟢 REFACTOR完了（モジュール完成）
- ✅ 結合確認済み

---

## 次のタスク

```
1. Unity上での結合テスト（NDMFパッケージ導入後）
2. Poiyomi・Liltoon シェーダーでのUVチャンネル判定実装
3. asmdefにNDMF参照を追加（パッケージ導入後）
4. package.json作成（VPM対応）
```

---

## 既知の問題・懸念事項

- TextureCropOptimizerPlugin.cs はNDMFパッケージ（nadena.dev.ndmf）への依存あり。asmdefに参照追加が必要
- ShaderPropertyResolverは現在UV0チャンネル判定を未実装（Poiyomi・Liltoonの固有プロパティ確認が必要）
- TextureReadableHandlerの非Readableテクスチャ対応はAssetImporter経由。NDMF内ではObjectRegistryを使う方が適切な可能性あり
- 同一メッシュが異なるUsedRectのテクスチャグループに参照される場合のUVリマップ整合性（V2課題）

---

## 完了したバグ修正

| 日時 | 内容 |
|---|---|
| 2026-02-22 | OptimizationPipeline: materialMap/meshMapがProcessTextureGroupごとにローカルだったバグを修正。グループ横断で共有するよう変更 |
| 2026-02-22 | TextureCropOptimizerPlugin: AAO/TTT/Modular Avatar実行後に動作するようAfterPlugin追加 |

---

## 中断ログ

| 日時 | 中断ポイント | 再開に必要な情報 |
|---|---|---|
| 2026-02-22 | Constants・TCOLogger完了後 | #3 UVIslandDetectorからRED開始 |
| 2026-02-22 | 全16モジュール初期実装完了 | 結合テスト・Poiyomi/Liltoon対応・NDMF統合 |
| 2026-02-22 | バグ修正・テスト強化完了 | 次はPoiyomi/Liltoon UVチャンネル判定 or Unity結合テスト |

---

## メモ

- Poiyomi・LiltoonのUVチャンネル設定プロパティ名は実装時に実際のシェーダーを参照して確認すること
- `TextureReadableHandler` はusingブロックで使うことを前提とした設計
- NDMFのAfterPluginでAAO（com.anatawa12.avatar-optimizer）・TTT（net.rs64.tex-trans-tool）・MA（nadena.dev.modular-avatar）より後に実行
- プロジェクト構成（asmdef）を作成済み：Runtime / Editor / Tests/EditMode
- UVIslandDetectorはUnion-Findアルゴリズムを使用。パス圧縮+ランク付きで効率的
- TextureRebuilderはRenderTextureを使ったリサイズを実装
- OptimizationPipelineは2フェーズ構成：Phase1（解析）→ Phase2（適用）
- materialMap/meshMapをグループ横断で共有することで、同一マテリアルの複数テクスチャプロパティを正しく差し替え
