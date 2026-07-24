# LLM 로컬 빌드 → GitHub → itch.io 배포

이 프로젝트는 GitHub Actions에서 Unity를 실행하지 않습니다. 프로젝트를
수정한 LLM 에이전트가 현재 PC에 설치된 Unity 6000.3.8f1과 활성화된
Unity Personal 라이선스를 사용해 직접 검증하고 배포합니다.

## 최초 1회 준비

1. itch.io에 HTML 프로젝트 페이지를 생성합니다.
2. itch.io `Account settings → API keys`에서 API 키를 발급합니다.
3. 사용자 환경변수를 등록합니다.

PowerShell 예시:

```powershell
[Environment]::SetEnvironmentVariable(
    'BUTLER_API_KEY',
    '발급받은_API_KEY',
    'User')

[Environment]::SetEnvironmentVariable(
    'ITCH_TARGET',
    'itch사용자명/게임-slug:webgl',
    'User')
```

환경변수를 등록한 후 Codex/터미널을 다시 시작해야 새 값이 반영됩니다.
API 키는 저장소나 `.env` 파일에 커밋하지 않습니다.

## 배포 명령

```powershell
.\Tools\Publish-Itch.ps1
```

스크립트는 다음 작업을 수행합니다.

1. 로컬 Unity로 런타임 조립 검증
2. WebGL 프로덕션 빌드
3. `butler`가 없으면 `.tools/butler`에 자동 설치
4. 생성된 `Builds/WebGL`을 itch.io `webgl` 채널에 업로드

Git 커밋과 푸시는 리소스 범위를 검토해야 하므로 스크립트가 임의로
`git add -A`하지 않습니다. 대신 루트 `AGENTS.md`가 LLM에게 빌드 성공 후
의도한 파일만 커밋·푸시하고, 마지막에 이 배포 스크립트를 실행하도록
요구합니다.

## 수동 대상 지정

환경변수 대신 한 번만 다른 대상으로 배포할 수 있습니다.

```powershell
.\Tools\Publish-Itch.ps1 -ItchTarget '사용자명/게임-slug:webgl'
```

API 키가 없으면 스크립트가 중단됩니다. `butler login`으로 생성한 로컬
인증 파일이 이미 있는 경우에는 `BUTLER_API_KEY` 없이도 실행할 수 있습니다.
