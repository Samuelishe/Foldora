# Requirements

- Windows 11.
- .NET SDK 10.x. Проект target framework: `net10.0` / `net10.0-windows`.
- Без прав администратора.
- Без фонового сервиса.
- JSON для настроек.
- xUnit для unit-тестов.
- HKCU registry для legacy context menu MVP.

PowerShell разрешен только для временного прототипирования и ручной проверки, но не как production-механизм.

## Local Tooling

Основная shell-среда: PowerShell 7 на Windows 11.

Доступные инструменты на текущей машине:

- dotnet SDK 10.x;
- git;
- rg / ripgrep;
- fd;
- bat;
- jq;
- yq;
- cmake;
- ninja;
- MSYS2 UCRT64 gcc/g++/gdb;
- Python 3.13 and 3.14 via Python Launcher;
- winget;
- nmap, только если явно нужна network diagnostics.

Для C/C++ из терминала предпочитать:

- CMake + Ninja;
- MSYS2 UCRT64 GCC/G++.

Не предполагать Visual Studio IDE. Не требовать Visual Studio, если задача явно не требует MSVC или Visual Studio Build Tools.

Не ставить Python-пакеты глобально в system Python. Для Python использовать project-local virtual environments.
