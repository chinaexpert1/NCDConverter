# Visual Call Chain Diagram

## Async Method Call Flow

```
┌─────────────────────────────────────────────────────────────┐
│                    User Interface (WPF)                     │
│                                                             │
│  [Convert Button] ──► ConvertButton_Click (async void)    │
│                                                             │
└──────────────────────────┬──────────────────────────────────┘
                           │ await
                           ▼
┌─────────────────────────────────────────────────────────────┐
│                     File Processing                         │
│                                                             │
│  ConvertFilesAsync (async Task)                           │
│    └─► Loops through selected files                       │
│        └─► For each file...                               │
└──────────────────────────┬──────────────────────────────────┘
                           │ await
                           ▼
┌─────────────────────────────────────────────────────────────┐
│                    File Type Router                         │
│                                                             │
│  ConvertNCDToParquetAsync (async Task)                    │
│    ├─► If Minute: ConvertMinuteFileAsync                  │
│    └─► If Tick:   ConvertTickFileAsync                    │
└──────────────────────────┬──────────────────────────────────┘
                           │ await
                           ▼
┌─────────────────────────────────────────────────────────────┐
│                    Data Conversion                          │
│                                                             │
│  ConvertMinuteFileAsync OR ConvertTickFileAsync           │
│    ├─► Read NCD file records                              │
│    └─► Call appropriate writer...                         │
└──────────────────────────┬──────────────────────────────────┘
                           │ await
                           ▼
┌─────────────────────────────────────────────────────────────┐
│                   Parquet Writing                           │
│                                                             │
│  WriteMinuteRecordsToParquetAsync OR                      │
│  WriteTickRecordsToParquetAsync                           │
│    ├─► Create schema & columns                            │
│    └─► Write to parquet file...                           │
└──────────────────────────┬──────────────────────────────────┘
                           │ await
                           ▼
┌─────────────────────────────────────────────────────────────┐
│                   Parquet.Net Library                       │
│                   (Version 4.18.1)                          │
│                                                             │
│  await ParquetWriter.CreateAsync(schema, stream)          │
│    └─► groupWriter.CreateRowGroup()                       │
│        └─► await groupWriter.WriteColumnAsync(column)     │
│            └─► (Repeat for each column)                   │
└─────────────────────────────────────────────────────────────┘
```

## Before vs After Comparison

### ❌ OLD API (v3.x) - SYNCHRONOUS
```
MainWindow
    │
    └─► ConvertFiles()                    [void]
        └─► ConvertNCDToParquet()         [void]
            └─► ConvertMinuteFile()       [void]
                └─► WriteMinuteRecords()  [void]
                    │
                    └─► ❌ new ParquetWriter(schema, stream)
                        └─► ❌ groupWriter.WriteColumn(col)
                        
Problem: Constructor and WriteColumn don't exist in v4.x!
```

### ✅ NEW API (v4.x) - ASYNCHRONOUS
```
MainWindow
    │
    └─► ConvertFilesAsync()                    [async Task]
        └─► ConvertNCDToParquetAsync()         [async Task]
            └─► ConvertMinuteFileAsync()       [async Task]
                └─► WriteMinuteRecordsAsync()  [async Task]
                    │
                    └─► ✅ await ParquetWriter.CreateAsync(schema, stream)
                        └─► ✅ await groupWriter.WriteColumnAsync(col)
                        
Solution: Use async factory method and async write operations!
```

## Detailed Method Transformation

### Method 1: WriteMinuteRecordsToParquet
```
BEFORE:                                  AFTER:
┌─────────────────────────────┐         ┌──────────────────────────────────┐
│ void                        │   →    │ async Task                       │
│ WriteMinuteRecordsToParquet │         │ WriteMinuteRecordsToParquetAsync │
│                             │         │                                  │
│ new ParquetWriter(...)      │   →    │ await CreateAsync(...)           │
│ WriteColumn(...)            │   →    │ await WriteColumnAsync(...)      │
└─────────────────────────────┘         └──────────────────────────────────┘
```

### Method 2: WriteTickRecordsToParquet
```
BEFORE:                                  AFTER:
┌─────────────────────────────┐         ┌──────────────────────────────────┐
│ void                        │   →    │ async Task                       │
│ WriteTickRecordsToParquet   │         │ WriteTickRecordsToParquetAsync   │
│                             │         │                                  │
│ new ParquetWriter(...)      │   →    │ await CreateAsync(...)           │
│ WriteColumn(...)            │   →    │ await WriteColumnAsync(...)      │
└─────────────────────────────┘         └──────────────────────────────────┘
```

