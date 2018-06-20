【タイトル】 UniteTTC

【登録月日】 2017/03/01
【制 作 者】 Y.Oz
【動作環境】(本文参照の事)
【必須環境】(本文参照の事)
【配布形態】 フリーソフト
【転　　載】 下記の“使用条件”の項をお読みください。
【連 絡 先】 yo1zv2ox3@y4ah5oo6.c7o. jp (数字を抜いてください)
                              (Please remove only the figure.)

.1 ■ 概要

    複数のttfファイルを結合してttcを作成したり、逆にttcを分割してttfにし
    たりするソフトです。

    また、ディレクトリ内に存在する総てのttfファイルをまとめてttc化する
    ツール、allunitettcも同梱しています。

    実行ファイル名は以下のようになっていますので、適宜、使い分けてくださ
    い。

    [Linux64]  unitettc64,     allunitettc64
    [Linux32]  unitettc32,     allunitettc32
    [Win64]    unitettc64.exe, allunitettc64.exe
    [Win32]    unitettc32.exe, allunitettc32.exe

    下記の説明では、必要な実行ファイルの名前を「unitettc99 (Linux)」か
    「unitettc99.exe (Windows)」で説明しています。

    allunitettcは内部で自分と同じバージョンのunitettcを名前を頼りに呼び
    出すため、基本的にリネームはしないでください（特例有り）。

    ※ 自作フォントの場合には問題はありませんが、それ以外の場合には、そ
      のフォントの使用条件をよくお読みになり、許される範囲でお使いくださ
      い。本ソフトを使用することによって生じたいかなることにおいても、作
      者Y.Ozは責任を負いません。

.2 ■ 使用方法

..2-1 ◆ 複数のttfを結合してttcを作る -- make ttc from ttf(s)

    コマンドラインから、以下のようにして下さい。

    $ ./unitettc99 to.ttc from1.ttf from2.ttf ……    [Linux]
    > unitettc99 to.ttc from1.ttf from2.ttf ……      [Windows]

    上記の例は、from1.ttf、from2.ttf、……をまとめてto.ttcにするためのも
    のです。

    あらかじめttc化することを意識して共通部分を多く作ってあるttf(s)であ
    れば、ファイルサイズを小さく抑えることが出来ますが、共通部分が全く無
    い場合には単純に合計サイズ（＋α）になるだけです。

...2-1-1 ▼ ディレクトリ内の総てのttfを結合してttcを作る -- make ttc from all ttf(s)

    ディレクトリ内（新規作成が望ましい）に、unitettc99
    (unitettc99.exe)、allunitettc99 (allunitettc99.exe)とttc化したい複数
    のttfフォントを入れ、allunitettc99を実行してください。成功すれば、カ
    レントディレクトリに「fonts.ttc」というttcフォントファイルが作成され
    ているはずです。適当なファイル名にリネームしてお使いください。

    各々のallunitettcは、以下のルールで実行ファイル本体を探して、適当な
    パラメータを与えて実行させます。

               呼び出し元           第1候補           第2候補
    [Linux64]  allunitettc64     → unitettc64     → unitettc
    [Linux32]  allunitettc32     → unitettc32     → unitettc
    [Win64]    allunitettc64.exe → unitettc64.exe → unitettc.exe
    [Win32]    allunitettc32.exe → unitettc32.exe → unitettc.exe

    第1候補のunitettcが見つからない場合、第2候補の「unitettc (Linux)」か
    「unitettc.exe (Windows)」が無いかどうか調べさせ、あればそれを使うよ
    うにしてあります。ですから、例えば自分が使うのが64bit Windowsで、
    「unitettc64.exe, allunitettc64.exe」しか使用しないのであれば、それ
    らを「unitettc.exe, allunitettc.exe」とリネームして使うと良いでしょ
    う。

..2-2 ◆ ttcを分割してttfにする -- break ttc to ttf(s)

    コマンドラインから、以下のようにして下さい。

    $ ./unitettc99 from.ttc    [Linux]
    > unitettc99 from.ttc      [Windows]

    なお、ttc化されたttfフォントは、元のファイル名が不明なため、ttcフォ
    ントの拡張子以外の部分に連番001、002、003、……を付加し、拡張子をttf
    に変えたものが使われます。上記の例ですと、from001.ttf、from002.ttf、
    from003.ttf……という名前で出力されます。適当なファイル名にリネーム
    してお使い下さい。

    フォント名は元のまま残っていますので、問題はないでしょう。ちなみに、
    ここでのフォント名とはフォントの名前（例: ＭＳ ゴシック）のことで、
    フォントのファイル名（例: msgothic.ttc）とは無関係です。

.3 ■ 使用条件

..3-1 ◆ 著作権と責任

    著作権は“Y.Oz”にあります。著作権・著作者人格権等、放棄しません。

    本ソフトを使用することによって生じたいかなることにおいても、作者Y.Oz
    は責任を負いません。また、総ての環境に於いて使用できることを保証する
    ものでもありません。あらかじめご了承の上、お使いください。本ソフト
    は、今後、予告無く改良/修正が行われることがあります。

..3-2 ◆ お願い

    再配布などを行われる際、メールでご一報ください。

    他のソフトウエア・書籍等にバンドルされて再配布されたものを、更に再配
    布する場合については、この限りではありません。

