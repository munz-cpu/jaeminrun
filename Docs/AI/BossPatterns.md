# 보스 패턴 사용 안내

전투1 씬의 보스에 BossPatern1이 연결되어 있습니다. 기본 순서는 두쫀쿠 5연발 → 위로 퇴장 → 중앙 ! 경고 → 좌우 U자 왕복 → 복귀 → 잡몹 소환 → 반복입니다.

- 보스를 선택하면 노란 입 위치 Gizmo와 이동 핸들이 보입니다. 핸들을 드래그하거나 Mouth Offset을 수정하세요. 별도 Mouth Transform도 지정할 수 있습니다.
- 두쫀쿠는 발사 순간 플레이어 위치를 조준합니다. Shot Interval, Flight Time, Projectile Gravity로 간격·포물선을 조절합니다.
- Hidden Delay, Warning Duration, Swing Duration, Swing Bottom Y로 돌진을 조절합니다.
- 전투1 씬의 Swing Bottom Y는 현재 7입니다. 값을 높이면 돌진 최저점이 더 위로 올라갑니다.
- 잡몹은 BossMinion 프리팹의 Move Speed로 이동 속도를 조절합니다. 체력 5, 팝콘은 1씩, 내려찍기는 즉사입니다. 바운스는 기존 BouncyRun2D를 자식 Visual에서 재사용하고 지형 높이를 따라갑니다.
- 전투1 씬의 Minion Count는 현재 20입니다. Inspector에서 변경할 수 있습니다.
- Combined Pattern Chance(기본 0.25)는 한 주기에서 두 패턴이 동시에 시작될 확률입니다. 발동 시 두쫀쿠+잡몹 또는 돌진+잡몹을 반반으로 선택하며, 잡몹 단독 단계는 생략합니다. 0이면 항상 기존 순서, 1이면 매 주기 조합이 나옵니다.
- 보스 비활성화/사망 시 소환물과 경고가 정리됩니다.
- PLAYER 루트에는 EntityHealth가 연결되어 있고 기본 체력은 5입니다. 두쫀쿠, 잡몹 접촉, 보스 돌진은 기본 1 피해를 줍니다. 잡몹과 돌진의 연속 접촉 피해 간격은 기본 1초입니다.
- Project Settings에서 Player와 Enemy 레이어의 충돌이 꺼져 있어도, 세 공격은 Collider2D 도형의 실제 겹침을 직접 검사하므로 피해 판정이 동작합니다. 잡몹은 내려찍기 상태를 먼저 처리하여 플레이어에게 접촉 피해를 주지 않고 즉시 죽습니다.
- 피해를 받으면 EntityHealth가 관리하는 스프라이트가 Hit Effect Sec 동안 빨간색으로 표시된 뒤 원래 색으로 돌아옵니다.
- 플레이어가 발사한 팝콘은 플레이어 자신의 EntityHealth를 무시합니다. 체력이 0이 되면 기존 EntityHealth 규칙에 따라 플레이어 오브젝트가 비활성화되고 제거됩니다.

Tools > Boss Patterns > Validate In Play Mode에 재실행 가능한 검증 메뉴를 추가했습니다. 완료 시 Docs/AI/BossValidation.txt에 결과를 기록합니다. 검증 메뉴는 씬 내 보스를 비활성화하는 테스트를 마지막에 수행하고 Play Mode를 종료하며, 저장된 씬은 바꾸지 않습니다.

작업 전 저장된 씬은 전투1.before-boss.unity.txt, 저장 전 사용자 편집까지 포함한 씬은 BeforeBossLive.unity로 백업했습니다. 복구 시 현재 씬을 별도로 백업한 뒤 필요한 버전을 사용하세요.
