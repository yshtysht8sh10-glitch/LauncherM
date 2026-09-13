# Architecture

## Implemented：現在の構造

- `LauncherM.sln` / `LauncherM/LauncherM.csproj`：WPF、.NET Framework 4.8。
- `App.xaml`のStartupUriは`01_UI/WorkspaceWindow.xaml`。Workspace選択、中央タイル、右側詳細、編集ダイアログをcode-behindで管理。
- `03_Domain/WorkspaceModels.cs`：WorkspaceDocument → Workspace → LaunchItem。概念を表すための新しいクラス階層は導入していません。
- `02_Application/LaunchService.cs`：個別起動とWorkspace一括起動。ProcessStartInfoの生成、起動したProcessの返却、起動失敗の集約を担当。
- `02_Application/WorkspaceSession.cs`：WorkspaceごとにLauncherMが取得した安全に識別可能なProcessをメモリ上で追跡し、段階的に終了。
- `04_Infrastructure/WorkspaceRepository.cs`：DataContractJsonSerializerで作業ディレクトリの`workspaces.json`を読み書き。Version = 1。
- `BrowserProfiles.cs`：標準Local Stateのprofile.info_cacheから表示名とDirectoryのみを読み、Chrome/Edgeの候補を表示。手入力も可能。FirefoxはProfile名。
- `WebsiteIconCache.cs`：URL faviconの取得・ローカルキャッシュ。IconPathは自動取得したfaviconと手動指定の双方に使用。
- `LaunchItemNameResolver.cs`：保存時にNameが空欄の場合だけ、Folder末尾名、拡張子付きFile名、exeのFileDescription / ProductName、URI path segment、Web hostnameの順でTarget主体の表示名を解決し、Opener、`LaunchItem`へフォールバックする。名前取得専用のネットワークアクセスは行わない。
- `IconResolver.cs`：LaunchItemの表示アイコンを一元解決。手動IconPath、専用Opener、Target、TargetType fallbackの順に解決する。実在ファイル／exeとフォルダはWindows Shell、HTTP(S)は既存faviconキャッシュ、その他の絶対URIはSchemeとしてWindows Protocol Associationを参照する。URI SchemeはUserChoiceのProgId、Scheme登録、AppModel RepositoryのURL関連付けとAppxManifestの順に調べ、Desktop / MSIX双方のアイコンを静的に取得する。解決のためにTargetを起動しない。
- 旧`0010_MainWindow`、設定画面、de.txt / se.txt関連コードは残存。MUGEN固有処理は旧UI側に留め、汎用Coreへ移しません。

## 中核概念と既存モデルの対応

設計思想の正本は[design-philosophy.md](../design-philosophy.md)です。
Workspaceは作業環境、LaunchItemはその構成要素。Target / Opener / Destinationは独立した役割で、それぞれ必要に応じたContextを持つ概念です。

| 概念 | 現在のフィールド / 処理 | 実装範囲 |
| --- | --- | --- |
| Target | Type、Target | Application / Folder / File / Url / Command。Project専用型なし |
| Opener | Browser、Opener、OpenerPath、LaunchServiceのType分岐 | URLはDefault / Chrome / Edge / Firefox。FolderはExplorer / Visual Studio Code / Codex / Default / Application |
| Opener Context | BrowserProfile | Chrome/EdgeのDirectory ID、FirefoxのProfile名。サービス側Identityではない |
| Opener Context | Arguments、WorkingDirectory | ArgumentsはApplication / Command、WorkingDirectoryはApplicationのみ使用 |
| Destination | 起動方法、BrowserWindowGroup、OpenerWindowMode | 明示指定なしはOS / アプリ任せ。Chrome/Edgeの一括起動、VS CodeのDefault / New Window / Reuse Window、CodexのNew Thread |
| Destination Context | BrowserWindowGroup | 同一Workspace内のBrowser・Profile・Group単位。既存Window IDやブラウザの色付きTab Groupではない |
| Target Context | 対応フィールドなし | 期待するサービスアカウント等の概念のみ |
| 表示・識別 | Id、Name、IconPath | 3要素や認証Contextには含めない |

