# MathCore.WAV

## Описание

MathCore.WAV - это библиотека для работы с аудиофайлами в формате WAV. Она предоставляет функционал для чтения, записи и обработки WAV-файлов, а также для выполнения различных математических операций над аудиоданными.

## Возможности

- Чтение и запись WAV-файлов
- Обработка аудиоданных
- Выполнение математических операций над аудиоданными

## Установка

Для установки библиотеки используйте следующую команду:

```bash
dotnet add package MathCore.WAV
```

## Примеры использования

### Чтение WAV-файла

```csharp
using MathCore.WAV;

var wav_file = new WavFile("path/to/file.wav");
var audio_data = wavFile.Read();
```

### Запись WAV-файла

```csharp
using MathCore.WAV;

var audio_data = new float[] { /* аудиоданные */ };
var wav_file = new WavFile(audio_data);
wav_file.Write("path/to/output.wav");
```

## Требования

- .NET 5.0 или выше

## Лицензия

Этот проект лицензирован под MIT License.