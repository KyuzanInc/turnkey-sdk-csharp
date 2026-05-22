# Unity port reference pins

The pre-existing C# Unity port at
`packages/turnkey-sdk-unity/Runtime/{Encoding,Crypto,ApiKeyStamper,Http,UnityConstants}.cs`
in the peak monorepo is used **only as a C# adaptation reference** for this
.NET port.

## Pin

| Submodule path                       | Submodule URL                                   | Pinned SHA                                   | Tag context        |
|---|---|---|---|
| `packages/turnkey-sdk-unity`         | `git@github.com:KyuzanInc/turnkey-sdk-unity.git` | `039d8e4801095e46cbadca188702535a0e76e5dd`   | v0.1.0-16-g039d8e4 |
| `packages/peak-sdk-unity`            | `git@github.com:KyuzanInc/peak-sdk-unity.git`    | `fc560e8b18bcc28b6a863520e82115513d6c8ced`   | v0.1.1-8-gfc560e8  |

These SHAs are taken from the peak monorepo `git submodule status` output at
the time this repo was bootstrapped (2026-05-23).

## Important: Unity port version drift vs peak-pinned versions

The Unity port was based on slightly newer Turnkey npm versions than what peak
actually consumes:

| Package                    | Unity port version | Peak-pinned version |
|---|---|---|
| @turnkey/crypto            | 2.8.9              | **2.8.8**           |
| @turnkey/http              | 3.16.1             | **3.16.0**          |
| @turnkey/api-key-stamper   | 0.6.0              | **0.5.0**           |
| @turnkey/encoding          | 0.6.0              | 0.6.0               |

**Rule**: when the Unity port and the peak-pinned TS source disagree, the
peak-pinned TS source wins. The Unity port may carry small inherited
divergence and must NOT be treated as authoritative.

## Files mapped to Unity port

| Standalone repo file        | Unity reference file                                       |
|---|---|
| src/Encoding.cs             | packages/turnkey-sdk-unity/Runtime/Encoding.cs             |
| src/CryptoConstants.cs      | packages/turnkey-sdk-unity/Runtime/UnityConstants.cs (rename) |
| src/Crypto.cs               | packages/turnkey-sdk-unity/Runtime/Crypto.cs               |
| src/ApiKeyStamper.cs        | packages/turnkey-sdk-unity/Runtime/ApiKeyStamper.cs        |
| src/Http.cs                 | packages/turnkey-sdk-unity/Runtime/Http.cs                 |
| src/TurnkeyJsonContext.cs   | (no Unity counterpart — new for IL2CPP-safe JSON source-gen) |