表示アイコンの解決順は、ユーザー明示IconPath、Folderの専用Opener、実在するTargetファイル／フォルダまたはHTTP(S) favicon、TargetType fallback。Folder + Codexはcodex SchemeのDesktop登録を先に参照し、取得できない場合はAppModel RepositoryのURL関連付けから該当MSIX packageを特定してAppxManifestのVisualElements logo（取得不可時はmanifestのExecutable）を使う。専用Openerを解決できない場合はTargetへ安全にフォールバックする。`IconPathIsAutomatic`で自動キャッシュと手動指定を区別し、旧データの管理下favicon pathも自動と推定する。Schemeごとの成功・失敗結果はプロセス内でキャッシュする。

表示名は主にTarget（WHAT）、表示アイコンは主にOpener（WITH）を表す。編集ダイアログは既存Nameをそのまま維持し、空欄で保存した新規・既存項目にだけ名前解決を適用するため、既存JSONの読込だけではNameを変更しない。

ApplicationはTarget自身を実行、FileはOS関連付け、Commandはcmd.exe /k。FolderはOpener未指定を含むExplorer、VS Code、OS既定、任意Applicationを切り替えます。
Folder + Codexは`CodexOpener`がFolderの存在とcodex Scheme登録を確認し、`Uri.EscapeDataString`で絶対パスをエンコードして`codex://threads/new?path=...`を生成し、`UseShellExecute = true`でWindows Shellへ委譲します。
これは現在の実装制約であり、TargetとOpenerが永久に固定される設計ではありません。
一括起動は未指定グループを個別起動し、Chrome/Edgeの明示グループに`--new-window`と複数URLを渡します。
個別起動ではWindow Groupを適用しません。Default / Firefoxで保存されたGroupは起動に使いません。
ブラウザ検出は両Registry ViewのApp Pathsとインストールパスを参照します。VS CodeはPATH上のcode/code.exe、両Registry ViewのApp Paths、LocalApplicationData / Program Filesの標準配置を順に参照します。

## Contextと実行保証

Target IdentityとOpener Identityを共通Userへ統合しません。ブラウザProfileを選んでもサービスのアカウント選択は保証されません。
Contextは実行パラメータにできるものと期待メタ情報を区別します。現在のモデルにはTarget Accountや汎用Contextの永続化はありません。
LauncherMはPassword / Cookie / Session / OAuth Tokenを扱わず、認証とログイン状態はブラウザ・サービスに委ねます。

## GUI

HeaderはLauncherMタイトルの右側に、高さを揃えたアイコン上・説明文下の設定ボタンと表示切替ボタンを並べます。
LaunchItem一覧上部の操作行は、横幅が不足した場合だけ横スクロールバーを表示します。
タブとカードのドラッグで表示した挿入用の隙間は、親コンテナの透明な背景をヒットテスト面として使い、同じ挿入位置としてドロップを受け付けます。

