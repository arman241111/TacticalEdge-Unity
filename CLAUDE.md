# Tactical Edge — Документация для Claude

## Проект
FPS шутер в стиле CS2 на Unity 6 (URP). Соревновательный режим 5v5 с бомбой.

## MCP Подключение к Unity
Все команды Unity выполняются через MCP CLI:
```bash
cmd.exe /c "unity-mcp-cli run-tool <TOOL> --input-file - --url http://localhost:20467 --token CV92fMoQlnZpfOV-SmhQw7gn1AMgsfdDNYi-kFVnQnY" <<'EOF'
{JSON_INPUT}
EOF
```

### Настройка MCP (если не работает)
```bash
claude mcp add --transport http ai-game-developer http://localhost:20467/mcp --header "Authorization: Bearer CV92fMoQlnZpfOV-SmhQw7gn1AMgsfdDNYi-kFVnQnY"
```
Unity должен быть открыт с Game Developer → MCP server: Running (http).

### Полезные MCP команды
- `scene-list-opened` — список сцен
- `scene-get-data` — объекты в сцене
- `gameobject-find` — найти объект по имени (передать `{"gameObjectRef":{"instanceID":0,"name":"NAME"}}`)
- `gameobject-create` — создать объект
- `gameobject-modify` — изменить объект
- `gameobject-component-add` — добавить компонент
- `script-execute` — выполнить C# код прямо в Unity (самый мощный инструмент!)
- `script-update-or-create` — создать/обновить скрипт .cs
- `screenshot-scene-view` — скриншот сцены

### script-execute — главный инструмент
Выполняет C# код прямо в Unity. Формат:
```json
{"csharpCode":"using UnityEngine;\npublic class MyClass {\n  public static string Run() {\n    // код\n    return \"результат\";\n  }\n}","className":"MyClass","methodName":"Run"}
```

## Структура проекта
```
Assets/Scripts/
├── PlayerMovement.cs    — движение игрока (New Input System!)
├── SimpleShoot.cs       — стрельба (New Input System!)
├── SimpleCrosshair.cs   — прицел и HUD (OnGUI)
├── PlayerHealth.cs      — здоровье игрока
├── EnemyAI.cs           — ИИ врага (патруль, стрельба, респавн)
├── EnemyHealth.cs       — здоровье врага
├── CharacterSetup.cs    — настройка модели врага (текстуры)
├── MapCollisions.cs     — коллизии карты (MeshCollider)
├── WeaponLoader.cs      — прикрепляет модель оружия к камере
├── WeaponColorFixer.cs  — исправляет белые текстуры оружия
├── GameSetup.cs         — инициализация WeaponSystem
├── TeamSelect.cs        — выбор команды CT/T
├── WeaponSystem.cs      — система оружий (не используется, заменён на SimpleShoot)
├── HUDManager.cs        — менеджер HUD (не используется, заменён на SimpleCrosshair)
```

## Важные правила
1. **New Input System** — Unity использует новую систему ввода. НЕ использовать `Input.GetKey()`, использовать `Mouse.current`, `Keyboard.current`
2. **URP** — материалы из Built-in нужно конвертировать через Render Pipeline Converter
3. **Модели** — НЕ перекрашивать скачанные модели, оставлять оригинальные текстуры
4. **cmd.exe** — MCP CLI работает только через `cmd.exe /c` потому что Unity на Windows
5. **Размеры** — mercenary scale = 1.7, Player CharacterController height = 2, camera Y = 0.6

## Текущие баги
- Враг респавнится в случайных координатах (может провалиться)
- Карта-прототип серая, нужно стилизовать
- Нет системы раундов
- Нет магазина оружий

## Иерархия сцены
```
SampleScene
├── Directional Light
├── Global Volume
├── Player (Tag: Player)
│   └── CameraHolder (Y=0.6)
│       └── Main Camera
│           └── Glock 19 (GEN4) (Tan) — оружие
├── Plane (Scale: 24,1,24) — пол
├── Glock 19 (GEN4) (Tan) — модель в сцене (подхватывается WeaponLoader)
├── Enemy — пустой
├── mercenary (Tag: Enemy, Scale: 1.7) — враг спецназ
├── terrorist — модель террориста (не используется пока)
└── PrototypeMap — карта
```

## Git
- Репо: https://github.com/arman241111/TacticalEdge-Unity
- Пуш через токен: `GH_TOKEN=$(cmd.exe /c "gh auth token" 2>&1 | tr -d '\r\n')`
- Remote: `https://arman241111:${GH_TOKEN}@github.com/arman241111/TacticalEdge-Unity.git`
