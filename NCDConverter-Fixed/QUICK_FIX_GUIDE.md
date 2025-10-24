# Quick Fix Reference: Parquet.Net v4.x Migration

## One-Line Summary
**Changed `new ParquetWriter()` → `await ParquetWriter.CreateAsync()` and `WriteColumn()` → `await WriteColumnAsync()`**

## Quick Migration Checklist

- [ ] Change `new ParquetWriter(schema, stream)` to `await ParquetWriter.CreateAsync(schema, stream)`
- [ ] Change `WriteColumn(column)` to `await WriteColumnAsync(column)`
- [ ] Add `async Task` to method signatures (was `void`)
- [ ] Add `await` before all async calls
- [ ] Propagate async changes up the call chain

## Before/After Quick Reference

### Constructor
```diff
- using (var writer = new ParquetWriter(schema, stream))
+ using (var writer = await ParquetWriter.CreateAsync(schema, stream))
```

### Write Column
```diff
- groupWriter.WriteColumn(column);
+ await groupWriter.WriteColumnAsync(column);
```

### Method Signature
```diff
- private void MyMethod()
+ private async Task MyMethodAsync()
```

### Method Call
```diff
- MyMethod();
+ await MyMethodAsync();
```

## Files Modified in NCDConverter

1. **NCDConverter/MainWindow.xaml.cs**
   - `WriteMinuteRecordsToParquet` → `WriteMinuteRecordsToParquetAsync`
   - `WriteTickRecordsToParquet` → `WriteTickRecordsToParquetAsync`
   - `ConvertMinuteFile` → `ConvertMinuteFileAsync`
   - `ConvertTickFile` → `ConvertTickFileAsync`
   - `ConvertNCDToParquet` → `ConvertNCDToParquetAsync`
   - `ConvertFiles` → `ConvertFilesAsync`
   - Updated `ConvertButton_Click` handler

## Build Command

```bash
cd NCDConverter
dotnet restore
dotnet build --configuration Release
```

## Expected Result

✅ **Build succeeds with 0 errors**
⚠️ 3 warnings (System.Windows.Forms assembly warnings - can be ignored)

## Common Gotchas

1. **Forgot to await**: Results in compiler error CS4014 or runtime issues
2. **Forgot async keyword**: Method can't use await
3. **Wrong return type**: Should be `Task` or `Task<T>`, not `void` (except event handlers)
4. **Breaking call chain**: Need to propagate async all the way up

## When to Use This Pattern

✅ **Use async/await when**:
- Writing/reading files
- Network operations
- Database operations  
- Long-running computations

❌ **Don't use async/await for**:
- Simple property getters/setters
- Pure computation (no I/O)
- Very short operations

## Testing Your Fix

```bash
# Test build
cd NCDConverter
dotnet build

# Should see: Build succeeded.
# Should NOT see: error CS1729 or error CS1061
```

## Resources

- Full documentation: `PARQUET_NET_API_FIXES.md`
- Parquet.Net docs: https://aloneguid.github.io/parquet-dotnet/
- Official repo: https://github.com/aloneguid/parquet-dotnet