### Method 3-6: Propagation Up Call Stack
```
┌─────────────────────┐         ┌──────────────────────────┐
│ void                │   →    │ async Task               │
│ ConvertMinuteFile   │         │ ConvertMinuteFileAsync   │
│                     │         │                          │
│ WriteMinute..()     │   →    │ await WriteMinute..()    │
└─────────────────────┘         └──────────────────────────┘

┌─────────────────────┐         ┌──────────────────────────┐
│ void                │   →    │ async Task               │
│ ConvertTickFile     │         │ ConvertTickFileAsync     │
│                     │         │                          │
│ WriteTick..()       │   →    │ await WriteTick..()      │
└─────────────────────┘         └──────────────────────────┘

┌─────────────────────┐         ┌──────────────────────────┐
│ void                │   →    │ async Task               │
│ ConvertNCDToParquet │         │ ConvertNCDToParquetAsync │
│                     │         │                          │
│ ConvertMinute..()   │   →    │ await ConvertMinute..()  │
│ ConvertTick..()     │   →    │ await ConvertTick..()    │
└─────────────────────┘         └──────────────────────────┘

┌─────────────────────┐         ┌──────────────────────────┐
│ void                │   →    │ async Task               │
│ ConvertFiles        │         │ ConvertFilesAsync        │
│                     │         │                          │
│ ConvertNCD..()      │   →    │ await ConvertNCD..()     │
└─────────────────────┘         └──────────────────────────┘
```

## Key Transformation Rules

```
┌──────────────────────────────────────────────────────────────┐
│  RULE 1: Change return type                                  │
│  ────────────────────────────                                │
│  void MyMethod()  →  async Task MyMethodAsync()             │
└──────────────────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────────────────┐
│  RULE 2: Add await to async calls                           │
│  ──────────────────────────────                              │
│  SomeMethod()  →  await SomeMethodAsync()                   │
└──────────────────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────────────────┐
│  RULE 3: Replace constructors with factory methods          │
│  ───────────────────────────────────────────────────         │
│  new ParquetWriter(s, f)  →  await ParquetWriter.CreateAsync │
└──────────────────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────────────────┐
│  RULE 4: Replace sync methods with async equivalents        │
│  ─────────────────────────────────────────────────────       │
│  WriteColumn(col)  →  await WriteColumnAsync(col)           │
└──────────────────────────────────────────────────────────────┘
```

## Error to Fix Mapping

```
ERROR: CS1729 (Constructor not found)
  │
  ├─ Location: Lines 313, 358 in MainWindow.xaml.cs
  ├─ Problem:  new ParquetWriter(schema, fileStream)
  └─ Fix:      await ParquetWriter.CreateAsync(schema, fileStream)

ERROR: CS1061 (Method not found)
  │
  ├─ Location: Lines 316-321, 361-365 in MainWindow.xaml.cs  
  ├─ Problem:  groupWriter.WriteColumn(column)
  └─ Fix:      await groupWriter.WriteColumnAsync(column)
```

## Dependencies & Versions

```
┌────────────────────────────────────────────┐
│  Package: Parquet.Net                      │
│  Version: 4.18.1                           │
│  ────────────────────────────────────      │
│  Breaking changes from v3.x:               │
│    • Constructor → Factory method          │
│    • Sync → Async patterns                 │
│    • WriteColumn → WriteColumnAsync        │
└────────────────────────────────────────────┘

┌────────────────────────────────────────────┐
│  Framework: .NET 8.0 Windows               │
│  Platform: WPF (Windows Presentation       │
│           Foundation)                       │
│  Language: C# 12                           │
└────────────────────────────────────────────┘
```

## Success Indicators

```
BEFORE (Build Failed):
┌──────────────────────────────────┐
│ ❌ 13 Errors                      │
│ ⚠️  3 Warnings                    │
│                                  │
│ Build FAILED                     │
└──────────────────────────────────┘

AFTER (Build Succeeded):
┌──────────────────────────────────┐
│ ✅ 0 Errors                       │
│ ⚠️  3 Warnings (ignorable)        │
│                                  │
│ Build SUCCEEDED                  │
└──────────────────────────────────┘
```

---

**Legend:**
- `→` : Changed to / Transformed into
- `✅` : Correct / Fixed
- `❌` : Incorrect / Broken
- `▼` : Flow continues downward
- `└─►` : Method calls / Dependencies
