# 전투 종료와 공용 결과 화면

- `Assets/Scenes/전투1.unity`의 독립 루트 `Battle End Controller`가 플레이어/보스 EntityHealth를 참조합니다. 사망 이벤트를 먼저 받은 결과를 한 번만 확정합니다.
- 종료 시 입력과 전투 시간을 멈추고 1.2초 동안 검게 페이드한 후 `Assets/Scenes/전투 끝.unity`를 로드합니다. 페이드는 unscaled time을 사용하며 씬이 바뀌면 이전 timeScale을 복구합니다.
- `ScoreManager`는 중복 생성을 막는 DontDestroyOnLoad 싱글톤입니다. 최신 BattleResult는 씬 이동 동안 유지되고, 새로운 전투 시작 시 초기화됩니다. 프로그램 종료 후 디스크에 남기는 영구 저장은 아닙니다.
- 결과 UI는 씬에 실제 Canvas/Text/Image/Button으로 저장되어 에디터에서 직접 꾸밀 수 있습니다. 한글 폰트, 승패별 문구/색상, 등급, 점수, 시간, 남은 체력, 보스 피해량과 재도전 버튼을 제공합니다. 결과 없이 씬을 직접 실행하면 안내 화면과 전투 시작 버튼이 표시됩니다.

## 다른 보스에 재사용

1. 해당 전투 씬의 독립된 빈 루트 오브젝트에 BattleEndController를 추가합니다. 죽으면 삭제되는 보스/플레이어에는 붙이지 않습니다.
2. playerHealth와 bossHealth에 해당 EntityHealth를 연결하고 bossDisplayName을 지정합니다. 특정 BossPatern1 구현에는 의존하지 않습니다.
3. resultScenePath는 공용 `Assets/Scenes/전투 끝.unity`를 사용합니다. 전투 씬과 결과 씬을 활성 빌드 씬 목록에 포함합니다.
4. 결과의 재도전 버튼은 기록에 저장된 원래 전투 씬 경로로 돌아갑니다. 결과 화면을 보스별로 복제할 필요가 없습니다.

## 점수/등급 기본값

- 보스에게 실제 적용된 피해 × 10. 초과 피해는 제외합니다.
- 승리 시 처치 1,000점 + 남은 체력 비율 × 500점 + 120초 중 남은 시간 × 5점.
- 패배에는 승리 보너스가 없으며 획득한 피해 점수를 표시합니다.
- 승리 등급: 체력 80% 이상 S, 50% 이상 A, 20% 이상 B, 그 미만 C. 패배 D.
- 점수 계수와 페이드 시간은 BattleEndController Inspector에서 조절합니다.

## 검증 (2026-09-13)

Unity 6000.6.0f1 컴파일 통과. 결과 씬 직접 시작 → 승리 → 재도전 → 패배를 Play Mode에서 실행했습니다. 초과 피해 제한, 종료 중 중복 사망, 결과 보존, 싱글톤 하나 유지, 시간 복구, 재도전 초기화, 승패 UI 표시 검증 통과. 캡처에서 한글과 레이아웃 확인. 관련 런타임 오류 없음. Unity AI NoSubscription 로그는 기존 도구 구독 관련 메시지입니다. 배포용 Player 빌드는 실행하지 않았습니다.

재검증: 결과 씬에서 Play Mode에 진입하고 `Tools > Battle Results > Run Play Mode Validation`을 실행합니다. 테스트는 임시로 백그라운드 실행을 켜고 완료/종료 시 복원합니다. 결과 로그는 `Docs/AI/BattleResultValidation.txt`입니다.
