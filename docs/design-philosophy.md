# Design Philosophy

## Background

開発や勉強を始めるたびに、エディタ、ブラウザ、フォルダ、ローカルサーバーなどを個別に起動する手間を減らす。

## Purpose

第一の目的は一般公開や販売ではなく、作者自身が日常的に使いたい作業開始体験を作ることです。

## Product Concept

「いつもの作業環境を、ワンクリックで。」

起動と対になる操作として「使い終わった作業環境を、ワンクリックで片付ける。」ことも重視します。

単一アプリではなく、「WebMUGEN開発」「勉強」などの作業単位を起動するWindowsランチャーを目指します。

## Core Principles

- 自分の困りごとの解決を最優先する
- 作業開始までの操作量を減らす
- アプリ、ファイル、フォルダ、URL、コマンドを可能な範囲で統一的に扱う
- 一括起動と個別起動を両立する
- MVPを小さく保ち、実際の利用で必要になった機能だけ追加する
- 既存実装を理解せず全面リライトしない
- UIの豪華さより実用性を優先する
- 汎用Launcher CoreとMUGEN Extensionを分離する
- WorkspaceをLaunchItem集合であると同時に、起動・追跡・終了のライフサイクルを持つ作業セッションとして扱う

## 中核モデル：何を / 何で / どこに

LauncherMは単なるアプリランチャーではなく、「いつもの作業環境を、ワンクリックで。」を実現するためのツールです。
Workspaceは作業環境の単位であり、LaunchItemはその環境を構成する1要素です。
各LaunchItemを、独立した3要素で考えます。

- **Target（何を開くか）**：最終的に開きたいURL、ファイル、フォルダ、アプリ、コマンド、プロジェクト、その他のリソース。
- **Opener（何で開くか）**：OS既定、ブラウザ、Explorer、エディタ、任意exe、shellなど、対象を開くアプリ・仕組み。
- **Destination（どこに開くか）**：起動結果を配置する場所・環境・コンテキスト。画面座標だけでなく、ウィンドウ、タブ、仮想デスクトップや実行環境も概念上含みます。

URLだからChrome、フォルダだからExplorerという固定関係を中核思想にしません。
同じmemo.txtをNotepadでもVS Codeでも、同じURLをChromeでもEdgeでも開ける方向を目指します。
現在はURLのBrowser選択のみが明示的なOpener選択であり、他の種類の任意Opener選択は未実装です。

## 独立したContextとIdentity

各要素は必要に応じて独立したContextを持てます。共通のUserフィールドへまとめません。
概念モデルは `LaunchItem = Target(Value, Context) + Opener(Value, Context) + Destination(Value, Context)` ですが、これをそのままクラス階層や保存形式へ強制しません。

例えば、Target = ChatGPT (`https://chatgpt.com`)、Target Context = 個人Googleアカウント、
Opener = Chrome、Opener Context = 個人開発Profile、Destination = Browser Window、
Destination Context = Mainという組合せです。
**Target Identity（サービス側で誰として使いたいか）とOpener Identity（アプリ側のどのProfileで使うか）は別物**です。
Chrome Profile Directoryの `Default` / `Profile 1` / `Profile 2` はOpener Contextであり、ChatGPTアカウントそのものではありません。

Contextを持てることと、LauncherMがそれを強制できることは同義ではありません。
Target Contextは、事前に準備したBrowser Profile、サービスが公開するURLの仕組み、将来のService-specific Adapterで実現できる場合だけ実行へ反映します。
実現できない期待Contextはメタ情報として扱える設計とします。現在、汎用Target Contextの保存・編集は未実装です。

LauncherMはPassword、Cookie、Session Token、OAuth Token、Google / Microsoft / GitHub等のPasswordを管理・保存・操作しません。
Webサービスへの自動ログインは目的に含めず、認証状態はブラウザProfileと各サービスへ任せます。

Destination Contextの例はBrowser Window Group = Main、Explorer Group = Project、
Virtual Environment = WSL UbuntuとそのLinux Userです。Monitor / X / Y / Width / Height / Maximized、既存ウィンドウのタブ、仮想デスクトップ、VM / Remote等は将来の概念であり、現時点の実装を意味しません。
WorkingDirectoryはプロセスが相対パスを解決するOpener Contextであり、ウィンドウの配置先ではありません。

## GUIと実装の原則

右側詳細・編集UIは「何を開く？」「何で開く？」「どこに開く？」を中心に整理します。
Workspaceの選択UIは横スクロール可能なタブへ集約し、タブのドラッグ＆ドロップで保存順を変更できます。内部の選択状態は単一に保ちます。メイン領域は左からWorkspace操作、LaunchItem一覧、LaunchItem詳細の3ペインとし、標準Splitterで幅を調整可能にします。
Contextは必要な場合だけ表示し、未対応機能を操作可能に見せません。
既存の灰色・ダークグレー・青アクセント、Header、歯車、LaunchItem一覧、右側詳細、Workspace選択、一括起動を維持します。操作経路が重複したHeaderボタンは、対応するタブ・ペイン操作へ集約します。
保存互換性とTime to Usableを優先し、Generic Context Framework、Plugin System、DI全面導入、Dynamic Property System、JSON Schema駆動UI、Workflow Engineを先行実装しません。

## MVP Scope

候補は、Workspace管理、Launch Item管理、アプリ・ファイル・フォルダ・URL・Commandの起動、Working Directory、個別／一括起動、設定保存です。これは候補であり、現時点の実装済み機能を意味しません。

## Non-Goals

今回、UI全面刷新、フレームワーク変更、大規模リファクタリング、MUGEN専用機能、自動更新、将来アイデアの先行実装は行いません。

## Future Ideas

起動順序、Delay、起動済み判定、サーバーReady待ち、ショートカット、自動更新などは、実際に必要になった時点で検討します。

## MUGEN Extension

MUGEN固有機能は将来の拡張として、汎用Launcher Coreと分離します。MUGENを知らないユーザーでも通常のWindowsランチャーとして利用できることを維持します。

## Time to Usable

LauncherMは複数の個人開発や作業環境を高速に切り替えるための基盤です。GUIの完成度や枝葉の機能より、実際の作業で利用可能になるまでの時間を優先します。拡張性は維持しますが、将来機能を先回りして実装しません。
