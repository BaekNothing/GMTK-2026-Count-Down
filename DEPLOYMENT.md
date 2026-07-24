# GitHub Actions → itch.io 배포 설정

`main` 브랜치에 push하거나 Actions 화면에서 수동 실행하면 WebGL을 빌드한
뒤 itch.io의 `webgl` 채널에 게시합니다.

## 1. itch.io 페이지 준비

1. itch.io에서 새 프로젝트를 만듭니다.
2. 프로젝트 종류는 `HTML`로 설정합니다.
3. 프로젝트 URL의 `사용자명/게임-slug`를 확인합니다.

## 2. GitHub Repository Variables

Repository `Settings → Secrets and variables → Actions → Variables`에 다음을
등록합니다.

- `ITCH_USER`: itch.io 사용자명
- `ITCH_GAME`: itch.io 게임 URL slug

## 3. GitHub Repository Secrets

같은 화면의 `Secrets` 탭에 다음을 등록합니다.

- `BUTLER_API_KEY`: itch.io `Account settings → API keys`에서 발급한 키
- `UNITY_LICENSE`: GameCI 방식으로 활성화한 Unity Personal 라이선스 전문
- `UNITY_EMAIL`: Unity 계정 이메일
- `UNITY_PASSWORD`: Unity 계정 비밀번호

Unity Pro 라이선스를 사용할 경우 워크플로를 `UNITY_SERIAL` 방식으로
조정해야 합니다. 현재 구성은 Unity Personal 기준입니다.

## 4. 첫 실행

Actions의 `Build and publish WebGL` 워크플로를 수동 실행합니다. 성공하면
itch.io 프로젝트의 `webgl` 채널에 빌드가 생성됩니다. 이후 `main`에
push될 때마다 같은 채널이 새 버전으로 갱신됩니다.

워크플로는 배포 전에 필요한 Secret/Variable을 검사하므로, 누락된 값은
빌드를 시작하기 전에 오류 메시지로 알려줍니다.
