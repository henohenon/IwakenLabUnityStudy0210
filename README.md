# IwakenLabUnityStudy01

Iwaken Lab. Unity勉強会プロジェクト

## 概要

このリポジトリはUnityにおけるコード設計を学ぶための勉強会向けプロジェクトです。

### ゲーム内容

「一筆書きで剣を描いて敵を斬る」ゲームのチュートリアル部分です。

1. マウスで一筆書きして剣を描く
2. 描いた剣がそのまま武器になる
3. WASD/矢印キーで移動、スペースキーで振る
4. 落ちてくるボールを斬ってチュートリアルクリア

## 課題の焦点

本課題ではスクリプトのリファクタリングに焦点を当ててください。
特に以下３ファイルを中心としたリファクタリングをしてください。

- `MainFlow.cs` - メインのゲームフロー制御
- `TutorialFlow.cs` - チュートリアルの流れ制御
- `WeaponDrawSequencer.cs` - 武器描画のシーケンス制御

## ヒント

UniTaskを利用することで、コードを読みやすくすることが出来ます。

## プロジェクト構造

```
Assets/
├── Scenes/
│   └── Main.unity          # メインシーン
└── Scripts/
    ├── MainFlow.cs         # ゲーム全体のフロー管理
    ├── TutorialFlow.cs     # チュートリアルのフロー管理
    ├── WeaponDrawSequencer.cs # 武器描画のシーケンス制御
    ├── DrawWeapon.cs       # 線の描画機能
    ├── BattleWeapon.cs     # 武器の動作とヒット判定
    ├── MouseInputObserver.cs # マウス入力の監視
    ├── FallingBall.cs      # 落下するボール
    └── ColliderForwarder.cs # コライダーイベント転送
```

## 参加方法

1. このリポジトリをフォーク
2. 新しいブランチを作成 (`git checkout -b feature/your-refactoring`)
3. 変更をコミット
4. プルリクエストを作成

## 動作環境

- Unity 6000.3.2f1
