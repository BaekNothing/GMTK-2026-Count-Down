# LLM 로컬 빌드 → GitHub → itch.io 배포

이 프로젝트는 GitHub Actions에서 Unity를 실행하지 않습니다. 프로젝트를
수정한 LLM 에이전트가 현재 PC에 설치된 Unity 6000.3.8f1과 활성화된
Unity Personal 라이선스로 검증, WebGL 빌드, Git 푸시, itch.io 발행을
처리합니다.

## 최초 1회 준비

1. itch.io에 HTML 프로젝트 페이지를 생성합니다.
2. itch.io `Account settings → API keys`에서 API 키를 발급합니다.
3. 저장소의 `.env.itch.example`을 `.env.itch.local`로 복사합니다.
4. `.env.itch.local`의 두 값을 실제 값으로 변경합니다.

```dotenv
BUTLER_API_KEY=발급받은_API_KEY
ITCH_TARGET=itch사용자명/게임-slug:webgl
```

`.env.itch.local`은 Git에서 무시되므로 API 키가 GitHub에 올라가지 않습니다.
키 값에 공백이나 특수문자가 있으면 작은따옴표 또는 큰따옴표로 감쌀 수
있습니다. 환경변수 등록이나 터미널 재시작은 필요하지 않습니다.

## 배포 명령

```powershell
.\Tools\Publish-Itch.ps1
```

스크립트는 다음 작업을 수행합니다.

1. `.env.itch.local` 로드
2. 로컬 Unity 런타임 조립 검증
3. WebGL 프로덕션 빌드
4. `butler`가 없으면 `.tools/butler`에 자동 설치
5. `Builds/WebGL`을 지정된 itch.io 채널에 업로드

Git 커밋과 푸시는 변경 범위를 검토해야 하므로 스크립트가 임의로
`git add -A`하지 않습니다. 루트 `AGENTS.md`가 LLM에게 의도한 파일만
커밋·푸시한 뒤 이 배포 스크립트를 실행하도록 요구합니다.

## 우선순위와 수동 대상 지정

이미 설정된 프로세스 환경변수는 `.env.itch.local`보다 우선합니다.
`-ItchTarget` 인수는 둘보다 우선하므로 임시 채널에도 배포할 수 있습니다.

```powershell
.\Tools\Publish-Itch.ps1 -ItchTarget '사용자명/게임-slug:test'
```

API 키 대신 `butler login`으로 만든 로컬 인증 파일도 사용할 수 있습니다.
