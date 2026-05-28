# PLAN: Turnkey C# SDK ↔ 公式実装 同等性検証プラン (v3)

**Status**: Draft v3 — awaiting Codex plan review (target: review-clean).

## v3 changes vs v2 (Codex r1 後の修正)

| Codex r1 指摘 | v3 の修正 |
|---|---|
| `tests/Fixtures/*.json` の link が壊れている (B-行 broken link) | 全箇所を `tests/Fixtures/{encoding,crypto,api-key-stamper}/*.json` に修正 |
| **HPKE/QOS 混同 (致命)**: v2 は「Turnkey HPKE は RFC 9180 でない独自構成」と書いたが、実は SDK `hpkeEncrypt/hpkeDecrypt` ([crypto.ts:88-138](../codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts)) は **JSdoc で RFC 9180 を明示**しており構成も HPKE 風。QOS `quorumKeyEncrypt` ([crypto.ts:219+](../codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts)) は別物で C# **未ポート** | **canonical wording を Section 2 に統一**: (a) C# が port しているのは SDK HPKE (RFC 9180 を意図) のみ、(b) QOS は scope 外、(c) SDK HPKE が RFC 9180 と byte-equal かは別 PR で評価する step を新設 (PR-7a)。Tier-3 RFC 9180 取り込みは **PR-7a の評価結果次第** で確定する条件付き |
| Tier-2 の決定論性が崩れる: SDK HPKE は ephemeral key 生成、QOS も nonce 生成 → encrypt 出力 bytes は毎回違う | **encrypt の byte snapshot は撤回**。代わりに (a) **SDK 生成済みの round-trip 等価** (上流 Node で encrypt した output を **fixture として固定**しておき、C# で **decrypt して plaintext 等価**を確認)、(b) encrypt 出力構造 (lengths, header bytes) の構造的 assertion、(c) HPKE が ephemeral key を **外部 inject できる private API** が無いか追加調査 (調査結果次第で encrypt の byte snapshot を限定的に復活) |
| @noble/curves v1.3.0 low-S deterministic が load-bearing だが pinned snapshot だけでは証明できない | PR-4 で **transitive deps を exact lock** し、**generator 起動時に既知 vector で `purejs.sign(message, privateKey)` を assert** する pre-flight test を追加。失敗したら fixture 生成中断 |
| monthly drift workflow が unbounded で月毎に issue 量産 | **idempotency rule** を Section 6.4 に追加: `gh issue list --label upstream-drift --json number --jq '.[0].number'` で既存 open issue を 1 つ取得、あれば update、無ければ create。Workflow yaml 例も入れる |
| PR-4 が `tests/turnkey-sdk-csharp.Tests.csproj` の Generators 排除を含んでいない | PR-4 内容に「`<None Remove="Fixtures/Generators/**" />` と `<Content Remove="Fixtures/Generators/**" />` を csproj に追加」を明記 |
| PR-3 規模が plan の表現より大きい | Section 6.1 に **欠落候補テスト名の concrete list** を追加 (Codex r1 列挙分を全部書き出す) |
| Tier-3 で Ed25519 / secp256k1 / Base58Check Bitcoin を忘れている | Section 6.3 に Ed25519 RFC 8032、secp256k1 SEC2、Base58Check Bitcoin double-SHA256 を追加 |
| PR-11 fixture-only seeding は consumer 無し golden data | PR-11 で **`ProofFixtureProvenanceTests` を必ず一緒に追加**: JSON parse、`_provenance` schema 検証、上流 sha256 一致、を assert。verify 実装本体は scope 外を維持 |
| Phase E は `PR-12 (docs)` 誤記 (PR-12 は drift CI) | `PR-14 (docs)` に修正、PR-14 の依存に PR-12/PR-13 を追加 |
| HPKE 説明が Section 2/6.3/10/12 で重複 | Section 2 を canonical source とし、他 3 箇所は `(Section 2 参照)` リンクに置換 |
| "README CI で更新" / "backend accepts means correct" が測定不能 | "README updated and committed in PR-14" / "wire-format byte-equal in Tier-2 fixtures" に置換 |
| 「公式他言語 SDK の test fixture」未検証 (G の unresolved) | Phase A の作業に **`peak/` リポジトリの grep で proof 呼出し有無を確認**する step を明示。他言語 SDK fixture は本プラン scope 外として明記 |

## v2 changes vs v1 (調査後の修正、参考)

| v1 の誤り | v2 の修正 |
|---|---|
| 「Tier 3 で HPKE RFC 9180 vectors を取り込む」 | (v3 で再修正) v2 では「Turnkey HPKE は RFC 9180 でない独自方式」とした上で RFC 9180 vectors を撤回したが、これも誤り。v3 で再評価 |
| proof.ts は scope 外で確定 | proof-tests.ts に**実 enclave 由来の `v1AppProof` / `v1BootProof` JSON ゴールデン**があり、cross-language 借用に最適。**proof ポート要否は別計画**を維持しつつ、本プランで `tests/Fixtures/proofs/` を Tier-1 として置く準備 (verify 実装は別計画) を含める |
| 「他言語 SDK に cross-language vector があるかもしれない」 | 公式 cross-language fixture セットは**存在しない**。代わりに `mockSenderPrivateKey=67ee05fc…` 等のハードコード値が `tkhq/sdk` / `swift-sdk` / `dart-sdk` で**完全一致でコピー**。実質的 reference vector として **Tier-1 で借用** |
| Tier-4 (live) | 維持。live e2e は `tkhq/python-sdk` のみが CI に組み込んでおり、他 SDK は mock 中心 |

## 1. Intent

`KyuzanInc.Turnkey.Sdk` が **Turnkey 公式 TypeScript SDK
(@turnkey/{crypto,http,api-key-stamper,encoding} の peak-pinned 版)
と機能的に等価** であることを、後付け的なレビューではなく
**機械検証可能な公式由来テストアセット** で恒常的に担保する。

本プランは、既存の Codex 3-round 行ごと等価レビュー
(`codex-crypto-reviews/`) と既存の Tier-1 fixture
(`tests/Fixtures/{encoding,crypto,api-key-stamper}/*.json`) を
**置き換えるものではない**。
それらが「**人が読んで一致を確認した**」レベルの保証であるのに対し、
本プランは「**入力に対する観測可能な出力を機械で照合し、
上流が変わったら自動で気付く**」レベルの保証を上乗せする。

「観測可能な出力」は具体的には以下のいずれか:

- 決定論経路: 入力が同じなら byte 等価 (例: stamp の DER 署名は
  `purejs` runtime で RFC 6979 deterministic-k、credential-bundle decrypt は
  入力 ciphertext + receiver private key で plaintext が一意)。
- 非決定論経路: ephemeral key/nonce を生成するため byte 等価は取れない
  ものに対しては (a) decrypt round-trip での plaintext 等価、
  (b) 構造的 assertion (フィールド長、ヘッダ、JSON キー順、署名検証成功)。

## 2. Goals (must-have)

1. **Coverage matrix**: 公式 `__tests__/*.ts` の各 `test()` ブロックが
   C# のどの `[Fact]` / `[Theory]` メソッドに対応するかを 1:1 で
   ドキュメント化する。欠落は CI で fail する。
2. **Byte-equal fixtures**: 公式 npm パッケージを `npm install` した
   Node 環境で生成した実バイト (stamp, signed-request body,
   credential-bundle decrypt 等) を `tests/Fixtures/**` に固定し、
   C# が同一入力で同一バイトを生成することを assert する。
3. **Standard test vectors (primitives + 条件付きで HPKE)**:
   HKDF (RFC 5869)、ECDSA P-256 deterministic-k (RFC 6979)、
   AES-GCM-256 (NIST CAVP)、SHA-256 (NIST SHS)、HMAC-SHA256 (RFC 4231)、
   Ed25519 (RFC 8032、SOLANA export 用) の **個別 primitive** で C# 実装を検証する。
   secp256k1 SEC2 / Base58Check Bitcoin double-SHA256 もここに含める。
   **HPKE 全体 (RFC 9180) の取り込みは条件付き**:
   PR-7a で 「pinned `@turnkey/crypto@2.8.8` の `hpkeEncrypt` が
   RFC 9180 base mode (DHKEM(P-256,HKDF-SHA256) + HKDF-SHA256 + AES-256-GCM)
   と byte-equal か」を `__tests__/crypto-test.ts` の HPKE 経路を
   実 Node で動かして固定 ephemeral key を inject (もし可能なら) し評価する。
   評価結果が **byte-equal なら RFC 9180 Appendix A vectors を取り込み**、
   **byte-equal でなければ取り込まない** (Tier-2 で SDK 生成 bytes を信頼)。
   ※ JSdoc は RFC 9180 を明示するが ([crypto.ts:88-91](../codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts))、
   実装の suite_id/labels と RFC 9180 base mode の literal が**完全一致するかは未確認**。
   QOS `quorumKeyEncrypt` は C# **未ポート**であり本プラン scope 外
   (Section 2 canonical 説明を参照)。
4. **Drift detection**: 公式 `__tests__/` の sha256 を pin し、
   月次 cron で再取得して差分があれば **upstream-drift というラベルの
   single open issue を idempotent に update/create する**
   (Section 6.4 / Section 8 / Section 9 で一貫した定義)。
   この workflow は **main CI gate ではない** (= PR をブロックしない)。
   issue が立ったら人手で再 pin と対応 C# テスト更新の PR を立てる。
5. **不確定要素の明示** (PR-14 README "Verification posture" 章で
   以下のチェックリストを必ず文書化する):
   - [ ] 公式 P-256 ECDSA は `nodecrypto.ts` / `webcrypto.ts` ランタイムでは
     non-deterministic。C# 側は **`purejs` ランタイム相当 (RFC 6979 + low-S) のみ
     ports** している事実
     (既に `ApiKeyStamper.cs-codex-findings-reconciliation.md` E1/E2 で合意済み)。
   - [ ] SDK HPKE と RFC 9180 base mode の byte-equality は **PR-7a の判定結果**を
     review posture に明記 (byte-equal / non-equal / inconclusive)。
   - [ ] **未ポート API リスト** (`src/Crypto.cs:25-30` の "Out of scope" コメントと一致):
     `hpkeAuthEncrypt`, `quorumKeyEncrypt`, `extractPrivateKeyFromPKCS8Bytes`,
     `fromDerSignature`, `toDerSignature`, `verifyStampSignature`,
     `verifyRequestStamp`, `encryptWalletToBundle`, `encryptToEnclave`,
     `encryptOauth2ClientSecret`, `encryptOnRampSecret`, `proof.ts`。
   - [ ] live e2e backend 経路は opt-in (`TURNKEY_TEST_ORG_API_KEY` 環境変数) のみ。
   - [ ] cross-language 借用値 (`mockSenderPrivateKey`, `mockCredentialBundle`, etc.)
     の上流 git revision pin。

### Non-goals

- 既存の `codex-crypto-reviews/` SOP の置き換え (補強のみ)。
- nuget.org への公開条件の変更。
- 新規機能 (proof.ts 完全移植、QOS `quorumKeyEncrypt` ポートなど) の追加。
  **proof モジュールに対するポート方針は本プランの第 12 章「未スコープ依存」で別計画とする。**
- 公式 Backend に対する live e2e の必須化 (live 経路は opt-in のまま)。

### 2.1. 暗号構成の canonical 用語 (重要)

以下の用語は本プラン全体で **canonical** にこの定義で使う。他章 (3, 4, 6.3, 9, 10, 12) は
**この章を再記述せず参照する**。

- **SDK HPKE**: pinned `@turnkey/crypto@2.8.8` の `hpkeEncrypt` / `hpkeDecrypt` /
  `hpkeAuthEncrypt` 経路。([crypto.ts:88-203](../codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts))
  - JSdoc が RFC 9180 を明示。
  - `SUITE_ID_1 = "KEM\0\x10"` (DHKEM(P-256, HKDF-SHA256))、
    `SUITE_ID_2 = "HPKE\0\x10\0\x01\0\x02"` (kem_id=0x0010, kdf_id=0x0001, aead_id=0x0002)
    の literal を使う ([Crypto.cs:77-79](../src/Crypto.cs))。
  - これらの suite_id literal は **RFC 9180 base mode の値と一致**:
    `kem_id=0x0010 = DHKEM(P-256, HKDF-SHA256)`,
    `kdf_id=0x0001 = HKDF-SHA256`, `aead_id=0x0002 = AES-256-GCM`。
    `extractAndExpand(..., 32)` で 32-byte AES key、`extractAndExpand(..., 12)` で
    12-byte IV を派生するのも AES-256-GCM の使用と整合。
  - ただし上記 suite_id 一致は「実装が RFC 9180 と完全互換」を即座に意味しない。
    HPKE label の literal バイト列 (`HPKE-v1`, `eae_prk`, `secret`, `shared_secret`)
    も RFC 9180 と完全一致するかは PR-7a で確認する。
  - **C# `Turnkey.Crypto.HpkeEncrypt` / `HpkeDecrypt` がポートしているのはこの経路のみ**。
    ([Crypto.cs:404-456](../src/Crypto.cs))。
- **QOS encryption**: 同じファイルの `quorumKeyEncrypt` ([crypto.ts:219+](../codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts))。
  - jsdoc に「Algorithm originally implemented in qos」と明示。
  - `QOS_ENCRYPTION_HMAC_MESSAGE = "qos_encryption_hmac_message"`、
    SHA-512 HMAC + AES-GCM-256、borsh serialization。
  - **C# **未ポート**。本プラン scope 外**。
- **RFC 9180**: IETF Hybrid Public Key Encryption 仕様。SDK HPKE と完全一致するかは
  PR-7a で評価する別タスク。「SDK HPKE = RFC 9180 と等価」とは現時点で言わない。

### 2.2. C# が ports している upstream 経路の一覧 (Non-Goal 確認用)

| C# public API | upstream 関数 | upstream 経路 | C# port 状況 |
|---|---|---|---|
| `Turnkey.Crypto.HpkeEncrypt` | `hpkeEncrypt` | SDK HPKE (RFC 9180 を意図) | **済** |
| `Turnkey.Crypto.HpkeDecrypt` | `hpkeDecrypt` | SDK HPKE | **済** |
| `Turnkey.Crypto.DecryptCredentialBundle` | `decryptCredentialBundle` | SDK HPKE + base58check + AES-GCM | **済** |
| `Turnkey.Crypto.DecryptExportBundle` | `decryptExportBundle` | SDK HPKE + mnemonic/PKCS#8 | **済** |
| `Turnkey.Crypto.EncryptPrivateKeyToBundle` | `encryptPrivateKeyToBundle` | SDK HPKE + base58check | **済** |
| (なし) | `quorumKeyEncrypt` (QOS) | QOS 独自 | **未ポート (scope 外)** |
| (なし) | `verify` / `verifyAppProofSignature` (proof) | P-256/P-384 + COSE/X.509 | **未ポート (proof scope 外)** |

## 3. 現状アセスメント (Where we are)

| 項目 | 既存状態 | 備考 |
|---|---|---|
| Upstream pinned snapshots | 4 パッケージ (`crypto@2.8.8`, `http@3.16.0`, `api-key-stamper@0.5.0`, `encoding@0.6.0`) を `codex-crypto-reviews/upstream-snapshots/` に **extracted npm tarball contents + 記録された tarball sha256** で固定 (`.tgz` ファイルは worktree にコミットしない方針、`tarball-checksums.txt` で sha256 のみ pin) | [turnkey-source-pins.md](../codex-crypto-reviews/turnkey-source-pins.md) |
| Tier-1 hand-transcribed fixtures | `tests/Fixtures/{encoding,crypto,api-key-stamper}/turnkey-*-vectors.json` の 3 ファイル。`_provenance` メタデータと `_source_line` 付き | `tests/Fixtures/http/` は**未整備** |
| 3-round Codex review | 6 ファイル全部 review-clean、`FINAL-INTEGRATED-REVIEW-20260523.md` で v0.1.0-alpha GO 済み | [FINAL-INTEGRATED-REVIEW-20260523.md](../codex-crypto-reviews/FINAL-INTEGRATED-REVIEW-20260523.md) |
| C# test 数 (合計約 113 `[Fact]/[Theory]`) | Encoding 48 / Crypto ~45 / Stamper 10 / Http 11 | 公式 (~30 `test()`) より細かいが、対応マッピングは未文書化 |
| Drift 検知 | `tarball-checksums.txt` のみ。`__tests__/*.ts` 自体の sha256 pin は無し | 公式テスト更新時に気付かないリスク |
| Node 生成 byte fixture | **なし** (`tests/Fixtures/Generators/` 未作成) | `Tier-1 README.md` に「node-generated 予定」と明記済み |
| RFC / NIST vectors | HKDF RFC 5869 A1-A3 のみ (`tests/CryptoTests.cs:91/111/149`) | HPKE 9180、ECDSA 6979 単独、AES-GCM CAVP は未取り込み |

### 公式テストの実状 (2026-05-27 時点・調査済み)

公式 `__tests__/*.ts` は **`__fixtures__/api-key.{private,public,public.pem}`**
というローカルファイルを `readFixture()` で読む構造で、
他言語に直接持ち込める JSON 形式の test vector dataset は**公開されていない**。
npm tarball にはテストもフィクスチャも含まれないため、
**GitHub の `tkhq/sdk` リポジトリの該当タグからコピーする**のが唯一の入口。
これは既存の `upstream-snapshots/.../ts-source/__fixtures__/` で対応済み。

公式テストの assertion 強度は (1) DER ECDSA 検証 + (2) スキーム文字列 +
(3) JSON キー順 + (4) publicKey 等価 のみ。**バイト等価 snapshot は無い**
(P-256 `nodecrypto.ts` は random-k なため bytes が毎回変わる)。
したがって C# 側で「`purejs` 等価のバイト」を追加で検証することは、
**公式テストより厳しい superset の検証**になる (=価値が高い)。

#### 公式他言語 SDK の検証スタイル (調査結果)

- **cross-language fixture セットは存在しない**。
  代わりに `mockSenderPrivateKey="67ee05fc..."`、
  `mockPrivateKey="20fa65df..."`、`mockCredentialBundle="w99a5xV6..."`、
  ニーモニック `"leaf lady until indicate praise final route toast cake minimum insect unknown"`、
  PEM 鍵 `487f361d…0213c` / 圧縮公開鍵 `02f739f8…0316` が
  **`tkhq/sdk` / `swift-sdk` / `dart-sdk` の 3 レポで完全一致のコピー**で
  共有されており、実質的な reference vector になっている。
  → C# でも同じ hex / mnemonic を使えば、3 言語と機械的に三角検証できる。
- **`tkhq/sdk/packages/crypto/src/__tests__/proof-tests.ts`** には
  **実 enclave が発行した複数の `v1AppProof` / `v1BootProof` JSON**
  がフルで埋め込み済。署名検証ロジックの cross-language ゴールデン
  として現状最高品質。
- **`tkhq/go-sdk/pkg/enclave_encrypt/encrypt_test.go::TestP256Verify`**
  は `// Values generated by rust code` コメント付きの **Go↔Rust の
  cross-language ECDSA vector**。「ある言語の出力を別言語の入力に使う」
  という稀有な明示例。
- **`tkhq/qos/src/qos_p256/SPEC.md`** は **QOS encryption の独自仕様書**
  (SDK HPKE とは別経路。Section 2.1 参照)。
  ECDH(P-256) → HMAC-SHA512 で AES-GCM-256 の key を派生、
  `QOS_ENCRYPTION_HMAC_MESSAGE` 定数、nonce 12B など、
  **RFC 9180 とは互換性のない独自構成**。
  C# は QOS をポートしておらず本プラン scope 外。
- 公式 backend に対する live e2e を CI に組み込んでいるのは
  `tkhq/python-sdk` のみ。他はすべて mocked。

## 4. 既存の検証ギャップ (Why this plan)

GAP-1. **Coverage matrix 不在** — 公式 `test("...")` と C# `[Fact]` Method の
       対応表が無く、公式が test を追加したときに C# 側で漏れる。
GAP-2. **HTTP fixture 不在** — `tests/Fixtures/http/turnkey-http-vectors.json`
       が無い ([Http.cs-codex-findings-reconciliation.md セクション F](../codex-crypto-reviews/Http.cs-codex-findings-reconciliation.md))。
GAP-3. **Node 生成バイト fixture 不在** — `Generators/` 機構が無いので
       `purejs` モードで実 Node が吐く DER 署名や `SignedRequest.body`
       バイトと C# 出力を比較できない。
GAP-4. **Drift 検知 (テスト側) 不在** — `__tests__/*.ts` 自体の sha256 が
       pin されていない。上流テスト更新を catch できない。
GAP-5. **Primitive 単独ベクトル不足** — HKDF 以外 (ECDSA P-256 RFC 6979 単独・
       AES-GCM-256 NIST CAVP・HMAC-SHA256 RFC 4231・SHA-256 NIST SHS・
       Ed25519 RFC 8032・secp256k1 SEC2・Base58Check Bitcoin double-SHA256) の
       単体テストが無い。SDK HPKE 全体 (RFC 9180 互換性は未確認、Section 2.1 参照)
       の取り込みは PR-7a の評価結果次第とし、それまでは primitives 単独ベクトル
       でカバーする。
GAP-6. **proof モジュール非対応** — `crypto/proof.ts` (129 行) と
       `proof-tests.ts` (実 enclave 由来の JSON ゴールデン) が C# に
       存在しない。**proof のポート要否は別計画**だが、本プランでは
       「ゴールデン JSON を Fixture 側に取り込む準備」 (実コード非実装) は
       含める。proof 検証は ECDSA P-256 + base64url 復号で実現できるため、
       既存 C# プリミティブで verify ロジックだけ書ける見込みが立つかは
       別プランで判定する。
GAP-7. **他言語 SDK との同名 hex 値共有が活用されていない** —
       `mockSenderPrivateKey` 等の同名定数が 3 言語で完全一致。
       これを C# 側でも採用すれば三角検証ができるが、現状ローカルで
       別の値を生成しているテストがある (例: `Hpke_EncryptThenDecrypt_RoundTripsArbitraryPayload`)。

## 5. 検証戦略 (4 Tier)

```
Tier 1: Upstream-transcribed JSON vectors    [PARTIAL — encoding/crypto/stamper のみ]
        ↑ どこから来た値か Provenance 必須
Tier 2: Node-generated byte snapshots        [NEW]
        ↑ 公式 npm を実 Node で動かして固定。`purejs` ランタイム選択
Tier 3: Standard RFC / NIST vectors          [NEW (HKDF を除き)]
        ↑ 上流が間違うリスクの保険
Tier 4: Live backend (opt-in)                [既存スキップ条件付き]
        ↑ env 変数があるときのみ
```

各 Tier の **provenance** を `tests/Fixtures/README.md` のレベル分類
(`upstream-test-vectors` / `node-generated` / `rfc` / `nist` /
`turnkey-sample`) で必ず明示する。

## 6. 設計 — コンポーネント分解

### 6.1 Coverage matrix generator (新規)

**目的**: 公式 `__tests__/*.ts` の `test()/it()` ブロック名一覧と、
C# テストの `[Fact]` メソッド名一覧をクロス参照し、対応表を生成する。

**実装案**: TypeScript ではなく **POSIX `awk` + dotnet test 一覧** のシェル
スクリプト1本で実装する。理由: 既存リポジトリに Node ランタイムへの
依存が無く、`codex-crypto-review.sh` も pure shell。一貫性を保つ。

ファイル:
- `codex-crypto-reviews/coverage-map.sh` — 解析スクリプト
- `codex-crypto-reviews/coverage-map.tsv` — 生成結果 (上流 test 名 \t C# メソッド名 \t 状態)
- `codex-crypto-reviews/coverage-map.md` — レビュー用 markdown レンダリング

**マッピング規約 (人手部分)**:
- C# テストメソッドの XML doc コメントに `/// upstream: <file>:line "<test name>"`
  を 1 行付ける。スクリプトはこれを正として読む。
- 上流 `test()` 名 → C# メソッド名の 1:N マッピングを許容
  (例: 公式 `pointDecode -> uncompressed valid` は C# で
  `UncompressRawPublicKey_UpstreamVector_Valid_*` 等に細分化される)。
- どの C# テストにも紐付かない上流 test は `MISSING` で出力。

**CI gate (PR-3 で enable、PR-2 では未 enable)**:
```bash
./codex-crypto-reviews/coverage-map.sh --check
# PR-2 merge 時点: --check は warning のみ (exit 0)、`coverage-map.md` を生成
# PR-3 merge 時点以降: MISSING 行ゼロ + N/A 行に必ず "reason" 列を要求し、空なら exit non-zero
```

`coverage-map.tsv` の N/A 行は **理由テキストが必須**。代表的な N/A 理由:
- `not-ported: <symbol>` — C# が当該 API を ports していない (例: `hpkeAuthEncrypt`,
  `withAsyncPolling`、proof verifier、QOS `quorumKeyEncrypt`)。Section 2.2 参照。
- `runtime-variant: <name>` — 上流が複数 runtime (nodecrypto/webcrypto/purejs/universal)
  をテストするが、C# は単一 (purejs 相当) のみ。
- `e2e-only: live-backend` — backend live 経路でしか発火しない (例: gRPC error details)。
- `proof-fixture-only: PR-11` — `tests/Fixtures/proofs/` の provenance assertion で代替。

### 6.1.1 PR-3 候補テスト一覧 (Codex r1 で列挙された欠落 / N/A 必須)

PR-3 で追加 or 明示的に `N/A` マッピングが必要な上流テスト
(`codex-crypto-reviews/upstream-snapshots/.../__tests__/`):

| 上流 file | 上流 test 名 | C# 側の現状 |
|---|---|---|
| `api-key-stamper/elliptic-curves-test.ts` | `pointDecode -> uncompressed invalid` | 似た振る舞いは `CryptoTests.UncompressRawPublicKey_BadPrefix_Throws` 等にあるが明示マップ要 |
| 同 | `pointDecode -> uncompressed valid` | 同上 |
| 同 | `pointDecode -> compressed` | 同上 |
| `api-key-stamper/signature-test.ts` | `sign with Turnkey fixture: sign (node crypto)` | C# 側 N/A (`nodecrypto.ts` runtime は port しない) |
| 同 | `sign with Turnkey fixture: sign (WebCrypto)` | C# 側 N/A (同) |
| 同 | `sign with Turnkey fixture: sign (PureJS)` | `ApiKeyStamperTests.SignWithApiKey_DeterministicWithRfc6979` 等にマップ |
| 同 | `sign with Turnkey fixture: sign (universal)` | C# 側 N/A |
| 同 | `sign with openssl generated key pairs: stamp (node/WebCrypto/universal)` | 各 N/A 理由を明示 |
| `api-key-stamper/stamp-test.ts` | `uses provided signature to make stamp` | `ApiKeyStamperTests.Stamp_UpstreamFixture_ProducesValidWireBytes` にマップ |
| `crypto/crypto-test.ts` | `hpkeAuthEncrypt and hpkeDecrypt - end-to-end encryption and decryption` | **C# N/A**: 現 C# は `HpkeAuthEncrypt` を public 公開していない (Section 2.2 参照、[Crypto.cs](../src/Crypto.cs))。N/A reason: `not-ported: hpkeAuthEncrypt`。peak monorepo の wallet flow がこれを必要としていないことは Phase A grep で確認する |
| 同 | `hpkeEncrypt and hpkeDecrypt - standard mode (ephemeral sender key)` | `CryptoTests.Hpke_EncryptThenDecrypt_RoundTripsArbitraryPayload` にマップ |
| 同 | `decryptExportBundle ... mnemonic` | 未対応、PR-3 で追加 |
| 同 | `decryptExportBundle ... non-mnemonic` | 未対応 |
| 同 | `getPublicKey/generateP256KeyPair/compressRawPublicKey/decryptCredentialBundle` 各 | 既存マップあり |
| 同 | `extractPrivateKeyFromPKCS8Bytes` | **C# N/A**: `src/Crypto.cs:25` の "Out of scope" コメントで peak Unity port と同じく未 ports と明示。reason: `not-ported: extractPrivateKeyFromPKCS8Bytes` |
| 同 | `verifyRequestStamp` | **C# N/A**: `src/Crypto.cs:25-26` で `verifyStampSignature` 等と同様 ports していない (peak Unity port も同様)。reason: `not-ported: verifyRequestStamp`。stamper の wire 検証は `HttpTests.Stamp_HeaderValueDecodesAndVerifies` と `ApiKeyStamperTests.Stamp_*` に間接的にカバーされる |
| 同 | `uncompressRawPublicKey` happy/invalid prefix/invalid length | 既存マップあり (`Uncompress*Tests`) |
| 同 | DER parser 14 ケース (2 valid at `crypto-test.ts:263-282` + 12 invalid/edge at `:287-454`) | **C# N/A**: `fromDerSignature` は `src/Crypto.cs:25-26` の "Out of scope" コメントで未 ports と明示。reason: `not-ported: fromDerSignature`。**stamper の wire output 側**の DER 検証 (`ApiKeyStamperTests.AssertSignatureVerifies` / `ParseDerEcdsa` 経由) は別観点なので、上流 input parser の負例テストには対応しない |
| 同 | `Session JWT signature: verifies the provided JWT against its public key` | `CryptoTests.VerifySessionJwtSignature_*` にマップ |
| `crypto/proof-tests.ts` | `should verify valid app proof signatures` | C# 側 N/A (proof 未ポート、Fixture のみ PR-11) |
| 同 | `should verify correct app proof / boot proof combos` | C# 側 N/A |
| 同 | `should NOT verify with malformed app proofs` | C# 側 N/A |
| 同 | `should NOT verify with malformed boot proofs` | C# 側 N/A |
| `http/async-test.ts` | `withAsyncPolling` 6 ケース | C# 側 N/A (peak monorepo は `withAsyncPolling` 経路を使わない方針を確認要 → Phase A) |
| `http/request-test.ts` | `requests are stamped after initialization` | C# 側 `HttpTests.Stamp_*` 群にマップ |
| 同 | `requests return grpc status details as part of their errors` | C# 側 N/A 検討 (peak monorepo の error 表面実装を確認) |
| `encoding/index-test.ts` | 5 ケース全部 | 既存 EncodingTests にマップ済 |

すべて `N/A` を出すには **理由テキスト** を `coverage-map.tsv` に書く義務を課す
(空 N/A は不可、CI で `awk` でブロック)。

### 6.2 Node-generated fixture generator (新規)

**目的**: pinned npm パッケージを実 Node で動かし、
C# が再現すべきバイト列を JSON にスナップショットする。

ディレクトリ:
```
tests/Fixtures/Generators/
├── package.json                — pinned exact versions, no semver range
├── package-lock.json           — committed (lock transitive deps)
├── generate-http-vectors.mjs   — emits tests/Fixtures/http/turnkey-http-vectors.json
├── generate-stamper-vectors.mjs — emits tests/Fixtures/api-key-stamper/turnkey-stamper-node-vectors.json
├── generate-crypto-vectors.mjs  — emits tests/Fixtures/crypto/turnkey-crypto-node-vectors.json
└── README.md                    — Node version pin, reproduction steps, output sha256
```

**重要な決定 (v3 改訂)**:

- **決定論経路のみ byte snapshot を取る**:
  - `ApiKeyStamper.stamp({ runtimeOverride: "purejs" })` の出力 (RFC 6979 + low-S)。
  - `decryptCredentialBundle(ciphertext, embeddedKey)` の plaintext (受信側演算なので決定論)。
  - `decryptExportBundle(...)` の plaintext。
  - `SignedRequest.body` の JSON.stringify bytes (stamp signature を含まないので決定論)。
- **非決定論経路は byte snapshot を取らない**:
  - `hpkeEncrypt`, `hpkeAuthEncrypt`, `quorumKeyEncrypt` (現状 scope 外) は
    ephemeral key/nonce を内部生成。**byte 等価検証は撤回**。
  - 代わりに「encrypt → decrypt round-trip で plaintext 等価」と
    「encrypt 出力の構造的 assertion (encapsulated key 長 33, ciphertext 長,
    AAD 構成)」を C# 側で確認する。
  - **追加調査**: pinned `crypto.ts` を grep して `hpkeEncrypt` に
    ephemeral key を外から inject できる private path があるか確認。
    あれば限定的に byte snapshot を復活する。なければ前述方針で確定。
- `purejs` 確実化のための **pre-flight test in generator**:
  - Generator 起動時に **既知ベクトル**
    (`api-key.private` を `purejs.sign("hello from TKHQ!")` した既知 DER 16 進)
    を実行し、`@noble/curves@1.3.0` の transitive resolution が
    low-S RFC 6979 を実際に出すことを assert。失敗したら exit non-zero。
- 入力は **公式 `__fixtures__/api-key.{private,public}` をそのまま使う**。
  Crypto-test.ts の `decryptCredentialBundle` ベクトルも同様に流用。
- 生成スクリプトは **新たに値を作らない**。既存の固定された入力で
  「公式が吐くであろうバイト」を確定するだけ。
- 出力 JSON は `_provenance` で
  `{ level: "node-generated", node_version, npm_lockfile_sha256, generator_sha256, output_sha256, transitive_lock_sha256 }`
  を必ず記録。`transitive_lock_sha256` は `npm ls --json` の sha256 で
  `@noble/curves` の解決バージョン drift を検知。

**CI 統合**:
- 通常 CI では fixture を**再生成しない**。生成済み JSON を read-only で
  使う (Node 不在環境でもテストが走る)。
- 月 1 回 (`schedule: cron`) または手動 `workflow_dispatch` で
  fixture 再生成 + diff チェックを別ワークフロー
  (`.github/workflows/fixture-regen.yml`) で実行。差分が出たら PR を起こす。
- `tests/Fixtures/Generators/` 配下は **csproj から除外**:
  PR-4 で `tests/turnkey-sdk-csharp.Tests.csproj` (現在 line 23-26 の
  `<None Include="Fixtures\**\*">` ItemGroup) を以下のいずれかに修正する:
  ```xml
  <None Include="Fixtures\**\*"
        Exclude="Fixtures\Generators\**"
        CopyToOutputDirectory="PreserveNewest" />
  <Content Remove="Fixtures\Generators\**" />
  <None Remove="Fixtures\Generators\**\*.json"
        Condition="false" /> <!-- 念のための明示的 None Remove (no-op) -->
  ```
  最低でも `Exclude="Fixtures\Generators\**"` と
  `<Content Remove="Fixtures\Generators\**" />` の両方を必須とし、
  `dotnet build` が `Generators/` 配下の `.mjs` / `package.json` / `node_modules`
  を巻き込まないことを `dotnet build -bl` で確認する。
  これは **PR-4 のスコープ**に含める (Codex r1/r2 指摘)。

### 6.3 標準テストベクトル取り込み (primitives + 条件付き HPKE)

(canonical 用語は Section 2.1 を参照。)

#### 6.3.1 必須 primitives ベクトル

ファイル配置:
```
tests/Fixtures/rfc/
├── hkdf-rfc5869/                  — A.1, A.2, A.3 (既存 CryptoTests.cs と同期)
│   ├── README.md
│   └── vectors.json
├── ecdsa-p256-rfc6979/            — Appendix A.2.5 (P-256, SHA-256)
│   ├── README.md
│   └── vectors.json
├── hmac-sha256-rfc4231/           — Section 4 test cases 1-7
│   ├── README.md                    (SDK HPKE が HKDF-SHA256 内で使う HMAC-SHA256)
│   └── vectors.json
└── ed25519-rfc8032/               — Section 7.1 test cases (SOLANA export 経路で使用)
    ├── README.md
    └── vectors.json
tests/Fixtures/nist/
├── aes-gcm-256-cavp/              — KeySize=256, TagSize=128, IV=96bit
│   ├── README.md                    (SDK HPKE は 32-byte key を派生して AES-GCM-256 使用、
│   │                                 credential bundle decrypt も AES-GCM-256。Section 2.1 参照)
│   └── vectors.json
├── ecdsa-p256-sigver-fips186-4/   — FIPS 186-4 SigVer (P-256, SHA-256)
│   ├── README.md
│   └── vectors.json
└── sha-256-shs-bytevectors/       — NIST SHS Byte test vectors
    ├── README.md
    └── vectors.json
tests/Fixtures/sec/                — secp256k1 関連 (`uncompressRawPublicKey(curve=secp256k1)` のため)
└── secp256k1-sec2/
    ├── README.md
    └── vectors.json
tests/Fixtures/bitcoin/            — Base58Check Bitcoin double-SHA256 known addresses
└── base58check-bip39/
    ├── README.md
    └── vectors.json
```

#### 6.3.2 条件付き: HPKE RFC 9180 ベクトル

**判定 step (PR-7a)**:
1. pinned `@turnkey/crypto@2.8.8` の `hpkeEncrypt` を、固定 ephemeral key を
   inject できる経路があるか調べる (`grep -n "ephemeralKeyPair" crypto.ts`)。
2. 注入できない場合は、Node generator から `hpkeEncrypt` をオーバーライド
   (関数 wrapping) して固定 ephemeral key を渡せるかを確認。
3. 確認できれば、RFC 9180 Appendix A.4 (DHKEM(P-256, HKDF-SHA256), HKDF-SHA256,
   AES-256-GCM) の test vector を `tests/Fixtures/rfc/hpke-rfc9180/` に取り込む。
4. **C# 側が同じ ephemeral key 注入で**同 byte を出すか assert する。
5. 一致しない場合は本仕様の差分点 (suite_id, label, info 順序等) を
   `codex-crypto-reviews/sdk-hpke-vs-rfc9180.md` に文書化し、RFC 9180 ベクトル
   取り込みは見送る。Tier-2 で SDK 生成 bytes を信頼。

PR-7a の評価が完了するまでは Tier-3 HPKE ベクトルは入れない。

#### 6.3.3 抜粋方針と provenance level

フル CAVP / SHS テストファイル (数千〜数万行) はコミットせず、
**SDK 経路で実際に使われる組合せ** だけを `vectors.json` に切り出す。
**原 CAVP ファイル URL + SHA256 + 切り出し範囲**を README に明示する。

`tests/Fixtures/README.md` の "Provenance levels" との対応:

| `tests/Fixtures/` 配下 | provenance level (README 内表記) | 追記要否 |
|---|---|---|
| `rfc/hkdf-rfc5869/` | `rfc` (RFC 5869) | 既存 level、Layout 例に追加 |
| `rfc/ecdsa-p256-rfc6979/` | `rfc` (RFC 6979) | 既存 level、Layout 例に追加 |
| `rfc/hmac-sha256-rfc4231/` | `rfc` (RFC 4231) | 既存 level、Layout 例に追加 |
| `rfc/ed25519-rfc8032/` | `rfc` (RFC 8032) | 既存 level、Layout 例に追加 |
| `nist/aes-gcm-256-cavp/` | `nist` (NIST CAVP) | 既存 level、Layout 例に追加 |
| `nist/ecdsa-p256-sigver-fips186-4/` | `nist` (NIST FIPS) | 既存 level、Layout 例に追加 |
| `nist/sha-256-shs-bytevectors/` | `nist` (NIST SHS) | 既存 level、Layout 例に追加 |
| `sec/secp256k1-sec2/` | **新規 level `sec2-published`** | PR-8 で `tests/Fixtures/README.md` に追加 |
| `bitcoin/base58check-bip39/` | **新規 level `bip-test-vectors`** | PR-8 で `tests/Fixtures/README.md` に追加 |
| `proofs/` (PR-11) | `upstream-test-vectors` | 既存 level (`proof-tests.ts` から借用) |
| `http/turnkey-http-vectors.json` (PR-5) | `node-generated` | 既存 level |

#### 6.3.4 実装

`tests/CryptoTests.cs` / `tests/EncodingTests.cs` に既存の `Hkdf_Rfc5869_A1`
パターンで `EcdsaP256_Rfc6979_A25_<msg>`, `HmacSha256_Rfc4231_TC<n>`,
`Ed25519_Rfc8032_TC<n>`, `AesGcm_Nist_<id>`, `Sha256_Nist_<id>`,
`Secp256k1_Sec2_<id>`, `Base58Check_Bitcoin_<id>` を追加。
Tier 3 は **SDK 実装が組み立てに使う primitives が RFC/NIST 仕様に
従っていることの独立証明**である。SDK HPKE 全体の構造的正しさは
Tier-2 (Node 生成 / round-trip) と (条件付きで) Tier-3 HPKE が担当する。

### 6.4 上流テストの sha256 pin と drift 検知 (新規)

ファイル:
- `codex-crypto-reviews/upstream-snapshots/test-file-checksums.txt` —
  各 `__tests__/*.ts` と `__fixtures__/*` の sha256。
- `.github/workflows/upstream-drift.yml` — 月次 cron で
  GitHub の `tkhq/sdk` 該当タグから `__tests__/` をフェッチ (npm tarball には
  test が無いので、`gh api repos/tkhq/sdk/contents/...?ref=<sha>`) し、
  pin 済 sha256 と比較。差分があれば issue を**idempotent に**起票/更新。

**Idempotency rule (Codex r1 指摘の解消)**:

```yaml
# 既存の open issue を 1 つ取得して再利用する
- name: Find or create drift issue
  env:
    GH_TOKEN: ${{ secrets.GITHUB_TOKEN }}
    DRIFT_LABEL: upstream-drift
    PACKAGE: ${{ matrix.package }}
  run: |
    BODY=$(cat drift-report.md)
    # ラベル + パッケージ名で 1 issue に固定
    EXISTING=$(gh issue list --state open \
      --label "$DRIFT_LABEL" --search "in:title $PACKAGE" \
      --json number --jq '.[0].number // empty')
    if [[ -n "$EXISTING" ]]; then
      gh issue edit "$EXISTING" --body "$BODY"
      gh issue comment "$EXISTING" --body "Re-detected on $(date -u +%FT%TZ)."
    else
      gh issue create --label "$DRIFT_LABEL" \
        --title "Upstream drift: $PACKAGE" --body "$BODY"
    fi
```

**ポリシー**: 上流テストファイルが変わっていた時、
1. 該当 C# テスト・fixture を更新する PR を作る (人手)。
2. coverage-map.sh の `MISSING` 行が増えていないか確認する。
3. 再 pin と Codex 3-round review を再実行する。

これにより、上流が test を追加したのに C# 側で気付かない、を防ぐ。
かつ「同じ drift で月ごとに新 issue が無限に増える」も防ぐ。

### 6.5 HTTP 等価性検証 (Tier-2 で吸収)

`tests/Fixtures/http/turnkey-http-vectors.json` の中身:
```json
{
  "_provenance": {
    "level": "node-generated",
    "generator": "tests/Fixtures/Generators/generate-http-vectors.mjs",
    "node_version": "20.x.y",
    "npm_lockfile_sha256": "...",
    "output_sha256": "...",
    "note": "@turnkey/http@3.16.0 + @turnkey/api-key-stamper@0.5.0 (purejs) で生成。stamp の signature は RFC 6979 deterministic-k なため byte 一意。"
  },
  "stampGetWhoami": [
    {
      "input": { "organizationId": "<UUID>", "baseUrl": "https://api.turnkey.com", "apiPrivateKey": "<hex>", "apiPublicKey": "<hex>" },
      "expectedSignedRequest": {
        "url": "...",
        "body": "...",         // JSON.stringify そのもの (改行・スペース無し)
        "stampHeaderName": "X-Stamp",
        "stampHeaderValue": "<base64url>"
      }
    }
  ],
  "stampInitImportPrivateKey": [ /* ... */ ],
  "stampImportPrivateKey":     [ /* ... */ ],
  "stampExportPrivateKey":     [ /* ... */ ],
  "stampExportWalletAccount":  [ /* ... */ ]
}
```

C# 側 (`HttpTests.cs`) の追加 assert:
- `signedRequest.body` (UTF-8 bytes) が**完全一致**。
- `signedRequest.stampHeaderValue` を base64url-decode → JSON 復元 →
  キー順 `["publicKey","scheme","signature"]` 一致。
- `signature` の DER 16 進文字列が**完全一致** (purejs deterministic 前提)。

これにより、既存の `HttpTests.Stamp_HeaderValueDecodesAndVerifies` の
「DER 形式と検証だけ」というギャップ
([Http.cs-codex-findings-reconciliation.md F](../codex-crypto-reviews/Http.cs-codex-findings-reconciliation.md))
を埋める。

## 7. 実装計画 (PR 単位)

| # | PR タイトル | 内容 | 依存 | サイズ |
|---|---|---|---|---|
| PR-1 | docs: equivalence verification design v3 | 本ファイル `plans/PLAN-EQUIVALENCE-VERIFICATION.md` を commit。Codex plan review を review-clean になるまで反復 | なし | S |
| PR-1b | docs: peak monorepo usage grep evidence | `codex-crypto-reviews/peak-usage-grep-evidence.md` を作成。peak のソースコードに対し以下を grep し結果をコミット: (a) `Turnkey.Crypto.Proofs` / `verifyAppProof` / `proof.ts` の呼び出し、(b) `withAsyncPolling` の利用、(c) `hpkeAuthEncrypt` の利用、(d) `extractPrivateKeyFromPKCS8Bytes` の利用、(e) QOS `quorumKeyEncrypt` の利用。各 grep 結果 (0 件 / >0 件) を文書化 | PR-1 | S |
| PR-2 | tests: upstream test-name coverage matrix (gate なし) | `coverage-map.sh` + `coverage-map.md` を追加。既存 C# テストに `/// upstream:` xml-doc を付ける。**この PR では `MISSING` 行を許容**し、`coverage-map.md` に documented リストとして残す。CI gate は **PR-3 で enable** する | PR-1, PR-1b | M |
| PR-3 | tests: missing C# tests + fail-on-MISSING gate enable | PR-2 で `MISSING` だった上流 test に対応する C# テストを追加。**追加できないもの (`hpkeAuthEncrypt`、`withAsyncPolling`、proof verifier、`extractPrivateKeyFromPKCS8Bytes`、`verifyRequestStamp` 等) は `coverage-map.tsv` で N/A 理由付きで documented** とし、それ以降は `MISSING` 行ゼロ + `N/A` 理由必須を CI で fail-on にする。テストのみで `src/*.cs` は触らない。N/A 判定根拠は PR-1b の grep evidence に依拠 | PR-1b, PR-2 | M〜L |
| PR-4 | tests: Node fixture generator scaffolding | `tests/Fixtures/Generators/` ディレクトリ、`package.json` (pinned)、`package-lock.json`、README、`.gitattributes` (LF)、**`tests/turnkey-sdk-csharp.Tests.csproj` の `Fixtures/**/*` ItemGroup に `Exclude="Fixtures/Generators/**"` と `<Content Remove="Fixtures/Generators/**" />` を追加して `dotnet build` から exclusion** (Section 6.2 末尾) | PR-1 | S |
| PR-5 | tests: Node-generated HTTP byte vectors | `generate-http-vectors.mjs` + `turnkey-http-vectors.json` + `HttpTests.cs` 追加 assert | PR-4 | M |
| PR-6 | tests: Node-generated stamper byte vectors | `generate-stamper-vectors.mjs` + `turnkey-stamper-node-vectors.json` + `ApiKeyStamperTests.cs` 追加 assert | PR-4 | M |
| PR-7 | tests: Node-generated HPKE round-trip + structural fixtures | encrypt → decrypt round-trip で plaintext 等価、encrypt 出力構造 (encapsulated key 長等) を assert。**encrypt の byte snapshot は取らない** (Section 2.1 の非決定論経路) | PR-4 | M |
| PR-7a | research: SDK HPKE ↔ RFC 9180 byte-equality 評価 | `tests/Fixtures/Generators/hpke-rfc9180-eval.mjs` で固定 ephemeral key 注入経路を探索し、`codex-crypto-reviews/sdk-hpke-vs-rfc9180.md` に結論。byte-equal なら PR-9b に進む、equal でないなら見送る | PR-4 | M |
| PR-8 | tests: Primitives standard vectors (HKDF/ECDSA/HMAC/Ed25519/secp256k1/Base58Check) | `tests/Fixtures/rfc/{hkdf,ecdsa-p256-rfc6979,hmac-sha256-rfc4231,ed25519-rfc8032}/`、`tests/Fixtures/sec/secp256k1-sec2/`、`tests/Fixtures/bitcoin/base58check-bip39/` + 対応 test 追加。**同 PR で `tests/Fixtures/README.md` の "Provenance levels" 章に `sec` (= "sec2-published") と `bitcoin` (= "bip-test-vectors") を新規 level として追加 + Layout 例を更新** | なし (PR-1 後すぐ可) | M |
| PR-9 | tests: Primitives NIST vectors (AES-GCM-256/SHA-256/ECDSA SigVer) | `tests/Fixtures/nist/` 整備 (AES-GCM **256 のみ**、SDK 経路で使われない 128 は取り込まない) | PR-8 | M |
| PR-9b | tests: HPKE RFC 9180 vectors (条件付き) | **PR-7a が byte-equal を確認した場合のみ実施**。RFC 9180 Appendix A.4 vectors を取り込む | PR-7a, PR-8 | M |
| PR-10 | tests: 他言語 SDK 共有ハードコード値の cross-language 借用 | `mockSenderPrivateKey`, `mockCredentialBundle`, ニーモニック等を C# で同名定数化、対応テスト追加 | PR-3 | S |
| PR-11 | tests: proof fixtures + provenance assertion (verify 本体は scope 外) | `tests/Fixtures/proofs/` に `proof-tests.ts` から AppProof / BootProof JSON を借用しコミット。**`ProofFixtureProvenanceTests` を一緒に追加**: JSON parse、`_provenance` schema validation、上流 sha256 一致を assert。verify ロジックの C# 実装は別計画 | PR-1 | S |
| PR-12 | ci: upstream-drift detection workflow (idempotent) | 月次 cron、上流テスト sha256 比較、差分時に **同 label + パッケージ名で 1 issue を update or create** (Section 6.4 yaml 例) | PR-2 | S |
| PR-13 | ci: fixture regeneration workflow (opt-in) | `workflow_dispatch` + 月次 cron で Generators を回し、diff を PR | PR-4〜7 | S |
| PR-13b | ci: setup-node 追加 (PR-13 内) | `.github/workflows/fixture-regen.yml` に `actions/setup-node@v4` を追加。**main CI (`ci.yml` / `release.yml`) は触らない** (Codex r2 で確認: 既存 CI は .NET のみで Node に触らない) | PR-13 と同 PR でも可 | S |
| PR-14 | docs: README "Verification posture" + NOTICE 更新 | 同等性検証の到達点と前提を README に明記、他言語 SDK 借用箇所の attribution、proof scope 外を明記 | PR-9, PR-10, PR-11, PR-12, PR-13 全部 + (PR-7a が byte-equal を確認した場合は) **PR-9b 完了後** | S |

PR-1 (本プラン commit) を最初に Codex review-clean まで反復してから、
PR-2 以降は依存順に進める。**全 PR が独立にコミット可能** で、
途中で打ち切ってもリポジトリは壊れない。

## 8. CI / 完了基準

ローカルおよび GitHub Actions で:

1. `dotnet test -c Release` がすべてグリーン。
2. `codex-crypto-reviews/coverage-map.sh --check` が exit 0
   (`MISSING` 行ゼロ、空 N/A もブロック)。
3. `upstream-drift.yml` の最新月次実行が成功 (= 差分なし、
   または **同じ open issue で update され続けている** 状態)。
4. `FINAL-INTEGRATED-REVIEW-*.md` の更新版が同条件で再 GO。
5. README "Verification posture" 章が **PR-14 で更新され main にマージ済み**。
6. Tier-2 fixtures の決定論経路は **byte-equal**、非決定論経路は
   **decrypt round-trip 等価 + 構造的 assertion パス**。
7. (PR-7a が byte-equal を確認した場合のみ) HPKE RFC 9180 vectors を C# 実装が **byte-equal** で通る。

## 9. リスクと対策

| リスク | 対策 |
|---|---|
| `purejs` runtime と `nodecrypto.ts` runtime の signature byte 差 | PR-5/6 では `runtimeOverride: "purejs"` を必ず指定。doc にも明記。さらに generator pre-flight で既知 vector check |
| Node version drift で fixture が再生成毎に変わる | `package-lock.json` を commit、`node_version` を fixture に記録、generator が `process.version` を起動時 assert |
| `@noble/curves` の transitive 解決が drift して low-S 出力が変わる | `transitive_lock_sha256` を fixture provenance に記録、generator 起動時に既知 hex を assert |
| 公式テストファイル sha256 を pin したのに毎月差分が出て CI が騒がしい | drift workflow は **idempotent な issue update のみ**で main の CI gate にはしない (=PR がブロックされない、issue が量産されない) |
| RFC vector を取り込んだら BouncyCastle の primitives が誤動作した | 設計上ありえないが、対処は Tier-3 失敗 → BouncyCastle pin を見直し、別 PR で対応 |
| SDK HPKE が RFC 9180 と互換でない (PR-7a で判明) | RFC 9180 vectors 取り込みを諦め、Tier-2 で SDK 生成 bytes を信頼。`codex-crypto-reviews/sdk-hpke-vs-rfc9180.md` に差分点を文書化 |
| 公式が test を消した | upstream-drift で検知、coverage-map で `REMOVED` 行を出して人手 review |
| Node generator 環境差で `package-lock.json` が壊れる (Apple Silicon vs x86_64 など) | platform-agnostic な依存しか入らないことを `npm ls` で確認、再現手順を README に明示 |
| 公式他言語 SDK の test fixture を借りる場合のライセンス | 上流 (tkhq/sdk Apache-2.0、tkhq/go-sdk Apache-2.0) に従い PR-14 で `NOTICE` に出典明記 |
| Generators ディレクトリが dotnet test のビルドを壊す | PR-4 で csproj の `Fixtures/**/*` ItemGroup から `Generators/**` を `Exclude` する (Section 6.2 末尾参照) |
| PR-11 fixture が consumer のない golden data になる | PR-11 で `ProofFixtureProvenanceTests` を同 PR で必ず追加し、機械検証経路を確立 |

## 10. 想定 FAQ (codex 想定突っ込み先回り)

**Q. 既存の `_provenance` 付き Tier-1 fixture と何が違うのか？**
A. Tier-1 は「上流テストファイルの値を**人手で転記**」している。
本プラン Tier-2 は「上流 npm パッケージを**実行**して**実バイト**を取る」
ので、上流コードに潜む branch (例: SDK HPKE の AAD 連結順序ミス) も検出できる。
両者は補完関係。Section 2.1 で定義した「決定論経路」のみ byte snapshot を取る。

**Q. なぜ `dotnet test` で Node を起動しないのか？**
A. (a) Codex review-clean 環境は read-only で Node を持たないことが多い、
(b) CI runner が macOS / Linux / Windows で生 Node を呼ぶと flaky になる、
(c) 出力は決定論的なので **生成済み JSON をコミット**する方が
高速で再現性が高い。fixture 再生成は別 workflow に分離する。

**Q. Tier-3 (RFC ベクトル) は冗長では？ 公式が正しいなら上流テスト等価で十分。**
A. v1 では「HPKE 全体の RFC 9180 vectors を取る」、v2 では「Turnkey HPKE は
独自構成だから RFC 9180 は無理」と振れたが、いずれも誤り。v3 canonical
(Section 2.1):
- C# が port している SDK HPKE は **JSdoc が RFC 9180 を明示** している。
- ただし完全互換かは未確認 → PR-7a で評価。**byte-equal なら PR-9b で
  RFC 9180 vectors を取り込む** (条件付き Tier-3)。
- それと別に、**primitive 単位** (ECDSA P-256 RFC 6979, HKDF RFC 5869,
  HMAC-SHA256 RFC 4231, AES-GCM NIST CAVP, SHA-256 NIST SHS, Ed25519 RFC 8032,
  secp256k1 SEC2, Base58Check Bitcoin) の標準ベクトルは PR-8/PR-9 で取り込む。
- これは「primitive レベルの C# 実装が RFC/NIST 仕様に従っている」ことの
  独立証明であり、SDK HPKE 全体は Tier-2 (round-trip + 構造) と
  条件付き Tier-3 が分担する。

**Q. `coverage-map.sh` を pure shell にするのは無理がないか？**
A. `tkhq/sdk` の `__tests__/*.ts` 内 `test("...")` は ESLint で一行 1 ブロックに
強制されているので awk で確実に grep できる。複雑な AST が必要なら
PR-2 のレビューで Node ベースに切替を検討する。本ドラフトでは
**「Node 依存を増やさない」を優先**する。

**Q. proof.ts のポートはやらないのか？**
A. proof モジュール本体のポート (= `Turnkey.Crypto.Proofs.Verify(...)` の
C# 実装) は scope 外。peak monorepo の wallet flow からこの API が呼ばれていないか
どうかは **Phase A で grep し `codex-crypto-reviews/peak-usage-grep-evidence.md`
にコミット**して確定させる (本プラン v3 で Phase A に明示)。
ただし `proof-tests.ts` に**実 enclave 由来の AppProof / BootProof
ゴールデン JSON** がある事実が今回判明したので、それだけは PR-11 で
`tests/Fixtures/proofs/` に**先行コミット**する。
PR-11 では **proof の cryptographic verifier は実装しない** (= 別計画) が、
`ProofFixtureProvenanceTests` (JSON parse、`_provenance` schema、上流 sha256 一致)
は同 PR で必ず一緒に追加する。別途 proof 実装プランを立てる際の前提が整う。

**Q. live e2e は無くて大丈夫か？**
A. 本プラン Tier-4 で `TURNKEY_TEST_ORG_API_KEY` opt-in は維持する。
ただし v0.1.0-alpha の必須条件ではない。Tier-1 ～ Tier-3 が
完備すれば backend を呼ばずに wire-format compatibility が保証される。

## 11. 想定スケジュール感

(エンジニア 1 人 × 半日単位の見積もり)

| Phase | PR 範囲 | 単位 |
|---|---|---|
| Phase A | PR-1 (本プラン Codex review-clean まで) + **PR-1b (`codex-crypto-reviews/peak-usage-grep-evidence.md`)** | 1.0 単位 |
| Phase B | PR-2, PR-3, PR-12 (coverage + drift) | 2 単位 |
| Phase C | PR-4, PR-5, PR-6, PR-7, PR-7a, PR-11, PR-13 (Node fixtures + HPKE 評価) | 3 〜 4 単位 |
| Phase D | PR-8, PR-9, (条件付き) PR-9b (RFC/NIST/HPKE9180) | 2 〜 3 単位 |
| Phase E | PR-10 (他言語 SDK 値借用), PR-14 (docs) | 1 単位 |
| 合計 |  | 8.5 〜 11 単位 |

## 12. 未スコープ依存 (このプランで決定しない)

- `Turnkey.Crypto.Proofs.Verify(...)` の C# 実装可否 (別計画)。
  ただし fixture 側 (`tests/Fixtures/proofs/`) と provenance assertion
  (`ProofFixtureProvenanceTests`) は PR-11 で先行整備する。
- QOS `quorumKeyEncrypt` の C# ポート可否 (本プランの C# ports スコープに無い、
  Section 2.1 / Section 2.2 参照)。peak monorepo が QOS 経路を使う場合のみ
  別計画化する。
- SDK HPKE と RFC 9180 の byte-equal 判定そのものは PR-7a の評価結果を
  待って決まる。byte-equal でなければ PR-9b は実施しない。
- Go-Rust cross-language vector (`enclave_encrypt::TestP256Verify`) の C# 取り込みは、
  価値はあるが Turnkey SDK の wire-format には直結しないため、本プランでは
  **PR-10 の延長 (オプション)** とし、必須化はしない。
- nuget.org への publish 条件への影響なし。

### 12.1. G で確定: 本プラン scope 外として記録するもの (Codex r2 残)

| 項目 | 本プランでの扱い |
|---|---|
| 他言語 SDK (`tkhq/sdk` / `swift-sdk` / `dart-sdk` / `go-sdk` / `rust-sdk`) 内 fixture の事実関係 (Section 3、Section 13) | **エージェント調査による状況証拠の記録**であり、その正確性は本プラン内で機械検証しない。PR-10 で C# 側に同名定数を採用する時点でのみ、実値を `tkhq/sdk` の `mocks.ts` から **手動コピー + 出典 URL コメント** で取り込む。コピー時の sha256 一致確認は PR-10 内で行う |
| `peak/` monorepo の `proof` / `withAsyncPolling` 利用有無 | Phase A の必須作業として「`peak/` repo を grep し、結果を `codex-crypto-reviews/peak-usage-grep-evidence.md` にコミット」を本プラン内に含める (= scope 内に格上げ)。grep 結果次第で PR-3 の N/A 理由が確定する |
| `tkhq/python-sdk` のみが live backend e2e、他 SDK は mock 中心 | **エージェント調査の記録**であり本プラン内で機械検証しない。Tier-4 (live) を opt-in に留める判断の補強材料として使うだけ |
| PR-7a の fixed-ephemeral injection 経路の実在性 | **PR-7a の成果物そのもの**。本プラン内で先取り判定しない。PR-7a が「不可能」と結論したら PR-9b は実施されない |

## 13. References

### 内部
- 既存検証 SOP: [codex-crypto-reviews/README.md](../codex-crypto-reviews/README.md)
- 既存 Tier-1 fixture スキーム: [tests/Fixtures/README.md](../tests/Fixtures/README.md)
- 最終 GO: [FINAL-INTEGRATED-REVIEW-20260523.md](../codex-crypto-reviews/FINAL-INTEGRATED-REVIEW-20260523.md)
- HTTP fixture 未整備の根拠: [Http.cs-codex-findings-reconciliation.md F](../codex-crypto-reviews/Http.cs-codex-findings-reconciliation.md)
- Stamper byte parity の方針: [ApiKeyStamper.cs-codex-findings-reconciliation.md E1, F](../codex-crypto-reviews/ApiKeyStamper.cs-codex-findings-reconciliation.md)
- 既存プラン形式 reference: [plans/plan-v2-codex-reviewed.md](./plan-v2-codex-reviewed.md), [plans/PLAN-RELEASE-WORKFLOW.md](./PLAN-RELEASE-WORKFLOW.md)

### 公式 SDK (Apache-2.0)
- [tkhq/sdk](https://github.com/tkhq/sdk) — TypeScript monorepo (本プランの pin 元)
- [tkhq/go-sdk](https://github.com/tkhq/go-sdk) — `pkg/apikey/apikey_test.go` の P-256 keygen vector、`pkg/enclave_encrypt/encrypt_test.go::TestP256Verify` の Go↔Rust cross-language vector
- [tkhq/rust-sdk](https://github.com/tkhq/rust-sdk) — `enclave_encrypt/src/lib.rs` の base58check known vector
- [tkhq/swift-sdk](https://github.com/tkhq/swift-sdk), [tkhq/dart-sdk](https://github.com/tkhq/dart-sdk), [tkhq/kotlin-sdk](https://github.com/tkhq/kotlin-sdk) — 共有ハードコード hex 値の参照元
- [tkhq/python-sdk](https://github.com/tkhq/python-sdk) — live backend e2e の参考実装
- [tkhq/qos/src/qos_p256/SPEC.md](https://github.com/tkhq/qos/blob/main/src/qos_p256/SPEC.md) — **QOS encryption の独自仕様書 (SDK HPKE とは別経路、C# 未ポート)**

### Turnkey 公開ドキュメント
- [X-Stamp header format](https://docs.turnkey.com/developer-reference/api-overview/stamps)
- [Enclave secure channels](https://docs.turnkey.com/security/enclave-secure-channels)

### 標準仕様 (primitives only)
- RFC 5869 (HKDF), RFC 6979 (deterministic-k ECDSA), RFC 4231 (HMAC test vectors; SDK HPKE が使う **HMAC-SHA256** を本プランで採用。HMAC-SHA512 は QOS のみで本プラン scope 外)
- NIST CAVP AES-GCM-256, NIST FIPS 186-4 ECDSA SigVer, NIST SHS SHA-256 byte test vectors
