# Design Philosophy

## Background

開発や勉強を始めるたびに、エディタ、ブラウザ、フォルダ、ローカルサーバーなどを個別に起動する手間を減らす。

## Purpose

第一の目的は一般公開や販売ではなく、作者自身が日常的に使いたい作業開始体験を作ることです。

## Product Concept

「いつもの作業環境を、ワンクリックで。」

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

## Workspace / Launch Item

Workspace / Group は複数の Launch Item を持つ作業単位の概念です。名称は仮称であり、既存コードの調査なしに全面変更しません。

## MVP Scope

候補は、Workspace管理、Launch Item管理、アプリ・ファイル・フォルダ・URL・Commandの起動、Working Directory、個別／一括起動、設定保存です。これは候補であり、現時点の実装済み機能を意味しません。

## Non-Goals

今回、UI全面刷新、フレームワーク変更、大規模リファクタリング、MUGEN専用機能、自動更新、将来アイデアの先行実装は行いません。

## Future Ideas

起動順序、Delay、起動済み判定、サーバーReady待ち、Workspace終了、ショートカット、自動更新などは、実際に必要になった時点で検討します。

## MUGEN Extension

MUGEN固有機能は将来の拡張として、汎用Launcher Coreと分離します。MUGENを知らないユーザーでも通常のWindowsランチャーとして利用できることを維持します。