詳細ペインと既存編集ダイアログを3セクションで表示。URLではBrowser選択、FolderではExplorer / Visual Studio Code / Codex / Default / Applicationを選択します。任意Applicationではexeパス、VS CodeではDefault / New Window / Reuse Window、CodexではNew Threadを表示します。明示BrowserだけProfile、Chrome/EdgeだけGroup、Application / Commandだけ引数、Applicationだけ保存済みWorkingDirectoryを表示します。
WorkingDirectoryは読み取り専用。Target Contextや未対応Destinationの入力欄は作りません。
灰色Header、歯車、ダークUI、Workspace選択・すべて起動は維持。Headerの表示切替ボタンで、選択Workspaceの3ペイン表示と全Workspace表示を切り替えます。全Workspace表示では左のWorkspace操作ペインを隠し、中央へWorkspace名、Workspace単位の一括起動・終了ボタン、そのWorkspaceのカードを順番に表示します。Workspace選択UIはタブへ集約し、既存ComboBoxは単一選択状態を保持する非表示コントロールとして使用します。タブ列末尾の＋ボタンからWorkspaceを追加でき、ドラッグ＆ドロップ時はWorkspaceDocumentのリスト順を変更して即保存します。左ペインのWorkspace名は通常はTextBlockで表示し、クリック時だけ同じ位置のTextBoxへ切り替えて編集します。Enterまたは編集欄の外側のクリックで保存し、Escでキャンセルします。中央ペインにはURLタイプを初期選択したリンク追加ボタンがあり、LaunchItemカードもドラッグ＆ドロップでリスト順を変更して即保存します。カード領域はScrollViewerの表示幅に追従し、通常表示・全Workspace表示ともカードを画面幅で折り返します。Ctrl＋マウスホイールはカードとアイコンだけを60～180%で拡大縮小し、文字サイズは変更しません。通常のマウスホイールは一覧をスクロールします。タブとカードのドラッグ中は半透明のスナップショットをAdornerで追従表示し、挿入先の左へ置く場合は対象を右へ、右へ置く場合は対象を左へ退避させて隙間を示します。左Workspace操作・中央LaunchItem一覧・右LaunchItem詳細の3ペインを2本のGridSplitterで区切り、左右幅はユーザー設定へ保存します。一覧と詳細の縦横スクロールバーは各方向で必要な場合だけ表示します。
設定ウィンドウはWindowsのインストール済みフォント、全体文字サイズ倍率、テーマカラーを選択し、ユーザー設定へ保存します。設定内容だけをスクロール領域に置き、縦スクロールバーは画面端、設定内容は右余白付きで配置します。通常のComboBoxは選択肢を確認できる固定幅とし、適用・OK・キャンセルはウィンドウ下部へ固定表示します。ダーク背景で使うCheckBoxは明るい文字色を明示します。Workspace画面は設定変更イベントを受けてフォント・倍率・背景・前景・アクセントを再適用します。
起動ショートカットを有効にすると、設定した修飾キーと英字／FunctionキーをHotkeyに持つ`LauncherM.lnk`をユーザーのスタートメニュープログラムへ作成します。Windows Shellがショートカットキーを処理するため、LauncherMが終了している状態から起動できます。無効化時はリンクを削除し、有効時はアプリ起動時に現在のexeパスへ更新します。
設定画面のインポート／エクスポートは、Version付きJSON 1ファイルへWorkspaceDocumentと全ユーザー設定（ペイン幅、フォント、文字倍率、テーマ、カード倍率、起動ショートカット）を保存します。インポートはVersion、Workspace構造、設定値を検証し、確認後に現在のWorkspaceと設定を置き換えて画面へ即時反映します。

## 保存互換性と既存制約

Opener / OpenerPath / OpenerWindowModeを省略可能なDataMemberとして追加し、JSON Versionは1を維持します。既存Folder JSONでOpenerが欠落する場合はExplorerへフォールバックします。Browser / Profile / Group欠落の旧JSONはOS既定起動へフォールバックします。
JSONがない場合のみde.txtを読み、launcher 0..3を4 Workspaceへ、タイトルとパスをApplicationへ移行してJSON保存します。元のde.txt / se.txtは上書きしません。
現行移行は旧icon / memo / visual / PathFileSelectをJSONへ移さず、表示順フィールドでも並べ替えません。バックアップ・atomic write・破損JSONからの復旧は未実装です。

## Workspace Lifecycle

WorkspaceはLaunchItem集合であると同時に、Launch、起動Resourceの追跡、Closeを持つ実行中セッションです。セッションは永続化せずLauncherMプロセス内だけで管理します。ApplicationとCommandで`Process.Start`が返したProcessだけを追跡し、Close時は`CloseMainWindow`、待機、必要時の強制終了の順で処理します。Commandは追跡PIDを起点に`taskkill /PID /T /F`で子プロセスも停止します。

URL、Folder、Fileは、既存ブラウザ・Explorer・VS Code・Codex・OS関連付け先へ合流する可能性があるためClose対象外です。Codexは既存プロセスを再利用し得るため、LauncherMが開いたWindowを安全に識別できない限り終了しません。Chrome/EdgeのWindow GroupもCLIによる新規Window要求であり、安全なWindow Identityを取得できないため追跡終了しません。

## Planned / Future（未実装）

Planned：File / URLを含む全Targetの汎用Opener拡張、Explorer Tab destination、Monitor・座標指定は次期検討対象で、実装順や時期は未確定。
Future：独立Target Contextのメタ情報、Service-specific Target Identity Adapter、Destination Context拡張、仮想デスクトップ、WSL / VM / Remote環境。
いずれも現在の起動保証ではなく、新しいGeneric Frameworkを必要とする前提にしません。
